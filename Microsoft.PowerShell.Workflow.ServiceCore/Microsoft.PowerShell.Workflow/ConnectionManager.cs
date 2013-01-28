using Microsoft.PowerShell.Activities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal class ConnectionManager : RunspaceProvider, IDisposable
	{
		private const int Servicing = 1;

		private const int NotServicing = 0;

		private const int TimerFired = 1;

		private const int TimerReset = 0;

		private const int CheckForDisconnect = 1;

		private const int DoNotCheckForDisconnect = 0;

		private const long NotMarked = 0L;

		private const long Marked = 1L;

		private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>> _connectionPool;

		private readonly ConcurrentDictionary<string, ConcurrentQueue<Tuple<WaitCallback, object>>> _cleanupComputers;

		private readonly int _idleTimeout;

		private readonly int _maxOutBoundConnections;

		private readonly int _maxConnectedSessions;

		private readonly int _maxDisconnectedSessions;

		private int _isServicing;

		private int _isServicingCallbacks;

		private int _isServicingCleanups;

		private readonly System.Timers.Timer _timer;

		private readonly ConcurrentQueue<ConnectionManager.RequestInfo> _inComingRequests;

		private readonly ConcurrentQueue<GetRunspaceAsyncResult> _callbacks;

		private int _timerFired;

		private readonly PowerShellTraceSource _tracer;

		private readonly object _syncObject;

		private readonly static PSPerfCountersMgr _perfCountersMgr;

		private readonly ManualResetEvent _servicingThreadRelease;

		private readonly ManualResetEvent _timerThreadRelease;

		private readonly List<ConnectionManager.RequestInfo> _pendingRequests;

		private int _connectedSessionCount;

		private int _disconnectedSessionCount;

		private int _createdConnections;

		private int _checkForDisconnect;

		private readonly ConcurrentDictionary<System.Timers.Timer, string> _timerMap;

		private readonly ConcurrentQueue<ThrottleOperation> _pendingQueue;

		private int _inProgressCount;

		private readonly int _throttleLimit;

		private int _isOperationsServiced;

		private int _isReconnectServicing;

		private long _newConnectionMarked;

		private readonly ManualResetEvent _testHelperCloseDone;

		static ConnectionManager()
		{
			ConnectionManager._perfCountersMgr = PSPerfCountersMgr.Instance;
		}

		internal ConnectionManager(int idleTimeout, int maxOutBoundConnections, int throttleLimit, int maxConnectedSessions, int maxDisconnectedSessions)
		{
			this._connectionPool = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>>();
			this._cleanupComputers = new ConcurrentDictionary<string, ConcurrentQueue<Tuple<WaitCallback, object>>>();
			this._inComingRequests = new ConcurrentQueue<ConnectionManager.RequestInfo>();
			this._callbacks = new ConcurrentQueue<GetRunspaceAsyncResult>();
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._syncObject = new object();
			this._servicingThreadRelease = new ManualResetEvent(false);
			this._timerThreadRelease = new ManualResetEvent(true);
			this._pendingRequests = new List<ConnectionManager.RequestInfo>();
			this._timerMap = new ConcurrentDictionary<Timer, string>();
			this._pendingQueue = new ConcurrentQueue<ThrottleOperation>();
			this._testHelperCloseDone = new ManualResetEvent(false);
			this._idleTimeout = idleTimeout;
			this._maxOutBoundConnections = maxOutBoundConnections;
			this._throttleLimit = throttleLimit;
			this._maxConnectedSessions = maxConnectedSessions;
			this._maxDisconnectedSessions = maxDisconnectedSessions;
			Timer timer = new Timer();
			timer.AutoReset = true;
			timer.Interval = (double)this._idleTimeout;
			timer.Enabled = false;
			this._timer = timer;
			this._timer.Elapsed += new ElapsedEventHandler(this.HandleTimerElapsed);
			this._timer.Start();
		}

		internal void AddToPendingCallback(GetRunspaceAsyncResult asyncResult)
		{
			this._callbacks.Enqueue(asyncResult);
			this.CheckAndStartCallbackServicingThread();
		}

		private void AssignConnection(ConnectionManager.RequestInfo requestInfo, Connection connection)
		{
			IAsyncResult asyncResult = requestInfo.AsyncResult;
			GetRunspaceAsyncResult getRunspaceAsyncResult = asyncResult as GetRunspaceAsyncResult;
			connection.Busy = true;
			connection.AsyncResult = getRunspaceAsyncResult;
			getRunspaceAsyncResult.Connection = connection;
			this.AddToPendingCallback(getRunspaceAsyncResult);
		}

		public override IAsyncResult BeginGetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval, AsyncCallback callback, object state)
		{
			GetRunspaceAsyncResult getRunspaceAsyncResult = new GetRunspaceAsyncResult(state, callback, Guid.Empty);
			ConnectionManager.RequestInfo requestInfo = new ConnectionManager.RequestInfo();
			requestInfo.ConnectionInfo = connectionInfo;
			requestInfo.RetryCount = retryCount;
			requestInfo.AsyncResult = getRunspaceAsyncResult;
			requestInfo.RetryInterval = retryInterval;
			ConnectionManager.RequestInfo requestInfo1 = requestInfo;
			this._tracer.WriteMessage("PSW ConnMgr: New incoming request for runspace queued");
			this._inComingRequests.Enqueue(requestInfo1);
			ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 24, (long)1, true);
			this.CheckAndStartRequiredThreads();
			return getRunspaceAsyncResult;
		}

		private void CheckAndStartCallbackServicingThread()
		{
			if (Interlocked.CompareExchange(ref this._isServicingCallbacks, 1, 0) == 0)
			{
				this.TraceThreadPoolInfo("Callback thread");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceCallbacks));
				return;
			}
			else
			{
				return;
			}
		}

		private void CheckAndStartCleanupThread()
		{
			if (Interlocked.CompareExchange(ref this._isServicingCleanups, 1, 0) == 0)
			{
				this.TraceThreadPoolInfo("Cleanup thread");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceCleanupRequests));
				return;
			}
			else
			{
				return;
			}
		}

		private void CheckAndStartConnectionServicingThread()
		{
			if (Interlocked.CompareExchange(ref this._isServicing, 1, 0) == 0)
			{
				this.TraceThreadPoolInfo("QueueUserWorkItem Connection Servicing thread");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceRequests));
				return;
			}
			else
			{
				return;
			}
		}

		private void CheckAndStartDisconnectReconnectThread()
		{
			if (this._checkForDisconnect == 1)
			{
				if (Interlocked.CompareExchange(ref this._isReconnectServicing, 1, 0) == 0)
				{
					this.TraceThreadPoolInfo("Queuing user workitem disconnect/reconnect worker");
					ThreadPool.QueueUserWorkItem(new WaitCallback(this.DisconnectReconnectWorker));
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void CheckAndStartRequiredThreads()
		{
			this.CheckAndStartConnectionServicingThread();
			this.CheckAndStartDisconnectReconnectThread();
			this.CheckAndStartThrottleManagerThread();
			this.CheckAndStartCleanupThread();
		}

		private void CheckAndStartThrottleManagerThread()
		{
			if (Interlocked.CompareExchange(ref this._isOperationsServiced, 1, 0) == 0)
			{
				this.TraceThreadPoolInfo("Queuing user workitem Running operations in throttle queue");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.StartOperationsFromQueue));
				return;
			}
			else
			{
				return;
			}
		}

		internal void ClearAll()
		{
			IEnumerable<Connection> connections = new ConnectionManager.ConnectionEnumerator(this._connectionPool).Cast<Connection>();
			foreach (CloseOperation closeOperation in connections.Select<Connection, CloseOperation>((Connection connection) => new CloseOperation(connection)))
			{
				closeOperation.OperationComplete += new EventHandler(this.OperationComplete);
				this.SubmitOperation(closeOperation);
			}
			this._testHelperCloseDone.WaitOne();
		}

		private Connection CreateConnection(ConnectionManager.RequestInfo requestInfo, ConcurrentDictionary<Guid, Connection> connections)
		{
			Connection connection = new Connection(this);
			connection.ConnectionInfo = requestInfo.ConnectionInfo;
			connection.RetryCount = requestInfo.RetryCount;
			connection.RetryInterval = requestInfo.RetryInterval;
			connection.RetryAttempt = 0;
			connection.AsyncResult = requestInfo.AsyncResult;
			connection.Busy = true;
			Connection connection1 = connection;
			connections.TryAdd(connection1.InstanceId, connection1);
			ConnectionManager connectionManager = this;
			connectionManager._createdConnections = connectionManager._createdConnections + 1;
			ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 27, (long)1, true);
			return connection1;
		}

		internal void DisconnectCalled()
		{
			lock (this._syncObject)
			{
				ConnectionManager connectionManager = this;
				connectionManager._disconnectedSessionCount = connectionManager._disconnectedSessionCount + 1;
				ConnectionManager connectionManager1 = this;
				connectionManager1._connectedSessionCount = connectionManager1._connectedSessionCount - 1;
			}
		}

		private void DisconnectReconnectWorker(object state)
		{
			this.TraceThreadPoolInfo("Running disconnect/reconnect worker");
			Interlocked.CompareExchange(ref this._newConnectionMarked, (long)0, (long)1);
			while (Interlocked.CompareExchange(ref this._newConnectionMarked, (long)1, (long)0) == (long)1)
			{
				foreach (Connection connection in new ConnectionManager.ConnectionEnumerator(this._connectionPool))
				{
					if (this._disconnectedSessionCount > this._maxDisconnectedSessions)
					{
						break;
					}
					if (!connection.Busy || !connection.ReadyForDisconnect)
					{
						continue;
					}
					connection.DisconnectedIntentionally = true;
					this.SubmitOperation(new DisconnectOperation(connection));
				}
				foreach (Connection connection1 in new ConnectionManager.ConnectionEnumerator(this._connectionPool))
				{
					if (this._connectedSessionCount > this._maxConnectedSessions)
					{
						break;
					}
					if (!connection1.ReadyForReconnect)
					{
						continue;
					}
					connection1.DisconnectedIntentionally = false;
					this.SubmitOperation(new ReconnectOperation(connection1));
				}
			}
			this._tracer.WriteMessage("PSW ConnMgr: Exiting disconnect reconnect worker");
			Interlocked.CompareExchange(ref this._isReconnectServicing, 0, 1);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool isDisposing)
		{
			GetRunspaceAsyncResult getRunspaceAsyncResult = null;
			ConnectionManager.RequestInfo requestInfo = null;
			ThrottleOperation throttleOperation = null;
			Interlocked.CompareExchange(ref this._isOperationsServiced, 1, 0);
			Interlocked.CompareExchange(ref this._isServicingCallbacks, 1, 0);
			Interlocked.CompareExchange(ref this._isServicingCleanups, 1, 0);
			Interlocked.CompareExchange(ref this._timerFired, 1, 0);
			this._timer.Elapsed -= new ElapsedEventHandler(this.HandleTimerElapsed);
			this._timer.Dispose();
			while (this._callbacks.Count > 0)
			{
				this._callbacks.TryDequeue(out getRunspaceAsyncResult);
			}
			this._cleanupComputers.Clear();
			this._connectionPool.Clear();
			while (this._inComingRequests.Count > 0)
			{
				this._inComingRequests.TryDequeue(out requestInfo);
			}
			while (this._pendingQueue.Count > 0)
			{
				this._pendingQueue.TryDequeue(out throttleOperation);
			}
			this._pendingRequests.Clear();
			this._timerMap.Clear();
			this._servicingThreadRelease.Close();
			this._timerThreadRelease.Close();
			this._testHelperCloseDone.Close();
			this._tracer.Dispose();
		}

		public override Runspace EndGetRunspace(IAsyncResult asyncResult)
		{
			if (asyncResult != null)
			{
				GetRunspaceAsyncResult getRunspaceAsyncResult = asyncResult as GetRunspaceAsyncResult;
				if (getRunspaceAsyncResult != null)
				{
					getRunspaceAsyncResult.EndInvoke();
					this._tracer.WriteMessage("PSW ConnMgr: Request serviced and runspace returned");
					Runspace runspace = getRunspaceAsyncResult.Connection.Runspace;
					return runspace;
				}
				else
				{
					throw new ArgumentException(Resources.InvalidAsyncResultSpecified, "asyncResult");
				}
			}
			else
			{
				throw new ArgumentNullException("asyncResult");
			}
		}

		internal IEnumerable GetConnectionEnumerator()
		{
			return new ConnectionManager.ConnectionEnumerator(this._connectionPool);
		}

		private Connection GetConnectionForRunspace(Runspace runspace)
		{
			ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs = null;
			ConcurrentDictionary<Guid, Connection> guids = null;
			Connection connection1;
			if (runspace != null)
			{
				WSManConnectionInfo originalConnectionInfo = runspace.OriginalConnectionInfo as WSManConnectionInfo;
				if (originalConnectionInfo == null)
				{
					this._tracer.WriteMessage("PSW ConnMgr: Incoming connectioninfo is null");
					ConnectionManager.ThrowInvalidRunspaceException(runspace);
				}
				string computerName = originalConnectionInfo.ComputerName;
				string shellUri = originalConnectionInfo.ShellUri;
				if (!this._connectionPool.TryGetValue(computerName, out strs))
				{
					this._tracer.WriteMessage(string.Concat("PSW ConnMgr: Cannot find table for computername ", computerName));
					ConnectionManager.ThrowInvalidRunspaceException(runspace);
				}
				if (!strs.TryGetValue(shellUri, out guids))
				{
					this._tracer.WriteMessage(string.Concat("PSW ConnMgr: Cannot find list for config ", shellUri));
					ConnectionManager.ThrowInvalidRunspaceException(runspace);
				}
				ICollection<Connection> values = guids.Values;
				IEnumerator<Connection> enumerator = values.Where<Connection>((Connection connection) => connection.Runspace != null).Where<Connection>((Connection connection) => connection.Runspace.InstanceId == runspace.InstanceId).GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						Connection current = enumerator.Current;
						connection1 = current;
					}
					else
					{
						this._tracer.WriteMessage("PSW ConnMgr: Cannot find the actual connection object");
						ConnectionManager.ThrowInvalidRunspaceException(runspace);
						return null;
					}
				}
				return connection1;
			}
			else
			{
				throw new ArgumentNullException("runspace");
			}
		}

		public override Runspace GetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval)
		{
			IAsyncResult asyncResult = this.BeginGetRunspace(connectionInfo, retryCount, retryInterval, null, null);
			return this.EndGetRunspace(asyncResult);
		}

		private void HandleCleanupWaitTimerElapsed(object sender, ElapsedEventArgs e)
		{
			string str = null;
			ConcurrentQueue<Tuple<WaitCallback, object>> tuples = null;
			Timer timer = sender as Timer;
			this._timerMap.TryGetValue(timer, out str);
			if (!string.IsNullOrEmpty(str))
			{
				this._cleanupComputers.TryRemove(str, out tuples);
			}
			this._timerMap.TryRemove(timer, out str);
			timer.Elapsed -= new ElapsedEventHandler(this.HandleCleanupWaitTimerElapsed);
			timer.Dispose();
			this.CheckAndStartRequiredThreads();
		}

		private void HandleCloseOperationComplete(object sender, EventArgs e)
		{
			ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs = null;
			ConcurrentDictionary<Guid, Connection> guids = null;
			Connection connection = null;
			ConcurrentDictionary<Guid, Connection> guids1 = null;
			ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs1 = null;
			CloseOperation closeOperation = sender as CloseOperation;
			closeOperation.OperationComplete -= new EventHandler(this.HandleCloseOperationComplete);
			Connection connection1 = closeOperation.Connection;
			WSManConnectionInfo connectionInfo = connection1.Runspace.ConnectionInfo as WSManConnectionInfo;
			if (connectionInfo != null)
			{
				string computerName = connectionInfo.ComputerName;
				string shellUri = connectionInfo.ShellUri;
				this._connectionPool.TryGetValue(computerName, out strs);
				if (strs != null)
				{
					strs.TryGetValue(shellUri, out guids);
					if (guids != null)
					{
						guids.TryRemove(connection1.InstanceId, out connection);
						if (guids.Count == 0)
						{
							strs.TryRemove(shellUri, out guids1);
							if (strs.Count == 0)
							{
								this._connectionPool.TryRemove(computerName, out strs1);
								if (strs1 != null)
								{
									this.RaiseCallbacksAfterCleanup(computerName);
								}
								return;
							}
							else
							{
								return;
							}
						}
						else
						{
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void HandleOperationComplete(object sender, EventArgs e)
		{
			ThrottleOperation throttleOperation = sender as ThrottleOperation;
			throttleOperation.OperationComplete -= new EventHandler(this.HandleOperationComplete);
			Interlocked.Decrement(ref this._inProgressCount);
			this.CheckAndStartRequiredThreads();
		}

		private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
		{
			Collection<string> strs = null;
			Collection<Connection> connections = null;
			Connection connection = null;
			ConcurrentDictionary<Guid, Connection> guids = null;
			ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs1 = null;
			if (Interlocked.CompareExchange(ref this._timerFired, 1, 0) != 1)
			{
				this._tracer.WriteMessage("PSW ConnMgr: Timer fired");
				this._servicingThreadRelease.WaitOne();
				this._timerThreadRelease.Reset();
				this._tracer.WriteMessage("PSW ConnMgr: Timer servicing started");
				Collection<string> strs2 = new Collection<string>();
				foreach (string str in strs)
				{
					ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> item = this._connectionPool[str];
					strs = new Collection<string>();
					foreach (string str1 in connections)
					{
						ConcurrentDictionary<Guid, Connection> item1 = item[str1];
						connections = new Collection<Connection>();
						lock (this._syncObject)
						{
							foreach (Connection value in item1.Values)
							{
								if (!value.Idle)
								{
									if (value.Busy)
									{
										continue;
									}
									value.Idle = true;
								}
								else
								{
									connections.Add(value);
								}
							}
						}
						IEnumerator<Connection> enumerator = connections.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								Connection connection1 = str1;
								ConnectionManager connectionManager = this;
								connectionManager._createdConnections = connectionManager._createdConnections - 1;
								item1.TryRemove(connection1.InstanceId, out connection);
								object[] computerName = new object[1];
								computerName[0] = connection1.Runspace.ConnectionInfo.ComputerName;
								this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Closing idle connection to {0}", computerName));
								this.SubmitOperation(new CloseOperation(connection1));
							}
						}
						if (item1.Count != 0)
						{
							continue;
						}
						strs.Add(str1);
					}
					IEnumerator<string> enumerator1 = strs.GetEnumerator();
					using (enumerator1)
					{
						while (enumerator1.MoveNext())
						{
							string str2 = str;
							item.TryRemove(str2, out guids);
						}
					}
					if (item.Keys.Count != 0)
					{
						continue;
					}
					strs2.Add(str);
				}
				foreach (string str3 in strs2)
				{
					this._connectionPool.TryRemove(str3, out strs1);
				}
				this._tracer.WriteMessage("PSW ConnMgr: Timer servicing completed");
				Interlocked.CompareExchange(ref this._timerFired, 0, 1);
				this._timerThreadRelease.Set();
				this.CheckAndStartRequiredThreads();
				return;
			}
			else
			{
				this._tracer.WriteMessage("PSW ConnMgr: Another timer thread is already servicing return");
				return;
			}
		}

		internal bool IsConnectionPoolEmpty()
		{
			return this._connectionPool.Keys.Count > 0;
		}

		public override bool IsDisconnectedByRunspaceProvider(Runspace runspace)
		{
			Connection connectionForRunspace = this.GetConnectionForRunspace(runspace);
			return connectionForRunspace.DisconnectedIntentionally;
		}

		private bool NeedToReturnFromServicing()
		{
			if (this._timerFired != 1)
			{
				if (this._createdConnections >= this._maxConnectedSessions)
				{
					this._tracer.WriteMessage("PSW ConnMgr: Setting check for runspaces disconnect flag");
					Interlocked.CompareExchange(ref this._checkForDisconnect, 1, 0);
				}
				return false;
			}
			else
			{
				this._tracer.WriteMessage("PSW ConnMgr: Returning from servicing since timer fired");
				return true;
			}
		}

		private void OperationComplete(object sender, EventArgs e)
		{
			CloseOperation closeOperation = sender as CloseOperation;
			closeOperation.OperationComplete -= new EventHandler(this.OperationComplete);
			if (this._pendingQueue.Count == 0)
			{
				this._testHelperCloseDone.Set();
			}
		}

		private void RaiseCallbacksAfterCleanup(string computerName)
		{
			Tuple<WaitCallback, object> tuple = null;
			string str = null;
			bool flag = this._timerMap.Values.Contains<string>(computerName, StringComparer.OrdinalIgnoreCase);
			Timer timer = new Timer();
			this._timerMap.TryAdd(timer, computerName);
			timer.Elapsed += new ElapsedEventHandler(this.HandleCleanupWaitTimerElapsed);
			ConcurrentQueue<Tuple<WaitCallback, object>> item = this._cleanupComputers[computerName];
			Collection<Tuple<WaitCallback, object>> tuples = new Collection<Tuple<WaitCallback, object>>();
			int cleanupTimeout = 0;
			bool flag1 = false;
			while (item.TryDequeue(out tuple))
			{
				if (tuple.Item2 != null)
				{
					RunCommandsArguments item2 = tuple.Item2 as RunCommandsArguments;
					if (item2 != null)
					{
						if (cleanupTimeout < item2.CleanupTimeout)
						{
							cleanupTimeout = item2.CleanupTimeout;
						}
						flag1 = true;
					}
				}
				tuples.Add(tuple);
			}
			if (cleanupTimeout == 0)
			{
				if (flag)
				{
					this._timerMap.TryRemove(timer, out str);
				}
				else
				{
					this.HandleCleanupWaitTimerElapsed(timer, null);
				}
			}
			else
			{
				timer.Interval = (double)(cleanupTimeout * 0x3e8);
				timer.AutoReset = false;
				timer.Enabled = true;
				foreach (Tuple<WaitCallback, object> tuple1 in tuples)
				{
					if (tuple1.Item1 == null)
					{
						continue;
					}
					tuple1.Item1(tuple1.Item2);
				}
			}
			if (!flag1 && this._cleanupComputers.TryGetValue(computerName, out item))
			{
				while (item.TryDequeue(out tuple))
				{
					if (tuple.Item1 == null)
					{
						continue;
					}
					tuple.Item1(tuple.Item2);
				}
			}
		}

		public override void ReadyForDisconnect(Runspace runspace)
		{
			Connection connectionForRunspace = this.GetConnectionForRunspace(runspace);
			this._tracer.WriteMessage("PSW ConnMgr: Runspace marked as ready for disconnect");
			if (connectionForRunspace.Busy)
			{
				lock (this._syncObject)
				{
					if (connectionForRunspace.Busy)
					{
						connectionForRunspace.ReadyForDisconnect = true;
						Interlocked.CompareExchange(ref this._newConnectionMarked, (long)0, (long)1);
					}
					else
					{
						return;
					}
				}
				this.CheckAndStartRequiredThreads();
			}
		}

		internal void ReconnectCalled()
		{
			lock (this._syncObject)
			{
				ConnectionManager connectionManager = this;
				connectionManager._connectedSessionCount = connectionManager._connectedSessionCount + 1;
				ConnectionManager connectionManager1 = this;
				connectionManager1._disconnectedSessionCount = connectionManager1._disconnectedSessionCount - 1;
			}
		}

		public override void ReleaseRunspace(Runspace runspace)
		{
			Connection connectionForRunspace = this.GetConnectionForRunspace(runspace);
			this._tracer.WriteMessage("PSW ConnMgr: Runspace released");
			connectionForRunspace.Busy = false;
			connectionForRunspace.AsyncResult = null;
			this.CheckAndStartRequiredThreads();
		}

		public override void RequestCleanup(WSManConnectionInfo connectionInfo, WaitCallback callback, object state)
		{
			string computerName = connectionInfo.ComputerName;
			ConcurrentQueue<Tuple<WaitCallback, object>> orAdd = this._cleanupComputers.GetOrAdd(computerName, new ConcurrentQueue<Tuple<WaitCallback, object>>());
			Tuple<WaitCallback, object> tuple = new Tuple<WaitCallback, object>(callback, state);
			orAdd.Enqueue(tuple);
			this.CheckAndStartCleanupThread();
		}

		private void SafelyReturnFromServicing()
		{
			this.CheckAndStartRequiredThreads();
			this._tracer.WriteMessage("PSW ConnMgr: Safely returning from servicing");
			Interlocked.CompareExchange(ref this._isServicing, 0, 1);
			this._servicingThreadRelease.Set();
		}

		private void ServiceCallbacks(object state)
		{
			GetRunspaceAsyncResult getRunspaceAsyncResult = null;
			while (this._callbacks.TryDequeue(out getRunspaceAsyncResult))
			{
				getRunspaceAsyncResult.SetAsCompleted(null);
			}
			Interlocked.CompareExchange(ref this._isServicingCallbacks, 0, 1);
		}

		private void ServiceCleanupRequests(object state)
		{
			ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs = null;
			ConcurrentDictionary<Guid, Connection> guids = null;
			foreach (string key in this._cleanupComputers.Keys)
			{
				this._connectionPool.TryGetValue(key, out strs);
				if (strs != null)
				{
					foreach (string str in strs.Keys)
					{
						strs.TryGetValue(str, out guids);
						if (guids == null)
						{
							continue;
						}
						lock (this._syncObject)
						{
							ICollection<Connection> values = guids.Values;
							IEnumerable<Connection> connections = values.Where<Connection>((Connection connection) => !connection.Busy);
							foreach (CloseOperation closeOperation in connections.Select<Connection, CloseOperation>((Connection connection) => new CloseOperation(connection)))
							{
								closeOperation.OperationComplete += new EventHandler(this.HandleCloseOperationComplete);
								this.SubmitOperation(closeOperation);
							}
						}
					}
				}
				else
				{
					this.RaiseCallbacksAfterCleanup(key);
				}
			}
			Interlocked.CompareExchange(ref this._isServicingCleanups, 0, 1);
		}

		private bool ServiceOneRequest(ConnectionManager.RequestInfo requestInfo)
		{
			int num;
			ConcurrentDictionary<Guid, Connection> orAdd;
			ConcurrentDictionary<Guid, Connection> item;
			bool flag;
			string computerName = requestInfo.ConnectionInfo.ComputerName;
			string shellUri = requestInfo.ConnectionInfo.ShellUri;
			if (!this._cleanupComputers.ContainsKey(computerName))
			{
				num = 0;
				ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)1, true);
				ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> strs = this._connectionPool.GetOrAdd(computerName, new ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>());
				num = strs.Keys.Sum<string>((string key) => this.table[key].Count);
				orAdd = strs.GetOrAdd(shellUri, new ConcurrentDictionary<Guid, Connection>());
				ICollection<Connection> values = orAdd.Values;
				IEnumerator<Connection> enumerator = values.Where<Connection>((Connection connection) => !connection.Busy).Where<Connection>((Connection connection) => ConnectionManager.ValidateConnection(requestInfo, connection)).GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						Connection current = enumerator.Current;
						this._tracer.WriteMessage("PSW ConnMgr: Assigning existing connection to request");
						this.AssignConnection(requestInfo, current);
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)-1, true);
						flag = true;
					}
					else
					{
						goto Label0;
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		Label0:
			if (num >= this._maxOutBoundConnections)
			{
				Connection connection1 = null;
				IEnumerator<string> enumerator1 = strs.Keys.GetEnumerator();
				using (enumerator1)
				{
					do
					{
						if (!enumerator1.MoveNext())
						{
							break;
						}
						string str = enumerator1.Current;
						item = strs[str];
						ICollection<Connection> connections = item.Values;
						connection1 = connections.Where<Connection>((Connection connection) => !connection.Busy).FirstOrDefault<Connection>();
					}
					while (connection1 == null);
				}
				if (connection1 != null)
				{
					if (!ConnectionManager.ValidateConnection(requestInfo, connection1))
					{
						item.TryRemove(connection1.InstanceId, out connection1);
						this._tracer.WriteMessage("PSW ConnMgr: Closing potential connection and creating a new one to service request");
						ConnectionManager connectionManager = this;
						connectionManager._createdConnections = connectionManager._createdConnections - 1;
						Connection connection2 = this.CreateConnection(requestInfo, orAdd);
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 29, (long)1, true);
						this.SubmitOperation(new CloseOneAndOpenAnotherOperation(connection1, connection2));
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)-1, true);
						return true;
					}
					else
					{
						this._tracer.WriteMessage("PSW ConnMgr: Assigning potential connection to service request");
						this.AssignConnection(requestInfo, connection1);
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)-1, true);
						return true;
					}
				}
				else
				{
					ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)-1, true);
					return false;
				}
			}
			else
			{
				this._tracer.WriteMessage("PSW ConnMgr: Creating new connection to service request");
				Connection connection3 = this.CreateConnection(requestInfo, orAdd);
				this.SubmitOperation(new OpenOperation(connection3));
				ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 25, (long)-1, true);
				return true;
			}
		}

		private void ServiceRequests(object state)
		{
			ConnectionManager.RequestInfo requestInfo = null;
			this.TraceThreadPoolInfo("Starting servicing thread");
			this._timerThreadRelease.WaitOne();
			this._servicingThreadRelease.Reset();
			if (!this.NeedToReturnFromServicing())
			{
				Collection<ConnectionManager.RequestInfo> requestInfos = new Collection<ConnectionManager.RequestInfo>();
				List<ConnectionManager.RequestInfo>.Enumerator enumerator = this._pendingRequests.GetEnumerator();
				try
				{
					do
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
						ConnectionManager.RequestInfo current = enumerator.Current;
						if (!this.ServiceOneRequest(current))
						{
							continue;
						}
						requestInfos.Add(current);
					}
					while (!this.NeedToReturnFromServicing());
				}
				finally
				{
					enumerator.Dispose();
				}
				foreach (ConnectionManager.RequestInfo requestInfo1 in requestInfos)
				{
					this._pendingRequests.Remove(requestInfo1);
					ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 26, (long)-1, true);
				}
				if (!this.NeedToReturnFromServicing())
				{
					do
					{
						if (!this._inComingRequests.TryDequeue(out requestInfo))
						{
							break;
						}
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 24, (long)-1, true);
						if (this.ServiceOneRequest(requestInfo))
						{
							continue;
						}
						this._pendingRequests.Add(requestInfo);
						ConnectionManager._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 26, (long)1, true);
					}
					while (!this.NeedToReturnFromServicing());
					this.SafelyReturnFromServicing();
					return;
				}
				else
				{
					this.SafelyReturnFromServicing();
					return;
				}
			}
			else
			{
				this.SafelyReturnFromServicing();
				return;
			}
		}

		private void StartOperationsFromQueue(object state)
		{
			ThrottleOperation throttleOperation = null;
			this.TraceThreadPoolInfo("Running operations in throttle queue");
			while (this._inProgressCount < this._throttleLimit && this._pendingQueue.TryDequeue(out throttleOperation))
			{
				throttleOperation.OperationComplete += new EventHandler(this.HandleOperationComplete);
				Interlocked.Increment(ref this._inProgressCount);
				throttleOperation.DoOperation();
			}
			this._tracer.WriteMessage("PSW ConnMgr: Done throttling");
			Interlocked.CompareExchange(ref this._isOperationsServiced, 0, 1);
		}

		private void SubmitOperation(ThrottleOperation operation)
		{
			this._pendingQueue.Enqueue(operation);
			this.CheckAndStartThrottleManagerThread();
		}

		private static void ThrowInvalidRunspaceException(Runspace runspace)
		{
			throw new ArgumentException(Resources.InvalidRunspaceSpecified, "runspace");
		}

		private void TraceThreadPoolInfo(string message)
		{
			object[] objArray = new object[1];
			objArray[0] = message;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PSW ConnMgr: {0}", objArray));
		}

		private static bool ValidateConnection(ConnectionManager.RequestInfo requestInfo, Connection connection)
		{
			if (connection.Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
			{
				WSManConnectionInfo connectionInfo = requestInfo.ConnectionInfo;
				WSManConnectionInfo originalConnectionInfo = connection.Runspace.OriginalConnectionInfo as WSManConnectionInfo;
				if (originalConnectionInfo != null)
				{
					if (WorkflowUtils.CompareConnectionUri(connectionInfo, originalConnectionInfo))
					{
						if (WorkflowUtils.CompareShellUri(connectionInfo.ShellUri, originalConnectionInfo.ShellUri))
						{
							if (WorkflowUtils.CompareAuthentication(connectionInfo.AuthenticationMechanism, originalConnectionInfo.AuthenticationMechanism))
							{
								if (WorkflowUtils.CompareCredential(connectionInfo.Credential, originalConnectionInfo.Credential))
								{
									if (WorkflowUtils.CompareCertificateThumbprint(connectionInfo.CertificateThumbprint, originalConnectionInfo.CertificateThumbprint))
									{
										if (WorkflowUtils.CompareProxySettings(connectionInfo, originalConnectionInfo))
										{
											if (WorkflowUtils.CompareOtherWSManSettings(connectionInfo, originalConnectionInfo))
											{
												if (originalConnectionInfo.IdleTimeout >= connectionInfo.IdleTimeout)
												{
													return true;
												}
												else
												{
													return false;
												}
											}
											else
											{
												return false;
											}
										}
										else
										{
											return false;
										}
									}
									else
									{
										return false;
									}
								}
								else
								{
									return false;
								}
							}
							else
							{
								return false;
							}
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private class ConnectionEnumerator : IEnumerator, IEnumerable
		{
			private Connection _currentConnection;

			private ConcurrentDictionary<Guid, Connection> _currentConnections;

			private ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>> _currentTable;

			private readonly IEnumerator _tableEnumerator;

			private IEnumerator _configEnumerator;

			private IEnumerator _connectionEnumerator;

			private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>> _connectionPool;

			public object Current
			{
				get
				{
					return this._currentConnection;
				}
			}

			internal ConnectionEnumerator(ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>> connectionPool)
			{
				this._connectionPool = connectionPool;
				this._tableEnumerator = this._connectionPool.Keys.GetEnumerator();
			}

			public IEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				do
				{
				Label0:
					if (this._connectionEnumerator == null || !this._connectionEnumerator.MoveNext())
					{
						if (this._configEnumerator == null || !this._configEnumerator.MoveNext())
						{
							if (!this._tableEnumerator.MoveNext())
							{
								return false;
							}
							else
							{
								string current = (string)this._tableEnumerator.Current;
								this._connectionPool.TryGetValue(current, out this._currentTable);
								this._configEnumerator = this._currentTable.Keys.GetEnumerator();
								goto Label0;
							}
						}
						else
						{
							string str = (string)this._configEnumerator.Current;
							this._currentTable.TryGetValue(str, out this._currentConnections);
							this._connectionEnumerator = this._currentConnections.Keys.GetEnumerator();
							goto Label0;
						}
					}
					else
					{
						Guid guid = (Guid)this._connectionEnumerator.Current;
						this._currentConnection = null;
						this._currentConnections.TryGetValue(guid, out this._currentConnection);
					}
				}
				while (this._currentConnection == null);
				return true;
			}

			public void Reset()
			{
				this._tableEnumerator.Reset();
				this._currentTable = (ConcurrentDictionary<string, ConcurrentDictionary<Guid, Connection>>)this._tableEnumerator.Current;
				this._configEnumerator = this._currentTable.Keys.GetEnumerator();
				this._currentConnections = (ConcurrentDictionary<Guid, Connection>)this._configEnumerator.Current;
				this._connectionEnumerator = this._currentConnections.Keys.GetEnumerator();
				Guid current = (Guid)this._connectionEnumerator.Current;
				this._currentConnections.TryGetValue(current, out this._currentConnection);
			}
		}

		private class RequestInfo
		{
			private WSManConnectionInfo _connectionInfo;

			private uint _retryCount;

			private GetRunspaceAsyncResult _asyncResult;

			private uint _retryInterval;

			internal GetRunspaceAsyncResult AsyncResult
			{
				get
				{
					return this._asyncResult;
				}
				set
				{
					this._asyncResult = value;
				}
			}

			internal WSManConnectionInfo ConnectionInfo
			{
				get
				{
					return this._connectionInfo;
				}
				set
				{
					this._connectionInfo = value;
				}
			}

			internal uint RetryCount
			{
				get
				{
					return this._retryCount;
				}
				set
				{
					this._retryCount = value;
				}
			}

			internal uint RetryInterval
			{
				get
				{
					return this._retryInterval;
				}
				set
				{
					this._retryInterval = value;
				}
			}

			public RequestInfo()
			{
			}
		}
	}
}