namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class PipelineBase : Pipeline
    {
        private bool _addToHistory;
        private bool _disposed;
        private static readonly string[] _emptyStringArray = new string[0];
        private ObjectStreamBase _errorStream;
        private Queue<ExecutionEventQueueItem> _executionEventQueue;
        private string _historyString;
        private PSInformationalBuffers _informationalBuffers;
        private ObjectStreamBase _inputStream;
        private bool _isNested;
        private bool _isPulsePipeline;
        private Thread _nestedPipelineExecutionThread;
        private ObjectStreamBase _outputStream;
        private bool _performNestedCheck;
        private ManualResetEvent _pipelineFinishedEvent;
        private System.Management.Automation.Runspaces.PipelineStateInfo _pipelineStateInfo;
        private System.Management.Automation.Runspaces.Runspace _runspace;
        private bool _syncInvokeCall;
        private object _syncRoot;

        public override event EventHandler<PipelineStateEventArgs> StateChanged;

        protected PipelineBase(PipelineBase pipeline) : this(pipeline.Runspace, null, false, pipeline.IsNested)
        {
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

        protected PipelineBase(System.Management.Automation.Runspaces.Runspace runspace, string command, bool addToHistory, bool isNested) : base(runspace)
        {
            this._pipelineStateInfo = new System.Management.Automation.Runspaces.PipelineStateInfo(System.Management.Automation.Runspaces.PipelineState.NotStarted);
            this._performNestedCheck = true;
            this._executionEventQueue = new Queue<ExecutionEventQueueItem>();
            this._syncRoot = new object();
            this.Initialize(runspace, command, addToHistory, isNested);
            this._inputStream = new ObjectStream();
            this._outputStream = new ObjectStream();
            this._errorStream = new ObjectStream();
        }

        protected PipelineBase(System.Management.Automation.Runspaces.Runspace runspace, CommandCollection command, bool addToHistory, bool isNested, ObjectStreamBase inputStream, ObjectStreamBase outputStream, ObjectStreamBase errorStream, PSInformationalBuffers infoBuffers) : base(runspace, command)
        {
            this._pipelineStateInfo = new System.Management.Automation.Runspaces.PipelineStateInfo(System.Management.Automation.Runspaces.PipelineState.NotStarted);
            this._performNestedCheck = true;
            this._executionEventQueue = new Queue<ExecutionEventQueueItem>();
            this._syncRoot = new object();
            this.Initialize(runspace, null, false, isNested);
            if (addToHistory)
            {
                string commandStringForHistory = command.GetCommandStringForHistory();
                this._historyString = commandStringForHistory;
                this._addToHistory = addToHistory;
            }
            this._inputStream = inputStream;
            this._outputStream = outputStream;
            this._errorStream = errorStream;
            this._informationalBuffers = infoBuffers;
        }

        public override Collection<PSObject> Connect()
        {
            throw PSTraceSource.NewNotSupportedException("PipelineStrings", "ConnectNotSupported", new object[0]);
        }

        public override void ConnectAsync()
        {
            throw PSTraceSource.NewNotSupportedException("PipelineStrings", "ConnectNotSupported", new object[0]);
        }

        private void CoreInvoke(IEnumerable input, bool syncCall)
        {
            lock (this.SyncRoot)
            {
                if (this._disposed)
                {
                    throw PSTraceSource.NewObjectDisposedException("pipeline");
                }
                if ((base.Commands == null) || (base.Commands.Count == 0))
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NoCommandInPipeline", new object[0]);
                }
                if (this.PipelineState != System.Management.Automation.Runspaces.PipelineState.NotStarted)
                {
                    InvalidPipelineStateException exception = new InvalidPipelineStateException(StringUtil.Format(RunspaceStrings.PipelineReInvokeNotAllowed, new object[0]), this.PipelineState, System.Management.Automation.Runspaces.PipelineState.NotStarted);
                    throw exception;
                }
                if ((syncCall && !(this._inputStream is PSDataCollectionStream<PSObject>)) && !(this._inputStream is PSDataCollectionStream<object>))
                {
                    if (input != null)
                    {
                        foreach (object obj2 in input)
                        {
                            this._inputStream.Write(obj2);
                        }
                    }
                    this._inputStream.Close();
                }
                this._syncInvokeCall = syncCall;
                this._pipelineFinishedEvent = new ManualResetEvent(false);
                this.RunspaceBase.DoConcurrentCheckAndAddToRunningPipelines(this, syncCall);
                this.SetPipelineState(System.Management.Automation.Runspaces.PipelineState.Running);
            }
            try
            {
                this.StartPipelineExecution();
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                this.RunspaceBase.RemoveFromRunningPipelineList(this);
                this.SetPipelineState(System.Management.Automation.Runspaces.PipelineState.Failed, exception2);
                throw;
            }
        }

        private void CoreStop(bool syncCall)
        {
            bool flag = false;
            lock (this.SyncRoot)
            {
                switch (this.PipelineState)
                {
                    case System.Management.Automation.Runspaces.PipelineState.NotStarted:
                        this.SetPipelineState(System.Management.Automation.Runspaces.PipelineState.Stopping);
                        this.SetPipelineState(System.Management.Automation.Runspaces.PipelineState.Stopped);
                        break;

                    case System.Management.Automation.Runspaces.PipelineState.Running:
                        this.SetPipelineState(System.Management.Automation.Runspaces.PipelineState.Stopping);
                        break;

                    case System.Management.Automation.Runspaces.PipelineState.Stopping:
                        flag = true;
                        break;

                    case System.Management.Automation.Runspaces.PipelineState.Stopped:
                    case System.Management.Automation.Runspaces.PipelineState.Completed:
                    case System.Management.Automation.Runspaces.PipelineState.Failed:
                        return;
                }
            }
            if (!flag)
            {
                this.RaisePipelineStateEvents();
                lock (this.SyncRoot)
                {
                    if (this.PipelineState == System.Management.Automation.Runspaces.PipelineState.Stopped)
                    {
                        return;
                    }
                }
                this.ImplementStop(syncCall);
            }
            else if (syncCall)
            {
                this.PipelineFinishedEvent.WaitOne();
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this._disposed)
                {
                    this._disposed = true;
                    if (disposing)
                    {
                        this._inputStream.Close();
                        this._outputStream.Close();
                        this._errorStream.Close();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void DoConcurrentCheck(bool syncCall, object syncObject, bool isInLock)
        {
            PipelineBase currentlyRunningPipeline = (PipelineBase) this.RunspaceBase.GetCurrentlyRunningPipeline();
            if (!this.IsNested)
            {
                if (currentlyRunningPipeline != null)
                {
                    if ((currentlyRunningPipeline != this.RunspaceBase.PulsePipeline) && (!currentlyRunningPipeline.IsNested || (this.RunspaceBase.PulsePipeline == null)))
                    {
                        throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "ConcurrentInvokeNotAllowed", new object[0]);
                    }
                    if (isInLock)
                    {
                        Monitor.Exit(syncObject);
                    }
                    try
                    {
                        this.RunspaceBase.WaitForFinishofPipelines();
                    }
                    finally
                    {
                        if (isInLock)
                        {
                            Monitor.Enter(syncObject);
                        }
                    }
                    this.DoConcurrentCheck(syncCall, syncObject, isInLock);
                }
            }
            else if (this._performNestedCheck)
            {
                if (!syncCall)
                {
                    throw PSTraceSource.NewInvalidOperationException("RunspaceStrings", "NestedPipelineInvokeAsync", new object[0]);
                }
                if (currentlyRunningPipeline == null)
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

        protected abstract void ImplementStop(bool syncCall);
        private void Initialize(System.Management.Automation.Runspaces.Runspace runspace, string command, bool addToHistory, bool isNested)
        {
            this._runspace = runspace;
            this._isNested = isNested;
            if (addToHistory && (command == null))
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            if (command != null)
            {
                base.Commands.Add(new Command(command, true, false));
            }
            this._addToHistory = addToHistory;
            if (this._addToHistory)
            {
                this._historyString = command;
            }
        }

        public override Collection<PSObject> Invoke(IEnumerable input)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("pipeline");
            }
            this.CoreInvoke(input, true);
            this.PipelineFinishedEvent.WaitOne();
            if (this.SyncInvokeCall)
            {
                this.RaisePipelineStateEvents();
            }
            if (this.PipelineStateInfo.State == System.Management.Automation.Runspaces.PipelineState.Stopped)
            {
                return new Collection<PSObject>();
            }
            if ((this.PipelineStateInfo.State == System.Management.Automation.Runspaces.PipelineState.Failed) && (this.PipelineStateInfo.Reason != null))
            {
                RuntimeException.LockStackTrace(this.PipelineStateInfo.Reason);
                throw this.PipelineStateInfo.Reason;
            }
            return this.Output.NonBlockingRead(0x7fffffff);
        }

        public override void InvokeAsync()
        {
            this.CoreInvoke(null, false);
        }

        internal override void InvokeAsyncAndDisconnect()
        {
            throw new NotSupportedException();
        }

        protected bool IsPipelineFinished()
        {
            if ((this.PipelineState != System.Management.Automation.Runspaces.PipelineState.Completed) && (this.PipelineState != System.Management.Automation.Runspaces.PipelineState.Failed))
            {
                return (this.PipelineState == System.Management.Automation.Runspaces.PipelineState.Stopped);
            }
            return true;
        }

        protected void RaisePipelineStateEvents()
        {
            Queue<ExecutionEventQueueItem> queue = null;
            EventHandler<PipelineStateEventArgs> stateChanged = null;
            bool hasAvailabilityChangedSubscribers = false;
            lock (this.SyncRoot)
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

        protected void SetPipelineState(System.Management.Automation.Runspaces.PipelineState state)
        {
            this.SetPipelineState(state, null);
        }

        protected void SetPipelineState(System.Management.Automation.Runspaces.PipelineState state, Exception reason)
        {
            lock (this.SyncRoot)
            {
                if (state != this.PipelineState)
                {
                    this._pipelineStateInfo = new System.Management.Automation.Runspaces.PipelineStateInfo(state, reason);
                    RunspaceAvailability runspaceAvailability = this._runspace.RunspaceAvailability;
                    this._runspace.UpdateRunspaceAvailability(this._pipelineStateInfo.State, false);
                    this._executionEventQueue.Enqueue(new ExecutionEventQueueItem(this._pipelineStateInfo.Clone(), runspaceAvailability, this._runspace.RunspaceAvailability));
                }
            }
        }

        protected abstract void StartPipelineExecution();
        public override void Stop()
        {
            this.CoreStop(true);
        }

        public override void StopAsync()
        {
            this.CoreStop(false);
        }

        internal bool AddToHistory
        {
            get
            {
                return this._addToHistory;
            }
            set
            {
                this._addToHistory = value;
            }
        }

        public override PipelineReader<object> Error
        {
            get
            {
                return this._errorStream.ObjectReader;
            }
        }

        protected ObjectStreamBase ErrorStream
        {
            get
            {
                return this._errorStream;
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

        protected PSInformationalBuffers InformationalBuffers
        {
            get
            {
                return this._informationalBuffers;
            }
        }

        public override PipelineWriter Input
        {
            get
            {
                return this._inputStream.ObjectWriter;
            }
        }

        protected ObjectStreamBase InputStream
        {
            get
            {
                return this._inputStream;
            }
        }

        internal override bool IsChild { get; set; }

        public override bool IsNested
        {
            get
            {
                return this._isNested;
            }
        }

        internal bool IsPulsePipeline
        {
            get
            {
                return this._isPulsePipeline;
            }
            set
            {
                this._isPulsePipeline = value;
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
                return this._outputStream.PSObjectReader;
            }
        }

        protected ObjectStreamBase OutputStream
        {
            get
            {
                return this._outputStream;
            }
        }

        internal bool PerformNestedCheck
        {
            set
            {
                this._performNestedCheck = value;
            }
        }

        internal ManualResetEvent PipelineFinishedEvent
        {
            get
            {
                return this._pipelineFinishedEvent;
            }
        }

        protected System.Management.Automation.Runspaces.PipelineState PipelineState
        {
            get
            {
                return this._pipelineStateInfo.State;
            }
        }

        public override System.Management.Automation.Runspaces.PipelineStateInfo PipelineStateInfo
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return this._pipelineStateInfo.Clone();
                }
            }
        }

        public override System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this._runspace;
            }
        }

        private System.Management.Automation.Runspaces.RunspaceBase RunspaceBase
        {
            get
            {
                return (System.Management.Automation.Runspaces.RunspaceBase) this.Runspace;
            }
        }

        protected bool SyncInvokeCall
        {
            get
            {
                return this._syncInvokeCall;
            }
        }

        protected internal object SyncRoot
        {
            get
            {
                return this._syncRoot;
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

