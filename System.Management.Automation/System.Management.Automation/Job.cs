namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Language;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class Job : IDisposable
    {
        private DateTime? _beginTime;
        private DateTime? _endTime;
        private static int _jobIdSeed;
        private bool _jobStreamsLoaded;
        private string _jobTypeName;
        private IList<Job> childJobs;
        private readonly string command;
        private PSDataCollection<DebugRecord> debug;
        private bool debugOwner;
        private PSDataCollection<ErrorRecord> error;
        private bool errorOwner;
        private ManualResetEvent finished;
        private Guid guid;
        private bool isDisposed;
        private string name;
        private PSDataCollection<PSObject> output;
        private bool outputOwner;
        private Lazy<int> parentActivityId;
        private PSDataCollection<ProgressRecord> progress;
        private bool progressOwner;
        private bool propagateThrows;
        private PSDataCollection<PSStreamObject> results;
        private bool resultsOwner;
        private readonly int sessionId;
        private System.Management.Automation.JobStateInfo stateInfo;
        private bool suppressOutputForwarding;
        internal readonly object syncObject;
        private PSDataCollection<VerboseRecord> verbose;
        private bool verboseOwner;
        private PSDataCollection<WarningRecord> warning;
        private bool warningOwner;

        public event EventHandler<JobStateEventArgs> StateChanged;

        protected Job()
        {
            this.stateInfo = new System.Management.Automation.JobStateInfo(JobState.NotStarted);
            this.finished = new ManualResetEvent(false);
            this.guid = Guid.NewGuid();
            this.syncObject = new object();
            this.results = new PSDataCollection<PSStreamObject>();
            this.resultsOwner = true;
            this.error = new PSDataCollection<ErrorRecord>();
            this.errorOwner = true;
            this.progress = new PSDataCollection<ProgressRecord>();
            this.progressOwner = true;
            this.verbose = new PSDataCollection<VerboseRecord>();
            this.verboseOwner = true;
            this.warning = new PSDataCollection<WarningRecord>();
            this.warningOwner = true;
            this.debug = new PSDataCollection<DebugRecord>();
            this.debugOwner = true;
            this.output = new PSDataCollection<PSObject>();
            this.outputOwner = true;
            this._beginTime = null;
            this._endTime = null;
            this._jobTypeName = string.Empty;
            this.sessionId = Interlocked.Increment(ref _jobIdSeed);
        }

        protected Job(string command) : this()
        {
            this.command = command;
            this.name = this.AutoGenerateJobName();
        }

        protected Job(string command, string name) : this(command)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.name = name;
            }
        }

        protected Job(string command, string name, IList<Job> childJobs) : this(command, name)
        {
            this.childJobs = childJobs;
        }

        protected Job(string command, string name, Guid instanceId) : this(command, name)
        {
            this.guid = instanceId;
        }

        protected Job(string command, string name, JobIdentifier token)
        {
            this.stateInfo = new System.Management.Automation.JobStateInfo(JobState.NotStarted);
            this.finished = new ManualResetEvent(false);
            this.guid = Guid.NewGuid();
            this.syncObject = new object();
            this.results = new PSDataCollection<PSStreamObject>();
            this.resultsOwner = true;
            this.error = new PSDataCollection<ErrorRecord>();
            this.errorOwner = true;
            this.progress = new PSDataCollection<ProgressRecord>();
            this.progressOwner = true;
            this.verbose = new PSDataCollection<VerboseRecord>();
            this.verboseOwner = true;
            this.warning = new PSDataCollection<WarningRecord>();
            this.warningOwner = true;
            this.debug = new PSDataCollection<DebugRecord>();
            this.debugOwner = true;
            this.output = new PSDataCollection<PSObject>();
            this.outputOwner = true;
            this._beginTime = null;
            this._endTime = null;
            this._jobTypeName = string.Empty;
            if (token == null)
            {
                throw PSTraceSource.NewArgumentNullException("token", "remotingerroridstrings", "JobIdentifierNull", new object[0]);
            }
            if (token.Id > _jobIdSeed)
            {
                throw PSTraceSource.NewArgumentException("token", "remotingerroridstrings", "JobIdNotYetAssigned", new object[] { token.Id });
            }
            this.command = command;
            this.sessionId = token.Id;
            this.guid = token.InstanceId;
            if (!string.IsNullOrEmpty(name))
            {
                this.name = name;
            }
            else
            {
                this.name = this.AutoGenerateJobName();
            }
        }

        private void AssertChangesAreAccepted()
        {
            this.AssertNotDisposed();
            lock (this.syncObject)
            {
                if (this.JobStateInfo.State == JobState.Running)
                {
                    throw new InvalidJobStateException(JobState.Running);
                }
            }
        }

        internal void AssertNotDisposed()
        {
            if (this.isDisposed)
            {
                throw PSTraceSource.NewObjectDisposedException("PSJob");
            }
        }

        protected string AutoGenerateJobName()
        {
            return ("Job" + this.sessionId.ToString(NumberFormatInfo.InvariantInfo));
        }

        internal void CloseAllStreams()
        {
            if (this.resultsOwner)
            {
                this.results.Complete();
            }
            if (this.outputOwner)
            {
                this.output.Complete();
            }
            if (this.errorOwner)
            {
                this.error.Complete();
            }
            if (this.progressOwner)
            {
                this.progress.Complete();
            }
            if (this.verboseOwner)
            {
                this.verbose.Complete();
            }
            if (this.warningOwner)
            {
                this.warning.Complete();
            }
            if (this.debugOwner)
            {
                this.debug.Complete();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                this.CloseAllStreams();
                lock (this.syncObject)
                {
                    if (this.finished != null)
                    {
                        this.finished.Close();
                        this.finished = null;
                    }
                }
                if (this.resultsOwner)
                {
                    this.results.Dispose();
                }
                if (this.outputOwner)
                {
                    this.output.Dispose();
                }
                if (this.errorOwner)
                {
                    this.error.Dispose();
                }
                if (this.debugOwner)
                {
                    this.debug.Dispose();
                }
                if (this.verboseOwner)
                {
                    this.verbose.Dispose();
                }
                if (this.warningOwner)
                {
                    this.warning.Dispose();
                }
                if (this.progressOwner)
                {
                    this.progress.Dispose();
                }
                this.isDisposed = true;
            }
        }

        protected virtual void DoLoadJobStreams()
        {
        }

        protected virtual void DoUnloadJobStreams()
        {
        }

        internal virtual void ForwardAllResultsToCmdlet(Cmdlet cmdlet)
        {
            foreach (PSStreamObject obj2 in this.Results)
            {
                obj2.WriteStreamObject(cmdlet, false);
            }
        }

        internal virtual void ForwardAvailableResultsToCmdlet(Cmdlet cmdlet)
        {
            foreach (PSStreamObject obj2 in this.Results.ReadAll())
            {
                obj2.WriteStreamObject(cmdlet, false);
            }
        }

        internal static string GetCommandTextFromInvocationInfo(InvocationInfo invocationInfo)
        {
            if (invocationInfo == null)
            {
                return null;
            }
            IScriptExtent scriptPosition = invocationInfo.ScriptPosition;
            if (((scriptPosition != null) && (scriptPosition.StartScriptPosition != null)) && !string.IsNullOrWhiteSpace(scriptPosition.StartScriptPosition.Line))
            {
                return scriptPosition.StartScriptPosition.Line.Substring(scriptPosition.StartScriptPosition.ColumnNumber - 1).Trim();
            }
            return invocationInfo.InvocationName;
        }

        private static Exception GetExceptionFromErrorRecord(ErrorRecord errorRecord)
        {
            RuntimeException exception = errorRecord.Exception as RuntimeException;
            if (exception == null)
            {
                return null;
            }
            RemoteException exception2 = exception as RemoteException;
            if (exception2 == null)
            {
                return null;
            }
            PSPropertyInfo info = exception2.SerializedRemoteException.Properties["WasThrownFromThrowStatement"];
            if ((info == null) || !((bool) info.Value))
            {
                return null;
            }
            exception.WasThrownFromThrowStatement = true;
            return exception;
        }

        internal List<Job> GetJobsForLocation(string location)
        {
            List<Job> list = new List<Job>();
            foreach (Job job in this.ChildJobs)
            {
                if (string.Equals(job.Location, location, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(job);
                }
            }
            return list;
        }

        internal virtual IEnumerable<RemoteRunspace> GetRunspaces()
        {
            return null;
        }

        private void InvokeCmdletMethodAndIgnoreResults(Action<Cmdlet> invokeCmdletMethod)
        {
            object obj2 = new object();
            CmdletMethodInvoker<object> invoker = new CmdletMethodInvoker<object> {
                Action = delegate (Cmdlet cmdlet) {
                    invokeCmdletMethod(cmdlet);
                    return null;
                },
                Finished = null,
                SyncObject = obj2
            };
            this.Results.Add(new PSStreamObject(PSStreamObjectType.BlockingError, invoker));
        }

        private T InvokeCmdletMethodAndWaitForResults<T>(Func<Cmdlet, T> invokeCmdletMethodAndReturnResult, out Exception exceptionThrownOnCmdletThread)
        {
            T methodResult = default(T);
            Exception closureSafeExceptionThrownOnCmdletThread = null;
            object resultsLock = new object();
            EventHandler<JobStateEventArgs> handler2 = null;
            using (ManualResetEventSlim gotResultEvent = new ManualResetEventSlim(false))
            {
                if (handler2 == null)
                {
                    handler2 = delegate (object sender, JobStateEventArgs eventArgs) {
                        if (this.IsFinishedState(eventArgs.JobStateInfo.State) || (eventArgs.JobStateInfo.State == JobState.Stopping))
                        {
                            lock (resultsLock)
                            {
                                closureSafeExceptionThrownOnCmdletThread = new OperationCanceledException();
                            }
                            gotResultEvent.Set();
                        }
                    };
                }
                EventHandler<JobStateEventArgs> handler = handler2;
                this.StateChanged += handler;
                Thread.MemoryBarrier();
                try
                {
                    handler(null, new JobStateEventArgs(this.JobStateInfo));
                    if (!gotResultEvent.IsSet)
                    {
                        this.SetJobState(JobState.Blocked);
                        CmdletMethodInvoker<T> invoker = new CmdletMethodInvoker<T> {
                            Action = invokeCmdletMethodAndReturnResult,
                            Finished = gotResultEvent,
                            SyncObject = resultsLock
                        };
                        PSStreamObjectType shouldMethod = PSStreamObjectType.ShouldMethod;
                        if (typeof(T) == typeof(object))
                        {
                            shouldMethod = PSStreamObjectType.BlockingError;
                        }
                        this.Results.Add(new PSStreamObject(shouldMethod, invoker));
                        gotResultEvent.Wait();
                        this.SetJobState(JobState.Running);
                        lock (resultsLock)
                        {
                            if (closureSafeExceptionThrownOnCmdletThread == null)
                            {
                                closureSafeExceptionThrownOnCmdletThread = invoker.ExceptionThrownOnCmdletThread;
                                methodResult = invoker.MethodResult;
                            }
                        }
                    }
                }
                finally
                {
                    this.StateChanged -= handler;
                }
            }
            lock (resultsLock)
            {
                exceptionThrownOnCmdletThread = closureSafeExceptionThrownOnCmdletThread;
                return methodResult;
            }
        }

        internal bool IsFinishedState(JobState state)
        {
            lock (this.syncObject)
            {
                return (((state == JobState.Completed) || (state == JobState.Failed)) || (state == JobState.Stopped));
            }
        }

        internal bool IsPersistentState(JobState state)
        {
            lock (this.syncObject)
            {
                return ((this.IsFinishedState(state) || (state == JobState.Disconnected)) || (state == JobState.Suspended));
            }
        }

        public void LoadJobStreams()
        {
            if (!this._jobStreamsLoaded)
            {
                lock (this.syncObject)
                {
                    if (this._jobStreamsLoaded)
                    {
                        return;
                    }
                    this._jobStreamsLoaded = true;
                }
                try
                {
                    this.DoLoadJobStreams();
                }
                catch (Exception exception)
                {
                    using (PowerShellTraceSource source = PowerShellTraceSourceFactory.GetTraceSource())
                    {
                        source.TraceException(exception);
                    }
                }
            }
        }

        internal virtual void NonblockingShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            this.InvokeCmdletMethodAndIgnoreResults(delegate (Cmdlet cmdlet) {
                ShouldProcessReason reason;
                cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption, out reason);
            });
        }

        internal Collection<PSStreamObject> ReadAll()
        {
            this.Output.Clear();
            this.Error.Clear();
            this.Debug.Clear();
            this.Warning.Clear();
            this.Verbose.Clear();
            this.Progress.Clear();
            return this.Results.ReadAll();
        }

        protected void SetJobState(JobState state)
        {
            this.AssertNotDisposed();
            this.SetJobState(state, null);
        }

        internal void SetJobState(JobState state, Exception reason)
        {
            using (PowerShellTraceSource source = PowerShellTraceSourceFactory.GetTraceSource())
            {
                this.AssertNotDisposed();
                bool flag = false;
                System.Management.Automation.JobStateInfo stateInfo = this.stateInfo;
                lock (this.syncObject)
                {
                    this.stateInfo = new System.Management.Automation.JobStateInfo(state, reason);
                    if (state == JobState.Running)
                    {
                        if (!this.PSBeginTime.HasValue)
                        {
                            this.PSBeginTime = new DateTime?(DateTime.Now);
                        }
                    }
                    else if (this.IsFinishedState(state))
                    {
                        flag = true;
                        if (!this.PSEndTime.HasValue)
                        {
                            this.PSEndTime = new DateTime?(DateTime.Now);
                        }
                    }
                }
                if (flag)
                {
                    this.CloseAllStreams();
                }
                try
                {
                    source.WriteMessage("Job", "SetJobState", Guid.Empty, this, "Invoking StateChanged event", null);
                    this.StateChanged.SafeInvoke<JobStateEventArgs>(this, new JobStateEventArgs(this.stateInfo.Clone(), stateInfo));
                }
                catch (Exception exception)
                {
                    source.WriteMessage("Job", "SetJobState", Guid.Empty, this, "Some Job StateChange event handler threw an unhandled exception.", null);
                    source.TraceException(exception);
                    CommandProcessorBase.CheckForSevereException(exception);
                }
                if (flag)
                {
                    lock (this.syncObject)
                    {
                        if (this.finished != null)
                        {
                            this.finished.Set();
                        }
                    }
                }
            }
        }

        internal void SetParentActivityIdGetter(Func<int> parentActivityIdGetter)
        {
            this.parentActivityId = new Lazy<int>(parentActivityIdGetter);
        }

        internal bool ShouldContinue(string query, string caption)
        {
            Exception exception;
            return this.ShouldContinue(query, caption, out exception);
        }

        internal bool ShouldContinue(string query, string caption, out Exception exceptionThrownOnCmdletThread)
        {
            return this.InvokeCmdletMethodAndWaitForResults<bool>(cmdlet => cmdlet.ShouldContinue(query, caption), out exceptionThrownOnCmdletThread);
        }

        internal virtual bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason, out Exception exceptionThrownOnCmdletThread)
        {
            ShouldProcessReason closureSafeShouldProcessReason = ShouldProcessReason.None;
            bool flag = this.InvokeCmdletMethodAndWaitForResults<bool>(cmdlet => cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption, out closureSafeShouldProcessReason), out exceptionThrownOnCmdletThread);
            shouldProcessReason = closureSafeShouldProcessReason;
            return flag;
        }

        public abstract void StopJob();
        public void UnloadJobStreams()
        {
            if (this._jobStreamsLoaded)
            {
                lock (this.syncObject)
                {
                    if (!this._jobStreamsLoaded)
                    {
                        return;
                    }
                    this._jobStreamsLoaded = false;
                }
                try
                {
                    this.DoUnloadJobStreams();
                }
                catch (Exception exception)
                {
                    using (PowerShellTraceSource source = PowerShellTraceSourceFactory.GetTraceSource())
                    {
                        source.TraceException(exception);
                    }
                }
            }
        }

        internal virtual void WriteDebug(string message)
        {
            this.Debug.Add(new DebugRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Debug, message));
        }

        internal virtual void WriteError(ErrorRecord errorRecord)
        {
            this.Error.Add(errorRecord);
            if (this.PropagateThrows)
            {
                Exception exceptionFromErrorRecord = GetExceptionFromErrorRecord(errorRecord);
                if (exceptionFromErrorRecord != null)
                {
                    this.Results.Add(new PSStreamObject(PSStreamObjectType.Exception, exceptionFromErrorRecord));
                    return;
                }
            }
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Error, errorRecord));
        }

        private void WriteError(Cmdlet cmdlet, ErrorRecord errorRecord)
        {
            if (this.PropagateThrows)
            {
                Exception exceptionFromErrorRecord = GetExceptionFromErrorRecord(errorRecord);
                if (exceptionFromErrorRecord != null)
                {
                    throw exceptionFromErrorRecord;
                }
            }
            errorRecord.PreserveInvocationInfoOnce = true;
            cmdlet.WriteError(errorRecord);
        }

        internal void WriteError(ErrorRecord errorRecord, out Exception exceptionThrownOnCmdletThread)
        {
            this.Error.Add(errorRecord);
            this.InvokeCmdletMethodAndWaitForResults<object>(delegate (Cmdlet cmdlet) {
                this.WriteError(cmdlet, errorRecord);
                return null;
            }, out exceptionThrownOnCmdletThread);
        }

        internal virtual void WriteObject(object outputObject)
        {
            PSObject item = (outputObject == null) ? null : PSObject.AsPSObject(outputObject);
            this.Output.Add(item);
            if (!this.suppressOutputForwarding)
            {
                this.Results.Add(new PSStreamObject(PSStreamObjectType.Output, item));
            }
        }

        internal virtual void WriteProgress(ProgressRecord progressRecord)
        {
            if ((progressRecord.ParentActivityId == -1) && (this.parentActivityId != null))
            {
                ProgressRecord record = new ProgressRecord(progressRecord) {
                    ParentActivityId = this.parentActivityId.Value
                };
                progressRecord = record;
            }
            this.Progress.Add(progressRecord);
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Progress, progressRecord));
        }

        internal virtual void WriteVerbose(string message)
        {
            this.Verbose.Add(new VerboseRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Verbose, message));
        }

        internal virtual void WriteWarning(string message)
        {
            this.Warning.Add(new WarningRecord(message));
            this.Results.Add(new PSStreamObject(PSStreamObjectType.Warning, message));
        }

        internal virtual bool CanDisconnect
        {
            get
            {
                return false;
            }
        }

        public IList<Job> ChildJobs
        {
            get
            {
                if (this.childJobs == null)
                {
                    lock (this.syncObject)
                    {
                        if (this.childJobs == null)
                        {
                            this.childJobs = new List<Job>();
                        }
                    }
                }
                return this.childJobs;
            }
        }

        public string Command
        {
            get
            {
                return this.command;
            }
        }

        public PSDataCollection<DebugRecord> Debug
        {
            get
            {
                this.LoadJobStreams();
                return this.debug;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Debug");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.debugOwner = false;
                    this.debug = value;
                }
            }
        }

        public PSDataCollection<ErrorRecord> Error
        {
            get
            {
                this.LoadJobStreams();
                return this.error;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Error");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.errorOwner = false;
                    this.error = value;
                    this._jobStreamsLoaded = true;
                }
            }
        }

        public WaitHandle Finished
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.finished != null)
                    {
                        return this.finished;
                    }
                    return new ManualResetEvent(true);
                }
            }
        }

        public abstract bool HasMoreData { get; }

        public int Id
        {
            get
            {
                return this.sessionId;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.guid;
            }
        }

        public System.Management.Automation.JobStateInfo JobStateInfo
        {
            get
            {
                return this.stateInfo;
            }
        }

        public abstract string Location { get; }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.AssertNotDisposed();
                this.name = value;
            }
        }

        public PSDataCollection<PSObject> Output
        {
            get
            {
                this.LoadJobStreams();
                return this.output;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Output");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.outputOwner = false;
                    this.output = value;
                    this._jobStreamsLoaded = true;
                }
            }
        }

        public PSDataCollection<ProgressRecord> Progress
        {
            get
            {
                this.LoadJobStreams();
                return this.progress;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Progress");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.progressOwner = false;
                    this.progress = value;
                    this._jobStreamsLoaded = true;
                }
            }
        }

        internal bool PropagateThrows
        {
            get
            {
                return this.propagateThrows;
            }
            set
            {
                this.propagateThrows = value;
            }
        }

        public DateTime? PSBeginTime
        {
            get
            {
                return this._beginTime;
            }
            protected set
            {
                this._beginTime = value;
            }
        }

        public DateTime? PSEndTime
        {
            get
            {
                return this._endTime;
            }
            protected set
            {
                this._endTime = value;
            }
        }

        public string PSJobTypeName
        {
            get
            {
                return this._jobTypeName;
            }
            protected internal set
            {
                this._jobTypeName = (value != null) ? value : base.GetType().ToString();
            }
        }

        internal PSDataCollection<PSStreamObject> Results
        {
            get
            {
                return this.results;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Results");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.resultsOwner = false;
                    this.results = value;
                }
            }
        }

        public abstract string StatusMessage { get; }

        internal bool SuppressOutputForwarding
        {
            get
            {
                return this.suppressOutputForwarding;
            }
            set
            {
                this.suppressOutputForwarding = value;
            }
        }

        internal bool UsesResultsCollection { get; set; }

        public PSDataCollection<VerboseRecord> Verbose
        {
            get
            {
                this.LoadJobStreams();
                return this.verbose;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Verbose");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.verboseOwner = false;
                    this.verbose = value;
                    this._jobStreamsLoaded = true;
                }
            }
        }

        public PSDataCollection<WarningRecord> Warning
        {
            get
            {
                this.LoadJobStreams();
                return this.warning;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Warning");
                }
                lock (this.syncObject)
                {
                    this.AssertChangesAreAccepted();
                    this.warningOwner = false;
                    this.warning = value;
                    this._jobStreamsLoaded = true;
                }
            }
        }
    }
}

