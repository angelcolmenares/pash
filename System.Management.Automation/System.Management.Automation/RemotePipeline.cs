namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class RemotePipeline : Pipeline
    {
        private bool _addToHistory;
        private CommandCollection _commands;
        private string _computerName;
        private ConnectCommandInfo _connectCmdInfo;
        private bool _disposed;
        private PSDataCollection<ErrorRecord> _errorCollection;
        private PSDataCollectionStream<ErrorRecord> _errorStream;
        private Queue<ExecutionEventQueueItem> _executionEventQueue;
        private string _historyString;
        private PSDataCollection<object> _inputCollection;
        private PSDataCollectionStream<object> _inputStream;
        private bool _isMethodExecutorStreamEnabled;
        private bool _isNested;
        private bool _isSteppable;
        private ObjectStream _methodExecutorStream;
        private Thread _nestedPipelineExecutionThread;
        private PSDataCollection<PSObject> _outputCollection;
        private PSDataCollectionStream<PSObject> _outputStream;
        private bool _performNestedCheck;
        private ManualResetEvent _pipelineFinishedEvent;
        private System.Management.Automation.Runspaces.PipelineStateInfo _pipelineStateInfo;
        private System.Management.Automation.PowerShell _powershell;
        private System.Management.Automation.Runspaces.Runspace _runspace;
        private Guid _runspaceId;
        private object _syncRoot;

        public override event EventHandler<PipelineStateEventArgs> StateChanged;

        private RemotePipeline(RemotePipeline pipeline) : this((RemoteRunspace) pipeline.Runspace, null, false, pipeline.IsNested)
        {
            this._isSteppable = pipeline._isSteppable;
            if (pipeline == null)
            {
                throw PSTraceSource.NewArgumentNullException("pipeline");
            }
            if (pipeline._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            }
            this._addToHistory = pipeline._addToHistory;
            this._historyString = pipeline._historyString;
            foreach (Command command in pipeline.Commands)
            {
                Command item = command.Clone();
                base.Commands.Add(item);
            }
        }

        internal RemotePipeline(RemoteRunspace runspace) : this(runspace, false, false)
        {
            if (runspace.RemoteCommand == null)
            {
                throw new InvalidOperationException(PipelineStrings.InvalidRemoteCommand);
            }
            this._connectCmdInfo = runspace.RemoteCommand;
            this._commands.Add(this._connectCmdInfo.Command);
            this.SetPipelineState(PipelineState.Disconnected, null);
            this._powershell = new System.Management.Automation.PowerShell(this._connectCmdInfo, this._inputStream, this._outputStream, this._errorStream, ((RemoteRunspace) this._runspace).RunspacePool);
            this._powershell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandleInvocationStateChanged);
        }

        private RemotePipeline(RemoteRunspace runspace, bool addToHistory, bool isNested) : base(runspace)
        {
            this._syncRoot = new object();
            this._pipelineStateInfo = new System.Management.Automation.Runspaces.PipelineStateInfo(PipelineState.NotStarted);
            this._commands = new CommandCollection();
            this._executionEventQueue = new Queue<ExecutionEventQueueItem>();
            this._performNestedCheck = true;
            this._addToHistory = addToHistory;
            this._isNested = isNested;
            this._isSteppable = false;
            this._runspace = runspace;
            this._computerName = ((RemoteRunspace) this._runspace).ConnectionInfo.ComputerName;
            this._runspaceId = this._runspace.InstanceId;
            this._inputCollection = new PSDataCollection<object>();
            this._inputCollection.ReleaseOnEnumeration = true;
            this._inputStream = new PSDataCollectionStream<object>(Guid.Empty, this._inputCollection);
            this._outputCollection = new PSDataCollection<PSObject>();
            this._outputStream = new PSDataCollectionStream<PSObject>(Guid.Empty, this._outputCollection);
            this._errorCollection = new PSDataCollection<ErrorRecord>();
            this._errorStream = new PSDataCollectionStream<ErrorRecord>(Guid.Empty, this._errorCollection);
            this._methodExecutorStream = new ObjectStream();
            this._isMethodExecutorStreamEnabled = false;
            base.SetCommandCollection(this._commands);
            this._pipelineFinishedEvent = new ManualResetEvent(false);
        }

        internal RemotePipeline(RemoteRunspace runspace, string command, bool addToHistory, bool isNested) : this(runspace, addToHistory, isNested)
        {
            if (command != null)
            {
                this._commands.Add(new Command(command, true));
            }
            this._powershell = new System.Management.Automation.PowerShell(this._inputStream, this._outputStream, this._errorStream, ((RemoteRunspace) this._runspace).RunspacePool);
            this._powershell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.HandleInvocationStateChanged);
        }

        private bool CanStopPipeline(out bool isAlreadyStopping)
        {
            bool flag = false;
            isAlreadyStopping = false;
            lock (this._syncRoot)
            {
                switch (this._pipelineStateInfo.State)
                {
                    case PipelineState.NotStarted:
                        this.SetPipelineState(PipelineState.Stopping, null);
                        this.SetPipelineState(PipelineState.Stopped, null);
                        flag = false;
                        goto Label_007D;

                    case PipelineState.Running:
                    case PipelineState.Disconnected:
                        this.SetPipelineState(PipelineState.Stopping, null);
                        flag = true;
                        goto Label_007D;

                    case PipelineState.Stopping:
                        isAlreadyStopping = true;
                        return false;

                    case PipelineState.Stopped:
                    case PipelineState.Completed:
                    case PipelineState.Failed:
                        return false;
                }
            }
        Label_007D:
            this.RaisePipelineStateEvents();
            return flag;
        }

        private void Cleanup()
        {
            if (this._outputStream.IsOpen)
            {
                try
                {
                    this._outputCollection.Complete();
                    this._outputStream.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            if (this._errorStream.IsOpen)
            {
                try
                {
                    this._errorCollection.Complete();
                    this._errorStream.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            if (this._inputStream.IsOpen)
            {
                try
                {
                    this._inputCollection.Complete();
                    this._inputStream.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            try
            {
                ((RemoteRunspace) this._runspace).RemoveFromRunningPipelineList(this);
                this._pipelineFinishedEvent.Set();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public override Collection<PSObject> Connect()
        {
            Collection<PSObject> collection;
            this.InitPowerShellForConnect(true);
            try
            {
                collection = this._powershell.Connect();
            }
            catch (InvalidRunspacePoolStateException)
            {
                InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipelineConnect, this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
                throw exception;
            }
            if (((collection.Count == 0) && (this._outputCollection != null)) && (this._outputCollection.Count > 0))
            {
                collection = new Collection<PSObject>(this._outputCollection);
            }
            return collection;
        }

        public override void ConnectAsync()
        {
            this.InitPowerShellForConnect(false);
            try
            {
                this._powershell.ConnectAsync();
            }
            catch (InvalidRunspacePoolStateException)
            {
                InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipelineConnect, this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
                throw exception;
            }
        }

        public override Pipeline Copy()
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            }
            return new RemotePipeline(this);
        }

        private void CoreInvokeAsync()
        {
            try
            {
                this._powershell.BeginInvoke();
            }
            catch (InvalidRunspacePoolStateException)
            {
                InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
                throw exception;
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
                        this.Stop();
                        if (this._powershell != null)
                        {
                            this._powershell.Dispose();
                            this._powershell = null;
                        }
                        this._inputCollection.Dispose();
                        this._inputStream.Dispose();
                        this._outputCollection.Dispose();
                        this._outputStream.Dispose();
                        this._errorCollection.Dispose();
                        this._errorStream.Dispose();
                        this._methodExecutorStream.Dispose();
                        this._pipelineFinishedEvent.Close();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void DoConcurrentCheck(bool syncCall)
        {
            RemotePipeline currentlyRunningPipeline = (RemotePipeline) ((RemoteRunspace) this._runspace).GetCurrentlyRunningPipeline();
            if (!this._isNested)
            {
                if (((((currentlyRunningPipeline != null) || (((RemoteRunspace) this._runspace).RunspaceAvailability == RunspaceAvailability.Busy)) && (((currentlyRunningPipeline != null) || (((RemoteRunspace) this._runspace).RemoteCommand == null)) || ((this._connectCmdInfo == null) || !object.Equals(((RemoteRunspace) this._runspace).RemoteCommand.CommandId, this._connectCmdInfo.CommandId)))) && ((currentlyRunningPipeline == null) || !object.ReferenceEquals(currentlyRunningPipeline, this))) && !this._isSteppable)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "ConcurrentInvokeNotAllowed", new object[0]);
                }
            }
            else if (this._performNestedCheck && !this._isSteppable)
            {
                if (!syncCall)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NestedPipelineInvokeAsync", new object[0]);
                }
                if ((currentlyRunningPipeline == null) && !this._isSteppable)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NestedPipelineNoParentPipeline", new object[0]);
                }
                Thread currentThread = Thread.CurrentThread;
                if (!currentlyRunningPipeline.NestedPipelineExecutionThread.Equals(currentThread))
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NestedPipelineNoParentPipeline", new object[0]);
                }
            }
        }

        internal System.Management.Automation.Runspaces.Runspace GetRunspace()
        {
            return this._runspace;
        }

        private void HandleHostCallReceived(object sender, RemoteDataEventArgs<RemoteHostCall> eventArgs)
        {
            ClientMethodExecutor.Dispatch(this._powershell.RemotePowerShell.DataStructureHandler.TransportManager, ((RemoteRunspace) this._runspace).RunspacePool.RemoteRunspacePoolInternal.Host, this._errorStream, this._methodExecutorStream, this.IsMethodExecutorStreamEnabled, ((RemoteRunspace) this._runspace).RunspacePool.RemoteRunspacePoolInternal, this._powershell.InstanceId, eventArgs.Data);
        }

        private void HandleInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            this.SetPipelineState((PipelineState) e.InvocationStateInfo.State, e.InvocationStateInfo.Reason);
            this.RaisePipelineStateEvents();
        }

        private void InitPowerShell(bool syncCall, bool invokeAndDisconnect = false)
        {
            if ((this._commands == null) || (this._commands.Count == 0))
            {
                throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoCommandInPipeline", new object[0]);
            }
            if (this._pipelineStateInfo.State != PipelineState.NotStarted)
            {
                InvalidPipelineStateException exception = new InvalidPipelineStateException(StringUtil.Format(RunspaceStrings.PipelineReInvokeNotAllowed, new object[0]), this._pipelineStateInfo.State, PipelineState.NotStarted);
                throw exception;
            }
            ((RemoteRunspace) this._runspace).DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
            PSInvocationSettings settings = new PSInvocationSettings {
                AddToHistory = this._addToHistory,
                InvokeAndDisconnect = invokeAndDisconnect
            };
            this._powershell.InitForRemotePipeline(this._commands, this._inputStream, this._outputStream, this._errorStream, settings, base.RedirectShellErrorOutputPipe);
            this._powershell.RemotePowerShell.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
        }

        private void InitPowerShellForConnect(bool syncCall)
        {
            if (this._pipelineStateInfo.State != PipelineState.Disconnected)
            {
                throw new InvalidPipelineStateException(StringUtil.Format(PipelineStrings.PipelineNotDisconnected, new object[0]), this._pipelineStateInfo.State, PipelineState.Disconnected);
            }
            RemotePipeline currentlyRunningPipeline = (RemotePipeline) ((RemoteRunspace) this._runspace).GetCurrentlyRunningPipeline();
            if ((currentlyRunningPipeline == null) || ((currentlyRunningPipeline != null) && !object.ReferenceEquals(currentlyRunningPipeline, this)))
            {
                ((RemoteRunspace) this._runspace).DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
            }
            if ((this._powershell.RemotePowerShell == null) || !this._powershell.RemotePowerShell.Initialized)
            {
                PSInvocationSettings settings = new PSInvocationSettings {
                    AddToHistory = this._addToHistory
                };
                this._powershell.InitForRemotePipelineConnect(this._inputStream, this._outputStream, this._errorStream, settings, base.RedirectShellErrorOutputPipe);
                this._powershell.RemotePowerShell.HostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleHostCallReceived);
            }
        }

        public override Collection<PSObject> Invoke(IEnumerable input)
        {
            Collection<PSObject> collection;
            if (input == null)
            {
                this.InputStream.Close();
            }
            this.InitPowerShell(true, false);
            try
            {
                collection = this._powershell.Invoke(input);
            }
            catch (InvalidRunspacePoolStateException)
            {
                InvalidRunspaceStateException exception = new InvalidRunspaceStateException(StringUtil.Format(RunspaceStrings.RunspaceNotOpenForPipeline, this._runspace.RunspaceStateInfo.State.ToString()), this._runspace.RunspaceStateInfo.State, RunspaceState.Opened);
                throw exception;
            }
            return collection;
        }

        public override void InvokeAsync()
        {
            this.InitPowerShell(false, false);
            this.CoreInvokeAsync();
        }

        internal override void InvokeAsyncAndDisconnect()
        {
            this.InitPowerShell(false, true);
            this.CoreInvokeAsync();
        }

        protected void RaisePipelineStateEvents()
        {
            Queue<ExecutionEventQueueItem> queue = null;
            EventHandler<PipelineStateEventArgs> stateChanged = null;
            bool hasAvailabilityChangedSubscribers = false;
            lock (this._syncRoot)
            {
                stateChanged = this.StateChanged;
                hasAvailabilityChangedSubscribers = this._runspace.HasAvailabilityChangedSubscribers;
                if ((stateChanged != null) || hasAvailabilityChangedSubscribers)
                {
                    queue = this._executionEventQueue;
                    this._executionEventQueue = new Queue<ExecutionEventQueueItem>();
                }
                else
                {
                    this._executionEventQueue.Clear();
                }
            }
            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    ExecutionEventQueueItem item = queue.Dequeue();
                    if (hasAvailabilityChangedSubscribers && (item.NewRunspaceAvailability != item.CurrentRunspaceAvailability))
                    {
                        this._runspace.RaiseAvailabilityChangedEvent(item.NewRunspaceAvailability);
                    }
                    if (stateChanged != null)
                    {
                        try
                        {
                            stateChanged(this, new PipelineStateEventArgs(item.PipelineStateInfo));
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

        internal override void SetHistoryString(string historyString)
        {
            this._powershell.HistoryString = historyString;
        }

        internal void SetIsNested(bool isNested)
        {
            this._isNested = isNested;
            this._powershell.SetIsNested(isNested);
        }

        internal void SetIsSteppable(bool isSteppable)
        {
            this._isSteppable = isSteppable;
        }

        private void SetPipelineState(PipelineState state, Exception reason)
        {
            PipelineState stopped = state;
            System.Management.Automation.Runspaces.PipelineStateInfo info = null;
            lock (this._syncRoot)
            {
                switch (this._pipelineStateInfo.State)
                {
                    case PipelineState.Running:
                        if (state != PipelineState.Running)
                        {
                            goto Label_005F;
                        }
                        return;

                    case PipelineState.Stopping:
                        if ((state != PipelineState.Running) && (state != PipelineState.Stopping))
                        {
                            break;
                        }
                        return;

                    case PipelineState.Stopped:
                    case PipelineState.Completed:
                    case PipelineState.Failed:
                        return;

                    default:
                        goto Label_005F;
                }
                stopped = PipelineState.Stopped;
            Label_005F:
                this._pipelineStateInfo = new System.Management.Automation.Runspaces.PipelineStateInfo(stopped, reason);
                info = this._pipelineStateInfo;
                RunspaceAvailability runspaceAvailability = this._runspace.RunspaceAvailability;
                this._runspace.UpdateRunspaceAvailability(this._pipelineStateInfo.State, false);
                this._executionEventQueue.Enqueue(new ExecutionEventQueueItem(this._pipelineStateInfo.Clone(), runspaceAvailability, this._runspace.RunspaceAvailability));
            }
            if (((info.State == PipelineState.Completed) || (info.State == PipelineState.Failed)) || (info.State == PipelineState.Stopped))
            {
                this.Cleanup();
            }
        }

        public override void Stop()
        {
            bool isAlreadyStopping = false;
            if (this.CanStopPipeline(out isAlreadyStopping) && (this._powershell != null))
            {
                IAsyncResult result = null;
                try
                {
                    result = this._powershell.BeginStop(null, null);
                }
                catch (ObjectDisposedException)
                {
                    throw PSTraceSource.NewObjectDisposedException("Pipeline");
                }
                result.AsyncWaitHandle.WaitOne();
            }
            this.PipelineFinishedEvent.WaitOne();
        }

        public override void StopAsync()
        {
            bool flag;
            if (this.CanStopPipeline(out flag))
            {
                try
                {
                    this._powershell.BeginStop(null, null);
                }
                catch (ObjectDisposedException)
                {
                    throw PSTraceSource.NewObjectDisposedException("Pipeline");
                }
            }
        }

        public bool AddToHistory
        {
            get
            {
                return this._addToHistory;
            }
        }

        public override PipelineReader<object> Error
        {
            get
            {
                return this._errorStream.GetObjectReaderForPipeline(this._computerName, this._runspaceId);
            }
        }

        internal string HistoryString
        {
            get
            {
                return this._historyString;
            }
            set
            {
                this._historyString = value;
            }
        }

        public override PipelineWriter Input
        {
            get
            {
                return this._inputStream.ObjectWriter;
            }
        }

        protected PSDataCollectionStream<object> InputStream
        {
            get
            {
                return this._inputStream;
            }
        }

        internal bool IsMethodExecutorStreamEnabled
        {
            get
            {
                return this._isMethodExecutorStreamEnabled;
            }
            set
            {
                this._isMethodExecutorStreamEnabled = value;
            }
        }

        public override bool IsNested
        {
            get
            {
                return this._isNested;
            }
        }

        internal ObjectStream MethodExecutorStream
        {
            get
            {
                return this._methodExecutorStream;
            }
        }

        internal Thread NestedPipelineExecutionThread
        {
            get
            {
                return this._nestedPipelineExecutionThread;
            }
            set
            {
                this._nestedPipelineExecutionThread = value;
            }
        }

        public override PipelineReader<PSObject> Output
        {
            get
            {
                return this._outputStream.GetPSObjectReaderForPipeline(this._computerName, this._runspaceId);
            }
        }

        internal ManualResetEvent PipelineFinishedEvent
        {
            get
            {
                return this._pipelineFinishedEvent;
            }
        }

        public override System.Management.Automation.Runspaces.PipelineStateInfo PipelineStateInfo
        {
            get
            {
                lock (this._syncRoot)
                {
                    return this._pipelineStateInfo.Clone();
                }
            }
        }

        internal System.Management.Automation.PowerShell PowerShell
        {
            get
            {
                return this._powershell;
            }
        }

        public override System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("pipeline");
                }
                return this._runspace;
            }
        }

        private class ExecutionEventQueueItem
        {
            public RunspaceAvailability CurrentRunspaceAvailability;
            public RunspaceAvailability NewRunspaceAvailability;
            public System.Management.Automation.Runspaces.PipelineStateInfo PipelineStateInfo;

            public ExecutionEventQueueItem(System.Management.Automation.Runspaces.PipelineStateInfo pipelineStateInfo, RunspaceAvailability currentAvailability, RunspaceAvailability newAvailability)
            {
                this.PipelineStateInfo = pipelineStateInfo;
                this.CurrentRunspaceAvailability = currentAvailability;
                this.NewRunspaceAvailability = newAvailability;
            }
        }
    }
}

