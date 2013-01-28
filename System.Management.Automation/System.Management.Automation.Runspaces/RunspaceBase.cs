namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Sqm;
    using System.Threading;

    internal abstract class RunspaceBase : Runspace
    {
        private bool _bSessionStateProxyCallInProgress;
        private bool _bypassRunspaceStateCheck;
        private PSHost _host;
        private System.Management.Automation.Runspaces.InitialSessionState _initialSessionState;
        private ArrayList _runningPipelines;
        private System.Management.Automation.Runspaces.RunspaceAvailability _runspaceAvailability;
        private System.Management.Automation.Runspaces.RunspaceConfiguration _runspaceConfiguration;
        private Queue<RunspaceEventQueueItem> _runspaceEventQueue;
        private System.Management.Automation.Runspaces.RunspaceStateInfo _runspaceStateInfo;
        private SessionStateProxy _sessionStateProxy;
        private object _syncRoot;
        private System.Version _version;
        private Pipeline currentlyRunningPipeline;
        private PipelineBase pulsePipeline;
        internal ManualResetEventSlim RunspaceOpening;

        public override event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;

        public override event EventHandler<RunspaceStateEventArgs> StateChanged;

        protected RunspaceBase(PSHost host, System.Management.Automation.Runspaces.InitialSessionState initialSessionState)
        {
            this._version = PSVersionInfo.PSVersion;
            this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(System.Management.Automation.Runspaces.RunspaceState.BeforeOpen);
            this._syncRoot = new object();
            this._runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
            this.RunspaceOpening = new ManualResetEventSlim(false);
            this._runningPipelines = new ArrayList();
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("initialSessionState");
            }
            this._host = host;
            this._initialSessionState = initialSessionState.Clone();
            base.ApartmentState = initialSessionState.ApartmentState;
            this.ThreadOptions = initialSessionState.ThreadOptions;
        }

        protected RunspaceBase (PSHost host, System.Management.Automation.Runspaces.RunspaceConfiguration runspaceConfiguration)
		{
			this._version = PSVersionInfo.PSVersion;
			this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo (System.Management.Automation.Runspaces.RunspaceState.BeforeOpen);
			this._syncRoot = new object ();
			this._runspaceEventQueue = new Queue<RunspaceEventQueueItem> ();
			this.RunspaceOpening = new ManualResetEventSlim (false);
			this._runningPipelines = new ArrayList ();
			if (host == null) {
				throw PSTraceSource.NewArgumentNullException ("host");
			}
			if (runspaceConfiguration == null) {
				throw PSTraceSource.NewArgumentNullException ("runspaceConfiguration");
			}
			this._host = host;
			this._runspaceConfiguration = runspaceConfiguration;
			if (this._runspaceConfiguration.ImportSystemModules) {
				this._initialSessionState = InitialSessionState.CreateDefault (); 
				ImportSystemModules ();
			}
        }

        protected RunspaceBase (PSHost host, System.Management.Automation.Runspaces.InitialSessionState initialSessionState, bool suppressClone)
		{
			this._version = PSVersionInfo.PSVersion;
			this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo (System.Management.Automation.Runspaces.RunspaceState.BeforeOpen);
			this._syncRoot = new object ();
			this._runspaceEventQueue = new Queue<RunspaceEventQueueItem> ();
			this.RunspaceOpening = new ManualResetEventSlim (false);
			this._runningPipelines = new ArrayList ();
			if (host == null) {
				throw PSTraceSource.NewArgumentNullException ("host");
			}
			if (initialSessionState == null) {
				throw PSTraceSource.NewArgumentNullException ("initialSessionState");
			}
			this._host = host;
			if (suppressClone) {
				this._initialSessionState = initialSessionState;
			} else {
				this._initialSessionState = initialSessionState.Clone ();
			}
            base.ApartmentState = initialSessionState.ApartmentState;
            this.ThreadOptions = initialSessionState.ThreadOptions;
        }

		private void ImportSystemModules ()
		{
			this._initialSessionState.ImportPSModulesFromPath (ModuleIntrinsics.GetSystemwideModulePath ());
			string localModulePath = Environment.GetEnvironmentVariable ("PSMODULEPATH");
			if (!string.IsNullOrEmpty (localModulePath)) {
				this._initialSessionState.ImportPSModulesFromPath (localModulePath);
			}
		}

        internal void AddToRunningPipelineList(PipelineBase pipeline)
        {
            lock (this._runningPipelines.SyncRoot)
            {
                if (!this._bypassRunspaceStateCheck && (this.RunspaceState != System.Management.Automation.Runspaces.RunspaceState.Opened))
                {
                    InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, this.RunspaceState.ToString()), this.RunspaceState, System.Management.Automation.Runspaces.RunspaceState.Opened);
                    throw exception;
                }
                this._runningPipelines.Add(pipeline);
                this.currentlyRunningPipeline = pipeline;
            }
        }

        public override void Close()
        {
            this.CoreClose(true);
        }

        public override void CloseAsync()
        {
            this.CoreClose(false);
        }

        protected abstract void CloseHelper(bool syncCall);
        public override void Connect()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.ConnectNotSupported);
        }

        public override void ConnectAsync()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.ConnectNotSupported);
        }

        private void CoreClose(bool syncCall)
        {
            bool flag = false;
            lock (this.SyncRoot)
            {
                if ((this.RunspaceState == System.Management.Automation.Runspaces.RunspaceState.Closed) || (this.RunspaceState == System.Management.Automation.Runspaces.RunspaceState.Broken))
                {
                    return;
                }
                if (this.RunspaceState == System.Management.Automation.Runspaces.RunspaceState.BeforeOpen)
                {
                    this.SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState.Closing, null);
                    this.SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState.Closed, null);
                    this.RaiseRunspaceStateEvents();
                    return;
                }
                if (this.RunspaceState == System.Management.Automation.Runspaces.RunspaceState.Opening)
                {
                    Monitor.Exit(this.SyncRoot);
                    try
                    {
                        this.RunspaceOpening.Wait();
                    }
                    finally
                    {
                        Monitor.Enter(this.SyncRoot);
                    }
                }
                if (this._bSessionStateProxyCallInProgress)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "RunspaceCloseInvalidWhileSessionStateProxy", new object[0]);
                }
                if (this.RunspaceState == System.Management.Automation.Runspaces.RunspaceState.Closing)
                {
                    flag = true;
                }
                else
                {
                    if (this.RunspaceState != System.Management.Automation.Runspaces.RunspaceState.Opened)
                    {
                        InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotInOpenedState, this.RunspaceState.ToString()), this.RunspaceState, System.Management.Automation.Runspaces.RunspaceState.Opened);
                        throw exception;
                    }
                    this.SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState.Closing);
                }
            }
            if (!flag)
            {
                this.RaiseRunspaceStateEvents();
                PSSQMAPI.NoteRunspaceEnd(base.InstanceId);
                this.CloseHelper(syncCall);
            }
            else if (syncCall)
            {
                this.WaitForFinishofPipelines();
            }
        }

        protected abstract Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested);
        private void CoreOpen(bool syncCall)
        {
            lock (this.SyncRoot)
            {
                if (this.RunspaceState != System.Management.Automation.Runspaces.RunspaceState.BeforeOpen)
                {
                    InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.CannotOpenAgain, new object[] { this.RunspaceState.ToString() }), this.RunspaceState, System.Management.Automation.Runspaces.RunspaceState.BeforeOpen);
                    throw exception;
                }
                this.SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState.Opening);
            }
            this.RaiseRunspaceStateEvents();
            PSSQMAPI.NoteRunspaceStart(base.InstanceId);
            this.OpenHelper(syncCall);
        }

        public override Pipeline CreateDisconnectedPipeline()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.DisconnectConnectNotSupported);
        }

        public override PowerShell CreateDisconnectedPowerShell()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.DisconnectConnectNotSupported);
        }

        public override Pipeline CreateNestedPipeline()
        {
            return this.CoreCreatePipeline(null, false, true);
        }

        public override Pipeline CreateNestedPipeline(string command, bool addToHistory)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            return this.CoreCreatePipeline(command, addToHistory, true);
        }

        public override Pipeline CreatePipeline()
        {
            return this.CoreCreatePipeline(null, false, false);
        }

        public override Pipeline CreatePipeline(string command)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            return this.CoreCreatePipeline(command, false, false);
        }

        public override Pipeline CreatePipeline(string command, bool addToHistory)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            return this.CoreCreatePipeline(command, addToHistory, false);
        }

        public override void Disconnect()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.DisconnectNotSupported);
        }

        public override void DisconnectAsync()
        {
            throw new InvalidRunspaceStateException(RunspaceStrings.DisconnectNotSupported);
        }

        internal void DoConcurrentCheckAndAddToRunningPipelines(PipelineBase pipeline, bool syncCall)
        {
            lock (this._syncRoot)
            {
                if (this._bSessionStateProxyCallInProgress)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoPipelineWhenSessionStateProxyInProgress", new object[0]);
                }
                pipeline.DoConcurrentCheck(syncCall, this._syncRoot, true);
                this.AddToRunningPipelineList(pipeline);
            }
        }

        private void DoConcurrentCheckAndMarkSessionStateProxyCallInProgress()
        {
            lock (this._syncRoot)
            {
                if (this.RunspaceState != System.Management.Automation.Runspaces.RunspaceState.Opened)
                {
                    InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotInOpenedState, this.RunspaceState.ToString()), this.RunspaceState, System.Management.Automation.Runspaces.RunspaceState.Opened);
                    throw exception;
                }
                if (this._bSessionStateProxyCallInProgress)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "AnotherSessionStateProxyInProgress", new object[0]);
                }
                Pipeline currentlyRunningPipeline = this.GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline != null)
                {
                    if ((currentlyRunningPipeline != this.pulsePipeline) && (!currentlyRunningPipeline.IsNested || (this.pulsePipeline == null)))
                    {
                        throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoSessionStateProxyWhenPipelineInProgress", new object[0]);
                    }
                    Monitor.Exit(this._syncRoot);
                    try
                    {
                        this.WaitForFinishofPipelines();
                    }
                    finally
                    {
                        Monitor.Enter(this._syncRoot);
                    }
                    this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                }
                else
                {
                    this._bSessionStateProxyCallInProgress = true;
                }
            }
        }

        protected abstract object DoGetVariable(string name);
        protected abstract void DoSetVariable(string name, object value);
        public override RunspaceCapability GetCapabilities()
        {
            return RunspaceCapability.Default;
        }

        internal override Pipeline GetCurrentlyRunningPipeline()
        {
            return this.currentlyRunningPipeline;
        }

        internal override SessionStateProxy GetSessionStateProxy()
        {
            if (this._sessionStateProxy == null)
            {
                this._sessionStateProxy = new SessionStateProxy(this);
            }
            return this._sessionStateProxy;
        }

        internal object GetVariable(string name)
        {
            object obj2;
            this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
            try
            {
                obj2 = this.DoGetVariable(name);
            }
            finally
            {
                lock (this._syncRoot)
                {
                    this._bSessionStateProxyCallInProgress = false;
                }
            }
            return obj2;
        }

        protected override void OnAvailabilityChanged(RunspaceAvailabilityEventArgs e)
        {
            EventHandler<RunspaceAvailabilityEventArgs> availabilityChanged = this.AvailabilityChanged;
            if (availabilityChanged != null)
            {
                try
                {
                    availabilityChanged(this, e);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        public override void Open()
        {
            this.CoreOpen(true);
        }

        public override void OpenAsync()
        {
            this.CoreOpen(false);
        }

        protected abstract void OpenHelper(bool syncCall);
        internal void Pulse()
        {
            bool flag = false;
            if (this.GetCurrentlyRunningPipeline() == null)
            {
                lock (this.SyncRoot)
                {
                    if (this.GetCurrentlyRunningPipeline() == null)
                    {
                        try
                        {
                            this.pulsePipeline = (PipelineBase) this.CreatePipeline("0");
                            this.pulsePipeline.IsPulsePipeline = true;
                            flag = true;
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
            }
            if (flag)
            {
                try
                {
                    this.pulsePipeline.Invoke();
                }
                catch (PSInvalidOperationException)
                {
                }
                catch (InvalidRunspaceStateException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        protected void RaiseRunspaceStateEvents()
        {
            Queue<RunspaceEventQueueItem> queue = null;
            EventHandler<RunspaceStateEventArgs> stateChanged = null;
            bool hasAvailabilityChangedSubscribers = false;
            lock (this.SyncRoot)
            {
                stateChanged = this.StateChanged;
                hasAvailabilityChangedSubscribers = this.HasAvailabilityChangedSubscribers;
                if ((stateChanged != null) || hasAvailabilityChangedSubscribers)
                {
                    queue = this._runspaceEventQueue;
                    this._runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
                }
                else
                {
                    this._runspaceEventQueue.Clear();
                }
            }
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    RunspaceEventQueueItem item = queue.Dequeue();
                    if (hasAvailabilityChangedSubscribers && (item.NewRunspaceAvailability != item.CurrentRunspaceAvailability))
                    {
                        this.OnAvailabilityChanged(new RunspaceAvailabilityEventArgs(item.NewRunspaceAvailability));
                    }
                    if (stateChanged != null)
                    {
                        try
                        {
                            stateChanged(this, new RunspaceStateEventArgs(item.RunspaceStateInfo));
                            continue;
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            continue;
                        }
                    }
                }
            }
        }

        internal void RemoveFromRunningPipelineList(PipelineBase pipeline)
        {
            lock (this._runningPipelines.SyncRoot)
            {
                this._runningPipelines.Remove(pipeline);
                if (this._runningPipelines.Count == 0)
                {
                    this.currentlyRunningPipeline = null;
                }
                else
                {
                    this.currentlyRunningPipeline = (Pipeline) this._runningPipelines[this._runningPipelines.Count - 1];
                }
                pipeline.PipelineFinishedEvent.Set();
            }
        }

        internal bool RunActionIfNoRunningPipelinesWithThreadCheck(Action action)
        {
            bool flag = false;
            bool flag2 = false;
            lock (this._runningPipelines.SyncRoot)
            {
                PipelineBase currentlyRunningPipeline = this.currentlyRunningPipeline as PipelineBase;
                if ((currentlyRunningPipeline == null) || Thread.CurrentThread.Equals(currentlyRunningPipeline.NestedPipelineExecutionThread))
                {
                    flag2 = true;
                }
            }
            if (flag2)
            {
                action();
                flag = true;
            }
            return flag;
        }

        protected void SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState state)
        {
            this.SetRunspaceState(state, null);
        }

        protected void SetRunspaceState(System.Management.Automation.Runspaces.RunspaceState state, Exception reason)
        {
            lock (this.SyncRoot)
            {
                if (state != this.RunspaceState)
                {
                    this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(state, reason);
                    System.Management.Automation.Runspaces.RunspaceAvailability currentAvailability = this._runspaceAvailability;
                    base.UpdateRunspaceAvailability(this._runspaceStateInfo.State, false);
                    this._runspaceEventQueue.Enqueue(new RunspaceEventQueueItem(this._runspaceStateInfo.Clone(), currentAvailability, this._runspaceAvailability));
                }
            }
        }

        internal void SetVariable(string name, object value)
        {
            this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
            try
            {
                this.DoSetVariable(name, value);
            }
            finally
            {
                lock (this.SyncRoot)
                {
                    this._bSessionStateProxyCallInProgress = false;
                }
            }
        }

        internal void StopNestedPipelines(Pipeline pipeline)
        {
            List<Pipeline> list = null;
            lock (this._runningPipelines.SyncRoot)
            {
                if (!this._runningPipelines.Contains(pipeline) || (this.GetCurrentlyRunningPipeline() == pipeline))
                {
                    return;
                }
                list = new List<Pipeline>();
                for (int i = this._runningPipelines.Count - 1; i >= 0; i--)
                {
                    if (this._runningPipelines[i] == pipeline)
                    {
                        break;
                    }
                    list.Add((Pipeline) this._runningPipelines[i]);
                }
            }
            if (list != null)
            {
                foreach (Pipeline pipeline2 in list)
                {
                    try
                    {
                        pipeline2.Stop();
                    }
                    catch (InvalidPipelineStateException)
                    {
                    }
                }
            }
        }

        protected void StopPipelines()
        {
            PipelineBase[] baseArray;
            lock (this._runningPipelines.SyncRoot)
            {
                baseArray = (PipelineBase[]) this.RunningPipelines.ToArray(typeof(PipelineBase));
            }
            if (baseArray.Length > 0)
            {
                for (int i = baseArray.Length - 1; i >= 0; i--)
                {
                    baseArray[i].Stop();
                }
            }
        }

        internal bool WaitForFinishofPipelines()
        {
            PipelineBase[] baseArray;
            lock (this._runningPipelines.SyncRoot)
            {
                baseArray = (PipelineBase[]) this.RunningPipelines.ToArray(typeof(PipelineBase));
            }
            if (baseArray.Length <= 0)
            {
                return true;
            }
            WaitHandle[] handleArray = new WaitHandle[baseArray.Length];
            for (int i = 0; i < baseArray.Length; i++)
            {
                handleArray[i] = baseArray[i].PipelineFinishedEvent;
            }
            if ((baseArray.Length > 1) && (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA))
            {
                using (ManualResetEvent event2 = new ManualResetEvent(false))
                {
                    Tuple<WaitHandle[], ManualResetEvent> state = new Tuple<WaitHandle[], ManualResetEvent>(handleArray, event2);
                    ThreadPool.QueueUserWorkItem(delegate (object s) {
                        Tuple<WaitHandle[], ManualResetEvent> tuple = (Tuple<WaitHandle[], ManualResetEvent>) s;
                        WaitHandle.WaitAll(tuple.Item1);
                        tuple.Item2.Set();
                    }, state);
                    return event2.WaitOne();
                }
            }
            return WaitHandle.WaitAll(handleArray);
        }

        internal List<string> Applications
        {
            get
            {
                List<string> doApplications;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doApplications = this.DoApplications;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doApplications;
            }
        }

        protected bool ByPassRunspaceStateCheck
        {
            get
            {
                return this._bypassRunspaceStateCheck;
            }
            set
            {
                this._bypassRunspaceStateCheck = value;
            }
        }

        public override RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return null;
            }
        }

        protected abstract List<string> DoApplications { get; }

        protected abstract DriveManagementIntrinsics DoDrive { get; }

        protected abstract CommandInvocationIntrinsics DoInvokeCommand { get; }

        protected abstract ProviderIntrinsics DoInvokeProvider { get; }

        protected abstract PSLanguageMode DoLanguageMode { get; set; }

        protected abstract PSModuleInfo DoModule { get; }

        protected abstract System.Management.Automation.PathIntrinsics DoPath { get; }

        protected abstract CmdletProviderManagementIntrinsics DoProvider { get; }

        protected abstract PSVariableIntrinsics DoPSVariable { get; }

        protected abstract List<string> DoScripts { get; }

        internal DriveManagementIntrinsics Drive
        {
            get
            {
                DriveManagementIntrinsics doDrive;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doDrive = this.DoDrive;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doDrive;
            }
        }

        internal override bool HasAvailabilityChangedSubscribers
        {
            get
            {
                return (this.AvailabilityChanged != null);
            }
        }

        protected PSHost Host
        {
            get
            {
                return this._host;
            }
        }

        public override System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                return this._initialSessionState;
            }
        }

        internal CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                CommandInvocationIntrinsics doInvokeCommand;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doInvokeCommand = this.DoInvokeCommand;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doInvokeCommand;
            }
        }

        internal ProviderIntrinsics InvokeProvider
        {
            get
            {
                ProviderIntrinsics doInvokeProvider;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doInvokeProvider = this.DoInvokeProvider;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doInvokeProvider;
            }
        }

        public PSLanguageMode LanguageMode
        {
            get
            {
                return this.DoLanguageMode;
            }
            set
            {
                this.DoLanguageMode = value;
            }
        }

        internal PSModuleInfo Module
        {
            get
            {
                PSModuleInfo doModule;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doModule = this.DoModule;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doModule;
            }
        }

        public override RunspaceConnectionInfo OriginalConnectionInfo
        {
            get
            {
                return null;
            }
        }

        internal System.Management.Automation.PathIntrinsics PathIntrinsics
        {
            get
            {
                System.Management.Automation.PathIntrinsics doPath;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doPath = this.DoPath;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doPath;
            }
        }

        internal CmdletProviderManagementIntrinsics Provider
        {
            get
            {
                CmdletProviderManagementIntrinsics doProvider;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doProvider = this.DoProvider;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doProvider;
            }
        }

        internal PSVariableIntrinsics PSVariable
        {
            get
            {
                PSVariableIntrinsics doPSVariable;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doPSVariable = this.DoPSVariable;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doPSVariable;
            }
        }

        internal PipelineBase PulsePipeline
        {
            get
            {
                return this.pulsePipeline;
            }
        }

        protected ArrayList RunningPipelines
        {
            get
            {
                return this._runningPipelines;
            }
        }

        public override System.Management.Automation.Runspaces.RunspaceAvailability RunspaceAvailability
        {
            get
            {
                return this._runspaceAvailability;
            }
            protected set
            {
                this._runspaceAvailability = value;
            }
        }

        public override System.Management.Automation.Runspaces.RunspaceConfiguration RunspaceConfiguration
        {
            get
            {
                return this._runspaceConfiguration;
            }
        }

        protected System.Management.Automation.Runspaces.RunspaceState RunspaceState
        {
            get
            {
                return this._runspaceStateInfo.State;
            }
        }

        public override System.Management.Automation.Runspaces.RunspaceStateInfo RunspaceStateInfo
        {
            get
            {
                lock (this._syncRoot)
                {
                    return this._runspaceStateInfo.Clone();
                }
            }
        }

        internal List<string> Scripts
        {
            get
            {
                List<string> doScripts;
                this.DoConcurrentCheckAndMarkSessionStateProxyCallInProgress();
                try
                {
                    doScripts = this.DoScripts;
                }
                finally
                {
                    lock (this._syncRoot)
                    {
                        this._bSessionStateProxyCallInProgress = false;
                    }
                }
                return doScripts;
            }
        }

        protected internal object SyncRoot
        {
            get
            {
                return this._syncRoot;
            }
        }

        public override System.Version Version
        {
            get
            {
                return this._version;
            }
        }

        private class RunspaceEventQueueItem
        {
            public RunspaceAvailability CurrentRunspaceAvailability;
            public RunspaceAvailability NewRunspaceAvailability;
            public System.Management.Automation.Runspaces.RunspaceStateInfo RunspaceStateInfo;

            public RunspaceEventQueueItem(System.Management.Automation.Runspaces.RunspaceStateInfo runspaceStateInfo, RunspaceAvailability currentAvailability, RunspaceAvailability newAvailability)
            {
                this.RunspaceStateInfo = runspaceStateInfo;
                this.CurrentRunspaceAvailability = currentAvailability;
                this.NewRunspaceAvailability = newAvailability;
            }
        }
    }
}

