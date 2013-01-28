namespace System.Management.Automation.Runspaces.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Management.Automation.Tracing;
    using System.Threading;

    internal class RunspacePoolInternal
    {
        protected System.Management.Automation.Runspaces.InitialSessionState _initialSessionState;
        private System.Threading.ApartmentState apartmentState;
        private PSPrimitiveDictionary applicationPrivateData;
        private TimeSpan cleanupInterval;
        private Timer cleanupTimer;
        private static readonly TimeSpan DefaultCleanupPeriod = new TimeSpan(0, 15, 0);
        protected PSHost host;
        protected Guid instanceId;
        private bool isDisposed;
        protected bool isServicingRequests;
        protected int maxPoolSz;
        protected int minPoolSz;
        protected Stack<Runspace> pool;
        protected static string resBaseName = "RunspacePoolStrings";
        protected System.Management.Automation.Runspaces.RunspaceConfiguration rsConfig;
        protected List<Runspace> runspaceList;
        protected Queue<GetRunspaceAsyncResult> runspaceRequestQueue;
        protected System.Management.Automation.RunspacePoolStateInfo stateInfo;
        protected object syncObject;
        private PSThreadOptions threadOptions;
        protected int totalRunspaces;
        protected Queue<GetRunspaceAsyncResult> ultimateRequestQueue;

        public event EventHandler<PSEventArgs> ForwardEvent;

        internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated;

        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged;

        internal RunspacePoolInternal()
        {
            this.runspaceList = new List<Runspace>();
            this.syncObject = new object();
            this.apartmentState = System.Threading.ApartmentState.Unknown;
        }

        protected RunspacePoolInternal(int minRunspaces, int maxRunspaces)
        {
            this.runspaceList = new List<Runspace>();
            this.syncObject = new object();
            this.apartmentState = System.Threading.ApartmentState.Unknown;
            if (maxRunspaces < 1)
            {
                throw PSTraceSource.NewArgumentException("maxRunspaces", resBaseName, "MaxPoolLessThan1", new object[0]);
            }
            if (minRunspaces < 1)
            {
                throw PSTraceSource.NewArgumentException("minRunspaces", resBaseName, "MinPoolLessThan1", new object[0]);
            }
            if (minRunspaces > maxRunspaces)
            {
                throw PSTraceSource.NewArgumentException("minRunspaces", resBaseName, "MinPoolGreaterThanMaxPool", new object[0]);
            }
            this.maxPoolSz = maxRunspaces;
            this.minPoolSz = minRunspaces;
            this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null);
            this.instanceId = Guid.NewGuid();
            PSEtwLog.SetActivityIdForCurrentThread(this.instanceId);
            this.cleanupInterval = DefaultCleanupPeriod;
            this.cleanupTimer = new Timer(new TimerCallback(this.CleanupCallback), null, -1, -1);
        }

        public RunspacePoolInternal(int minRunspaces, int maxRunspaces, System.Management.Automation.Runspaces.InitialSessionState initialSessionState, PSHost host) : this(minRunspaces, maxRunspaces)
        {
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("initialSessionState");
            }
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            this._initialSessionState = initialSessionState.Clone();
            this.apartmentState = initialSessionState.ApartmentState;
            this.threadOptions = initialSessionState.ThreadOptions;
            this.host = host;
            this.pool = new Stack<Runspace>();
            this.runspaceRequestQueue = new Queue<GetRunspaceAsyncResult>();
            this.ultimateRequestQueue = new Queue<GetRunspaceAsyncResult>();
        }

        public RunspacePoolInternal(int minRunspaces, int maxRunspaces, System.Management.Automation.Runspaces.RunspaceConfiguration runspaceConfiguration, PSHost host) : this(minRunspaces, maxRunspaces)
        {
            if (runspaceConfiguration == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceConfiguration");
            }
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            this.rsConfig = runspaceConfiguration;
            this.host = host;
            this.pool = new Stack<Runspace>();
            this.runspaceRequestQueue = new Queue<GetRunspaceAsyncResult>();
            this.ultimateRequestQueue = new Queue<GetRunspaceAsyncResult>();
        }

        protected void AssertIfStateIsBeforeOpen()
        {
            if (this.stateInfo.State != RunspacePoolState.BeforeOpen)
            {
                InvalidRunspacePoolStateException exception = new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.CannotOpenAgain, new object[] { this.stateInfo.State.ToString() }), this.stateInfo.State, RunspacePoolState.BeforeOpen);
                throw exception;
            }
        }

        internal void AssertPoolIsOpen()
        {
            lock (this.syncObject)
            {
                if (this.stateInfo.State != RunspacePoolState.Opened)
                {
                    throw new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Opened, this.stateInfo.State), this.stateInfo.State, RunspacePoolState.Opened);
                }
            }
        }

        public virtual IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.CoreClose(true, callback, state);
        }

        public virtual IAsyncResult BeginConnect(AsyncCallback callback, object state)
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        public virtual IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        internal IAsyncResult BeginGetRunspace(AsyncCallback callback, object state)
        {
            this.AssertPoolIsOpen();
            GetRunspaceAsyncResult requestToEnqueue = new GetRunspaceAsyncResult(this.InstanceId, callback, state);
            this.EnqueueCheckAndStartRequestServicingThread(requestToEnqueue, true);
            return requestToEnqueue;
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.CoreOpen(true, callback, state);
        }

        internal void CancelGetRunspace(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            GetRunspaceAsyncResult result = asyncResult as GetRunspaceAsyncResult;
            if ((result == null) || (result.OwnerId != this.instanceId))
            {
                throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginGetRunspace" });
            }
            result.IsActive = false;
        }

        protected void CleanupCallback(object state)
        {
            bool flag = false;
            while (this.totalRunspaces > this.minPoolSz)
            {
                if (this.stateInfo.State == RunspacePoolState.Closing)
                {
                    return;
                }
                Runspace runspace = null;
                lock (this.pool)
                {
                    if (this.pool.Count <= 0)
                    {
                        break;
                    }
                    runspace = this.pool.Pop();
                }
                if (!flag)
                {
                    lock (this.syncObject)
                    {
                        this.cleanupTimer.Change(-1, -1);
                        flag = true;
                    }
                }
                this.DestroyRunspace(runspace);
            }
        }

        public virtual void Close()
        {
            this.CoreClose(false, null, null);
        }

        private void CloseHelper()
        {
            try
            {
                this.InternalClearAllResources();
            }
            finally
            {
                this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.Closed, null);
                this.RaiseStateChangeEvent(this.stateInfo);
            }
        }

        private void CloseThreadProc(object o)
        {
            AsyncResult result = (AsyncResult) o;
            Exception exception = null;
            try
            {
                this.CloseHelper();
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                exception = exception2;
            }
            finally
            {
                result.SetAsCompleted(exception);
            }
        }

        public virtual void Connect()
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        private IAsyncResult CoreClose(bool isAsync, AsyncCallback callback, object asyncState)
        {
            lock (this.syncObject)
            {
                if (((this.stateInfo.State == RunspacePoolState.Closed) || (this.stateInfo.State == RunspacePoolState.Broken)) || (((this.stateInfo.State == RunspacePoolState.Closing) || (this.stateInfo.State == RunspacePoolState.Disconnecting)) || (this.stateInfo.State == RunspacePoolState.Disconnected)))
                {
                    if (isAsync)
                    {
                        RunspacePoolAsyncResult result = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
                        result.SetAsCompleted(null);
                        return result;
                    }
                    return null;
                }
                this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.Closing, null);
            }
            this.RaiseStateChangeEvent(this.stateInfo);
            if (isAsync)
            {
                RunspacePoolAsyncResult state = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.CloseThreadProc), state);
                return state;
            }
            this.CloseHelper();
            return null;
        }

        protected virtual IAsyncResult CoreOpen(bool isAsync, AsyncCallback callback, object asyncState)
        {
            lock (this.syncObject)
            {
                this.AssertIfStateIsBeforeOpen();
                this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.Opening, null);
            }
            this.RaiseStateChangeEvent(this.stateInfo);
            if (isAsync)
            {
                AsyncResult state = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, true);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.OpenThreadProc), state);
                return state;
            }
            this.OpenHelper();
            return null;
        }

        public virtual Collection<PowerShell> CreateDisconnectedPowerShells(RunspacePool runspacePool)
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        protected Runspace CreateRunspace()
        {
            Runspace runspace = null;
            if (this.rsConfig != null)
            {
                runspace = RunspaceFactory.CreateRunspace(this.host, this.rsConfig);
            }
            else
            {
                runspace = RunspaceFactory.CreateRunspaceFromSessionStateNoClone(this.host, this._initialSessionState);
            }
            runspace.ThreadOptions = (this.ThreadOptions == PSThreadOptions.Default) ? PSThreadOptions.ReuseThread : this.ThreadOptions;
            runspace.ApartmentState = this.ApartmentState;
            this.PropagateApplicationPrivateData(runspace);
            runspace.Open();
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
            {
                runspace.ExecutionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
            }
            runspace.Events.ForwardEvent += new EventHandler<PSEventArgs>(this.OnRunspaceForwardEvent);
            lock (this.runspaceList)
            {
                this.runspaceList.Add(runspace);
                this.totalRunspaces = this.runspaceList.Count;
            }
            lock (this.syncObject)
            {
                this.cleanupTimer.Change(this.CleanupInterval, this.CleanupInterval);
            }
            this.RunspaceCreated.SafeInvoke<RunspaceCreatedEventArgs>(this, new RunspaceCreatedEventArgs(runspace));
            return runspace;
        }

        protected void DestroyRunspace(Runspace runspace)
        {
            runspace.Events.ForwardEvent -= new EventHandler<PSEventArgs>(this.OnRunspaceForwardEvent);
            runspace.Close();
            runspace.Dispose();
            lock (this.runspaceList)
            {
                this.runspaceList.Remove(runspace);
                this.totalRunspaces = this.runspaceList.Count;
            }
        }

        public virtual void Disconnect()
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Close();
                    this.cleanupTimer.Dispose();
                    this._initialSessionState = null;
                    this.host = null;
                }
                this.isDisposed = true;
            }
        }

        public virtual void EndClose(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            RunspacePoolAsyncResult result = asyncResult as RunspacePoolAsyncResult;
            if (((result == null) || (result.OwnerId != this.instanceId)) || result.IsAssociatedWithAsyncOpen)
            {
                throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginClose" });
            }
            result.EndInvoke();
        }

        public virtual void EndConnect(IAsyncResult asyncResult)
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        public virtual void EndDisconnect(IAsyncResult asyncResult)
        {
            throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceDisconnectConnectNotSupported", new object[0]);
        }

        internal Runspace EndGetRunspace(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            GetRunspaceAsyncResult result = asyncResult as GetRunspaceAsyncResult;
            if ((result == null) || (result.OwnerId != this.instanceId))
            {
                throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginGetRunspace" });
            }
            result.EndInvoke();
            return result.Runspace;
        }

        public void EndOpen(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw PSTraceSource.NewArgumentNullException("asyncResult");
            }
            RunspacePoolAsyncResult result = asyncResult as RunspacePoolAsyncResult;
            if (((result == null) || (result.OwnerId != this.instanceId)) || !result.IsAssociatedWithAsyncOpen)
            {
                throw PSTraceSource.NewArgumentException("asyncResult", resBaseName, "AsyncResultNotOwned", new object[] { "IAsyncResult", "BeginOpen" });
            }
            result.EndInvoke();
        }

        protected void EnqueueCheckAndStartRequestServicingThread(GetRunspaceAsyncResult requestToEnqueue, bool useCallingThread)
        {
            bool flag = false;
            lock (this.runspaceRequestQueue)
            {
                if (requestToEnqueue != null)
                {
                    this.runspaceRequestQueue.Enqueue(requestToEnqueue);
                }
                if (this.isServicingRequests)
                {
                    return;
                }
                if ((this.runspaceRequestQueue.Count + this.ultimateRequestQueue.Count) > 0)
                {
                    lock (this.pool)
                    {
                        if ((this.pool.Count > 0) || (this.totalRunspaces < this.maxPoolSz))
                        {
                            this.isServicingRequests = true;
                            if (useCallingThread && (this.ultimateRequestQueue.Count == 0))
                            {
                                flag = true;
                            }
                            else
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServicePendingRequests), false);
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                this.ServicePendingRequests(true);
            }
        }

        internal virtual PSPrimitiveDictionary GetApplicationPrivateData()
        {
            if (this.applicationPrivateData == null)
            {
                lock (this.syncObject)
                {
                    if (this.applicationPrivateData == null)
                    {
                        this.applicationPrivateData = new PSPrimitiveDictionary();
                    }
                }
            }
            return this.applicationPrivateData;
        }

        internal virtual int GetAvailableRunspaces()
        {
            lock (this.syncObject)
            {
                if (this.stateInfo.State == RunspacePoolState.Opened)
                {
                    int num = ((this.maxPoolSz - this.totalRunspaces) < 0) ? 0 : (this.maxPoolSz - this.totalRunspaces);
                    return (this.pool.Count + num);
                }
                if ((this.stateInfo.State != RunspacePoolState.BeforeOpen) && (this.stateInfo.State != RunspacePoolState.Opening))
                {
                    throw new InvalidOperationException(HostInterfaceExceptionsStrings.RunspacePoolNotOpened);
                }
                if (this.stateInfo.State == RunspacePoolState.Disconnected)
                {
                    throw new InvalidOperationException(RunspacePoolStrings.CannotWhileDisconnected);
                }
                return this.maxPoolSz;
            }
        }

        public virtual RunspacePoolCapability GetCapabilities()
        {
            return RunspacePoolCapability.Default;
        }

        public int GetMaxRunspaces()
        {
            return this.maxPoolSz;
        }

        public int GetMinRunspaces()
        {
            return this.minPoolSz;
        }

        public Runspace GetRunspace()
        {
            this.AssertPoolIsOpen();
            GetRunspaceAsyncResult result = (GetRunspaceAsyncResult) this.BeginGetRunspace(null, null);
            result.AsyncWaitHandle.WaitOne();
            if (result.Exception != null)
            {
                throw result.Exception;
            }
            return result.Runspace;
        }

        private void InternalClearAllResources()
        {
            Exception exception = new InvalidRunspacePoolStateException(StringUtil.Format(RunspacePoolStrings.InvalidRunspacePoolState, RunspacePoolState.Opened, this.stateInfo.State), this.stateInfo.State, RunspacePoolState.Opened);
            lock (this.runspaceRequestQueue)
            {
                while (this.runspaceRequestQueue.Count > 0)
                {
                    this.runspaceRequestQueue.Dequeue().SetAsCompleted(exception);
                }
            }
            lock (this.ultimateRequestQueue)
            {
                while (this.ultimateRequestQueue.Count > 0)
                {
                    this.ultimateRequestQueue.Dequeue().SetAsCompleted(exception);
                }
            }
            List<Runspace> list = new List<Runspace>();
            lock (this.runspaceList)
            {
                list.AddRange(this.runspaceList);
                this.runspaceList.Clear();
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                try
                {
                    list[i].Close();
                    list[i].Dispose();
                }
                catch (InvalidRunspaceStateException exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                }
            }
            lock (this.pool)
            {
                this.pool.Clear();
            }
        }

        protected virtual void OnForwardEvent(PSEventArgs e)
        {
            EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
            if (forwardEvent != null)
            {
                forwardEvent(this, e);
            }
        }

        private void OnRunspaceForwardEvent(object sender, PSEventArgs e)
        {
            if (e.ForwardEvent)
            {
                this.OnForwardEvent(e);
            }
        }

        public virtual void Open()
        {
            this.CoreOpen(false, null, null);
        }

        protected void OpenHelper()
        {
            try
            {
                PSEtwLog.SetActivityIdForCurrentThread(this.InstanceId);
                Runspace item = this.CreateRunspace();
                this.pool.Push(item);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this.SetStateToBroken(exception);
                throw;
            }
            bool flag = false;
            lock (this.syncObject)
            {
                if (this.stateInfo.State == RunspacePoolState.Opening)
                {
                    this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.Opened, null);
                    flag = true;
                }
            }
            if (flag)
            {
                this.RaiseStateChangeEvent(this.stateInfo);
            }
        }

        protected void OpenThreadProc(object o)
        {
            AsyncResult result = (AsyncResult) o;
            Exception exception = null;
            try
            {
                this.OpenHelper();
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                exception = exception2;
            }
            finally
            {
                result.SetAsCompleted(exception);
            }
        }

        internal virtual void PropagateApplicationPrivateData(Runspace runspace)
        {
            runspace.SetApplicationPrivateData(this.GetApplicationPrivateData());
        }

		internal void SetStateInfo(System.Management.Automation.RunspacePoolStateInfo stateInfo)
		{
			this.stateInfo = stateInfo;
		}

        protected void RaiseStateChangeEvent(System.Management.Automation.RunspacePoolStateInfo stateInfo)
        {
            this.StateChanged.SafeInvoke<RunspacePoolStateChangedEventArgs>(this, new RunspacePoolStateChangedEventArgs(stateInfo));
        }

        public void ReleaseRunspace(Runspace runspace)
        {
            if (runspace == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspace");
            }
            this.AssertPoolIsOpen();
            bool flag = false;
            bool flag2 = false;
            lock (this.runspaceList)
            {
                if (!this.runspaceList.Contains(runspace))
                {
                    throw PSTraceSource.NewInvalidOperationException(resBaseName, "RunspaceNotBelongsToPool", new object[0]);
                }
            }
            if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
            {
                lock (this.pool)
                {
                    if (this.pool.Count < this.maxPoolSz)
                    {
                        flag = true;
                        this.pool.Push(runspace);
                    }
                    else
                    {
                        flag = true;
                        flag2 = true;
                    }
                    goto Label_00B3;
                }
            }
            flag2 = true;
            flag = true;
        Label_00B3:
            if (flag2)
            {
                this.DestroyRunspace(runspace);
            }
            if (flag)
            {
                this.EnqueueCheckAndStartRequestServicingThread(null, false);
            }
        }

        protected void ServicePendingRequests(object useCallingThreadState)
        {
            if ((this.stateInfo.State == RunspacePoolState.Closed) || (this.stateInfo.State == RunspacePoolState.Closing))
            {
                return;
            }
            bool flag = (bool) useCallingThreadState;
            GetRunspaceAsyncResult result = null;
            try
            {
                bool flag4;
                Queue<GetRunspaceAsyncResult> queue = null;
            Label_0026:
                flag4 = false;
                try
                {
                    Monitor.Enter(queue = this.ultimateRequestQueue, ref flag4);
                    while (this.ultimateRequestQueue.Count > 0)
                    {
                        Runspace runspace;
                        if (this.stateInfo.State == RunspacePoolState.Closing)
                        {
                            return;
                        }
                        lock (this.pool)
                        {
                            if (this.pool.Count > 0)
                            {
                                runspace = this.pool.Pop();
                            }
                            else
                            {
                                if (this.totalRunspaces >= this.maxPoolSz)
                                {
                                    return;
                                }
                                runspace = this.CreateRunspace();
                            }
                        }
                        result = this.ultimateRequestQueue.Dequeue();
                        if (!result.IsActive)
                        {
                            lock (this.pool)
                            {
                                this.pool.Push(runspace);
                            }
                            result.Release();
                        }
                        else
                        {
                            result.Runspace = runspace;
                            if (flag)
                            {
                                goto Label_01B9;
                            }
                            ThreadPool.QueueUserWorkItem(new WaitCallback(result.DoComplete));
                        }
                    }
                }
                finally
                {
                    if (flag4)
                    {
                        Monitor.Exit(queue);
                    }
                }
                lock (this.runspaceRequestQueue)
                {
                    if (this.runspaceRequestQueue.Count != 0)
                    {
                        goto Label_0167;
                    }
                    goto Label_01B9;
                Label_0151:
                    this.ultimateRequestQueue.Enqueue(this.runspaceRequestQueue.Dequeue());
                Label_0167:
                    if (this.runspaceRequestQueue.Count > 0)
                    {
                        goto Label_0151;
                    }
                    goto Label_0026;
                }
            }
            finally
            {
                lock (this.runspaceRequestQueue)
                {
                    this.isServicingRequests = false;
                    this.EnqueueCheckAndStartRequestServicingThread(null, false);
                }
            }
        Label_01B9:
            if (flag && (result != null))
            {
                result.DoComplete(null);
            }
        }

        internal virtual bool SetMaxRunspaces(int maxRunspaces)
        {
            bool flag = false;
            lock (this.pool)
            {
                Runspace runspace;
                if (maxRunspaces < this.minPoolSz)
                {
                    return false;
                }
                if (maxRunspaces <= this.maxPoolSz)
                {
                    goto Label_0041;
                }
                flag = true;
                goto Label_004F;
            Label_002E:
                runspace = this.pool.Pop();
                this.DestroyRunspace(runspace);
            Label_0041:
                if (this.pool.Count > maxRunspaces)
                {
                    goto Label_002E;
                }
            Label_004F:
                this.maxPoolSz = maxRunspaces;
            }
            if (flag)
            {
                this.EnqueueCheckAndStartRequestServicingThread(null, false);
            }
            return true;
        }

        internal virtual bool SetMinRunspaces(int minRunspaces)
        {
            lock (this.pool)
            {
                if ((minRunspaces < 1) || (minRunspaces > this.maxPoolSz))
                {
                    return false;
                }
                this.minPoolSz = minRunspaces;
            }
            return true;
        }

        private void SetStateToBroken(Exception reason)
        {
            bool flag = false;
            lock (this.syncObject)
            {
                if (((this.stateInfo.State == RunspacePoolState.Opening) || (this.stateInfo.State == RunspacePoolState.Opened)) || (((this.stateInfo.State == RunspacePoolState.Disconnecting) || (this.stateInfo.State == RunspacePoolState.Disconnected)) || (this.stateInfo.State == RunspacePoolState.Connecting)))
                {
                    this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(RunspacePoolState.Broken, null);
                    flag = true;
                }
            }
            if (flag)
            {
                System.Management.Automation.RunspacePoolStateInfo stateInfo = new System.Management.Automation.RunspacePoolStateInfo(this.stateInfo.State, reason);
                this.RaiseStateChangeEvent(stateInfo);
            }
        }

        internal System.Threading.ApartmentState ApartmentState
        {
            get
            {
                return this.apartmentState;
            }
            set
            {
                this.apartmentState = value;
            }
        }

        public TimeSpan CleanupInterval
        {
            get
            {
                return this.cleanupInterval;
            }
            set
            {
                lock (this.syncObject)
                {
                    this.cleanupInterval = value;
                }
            }
        }

        public virtual RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return null;
            }
        }

        public System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                return this._initialSessionState;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }

        public System.Management.Automation.Runspaces.RunspaceConfiguration RunspaceConfiguration
        {
            get
            {
                return this.rsConfig;
            }
        }

        public virtual System.Management.Automation.Runspaces.RunspacePoolAvailability RunspacePoolAvailability
        {
            get
            {
                if (this.stateInfo.State != RunspacePoolState.Opened)
                {
                    return System.Management.Automation.Runspaces.RunspacePoolAvailability.None;
                }
                return System.Management.Automation.Runspaces.RunspacePoolAvailability.Available;
            }
        }

        public System.Management.Automation.RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                return this.stateInfo;
            }
        }

        internal PSThreadOptions ThreadOptions
        {
            get
            {
                return this.threadOptions;
            }
            set
            {
                this.threadOptions = value;
            }
        }
    }
}

