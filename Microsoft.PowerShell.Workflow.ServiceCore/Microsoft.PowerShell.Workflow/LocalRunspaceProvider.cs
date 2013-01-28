using Microsoft.PowerShell.Activities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Security;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	internal class LocalRunspaceProvider : RunspaceProvider, IDisposable
	{
		private const int Servicing = 1;

		private const int NotServicing = 0;

		private const int MaxRunspacesPossible = -1;

		private const int DefaultMaxRunspaces = -1;

		private readonly TimeBasedCache<Runspace> _runspaceCache;

		private readonly int _maxRunspaces;

		private readonly PSLanguageMode? _languageMode;

		private readonly ConcurrentQueue<LocalRunspaceAsyncResult> _requests;

		private readonly ConcurrentQueue<LocalRunspaceAsyncResult> _callbacks;

		private int _isServicing;

		private int _isServicingCallbacks;

		private readonly PowerShellTraceSource _tracer;

		private readonly static object SyncObject;

		private static TypeTable _sharedTypeTable;

		internal TimeBasedCache<Runspace> RunspaceCache
		{
			get
			{
				return this._runspaceCache;
			}
		}

		internal static TypeTable SharedTypeTable
		{
			get
			{
				if (LocalRunspaceProvider._sharedTypeTable == null)
				{
					lock (LocalRunspaceProvider.SyncObject)
					{
						if (LocalRunspaceProvider._sharedTypeTable == null)
						{
							LocalRunspaceProvider._sharedTypeTable = TypeTable.LoadDefaultTypeFiles();
						}
					}
				}
				return LocalRunspaceProvider._sharedTypeTable;
			}
		}

		static LocalRunspaceProvider()
		{
			LocalRunspaceProvider.SyncObject = new object();
		}

		internal LocalRunspaceProvider(int timeoutSeconds, PSLanguageMode? languageMode) : this(timeoutSeconds, -1, languageMode)
		{
		}

		internal LocalRunspaceProvider(int timeoutSeconds, int maxRunspaces, PSLanguageMode? languageMode)
		{
			this._requests = new ConcurrentQueue<LocalRunspaceAsyncResult>();
			this._callbacks = new ConcurrentQueue<LocalRunspaceAsyncResult>();
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._runspaceCache = new TimeBasedCache<Runspace>(timeoutSeconds);
			this._maxRunspaces = maxRunspaces;
			this._languageMode = languageMode;
		}

		private void AddToPendingCallbacks(LocalRunspaceAsyncResult asyncResult)
		{
			this._callbacks.Enqueue(asyncResult);
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

		private Runspace AssignRunspaceIfPossible()
		{
			Runspace value = null;
			lock (this._runspaceCache.TimerServicingSyncObject)
			{
				IEnumerable<Item<Runspace>> items = this._runspaceCache.Cast<Item<Runspace>>();
				IEnumerator<Item<Runspace>> enumerator = items.Where<Item<Runspace>>((Item<Runspace> item) => !item.Busy).GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						Item<Runspace> current = enumerator.Current;
						current.Idle = false;
						current.Busy = true;
						value = current.Value;
					}
				}
				if ((value == null || value.RunspaceStateInfo.State != RunspaceState.Opened) && (this._maxRunspaces == -1 || this._runspaceCache.Cache.Count < this._maxRunspaces))
				{
					value = LocalRunspaceProvider.CreateLocalActivityRunspace(this._languageMode, true);
					value.Open();
					this._tracer.WriteMessage("New local runspace created");
					this._runspaceCache.Add(new Item<Runspace>(value, value.InstanceId));
				}
			}
			return value;
		}

		public override IAsyncResult BeginGetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval, AsyncCallback callback, object state)
		{
			if (connectionInfo == null)
			{
				LocalRunspaceAsyncResult localRunspaceAsyncResult = new LocalRunspaceAsyncResult(state, callback, Guid.Empty);
				Runspace runspace = this.AssignRunspaceIfPossible();
				if (runspace == null)
				{
					this._requests.Enqueue(localRunspaceAsyncResult);
					this.CheckAndStartRequestServicingThread();
				}
				else
				{
					localRunspaceAsyncResult.Runspace = runspace;
					localRunspaceAsyncResult.CompletedSynchronously = true;
					localRunspaceAsyncResult.SetAsCompleted(null);
				}
				return localRunspaceAsyncResult;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private void CheckAndStartRequestServicingThread()
		{
			if (Interlocked.CompareExchange(ref this._isServicing, 1, 0) == 0)
			{
				this.TraceThreadPoolInfo("QueueUserWorkItem Request Servicing thread");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceRequests));
				return;
			}
			else
			{
				return;
			}
		}

		internal static Runspace CreateLocalActivityRunspace(PSLanguageMode? languageMode = null, bool useCurrentThreadForExecution = true)
		{
			InitialSessionState initialSessionStateWithSharedTypesAndNoFormat = LocalRunspaceProvider.GetInitialSessionStateWithSharedTypesAndNoFormat();
			if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
			{
				initialSessionStateWithSharedTypesAndNoFormat.LanguageMode = PSLanguageMode.ConstrainedLanguage;
			}
			if (languageMode.HasValue && languageMode.HasValue)
			{
				initialSessionStateWithSharedTypesAndNoFormat.LanguageMode = languageMode.Value;
			}
			SessionStateVariableEntry sessionStateVariableEntry = new SessionStateVariableEntry("RunningInPSWorkflowEndpoint", (object)((bool)1), "True if we're in a Workflow Endpoint", ScopedItemOptions.Constant);
			initialSessionStateWithSharedTypesAndNoFormat.Variables.Add(sessionStateVariableEntry);
			Runspace runspace = RunspaceFactory.CreateRunspace(initialSessionStateWithSharedTypesAndNoFormat);
			if (useCurrentThreadForExecution)
			{
				runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
			}
			return runspace;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				this._runspaceCache.Dispose();
			}
		}

		public override Runspace EndGetRunspace(IAsyncResult asyncResult)
		{
			if (asyncResult != null)
			{
				LocalRunspaceAsyncResult localRunspaceAsyncResult = asyncResult as LocalRunspaceAsyncResult;
				if (localRunspaceAsyncResult != null)
				{
					localRunspaceAsyncResult.EndInvoke();
					this._tracer.WriteMessage("LocalRunspaceProvider: Request serviced and runspace returned");
					Runspace runspace = localRunspaceAsyncResult.Runspace;
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

		private static InitialSessionState GetInitialSessionStateWithSharedTypesAndNoFormat()
		{
			InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
			initialSessionState.Types.Clear();
			initialSessionState.Formats.Clear();
			initialSessionState.Types.Add(new SessionStateTypeEntry(LocalRunspaceProvider.SharedTypeTable.Clone(true)));
			initialSessionState.DisableFormatUpdates = true;
			return initialSessionState;
		}

		public override Runspace GetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval)
		{
			IAsyncResult asyncResult = this.BeginGetRunspace(connectionInfo, 0, 0, null, null);
			return this.EndGetRunspace(asyncResult);
		}

		public override void ReleaseRunspace(Runspace runspace)
		{
			bool flag;
			Func<Item<Runspace>, bool> func = null;
			runspace.ResetRunspaceState();
			lock (this._runspaceCache.TimerServicingSyncObject)
			{
				IEnumerable<Item<Runspace>> items = this._runspaceCache.Cast<Item<Runspace>>();
				if (func == null)
				{
					func = (Item<Runspace> item) => item.InstanceId == runspace.InstanceId;
				}
				foreach (Item<Runspace> item1 in items.Where<Item<Runspace>>(func))
				{
					item1.Busy = false;
					flag = true;
				}
				if (!flag)
				{
					throw new InvalidOperationException();
				}
			}
			if (this._maxRunspaces != -1)
			{
				this.CheckAndStartRequestServicingThread();
			}
		}

		internal void Reset()
		{
			foreach (Item<Runspace> item in this._runspaceCache)
			{
				item.Value.Dispose();
			}
			this._runspaceCache.Cache.Clear();
		}

		private void ServiceCallbacks(object state)
		{
			LocalRunspaceAsyncResult localRunspaceAsyncResult = null;
			while (this._callbacks.TryDequeue(out localRunspaceAsyncResult))
			{
				localRunspaceAsyncResult.SetAsCompleted(null);
			}
			Interlocked.CompareExchange(ref this._isServicingCallbacks, 0, 1);
		}

		private void ServiceRequests(object state)
		{
			LocalRunspaceAsyncResult localRunspaceAsyncResult = null;
			bool flag;
			Runspace runspace;
			lock (this._runspaceCache.TimerServicingSyncObject)
			{
				Runspace runspace1 = this.AssignRunspaceIfPossible();
				while (runspace1 != null && this._requests.TryDequeue(out localRunspaceAsyncResult))
				{
					localRunspaceAsyncResult.Runspace = runspace1;
					flag = true;
					this.AddToPendingCallbacks(localRunspaceAsyncResult);
					if (this._runspaceCache.Cache.Count < this._maxRunspaces)
					{
						runspace = this.AssignRunspaceIfPossible();
					}
					else
					{
						runspace = null;
					}
					runspace1 = runspace;
				}
				if (!flag && runspace1 != null)
				{
					this.ReleaseRunspace(runspace1);
				}
			}
			Interlocked.CompareExchange(ref this._isServicing, 0, 1);
		}

		private void TraceThreadPoolInfo(string message)
		{
			object[] objArray = new object[1];
			objArray[0] = message;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PSW ConnMgr: {0}", objArray));
		}
	}
}