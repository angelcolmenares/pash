namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class RemoteRunspace : Runspace, IDisposable
    {
        private bool _bSessionStateProxyCallInProgress;
        private bool _bypassRunspaceStateCheck;
        private RunspaceConnectionInfo _connectionInfo;
        private bool _disposed;
        private PSRemoteEventManager _eventManager;
        private RunspaceConnectionInfo _originalConnectionInfo;
        private ArrayList _runningPipelines;
        private System.Management.Automation.Runspaces.RunspaceAvailability _runspaceAvailability;
        private Queue<RunspaceEventQueueItem> _runspaceEventQueue;
        private System.Management.Automation.Runspaces.RunspacePool _runspacePool;
        private System.Management.Automation.Runspaces.RunspaceStateInfo _runspaceStateInfo;
        private bool _shouldCloseOnPop;
        private object _syncRoot;
        private System.Version _version;
        private PSThreadOptions createThreadOptions;
        private InvokeCommandCommand currentInvokeCommand;
        private long currentLocalPipelineId;
        private int id;
        private RemoteSessionStateProxy sessionStateProxy;
		private PSPrimitiveDictionary _applicationArguments;

        public override event EventHandler<RunspaceAvailabilityEventArgs> AvailabilityChanged;

        public override event EventHandler<RunspaceStateEventArgs> StateChanged;

        internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

        internal RemoteRunspace(System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            this._runningPipelines = new ArrayList();
            this._syncRoot = new object();
            this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(RunspaceState.BeforeOpen);
            this._version = PSVersionInfo.PSVersion;
            this._runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
            this.id = -1;
            if ((runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Disconnected) || !(runspacePool.ConnectionInfo is WSManConnectionInfo))
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "InvalidRunspacePool", new object[0]);
            }
            this._runspacePool = runspacePool;
            this._runspacePool.RemoteRunspacePoolInternal.SetMinRunspaces(1);
            this._runspacePool.RemoteRunspacePoolInternal.SetMaxRunspaces(1);
            this._connectionInfo = ((WSManConnectionInfo) runspacePool.ConnectionInfo).Copy();
            this.SetRunspaceState(RunspaceState.Disconnected, null);
            this._runspaceAvailability = this._runspacePool.RemoteRunspacePoolInternal.AvailableForConnection ? System.Management.Automation.Runspaces.RunspaceAvailability.None : System.Management.Automation.Runspaces.RunspaceAvailability.Busy;
            this.SetEventHandlers();
            PSEtwLog.SetActivityIdForCurrentThread(base.InstanceId);
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspaceConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[] { base.InstanceId.ToString() });
        }

        internal RemoteRunspace(TypeTable typeTable, RunspaceConnectionInfo connectionInfo, PSHost host, PSPrimitiveDictionary applicationArguments, string name = null, int id = -1)
        {
            this._runningPipelines = new ArrayList();
            this._syncRoot = new object();
            this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(RunspaceState.BeforeOpen);
            this._version = PSVersionInfo.PSVersion;
            this._runspaceEventQueue = new Queue<RunspaceEventQueueItem>();
            this.id = -1;
            PSEtwLog.SetActivityIdForCurrentThread(base.InstanceId);
			this._applicationArguments = applicationArguments;
            PSEtwLog.LogOperationalVerbose(PSEventId.RunspaceConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[] { base.InstanceId.ToString() });
            if (connectionInfo is WSManConnectionInfo)
            {
                this._connectionInfo = ((WSManConnectionInfo) connectionInfo).Copy();
                this._originalConnectionInfo = ((WSManConnectionInfo) connectionInfo).Copy();
            }
            else if (connectionInfo is NewProcessConnectionInfo)
            {
                this._connectionInfo = ((NewProcessConnectionInfo) connectionInfo).Copy();
                this._originalConnectionInfo = ((NewProcessConnectionInfo) connectionInfo).Copy();
            }
            this._runspacePool = new System.Management.Automation.Runspaces.RunspacePool(1, 1, typeTable, host, applicationArguments, connectionInfo, name);
            this.Id = id;
            this.SetEventHandlers();
        }

        internal void AddToRunningPipelineList(RemotePipeline pipeline)
        {
            lock (this._syncRoot)
            {
                if ((!this._bypassRunspaceStateCheck && (this._runspaceStateInfo.State != RunspaceState.Opened)) && (this._runspaceStateInfo.State != RunspaceState.Disconnected))
                {
                    InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, this._runspaceStateInfo.State.ToString()), this._runspaceStateInfo.State, RunspaceState.Opened);
                    if (this.ConnectionInfo != null)
                    {
                        exception.Source = this.ConnectionInfo.ComputerName;
                    }
                    throw exception;
                }
                this._runningPipelines.Add(pipeline);
            }
        }

        private void AssertIfStateIsBeforeOpen()
        {
            lock (this._syncRoot)
            {
                if (this._runspaceStateInfo.State != RunspaceState.BeforeOpen)
                {
                    InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.CannotOpenAgain, new object[] { this._runspaceStateInfo.State.ToString() }), this._runspaceStateInfo.State, RunspaceState.BeforeOpen);
                    throw exception;
                }
            }
        }

        internal void ClearInvokeCommand()
        {
            this.currentLocalPipelineId = 0L;
            this.currentInvokeCommand = null;
        }

        public override void Close()
        {
            try
            {
                IAsyncResult asyncResult = this._runspacePool.BeginClose(null, null);
                this.WaitForFinishofPipelines();
                if (asyncResult != null)
                {
                    this._runspacePool.EndClose(asyncResult);
                }
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        public override void CloseAsync()
        {
            try
            {
                this._runspacePool.BeginClose(null, null);
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        public override void Connect()
        {
            if (!this.CanConnect)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "CannotConnect", new object[0]);
            }
            this.UpdatePoolDisconnectOptions();
            try
            {
                this._runspacePool.Connect();
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        public override void ConnectAsync()
        {
            if (!this.CanConnect)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "CannotConnect", new object[0]);
            }
            this.UpdatePoolDisconnectOptions();
            try
            {
                this._runspacePool.BeginConnect(null, null);
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        private Pipeline CoreCreatePipeline(string command, bool addToHistory, bool isNested)
        {
            return new RemotePipeline(this, command, addToHistory, isNested);
        }

        public override Pipeline CreateDisconnectedPipeline()
        {
            if (this.RemoteCommand == null)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoDisconnectedCommand", new object[0]);
            }
            return new RemotePipeline(this);
        }

        public override PowerShell CreateDisconnectedPowerShell()
        {
            if (this.RemoteCommand == null)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoDisconnectedCommand", new object[0]);
            }
            return new PowerShell(this.RemoteCommand, this);
        }

        public override Pipeline CreateNestedPipeline()
        {
            throw PSTraceSource.NewNotSupportedException("RemotingErrorIdStrings", PSRemotingErrorId.NestedPipelineNotSupported.ToString(), new object[0]);
        }

        public override Pipeline CreateNestedPipeline(string command, bool addToHistory)
        {
            throw PSTraceSource.NewNotSupportedException("RemotingErrorIdStrings", PSRemotingErrorId.NestedPipelineNotSupported.ToString(), new object[0]);
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
            if (!this.CanDisconnect)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "DisconnectNotSupportedOnServer", new object[0]);
            }
            this.UpdatePoolDisconnectOptions();
            try
            {
                this._runspacePool.Disconnect();
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        public override void DisconnectAsync()
        {
            if (!this.CanDisconnect)
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "DisconnectNotSupportedOnServer", new object[0]);
            }
            this.UpdatePoolDisconnectOptions();
            try
            {
                this._runspacePool.BeginDisconnect(null, null);
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this._disposed)
                {
                    lock (this._syncRoot)
                    {
                        if (this._disposed)
                        {
                            return;
                        }
                        this._disposed = true;
                    }
                    if (disposing)
                    {
                        this.Close();
                        try
                        {
                            this._runspacePool.StateChanged -= new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
                            this._runspacePool.RemoteRunspacePoolInternal.HostCallReceived -= new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
                            this._runspacePool.RemoteRunspacePoolInternal.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
                            this._runspacePool.ForwardEvent -= new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
                            this._runspacePool.RemoteRunspacePoolInternal.SessionCreateCompleted -= new EventHandler<CreateCompleteEventArgs>(this.HandleSessionCreateCompleted);
                            this._eventManager = null;
                            this._runspacePool.Dispose();
                        }
                        catch (InvalidRunspacePoolStateException exception)
                        {
                            throw exception.ToInvalidRunspaceStateException();
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void DoConcurrentCheckAndAddToRunningPipelines(RemotePipeline pipeline, bool syncCall)
        {
            lock (this._syncRoot)
            {
                if (this._bSessionStateProxyCallInProgress)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoPipelineWhenSessionStateProxyInProgress", new object[0]);
                }
                pipeline.DoConcurrentCheck(syncCall);
                this.AddToRunningPipelineList(pipeline);
            }
        }

        public override PSPrimitiveDictionary GetApplicationPrivateData()
        {
            PSPrimitiveDictionary applicationPrivateData;
            try
            {
                applicationPrivateData = this._runspacePool.GetApplicationPrivateData();
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
            return applicationPrivateData;
        }

        public override RunspaceCapability GetCapabilities()
        {
            RunspaceCapability capability = RunspaceCapability.Default;
            if (this.CanDisconnect)
            {
                capability |= RunspaceCapability.SupportsDisconnect;
            }
            return capability;
        }

        internal override Pipeline GetCurrentlyRunningPipeline()
        {
            lock (this._syncRoot)
            {
                if (this._runningPipelines.Count != 0)
                {
                    return (Pipeline) this._runningPipelines[this._runningPipelines.Count - 1];
                }
                return null;
            }
        }

        internal static Runspace[] GetRemoteRunspaces(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            List<Runspace> list = new List<Runspace>();
            foreach (System.Management.Automation.Runspaces.RunspacePool pool in RemoteRunspacePoolInternal.GetRemoteRunspacePools(connectionInfo, host, typeTable))
            {
                if (pool.RemoteRunspacePoolInternal.ConnectCommands.Length < 2)
                {
                    list.Add(new RemoteRunspace(pool));
                }
            }
            return list.ToArray();
        }

        internal override SessionStateProxy GetSessionStateProxy()
        {
            if (this.sessionStateProxy == null)
            {
                this.sessionStateProxy = new RemoteSessionStateProxy(this);
            }
            return this.sessionStateProxy;
        }

        private void HandleHostCallReceived(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            ClientMethodExecutor.Dispatch(this._runspacePool.RemoteRunspacePoolInternal.DataStructureHandler.TransportManager, this._runspacePool.RemoteRunspacePoolInternal.Host, null, null, false, this._runspacePool.RemoteRunspacePoolInternal, Guid.Empty, eventArgs.Data);
        }

        private void HandleRunspacePoolForwardEvent(object sender, PSEventArgs e)
        {
            this._eventManager.AddForwardedEvent(e);
        }

        private void HandleRunspacePoolStateChanged(object sender, RunspacePoolStateChangedEventArgs e)
        {
            this.SetRunspaceState((RunspaceState) e.RunspacePoolStateInfo.State, e.RunspacePoolStateInfo.Reason);
            this.RaiseRunspaceStateEvents();
        }

        private void HandleSessionCreateCompleted(object sender, CreateCompleteEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                this._connectionInfo.IdleTimeout = eventArgs.ConnectionInfo.IdleTimeout;
                this._connectionInfo.MaxIdleTimeout = eventArgs.ConnectionInfo.MaxIdleTimeout;
                WSManConnectionInfo info = this._connectionInfo as WSManConnectionInfo;
                if (info != null)
                {
                    info.OutputBufferingMode = ((WSManConnectionInfo) eventArgs.ConnectionInfo).OutputBufferingMode;
                }
            }
        }

        private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
        {
            WSManConnectionInfo info = this._connectionInfo as WSManConnectionInfo;
            if (info != null)
            {
                info.ConnectionUri = eventArgs.Data;
                this.URIRedirectionReported.SafeInvoke<RemoteDataEventArgs<Uri>>(this, eventArgs);
            }
        }

        internal bool IsAnotherInvokeCommandExecuting(InvokeCommandCommand invokeCommand, long localPipelineId)
        {
            if ((this.currentLocalPipelineId != localPipelineId) && (this.currentLocalPipelineId != 0L))
            {
                return false;
            }
            if (this.currentInvokeCommand == null)
            {
                this.SetCurrentInvokeCommand(invokeCommand, localPipelineId);
                return false;
            }
            if (this.currentInvokeCommand.Equals(invokeCommand))
            {
                return false;
            }
            return true;
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
            this.AssertIfStateIsBeforeOpen();
            try
            {
                this._runspacePool.ThreadOptions = this.ThreadOptions;
                this._runspacePool.ApartmentState = base.ApartmentState;
                this._runspacePool.Open();
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        public override void OpenAsync()
        {
            this.AssertIfStateIsBeforeOpen();
            try
            {
                this._runspacePool.BeginOpen(null, null);
            }
            catch (InvalidRunspacePoolStateException exception)
            {
                throw exception.ToInvalidRunspaceStateException();
            }
        }

        private void RaiseRunspaceStateEvents()
        {
            Queue<RunspaceEventQueueItem> queue = null;
            EventHandler<RunspaceStateEventArgs> stateChanged = null;
            bool hasAvailabilityChangedSubscribers = false;
            lock (this._syncRoot)
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

        internal void RemoveFromRunningPipelineList(RemotePipeline pipeline)
        {
            lock (this._syncRoot)
            {
                this._runningPipelines.Remove(pipeline);
                pipeline.PipelineFinishedEvent.Set();
            }
        }

        internal override void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
        {
        }

        internal void SetCurrentInvokeCommand(InvokeCommandCommand invokeCommand, long localPipelineId)
        {
            this.currentInvokeCommand = invokeCommand;
            this.currentLocalPipelineId = localPipelineId;
        }

        private void SetEventHandlers()
        {
            base.InstanceId = this._runspacePool.InstanceId;
            this._eventManager = new PSRemoteEventManager(this._connectionInfo.ComputerName, base.InstanceId);
            this._runspacePool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.HandleRunspacePoolStateChanged);
            this._runspacePool.RemoteRunspacePoolInternal.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
            this._runspacePool.RemoteRunspacePoolInternal.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
            this._runspacePool.ForwardEvent += new EventHandler<PSEventArgs>(this.HandleRunspacePoolForwardEvent);
            this._runspacePool.RemoteRunspacePoolInternal.SessionCreateCompleted += new EventHandler<CreateCompleteEventArgs>(this.HandleSessionCreateCompleted);
        }

        private void SetRunspaceState(RunspaceState state, Exception reason)
        {
            lock (this._syncRoot)
            {
                if (state != this._runspaceStateInfo.State)
                {
                    this._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(state, reason);
                    System.Management.Automation.Runspaces.RunspaceAvailability currentAvailability = this._runspaceAvailability;
                    base.UpdateRunspaceAvailability(this._runspaceStateInfo.State, false);
                    this._runspaceEventQueue.Enqueue(new RunspaceEventQueueItem(this._runspaceStateInfo.Clone(), currentAvailability, this._runspaceAvailability));
                    PSEtwLog.LogOperationalVerbose(PSEventId.RunspaceStateChange, PSOpcode.Open, PSTask.CreateRunspace, PSKeyword.UseAlwaysOperational, new object[] { state.ToString() });
                }
            }
        }

        private void UpdatePoolDisconnectOptions()
        {
            WSManConnectionInfo connectionInfo = this._runspacePool.ConnectionInfo as WSManConnectionInfo;
            WSManConnectionInfo info2 = this.ConnectionInfo as WSManConnectionInfo;
            connectionInfo.IdleTimeout = info2.IdleTimeout;
            connectionInfo.OutputBufferingMode = info2.OutputBufferingMode;
        }

        private bool WaitForFinishofPipelines()
        {
            RemotePipeline[] pipelineArray;
            lock (this._syncRoot)
            {
                pipelineArray = (RemotePipeline[]) this._runningPipelines.ToArray(typeof(RemotePipeline));
            }
            if (pipelineArray.Length <= 0)
            {
                return true;
            }
            WaitHandle[] waitHandles = new WaitHandle[pipelineArray.Length];
            for (int i = 0; i < pipelineArray.Length; i++)
            {
                waitHandles[i] = pipelineArray[i].PipelineFinishedEvent;
            }
            return WaitHandle.WaitAll(waitHandles);
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

        internal bool CanConnect
        {
            get
            {
                return this._runspacePool.RemoteRunspacePoolInternal.AvailableForConnection;
            }
        }

        internal bool CanDisconnect
        {
            get
            {
                return this._runspacePool.RemoteRunspacePoolInternal.CanDisconnect;
            }
        }

        internal System.Management.Automation.Remoting.ClientRemoteSession ClientRemoteSession
        {
            get
            {
                System.Management.Automation.Remoting.ClientRemoteSession remoteSession;
                try
                {
                    remoteSession = this._runspacePool.RemoteRunspacePoolInternal.DataStructureHandler.RemoteSession;
                }
                catch (InvalidRunspacePoolStateException exception)
                {
                    throw exception.ToInvalidRunspaceStateException();
                }
                return remoteSession;
            }
        }

        public override RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return this._connectionInfo;
            }
        }

        public override PSEventManager Events
        {
            get
            {
                return this._eventManager;
            }
        }

        internal override System.Management.Automation.ExecutionContext GetExecutionContext
        {
            get
            {
                throw PSTraceSource.NewNotImplementedException();
            }
        }

        internal override bool HasAvailabilityChangedSubscribers
        {
            get
            {
                return (this.AvailabilityChanged != null);
            }
        }

        internal int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public override System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                throw PSTraceSource.NewNotImplementedException();
            }
        }

        internal override bool InNestedPrompt
        {
            get
            {
                return false;
            }
        }

        public override System.Management.Automation.JobManager JobManager
        {
            get
            {
                throw PSTraceSource.NewNotImplementedException();
            }
        }

        internal string Name
        {
            get
            {
                return this._runspacePool.RemoteRunspacePoolInternal.Name;
            }
            set
            {
                this._runspacePool.RemoteRunspacePoolInternal.Name = value;
            }
        }

        public override RunspaceConnectionInfo OriginalConnectionInfo
        {
            get
            {
                return this._originalConnectionInfo;
            }
        }

        internal ConnectCommandInfo RemoteCommand
        {
            get
            {
                if ((this._runspacePool.RemoteRunspacePoolInternal.ConnectCommands != null) && (this._runspacePool.RemoteRunspacePoolInternal.ConnectCommands.Length > 0))
                {
                    return this._runspacePool.RemoteRunspacePoolInternal.ConnectCommands[0];
                }
                return null;
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
                throw PSTraceSource.NewNotImplementedException();
            }
        }

        internal System.Management.Automation.Runspaces.RunspacePool RunspacePool
        {
            get
            {
                return this._runspacePool;
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

        internal bool ShouldCloseOnPop
        {
            get
            {
                return this._shouldCloseOnPop;
            }
            set
            {
                this._shouldCloseOnPop = value;
            }
        }

        public override PSThreadOptions ThreadOptions
        {
            get
            {
                return this.createThreadOptions;
            }
            set
            {
                lock (this._syncRoot)
                {
                    if (value != this.createThreadOptions)
                    {
                        if (this.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
                        {
                            throw new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.ChangePropertyAfterOpen, new object[0]));
                        }
                        this.createThreadOptions = value;
                    }
                }
            }
        }

        public override System.Version Version
        {
            get
            {
                return this._version;
            }
        }

		public PSObject ToPSObjectForRemoting ()
		{
			var obj = RemotingEncoder.CreateEmptyPSObject();
			obj.AddOrSetProperty (new PSNoteProperty("Version", this.Version));
			obj.AddOrSetProperty (new PSNoteProperty("ConnectionInfoType", this.ConnectionInfo.GetType ().Name));
			obj.AddOrSetProperty (new PSNoteProperty("ConnectionInfo", this.ConnectionInfo.ToPSObjectForRemoting ()));
			obj.AddOrSetProperty (new PSNoteProperty("OriginalConnectionInfoType", this.OriginalConnectionInfo.GetType ().Name));
			obj.AddOrSetProperty (new PSNoteProperty("OriginalConnectionInfo", this.OriginalConnectionInfo.ToPSObjectForRemoting ()));
			obj.AddOrSetProperty (new PSNoteProperty("ApplicationArguments", this._applicationArguments));
			obj.AddOrSetProperty (new PSNoteProperty("RunspaceStateInfo", this.RunspaceStateInfo.State));
			obj.AddOrSetProperty (new PSNoteProperty("ApartmentState", this.ApartmentState));
			obj.AddOrSetProperty (new PSNoteProperty("ByPassRunspaceStateCheck", this.ByPassRunspaceStateCheck));
			obj.AddOrSetProperty (new PSNoteProperty("SessionStateProxyCallInProgress", this._bSessionStateProxyCallInProgress));
			obj.AddOrSetProperty (new PSNoteProperty("CanConnect", this.CanConnect));
			obj.AddOrSetProperty (new PSNoteProperty("CanDisconnect", this.CanDisconnect));
			obj.AddOrSetProperty (new PSNoteProperty("EngineActivityId", this.EngineActivityId));
			obj.AddOrSetProperty (new PSNoteProperty("HasAvailabilityChangedSubscribers", this.HasAvailabilityChangedSubscribers));
			obj.AddOrSetProperty (new PSNoteProperty("Id", this.Id));
			obj.AddOrSetProperty (new PSNoteProperty("InNestedPrompt", this.InNestedPrompt));
			obj.AddOrSetProperty (new PSNoteProperty("InstanceId", this.InstanceId));
			obj.AddOrSetProperty (new PSNoteProperty("Name", this.Name));
			obj.AddOrSetProperty (new PSNoteProperty("RemoteCommand", this.RemoteCommand));
			obj.AddOrSetProperty (new PSNoteProperty("RunspaceAvailability", this.RunspaceAvailability));
			obj.AddOrSetProperty (new PSNoteProperty("ShouldCloseOnPop", this.ShouldCloseOnPop));
			obj.AddOrSetProperty (new PSNoteProperty("ThreadOptions", this.ThreadOptions));
			return obj;
		}

		public static RemoteRunspace FromPSObjectForRemoting (PSObject obj)
		{
			int id = RemotingDecoder.GetPropertyValue<int> (obj, "Id");
			string name = RemotingDecoder.GetPropertyValue<string> (obj, "Name");
			PSPrimitiveDictionary appArgs = RemotingDecoder.GetPropertyValue<PSPrimitiveDictionary> (obj, "ApplicationArguments");
			string connectionInfoType = RemotingDecoder.GetPropertyValue<string> (obj, "ConnectionInfoType");
			PSObject connectionObj = RemotingDecoder.GetPropertyValue<PSObject> (obj, "ConnectionInfo");
			RunspaceConnectionInfo connectionInfo = null;
			if (connectionInfoType == "WSManConnectionInfo") {
				connectionInfo = WSManConnectionInfo.FromPSObjectForRemoting (connectionObj);
			} 
			else 
			{
				var securePassord = new System.Security.SecureString();
				securePassord.AppendChar ('z');
				connectionInfo = new NewProcessConnectionInfo(new PSCredential("Anonymous", securePassord));
			}
			var runspace = new RemoteRunspace(new TypeTable(), connectionInfo, null, appArgs, name, id);
			runspace._version =	RemotingDecoder.GetPropertyValue<Version>(obj, "Version");
			runspace.ApartmentState = RemotingDecoder.GetPropertyValue<ApartmentState>(obj, "ApartmentState");
			runspace.InstanceId = RemotingDecoder.GetPropertyValue<Guid>(obj, "InstanceId");
			runspace._shouldCloseOnPop = RemotingDecoder.GetPropertyValue<bool>(obj, "ShouldCloseOnPop");
			runspace._runspaceStateInfo = new System.Management.Automation.Runspaces.RunspaceStateInfo(RemotingDecoder.GetPropertyValue<RunspaceState>(obj, "RunspaceStateInfo"));
			runspace._runspaceAvailability = RemotingDecoder.GetPropertyValue<RunspaceAvailability>(obj, "RunspaceAvailability");
			runspace.ThreadOptions = RemotingDecoder.GetPropertyValue<PSThreadOptions>(obj, "ThreadOptions");
			runspace.EngineActivityId = RemotingDecoder.GetPropertyValue<Guid>(obj, "EngineActivityId");
			runspace._bSessionStateProxyCallInProgress = RemotingDecoder.GetPropertyValue<bool>(obj, "SessionStateProxyCallInProgress");
			runspace._bypassRunspaceStateCheck = RemotingDecoder.GetPropertyValue<bool>(obj, "ByPassRunspaceStateCheck");
			PSEtwLog.SetActivityIdForCurrentThread(runspace.InstanceId);
			string originalConnectionInfoType = RemotingDecoder.GetPropertyValue<string> (obj, "OriginalConnectionInfoType");
			PSObject originalConnectionObj = RemotingDecoder.GetPropertyValue<PSObject> (obj, "OriginalConnectionInfo");
			RunspaceConnectionInfo originalConnectionInfo = null;
			if (originalConnectionInfoType == "WSManConnectionInfo") {
				originalConnectionInfo = WSManConnectionInfo.FromPSObjectForRemoting (originalConnectionObj);
			}
			else 
			{
				var securePassord = new System.Security.SecureString();
				securePassord.AppendChar ('z');
				originalConnectionInfo = new NewProcessConnectionInfo(new PSCredential("Anonymous", securePassord));
			}
			runspace._originalConnectionInfo = originalConnectionInfo;
			runspace.RunspacePool.RemoteRunspacePoolInternal.SetStateInfo (new RunspacePoolStateInfo(RunspacePoolState.Opened, null));
			return runspace;
		}

        protected class RunspaceEventQueueItem
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

