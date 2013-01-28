using System;
using System.Globalization;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal class Connection
	{
		private const int Open = 1;

		private const int Close = 2;

		private const int Disconnect = 3;

		private const int Reconnect = 4;

		private Runspace _runspace;

		private bool _busy;

		private bool _idle;

		private readonly object _syncObject;

		private readonly static EventArgs EventArgs;

		private readonly Guid _instanceId;

		private readonly PowerShellTraceSource _tracer;

		private readonly Tracer _structuredTracer;

		private readonly static PSPerfCountersMgr _perfCountersMgr;

		private bool _readyForDisconnect;

		private bool _readyForReconnect;

		private ConnectionManager _manager;

		private uint _retryInterval;

		internal GetRunspaceAsyncResult AsyncResult
		{
			get;
			set;
		}

		internal bool Busy
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._busy;
				}
				return flag;
			}
			set
			{
				lock (this._syncObject)
				{
					this._busy = value;
					if (this._busy)
					{
						this._idle = false;
					}
					this._readyForDisconnect = false;
					this._readyForReconnect = false;
				}
			}
		}

		internal WSManConnectionInfo ConnectionInfo
		{
			get;
			set;
		}

		internal bool DisconnectedIntentionally
		{
			get;
			set;
		}

		internal bool Idle
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._idle;
				}
				return flag;
			}
			set
			{
				lock (this._syncObject)
				{
					this._idle = value;
				}
			}
		}

		internal Guid InstanceId
		{
			get
			{
				return this._instanceId;
			}
		}

		internal bool ReadyForDisconnect
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._readyForDisconnect;
				}
				return flag;
			}
			set
			{
				lock (this._syncObject)
				{
					this._readyForDisconnect = value;
					if (this._readyForDisconnect)
					{
						this._readyForReconnect = false;
					}
				}
			}
		}

		internal bool ReadyForReconnect
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._readyForReconnect;
				}
				return flag;
			}
			set
			{
				lock (this._syncObject)
				{
					this._readyForReconnect = value;
					if (this._readyForReconnect)
					{
						this._readyForDisconnect = false;
					}
				}
			}
		}

		internal uint RetryAttempt
		{
			get;
			set;
		}

		internal uint RetryCount
		{
			get;
			set;
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
				if (this._retryInterval == 0)
				{
					this._retryInterval = 1;
				}
			}
		}

		internal Runspace Runspace
		{
			get
			{
				return this._runspace;
			}
		}

		static Connection()
		{
			Connection.EventArgs = new EventArgs();
			Connection._perfCountersMgr = PSPerfCountersMgr.Instance;
		}

		internal Connection(ConnectionManager manager)
		{
			this._syncObject = new object();
			this._instanceId = Guid.NewGuid();
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._structuredTracer = new Tracer();
			this._retryInterval = 1;
			this._manager = manager;
			this._tracer.WriteMessage("PSW Conn: Creating new connection");
		}

		internal void CloseAsync()
		{
			this._tracer.WriteMessage("PSW Conn: Calling CloseAsync on runspace");
			if (this._runspace.RunspaceStateInfo.State == RunspaceState.Broken || this._runspace.RunspaceStateInfo.State == RunspaceState.Closed)
			{
				this.RaiseEvents(2);
				return;
			}
			else
			{
				this._runspace.CloseAsync();
				return;
			}
		}

		internal void DisconnectAsync()
		{
			bool flag = false;
			if (this._readyForDisconnect)
			{
				lock (this._syncObject)
				{
					if (this._readyForDisconnect)
					{
						this._readyForDisconnect = false;
						this._readyForReconnect = false;
						flag = true;
					}
				}
				if (flag)
				{
					this._tracer.WriteMessage("PSW Conn: Calling Disconnect Async");
					this._manager.DisconnectCalled();
					this._runspace.DisconnectAsync();
					return;
				}
			}
			this.RaiseEvents(3);
		}

		private void DisposeRunspace()
		{
			this._runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.RunspaceStateChanged);
			this._tracer.WriteMessage("PSW Conn: disposing runspace");
			this._runspace.Dispose();
			Connection._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 28, (long)1, true);
		}

		internal void OpenAsync()
		{
			WSManConnectionInfo wSManConnectionInfo = this.ConnectionInfo.Copy();
			wSManConnectionInfo.EnableNetworkAccess = true;
			this._runspace = RunspaceFactory.CreateRunspace(wSManConnectionInfo, null, LocalRunspaceProvider.SharedTypeTable);
			this._runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.RunspaceStateChanged);
			this._tracer.WriteMessage("PSW Conn: Calling OpenAsync on runspace");
			this._runspace.OpenAsync();
		}

		private void RaiseEvents(int eventType)
		{
			int num = eventType;
			if (num == 1)
			{
				if (this.OpenCompleted == null)
				{
					return;
				}
				this.OpenCompleted(this, Connection.EventArgs);
				return;
			}
			else if (num == 2)
			{
				if (this.CloseCompleted == null)
				{
					return;
				}
				this.CloseCompleted(this, Connection.EventArgs);
				return;
			}
			else if (num == 3)
			{
				if (this.DisconnectCompleted == null)
				{
					return;
				}
				this.DisconnectCompleted(this, Connection.EventArgs);
				return;
			}
			else if (num == 4)
			{
				if (this.ReconnectCompleted == null)
				{
					return;
				}
				this.ReconnectCompleted(this, Connection.EventArgs);
			}
			else
			{
				return;
			}
		}

		internal void ReconnectAsync()
		{
			bool flag = false;
			if (this._readyForReconnect)
			{
				lock (this._syncObject)
				{
					if (this._readyForReconnect)
					{
						this._readyForReconnect = false;
						this._readyForDisconnect = false;
						flag = true;
					}
				}
				if (flag)
				{
					this._tracer.WriteMessage("PSW Conn: Calling reconnect async");
					this._manager.ReconnectCalled();
					this._runspace.ConnectAsync();
					return;
				}
			}
			this.RaiseEvents(4);
		}

		private void RetryTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			this._runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.RunspaceStateChanged);
			Timer timer = sender as Timer;
			if (timer != null)
			{
				timer.Dispose();
			}
			this.OpenAsync();
		}

		private void RunspaceStateChanged(object sender, RunspaceStateEventArgs e)
		{
			Guid guid = this._instanceId;
			this._structuredTracer.RunspaceStateChanged(guid.ToString(), e.RunspaceStateInfo.State.ToString(), string.Empty);
			this._tracer.WriteMessage(string.Concat("PSW Conn: runspace state", e.RunspaceStateInfo.State.ToString()));
			RunspaceState state = e.RunspaceStateInfo.State;
			switch (state)
			{
				case RunspaceState.Opened:
				{
					this.RetryAttempt = 0;
					this.ReadyForReconnect = false;
					this.AsyncResult.Connection = this;
					this.AsyncResult.SetAsCompleted(null);
					this.RaiseEvents(1);
					this.RaiseEvents(4);
					return;
				}
				case RunspaceState.Closed:
				{
					this.DisposeRunspace();
					this.RaiseEvents(2);
					return;
				}
				case RunspaceState.Broken:
				{
					if (this.RetryCount <= 0 || this.RetryAttempt >= this.RetryCount)
					{
						this.Busy = false;
						lock (this._syncObject)
						{
							if (this.AsyncResult != null)
							{
								this.AsyncResult.Connection = null;
								this.AsyncResult.SetAsCompleted(e.RunspaceStateInfo.Reason);
							}
						}
						object[] computerName = new object[1];
						computerName[0] = this._runspace.ConnectionInfo.ComputerName;
						this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Disposing broken connection to {0}", computerName));
						this.DisposeRunspace();
						this.RaiseEvents(1);
						this.RaiseEvents(3);
						this.RaiseEvents(2);
						this.RaiseEvents(4);
						return;
					}
					else
					{
						Connection retryAttempt = this;
						retryAttempt.RetryAttempt = retryAttempt.RetryAttempt + 1;
						Timer timer = new Timer();
						timer.AutoReset = false;
						timer.Enabled = false;
						timer.Interval = (double)((float)(this._retryInterval * 0x3e8));
						Timer timer1 = timer;
						timer1.Elapsed += new ElapsedEventHandler(this.RetryTimerElapsed);
						timer1.Start();
						return;
					}
				}
				case RunspaceState.Disconnecting:
				{
					this.ReadyForDisconnect = false;
					return;
				}
				case RunspaceState.Disconnected:
				{
					this.ReadyForReconnect = true;
					this.RaiseEvents(3);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal event EventHandler CloseCompleted;
		internal event EventHandler DisconnectCompleted;
		internal event EventHandler OpenCompleted;
		internal event EventHandler ReconnectCompleted;
	}
}