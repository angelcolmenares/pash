namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class PSJobProxy : Job2
    {
        private int _blockedChildJobsCount;
        private bool _childEventsRegistered;
        private readonly Hashtable _childJobsMapping;
        private JobState _computedJobState;
        private EventHandler<JobDataAddedEventArgs> _dataAddedHandler;
        private int _failedChildJobsCount;
        private int _finishedChildJobsCount;
        private bool _inProgress;
        private object _inProgressSyncObject;
        private bool _isDisposed;
        private readonly ManualResetEvent _jobInitialiedWaitHandle;
        private bool _jobInitialized;
        private ManualResetEvent _jobRunningOrFinished;
        private ManualResetEvent _jobSuspendedOrFinished;
        private bool _moreData;
        private ConcurrentQueue<QueueOperation> _pendingOperations;
        private JobState _previousState;
        private PSCommand _pscommand;
        private bool _receiveIsValidCall;
        private readonly PowerShell _receivePowerShell;
        private readonly PSDataCollection<PSObject> _receivePowerShellOutput;
        private Guid _remoteJobInstanceId;
        private string _remoteJobLocation;
        private bool _remoteJobRemoved;
        private string _remoteJobStatusMessage;
        private bool _removeCalled;
        private ManualResetEvent _removeComplete;
        private bool _removeRemoteJobOnCompletion;
        private System.Management.Automation.Runspaces.Runspace _runspace;
        private System.Management.Automation.Runspaces.RunspacePool _runspacePool;
        private bool _startCalled;
        private EventHandler<JobStateEventArgs> _stateChangedHandler;
        private int _stoppedChildJobsCount;
        private int _suspendedChildJobsCount;
        private int _suspendingChildJobsCount;
        private readonly PowerShellTraceSource _tracer;
        private bool _workerCreated;
        private const string ClassNameTrace = "PSJobProxy";
        private const string MaxElapsedTimeSeconds = "MaxElapsedTimeSeconds";
        private const string MaxRunningTimeSeconds = "MaxRunningTimeSeconds";
        private const string ResBaseName = "PowerShellStrings";
        private const string RetryCount = "RetryCount";
        private const string RetryFrequency = "RetryFrequency";
        private static Tracer StructuredTracer = new Tracer();

        public event EventHandler<AsyncCompletedEventArgs> RemoveJobCompleted;

        internal PSJobProxy(string command) : base(command)
        {
            this._pendingOperations = new ConcurrentQueue<QueueOperation>();
            this._jobInitialiedWaitHandle = new ManualResetEvent(false);
            this._remoteJobInstanceId = Guid.Empty;
            this._remoteJobStatusMessage = string.Empty;
            this._remoteJobLocation = string.Empty;
            this._childJobsMapping = new Hashtable();
            this._receivePowerShell = PowerShell.Create();
            this._receivePowerShellOutput = new PSDataCollection<PSObject>();
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._inProgressSyncObject = new object();
            this._tracer.WriteMessage("PSJobProxy", "ctor", this._remoteJobInstanceId, this, "Constructing proxy job", null);
            base.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleMyStateChange);
        }

        private void AddReceiveJobCommandToPowerShell(PowerShell powershell, bool writeJob)
        {
            powershell.AddCommand("Receive-Job").AddParameter("Wait").AddParameter("WriteEvents").AddParameter("Verbose").AddParameter("Debug");
            if (writeJob)
            {
                powershell.AddParameter("WriteJobInResults");
            }
            if (this.RemoveRemoteJobOnCompletion)
            {
                powershell.AddParameter("AutoRemoveJob");
            }
        }

        private void AssertChangesCanBeAccepted()
        {
            lock (base.SyncRoot)
            {
                this.AssertNotDisposed();
                if (base.JobStateInfo.State != JobState.NotStarted)
                {
                    throw new InvalidJobStateException(base.JobStateInfo.State);
                }
            }
        }

        private void AssertJobCanBeStartedAndSetStateToRunning()
        {
            lock (base.SyncRoot)
            {
                this.AssertNotDisposed();
                if ((base.JobStateInfo.State != JobState.NotStarted) && !base.IsFinishedState(base.JobStateInfo.State))
                {
                    throw new InvalidJobStateException(base.JobStateInfo.State, StringUtil.Format(PowerShellStrings.JobCannotBeStartedWhenRunning, new object[0]));
                }
                if (this._startCalled)
                {
                    throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "JobCanBeStartedOnce", new object[0]);
                }
                this._startCalled = true;
            }
            this._tracer.WriteMessage("PSJobProxy", "AssertJobCanBeStartedAndSetStateToRunning", this._remoteJobInstanceId, this, "Setting job state to running", null);
            this.ValidateAndDoSetJobState(JobState.Running, null);
        }

        private void AssertNotDisposed()
        {
            if (this._isDisposed)
            {
                throw PSTraceSource.NewObjectDisposedException("PSJobProxy");
            }
        }

        private void AssertRemoveJobIsValid()
        {
            if ((base.JobStateInfo.State == JobState.NotStarted) || (this._remoteJobInstanceId == Guid.Empty))
            {
                throw new InvalidJobStateException(base.JobStateInfo.State);
            }
        }

        private void AssertResumeJobIsValidAndSetToRunning()
        {
            lock (base.SyncRoot)
            {
                this.AssertNotDisposed();
                if (((base.JobStateInfo.State != JobState.Suspended) && (base.JobStateInfo.State != JobState.Suspending)) && (base.JobStateInfo.State != JobState.Running))
                {
                    throw new InvalidJobStateException(base.JobStateInfo.State);
                }
            }
            if (base.JobStateInfo.State != JobState.Running)
            {
                this.ValidateAndDoSetJobState(JobState.Running, null);
            }
            foreach (PSChildJobProxy proxy in base.ChildJobs)
            {
                if (!base.IsFinishedState(proxy.JobStateInfo.State))
                {
                    proxy.DoSetJobState(JobState.Running, null);
                }
            }
        }

        private bool AssertStopJobIsValidAndSetToStopping()
        {
            lock (base.SyncRoot)
            {
                this.AssertNotDisposed();
                if (base.JobStateInfo.State == JobState.NotStarted)
                {
                    throw new InvalidJobStateException(JobState.NotStarted);
                }
                if (base.IsFinishedState(base.JobStateInfo.State))
                {
                    return false;
                }
            }
            this.DoSetJobState(JobState.Stopping, null);
            return true;
        }

        private bool AssertSuspendJobIsValidAndSetToSuspending()
        {
            lock (base.SyncRoot)
            {
                this.AssertNotDisposed();
                if (((base.JobStateInfo.State != JobState.Suspended) && (base.JobStateInfo.State != JobState.Suspending)) && (base.JobStateInfo.State != JobState.Running))
                {
                    throw new InvalidJobStateException(base.JobStateInfo.State);
                }
            }
            if (base.JobStateInfo.State != JobState.Running)
            {
                return false;
            }
            this.DoSetJobState(JobState.Suspending, null);
            return true;
        }

        private void AssignRunspaceOrRunspacePool(PowerShell powershell)
        {
            if (this._runspacePool == null)
            {
                powershell.Runspace = this._runspace;
            }
            else
            {
                powershell.RunspacePool = this._runspacePool;
            }
        }

        private void CleanupReceivePowerShell(IAsyncResult asyncResult)
        {
            try
            {
                this._receivePowerShell.EndInvoke(asyncResult);
                this._tracer.WriteMessage("PSJobProxy", "CleanupReceivePowerShell", Guid.Empty, this, "Setting job state to {0} from computed stated", new string[] { this._computedJobState.ToString() });
                this.ValidateAndDoSetJobState(this._computedJobState, null);
            }
            catch (PipelineStoppedException exception)
            {
                this._tracer.TraceException(exception);
            }
            catch (PSRemotingDataStructureException exception2)
            {
                this._tracer.TraceException(exception2);
            }
            catch (RemoteException exception3)
            {
                if (Deserializer.IsInstanceOfType(exception3.SerializedRemoteException, typeof(PipelineStoppedException)))
                {
                    this._tracer.TraceException(exception3);
                }
                else
                {
                    this._tracer.TraceException(exception3);
                    this.DoSetJobState(JobState.Failed, exception3);
                }
            }
            catch (Exception exception4)
            {
                this._tracer.WriteMessage("PSJobProxy", "CleanupReceivePowerShell", this._remoteJobInstanceId, this, "Exception calling receivePowerShell.EndInvoke", null);
                this._tracer.TraceException(exception4);
                this.DoSetJobState(JobState.Failed, exception4);
            }
        }

        private static bool CollectionHasMoreData<T>(PSDataCollection<T> collection)
        {
            if (!collection.IsOpen)
            {
                return (collection.Count > 0);
            }
            return true;
        }

        private void CommonInit(System.Management.Automation.Runspaces.Runspace runspace, System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            this._runspacePool = runspacePool;
            this._runspace = runspace;
            this._receivePowerShell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.ReceivePowerShellInvocationStateChanged);
            this._receivePowerShellOutput.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToOutput);
            this._receivePowerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToError);
            this._receivePowerShell.Streams.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToDebug);
            this._receivePowerShell.Streams.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToVerbose);
            this._receivePowerShell.Streams.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToWarning);
            this._receivePowerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.DataAddedToProgress);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace)
        {
            return Create(runspace, null, null, null, true);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            return Create(runspacePool, null, null, null, true);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace, Hashtable filter)
        {
            return Create(runspace, filter, null, null, true);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.RunspacePool runspacePool, Hashtable filter)
        {
            return Create(runspacePool, filter, null, null, true);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace, Hashtable filter, bool receiveImmediately)
        {
            return Create(runspace, filter, null, null, receiveImmediately);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.RunspacePool runspacePool, Hashtable filter, bool receiveImmediately)
        {
            return Create(runspacePool, filter, null, null, receiveImmediately);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace, Hashtable filter, EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged)
        {
            return Create(runspace, filter, dataAdded, stateChanged, true);
        }

        public static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.RunspacePool runspacePool, Hashtable filter, EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged)
        {
            return Create(runspacePool, filter, dataAdded, stateChanged, true);
        }

        private static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace, Hashtable filter, EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, bool connectImmediately)
        {
            if (runspace == null)
            {
                throw new PSArgumentNullException("runspace");
            }
            return Create(runspace, null, filter, dataAdded, stateChanged, connectImmediately);
        }

        private static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.RunspacePool runspacePool, Hashtable filter, EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, bool connectImmediately)
        {
            if (runspacePool == null)
            {
                throw new PSArgumentNullException("runspacePool");
            }
            return Create(null, runspacePool, filter, dataAdded, stateChanged, connectImmediately);
        }

        private static ICollection<PSJobProxy> Create(System.Management.Automation.Runspaces.Runspace runspace, System.Management.Automation.Runspaces.RunspacePool runspacePool, Hashtable filter, EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, bool connectImmediately)
        {
            Collection<PSObject> collection;
            using (PowerShell shell = PowerShell.Create())
            {
                if (runspacePool == null)
                {
                    shell.Runspace = runspace;
                }
                else
                {
                    shell.RunspacePool = runspacePool;
                }
                shell.AddCommand("Get-Job");
                if (filter != null)
                {
                    shell.AddParameter("Filter", filter);
                }
                collection = shell.Invoke();
            }
            Collection<PSJobProxy> collection2 = new Collection<PSJobProxy>();
            foreach (PSObject obj2 in collection)
            {
                if (Deserializer.IsDeserializedInstanceOfType(obj2, typeof(Job)))
                {
                    string propertyValue = string.Empty;
                    TryGetJobPropertyValue<string>(obj2, "Command", out propertyValue);
                    PSJobProxy item = new PSJobProxy(propertyValue);
                    item.InitializeExistingJobProxy(obj2, runspace, runspacePool);
                    item._receiveIsValidCall = true;
                    if (connectImmediately)
                    {
                        item.ReceiveJob(dataAdded, stateChanged);
                    }
                    collection2.Add(item);
                }
            }
            return collection2;
        }

        private void DataAddedToDebug(object sender, DataAddedEventArgs e)
        {
            DebugRecord record = this.GetRecord<DebugRecord>(sender);
            this.SortDebug(record);
        }

        private void DataAddedToError(object sender, DataAddedEventArgs e)
        {
            ErrorRecord record = this.GetRecord<ErrorRecord>(sender);
            this.SortError(record);
        }

        private void DataAddedToOutput(object sender, DataAddedEventArgs e)
        {
            PSObject record = this.GetRecord<PSObject>(sender);
            if (!this._jobInitialized)
            {
                this._jobInitialized = true;
                if (!Deserializer.IsDeserializedInstanceOfType(record, typeof(Job)))
                {
                    this._tracer.WriteMessage("PSJobProxy", "DataAddedToOutput", this._remoteJobInstanceId, this, "Setting job state to failed. Command did not return a job object.", null);
                    Exception reason = ((this._receivePowerShell.Streams.Error.Count == 0) || (this._receivePowerShell.Streams.Error[0].Exception == null)) ? PSTraceSource.NewNotSupportedException("PowerShellStrings", "CommandDoesNotWriteJob", new object[0]) : this._receivePowerShell.Streams.Error[0].Exception;
                    this.DoSetJobState(JobState.Failed, reason);
                    this._jobInitialiedWaitHandle.Set();
                    this.OnStartJobCompleted(new AsyncCompletedEventArgs(reason, false, null));
                }
                else
                {
                    this.PopulateJobProperties(record);
                    this.RegisterChildEvents();
                    foreach (PSChildJobProxy proxy in base.ChildJobs)
                    {
                        proxy.DoSetJobState(JobState.Running, null);
                    }
                    this._jobInitialiedWaitHandle.Set();
                    this._tracer.WriteMessage("PSJobProxy", "DataAddedToOutput", Guid.Empty, this, "BEGIN Invoke StartJobCompleted event", null);
                    this.OnStartJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                    this._tracer.WriteMessage("PSJobProxy", "DataAddedToOutput", Guid.Empty, this, "END Invoke StartJobCompleted event", null);
                    this.ProcessQueue();
                }
            }
            else if (record.Properties[RemotingConstants.EventObject] != null)
            {
                PSPropertyInfo info = record.Properties[RemotingConstants.SourceJobInstanceId];
                Guid key = (info != null) ? ((Guid) info.Value) : Guid.Empty;
                if ((info != null) && (key != Guid.Empty))
                {
                    if (!this._childJobsMapping.ContainsKey(key))
                    {
                        bool flag1 = key != this._remoteJobInstanceId;
                    }
                    else
                    {
                        JobStateEventArgs args = (record.BaseObject as JobStateEventArgs) ?? DeserializingTypeConverter.RehydrateJobStateEventArgs(record);
                        if (args != null)
                        {
                            this._tracer.WriteMessage("PSJobProxy", "DataAddedToOutput", Guid.Empty, this, "Updating child job {0} state to {1} ", new string[] { key.ToString(), args.JobStateInfo.State.ToString() });
                            ((PSChildJobProxy) this._childJobsMapping[key]).DoSetJobState(args.JobStateInfo.State, args.JobStateInfo.Reason);
                            this._tracer.WriteMessage("PSJobProxy", "DataAddedToOutput", Guid.Empty, this, "Finished updating child job {0} state to {1} ", new string[] { key.ToString(), args.JobStateInfo.State.ToString() });
                        }
                    }
                }
            }
            else
            {
                this.SortOutputObject(record);
            }
        }

        private void DataAddedToProgress(object sender, DataAddedEventArgs e)
        {
            ProgressRecord newRecord = this.GetRecord<ProgressRecord>(sender);
            this.SortProgress(newRecord);
        }

        private void DataAddedToVerbose(object sender, DataAddedEventArgs e)
        {
            VerboseRecord record = this.GetRecord<VerboseRecord>(sender);
            this.SortVerbose(record);
        }

        private void DataAddedToWarning(object sender, DataAddedEventArgs e)
        {
            WarningRecord record = this.GetRecord<WarningRecord>(sender);
            this.SortWarning(record);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this._isDisposed)
            {
                lock (base.SyncRoot)
                {
                    if (this._isDisposed)
                    {
                        return;
                    }
                    this._isDisposed = true;
                }
                if (this._receivePowerShell != null)
                {
                    this._receivePowerShell.Stop();
                    this._receivePowerShell.InvocationStateChanged -= new EventHandler<PSInvocationStateChangedEventArgs>(this.ReceivePowerShellInvocationStateChanged);
                    this._receivePowerShell.Streams.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToError);
                    this._receivePowerShell.Streams.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToWarning);
                    this._receivePowerShell.Streams.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToVerbose);
                    this._receivePowerShell.Streams.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToProgress);
                    this._receivePowerShell.Streams.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToDebug);
                    this._receivePowerShell.Dispose();
                }
                this.UnregisterChildEvents();
                base.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleMyStateChange);
                this._receivePowerShellOutput.DataAdded -= new EventHandler<DataAddedEventArgs>(this.DataAddedToOutput);
                if (this._receivePowerShellOutput != null)
                {
                    this._receivePowerShellOutput.Dispose();
                }
                if (this._removeComplete != null)
                {
                    this._removeComplete.Dispose();
                }
                if (this._jobRunningOrFinished != null)
                {
                    this._jobRunningOrFinished.Dispose();
                }
                this._jobInitialiedWaitHandle.Dispose();
                if (this._jobSuspendedOrFinished != null)
                {
                    this._jobSuspendedOrFinished.Dispose();
                }
                if ((base.ChildJobs != null) && (base.ChildJobs.Count > 0))
                {
                    foreach (Job job in base.ChildJobs)
                    {
                        job.Dispose();
                    }
                }
                this._tracer.Dispose();
            }
        }

        private void DoRemove(object state)
        {
            this.AssertNotDisposed();
            this._tracer.WriteMessage("PSJobProxy", "DoRemove", this._remoteJobInstanceId, this, "Start", null);
            if (!this._isDisposed && !this._remoteJobRemoved)
            {
                lock (base.SyncRoot)
                {
                    if (((this._isDisposed || this._remoteJobRemoved) || this._removeCalled) || ((this._remoteJobInstanceId == Guid.Empty) && !this._startCalled))
                    {
                        return;
                    }
                    this.AssertRemoveJobIsValid();
                    this._removeCalled = true;
                    this.RemoveComplete.Reset();
                }
                try
                {
                    this._jobInitialiedWaitHandle.WaitOne();
                    if (this._remoteJobInstanceId != Guid.Empty)
                    {
                        this._receivePowerShell.Stop();
                        using (PowerShell shell = PowerShell.Create())
                        {
                            this.AssignRunspaceOrRunspacePool(shell);
                            shell.Commands.AddCommand("Remove-Job").AddParameter("InstanceId", this._remoteJobInstanceId);
                            if ((bool) state)
                            {
                                shell.AddParameter("Force", true).AddParameter("ErrorAction", ActionPreference.SilentlyContinue);
                            }
                            try
                            {
                                this._tracer.WriteMessage("PSJobProxy", "DoRemove", this._remoteJobInstanceId, this, "Invoking Remove-Job", null);
                                shell.Invoke();
                            }
                            catch (Exception exception)
                            {
                                CommandProcessorBase.CheckForSevereException(exception);
                                this._tracer.WriteMessage("PSJobProxy", "DoRemove", this._remoteJobInstanceId, this, "Setting job state to failed since invoking Remove-Job failed.", null);
                                this.DoSetJobState(JobState.Failed, exception);
                                throw;
                            }
                            if ((shell.Streams.Error != null) && (shell.Streams.Error.Count > 0))
                            {
                                throw shell.Streams.Error[0].Exception;
                            }
                        }
                        this._tracer.WriteMessage("PSJobProxy", "DoRemove", this._remoteJobInstanceId, this, "Completed Invoking Remove-Job", null);
                        lock (base.SyncRoot)
                        {
                            this._remoteJobRemoved = true;
                        }
                        if (!base.IsFinishedState(base.JobStateInfo.State))
                        {
                            this.DoSetJobState(JobState.Stopped, null);
                        }
                    }
                }
                catch (Exception)
                {
                    lock (base.SyncRoot)
                    {
                        this._removeCalled = false;
                    }
                    throw;
                }
                finally
                {
                    this.RemoveComplete.Set();
                }
            }
        }

        private void DoResumeAsync()
        {
            this.AssertResumeJobIsValidAndSetToRunning();
            if (base.JobStateInfo.State != JobState.Running)
            {
                this.OnResumeJobCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                this._receivePowerShell.Stop();
                this._receivePowerShell.Commands.Clear();
                this._receivePowerShell.GenerateNewInstanceId();
                this._receivePowerShell.AddCommand("Resume-Job").AddParameter("InstanceId", this._remoteJobInstanceId).AddParameter("Wait");
                this.AddReceiveJobCommandToPowerShell(this._receivePowerShell, false);
                this._receivePowerShell.BeginInvoke<PSObject, PSObject>(null, this._receivePowerShellOutput, null, new AsyncCallback(this.CleanupReceivePowerShell), null);
            }
        }

        private void DoSetJobState(JobState state, Exception reason = null)
        {
            if (!this._isDisposed)
            {
                lock (base.SyncRoot)
                {
                    if (this._previousState == state)
                    {
                        return;
                    }
                    this._previousState = state;
                }
                try
                {
                    this._tracer.WriteMessage("PSJobProxy", "DoSetJobState", this._remoteJobInstanceId, this, "BEGIN Set job state to {0} and call event handlers", new string[] { state.ToString() });
                    StructuredTracer.EndProxyJobExecution(base.InstanceId);
                    StructuredTracer.BeginProxyJobEventHandler(base.InstanceId);
                    base.SetJobState(state, reason);
                    StructuredTracer.EndProxyJobEventHandler(base.InstanceId);
                    this._tracer.WriteMessage("PSJobProxy", "DoSetJobState", this._remoteJobInstanceId, this, "END Set job state to {0} and call event handlers", new string[] { state.ToString() });
                }
                catch (ObjectDisposedException)
                {
                    this._tracer.WriteMessage("PSJobProxy", "DoSetJobState", this._remoteJobInstanceId, this, "Caught object disposed exception", null);
                }
            }
        }

        private void DoStartAsync(EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, PSDataCollection<object> input)
        {
            this.AssertJobCanBeStartedAndSetStateToRunning();
            lock (this._inProgressSyncObject)
            {
                this._inProgress = true;
            }
            lock (base.SyncRoot)
            {
                this._dataAddedHandler = dataAdded;
                this._stateChangedHandler = stateChanged;
            }
            this._tracer.WriteMessage("PSJobProxy", "DoStartAsync", this._remoteJobInstanceId, this, "Starting command invocation.", null);
            StructuredTracer.BeginProxyJobExecution(base.InstanceId);
            this.DoStartPrepare();
            this._receivePowerShell.BeginInvoke<object, PSObject>(input, this._receivePowerShellOutput, null, new AsyncCallback(this.CleanupReceivePowerShell), null);
        }

        private void DoStartPrepare()
        {
            if ((this._runspacePool == null) && (this._runspace == null))
            {
                throw PSTraceSource.NewInvalidOperationException("PowerShellStrings", "RunspaceAndRunspacePoolNull", new object[0]);
            }
            this.AssignRunspaceOrRunspacePool(this._receivePowerShell);
            bool flag = false;
            if ((base.StartParameters != null) && (base.StartParameters.Count > 0))
            {
                foreach (CommandParameter parameter in base.StartParameters[0])
                {
                    this._pscommand.Commands[0].Parameters.Add(parameter);
                    if (string.Compare(parameter.Name, "AsJob", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!(parameter.Value is bool) || !((bool) parameter.Value))
                        {
                            throw PSTraceSource.NewInvalidOperationException("PowerShellStrings", "JobProxyAsJobMustBeTrue", new object[0]);
                        }
                        flag = true;
                    }
                }
            }
            if (!flag)
            {
                this._pscommand.Commands[0].Parameters.Add("AsJob", true);
            }
            this._receivePowerShell.Commands = this._pscommand;
            this.AddReceiveJobCommandToPowerShell(this._receivePowerShell, true);
        }

        private void DoStopAsync()
        {
            if (!this.AssertStopJobIsValidAndSetToStopping())
            {
                this.OnStopJobCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                this._receivePowerShell.Stop();
                this._receivePowerShell.Commands.Clear();
                this._receivePowerShell.GenerateNewInstanceId();
                this._receivePowerShell.AddCommand("Stop-Job").AddParameter("InstanceId", this._remoteJobInstanceId).AddParameter("PassThru");
                this.AddReceiveJobCommandToPowerShell(this._receivePowerShell, false);
                this._receivePowerShell.BeginInvoke<PSObject, PSObject>(null, this._receivePowerShellOutput, null, new AsyncCallback(this.CleanupReceivePowerShell), null);
            }
        }

        private void DoSuspendAsync()
        {
            if (!this.AssertSuspendJobIsValidAndSetToSuspending())
            {
                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                this._receivePowerShell.Stop();
                this._receivePowerShell.Commands.Clear();
                this._receivePowerShell.GenerateNewInstanceId();
                this._receivePowerShell.AddCommand("Suspend-Job").AddParameter("InstanceId", this._remoteJobInstanceId).AddParameter("Wait");
                this.AddReceiveJobCommandToPowerShell(this._receivePowerShell, false);
                this._receivePowerShell.BeginInvoke<PSObject, PSObject>(null, this._receivePowerShellOutput, null, new AsyncCallback(this.CleanupReceivePowerShell), null);
            }
        }

        private T GetRecord<T>(object sender)
        {
            lock (base.SyncRoot)
            {
                PSDataCollection<T> datas = sender as PSDataCollection<T>;
                return datas.ReadAndRemoveAt0();
            }
        }

        private void HandleChildProxyJobStateChanged(object sender, JobStateEventArgs e)
        {
            JobState state;
            if (ContainerParentJob.ComputeJobStateFromChildJobStates("PSJobProxy", e, ref this._blockedChildJobsCount, ref this._suspendedChildJobsCount, ref this._suspendingChildJobsCount, ref this._finishedChildJobsCount, ref this._failedChildJobsCount, ref this._stoppedChildJobsCount, base.ChildJobs.Count, out state) && (state != JobState.Suspending))
            {
                this._tracer.WriteMessage("PSJobProxy", "HandleChildProxyJobStateChanged", Guid.Empty, this, "storing job state to {0}", new string[] { state.ToString() });
                this._computedJobState = state;
            }
        }

        private void HandleMyStateChange(object sender, JobStateEventArgs e)
        {
            bool flag3;
            object obj4 = null;
            switch (e.JobStateInfo.State)
            {
                case JobState.Running:
                    lock (base.SyncRoot)
                    {
                        if (e.PreviousJobStateInfo.State == JobState.NotStarted)
                        {
                            base.PSBeginTime = new DateTime?(DateTime.Now);
                        }
                        this.JobRunningOrFinished.Set();
                        this.JobSuspendedOrFinished.Reset();
                        this.OnResumeJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                        goto Label_01A1;
                    }
                    break;

                case JobState.Completed:
                case JobState.Failed:
                case JobState.Stopped:
                    goto Label_00EE;

                case JobState.Suspended:
                    break;

                default:
                    goto Label_01A1;
            }
            lock (base.SyncRoot)
            {
                base.PSEndTime = new DateTime?(DateTime.Now);
                this.JobSuspendedOrFinished.Set();
                this.JobRunningOrFinished.Reset();
                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                goto Label_01A1;
            }
        Label_00EE:
            flag3 = false;
            try
            {
                Monitor.Enter(obj4 = base.SyncRoot, ref flag3);
                base.PSEndTime = new DateTime?(DateTime.Now);
                this.JobRunningOrFinished.Set();
                this.OnResumeJobCompleted(new AsyncCompletedEventArgs(e.JobStateInfo.Reason, false, null));
                this.JobSuspendedOrFinished.Set();
                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(e.JobStateInfo.Reason, false, null));
                this._jobInitialiedWaitHandle.Set();
                this.OnStartJobCompleted(new AsyncCompletedEventArgs(e.JobStateInfo.Reason, false, null));
                this.OnStopJobCompleted(new AsyncCompletedEventArgs(e.JobStateInfo.Reason, false, null));
            }
            finally
            {
                if (flag3)
                {
                    Monitor.Exit(obj4);
                }
            }
        Label_01A1:
            this.ProcessQueue();
        }

        internal void InitializeExistingJobProxy(PSObject o, System.Management.Automation.Runspaces.Runspace runspace, System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            this._tracer.WriteMessage("PSJobProxy", "InitializeExistingJobProxy", this._remoteJobInstanceId, this, "Initializing job proxy for existing job.", null);
            this._pscommand = null;
            this._startCalled = true;
            this._jobInitialized = true;
            this.CommonInit(runspace, runspacePool);
            this.PopulateJobProperties(o);
            List<Hashtable> list = new List<Hashtable>();
            object obj2 = null;
            foreach (PSChildJobProxy proxy in base.ChildJobs)
            {
                if (proxy.StartParameters.Count != 0)
                {
                    Hashtable hashtable = new Hashtable();
                    foreach (CommandParameter parameter in proxy.StartParameters[0])
                    {
                        if ((obj2 == null) && parameter.Name.Equals("PSPrivateMetadata", StringComparison.OrdinalIgnoreCase))
                        {
                            obj2 = parameter.Value;
                        }
                        hashtable.Add(parameter.Name, parameter.Value);
                    }
                    list.Add(hashtable);
                }
            }
            CommandParameterCollection item = new CommandParameterCollection();
            item.Add(new CommandParameter("PSParameterCollection", list));
            if (obj2 != null)
            {
                item.Add(new CommandParameter("PSPrivateMetadata", obj2));
            }
            base.StartParameters.Add(item);
        }

        internal void InitializeJobProxy(PSCommand command, System.Management.Automation.Runspaces.Runspace runspace, System.Management.Automation.Runspaces.RunspacePool runspacePool)
        {
            this._tracer.WriteMessage("PSJobProxy", "InitializeJobProxy", this._remoteJobInstanceId, this, "Initializing Job Proxy.", null);
            this._pscommand = command.Clone();
            CommandParameterCollection parameters = new CommandParameterCollection();
            foreach (CommandParameter parameter in this._pscommand.Commands[0].Parameters)
            {
                parameters.Add(parameter);
            }
            List<CommandParameterCollection> list = new List<CommandParameterCollection> {
                parameters
            };
            base.StartParameters = list;
            this._pscommand.Commands[0].Parameters.Clear();
            this.CommonInit(runspace, runspacePool);
        }

        private void JobActionAsyncCompleted(object operationState)
        {
            AsyncCompleteContainer container = operationState as AsyncCompleteContainer;
            this._tracer.WriteMessage("PSJobProxy", "JobActionAsyncCompleted", this._remoteJobInstanceId, this, "Async operation {0} completed", new string[] { container.Action.ToString() });
            if (container.Action == ActionType.Remove)
            {
                if (container.EventArgs.Error == null)
                {
                    this.RemoveComplete.WaitOne();
                }
                this.OnRemoveJobCompleted(container.EventArgs);
            }
        }

        private void JobActionWorker(AsyncOperation asyncOp, ActionType action)
        {
            Exception error = null;
            try
            {
                if (action == ActionType.Remove)
                {
                    this.DoRemove(asyncOp.UserSuppliedState);
                }
            }
            catch (Exception exception2)
            {
                error = exception2;
            }
            AsyncCompletedEventArgs args = new AsyncCompletedEventArgs(error, false, asyncOp.UserSuppliedState);
            AsyncCompleteContainer arg = new AsyncCompleteContainer {
                EventArgs = args,
                Action = action
            };
            asyncOp.PostOperationCompleted(new SendOrPostCallback(this.JobActionAsyncCompleted), arg);
        }

        private void OnRemoveJobCompleted(AsyncCompletedEventArgs eventArgs)
        {
            EventHandler<AsyncCompletedEventArgs> removeJobCompleted = this.RemoveJobCompleted;
            try
            {
                if (removeJobCompleted != null)
                {
                    removeJobCompleted(this, eventArgs);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.TraceException(exception);
            }
        }

        private void PopulateJobProperties(PSObject o)
        {
            string str;
            PSObject obj2;
            TryGetJobPropertyValue<Guid>(o, "InstanceId", out this._remoteJobInstanceId);
            TryGetJobPropertyValue<string>(o, "StatusMessage", out this._remoteJobStatusMessage);
            TryGetJobPropertyValue<string>(o, "Location", out this._remoteJobLocation);
            StructuredTracer.ProxyJobRemoteJobAssociation(base.InstanceId, this._remoteJobInstanceId);
            TryGetJobPropertyValue<string>(o, "Name", out str);
            base.Name = str;
            if (TryGetJobPropertyValue<PSObject>(o, "ChildJobs", out obj2))
            {
                ArrayList baseObject = obj2.BaseObject as ArrayList;
                foreach (PSObject obj3 in from job in baseObject.Cast<PSObject>()
                    where !(job.BaseObject is string)
                    select job)
                {
                    Guid guid;
                    if (TryGetJobPropertyValue<Guid>(obj3, "InstanceId", out guid))
                    {
                        PSObject obj4;
                        PSChildJobProxy proxy = new PSChildJobProxy(base.Command, obj3);
                        this._childJobsMapping.Add(guid, proxy);
                        proxy.Output.DataAddedCount = base.Output.DataAddedCount;
                        proxy.Error.DataAddedCount = base.Error.DataAddedCount;
                        proxy.Progress.DataAddedCount = base.Progress.DataAddedCount;
                        proxy.Warning.DataAddedCount = base.Warning.DataAddedCount;
                        proxy.Verbose.DataAddedCount = base.Verbose.DataAddedCount;
                        proxy.Debug.DataAddedCount = base.Debug.DataAddedCount;
                        if (TryGetJobPropertyValue<PSObject>(obj3, "StartParameters", out obj4))
                        {
                            this.PopulateStartParametersOnChild(obj4, proxy);
                        }
                        base.ChildJobs.Add(proxy);
                    }
                }
            }
        }

        private void PopulateStartParametersOnChild(PSObject childJobStartParametersObject, PSChildJobProxy childProxyJob)
        {
            ArrayList baseObject = childJobStartParametersObject.BaseObject as ArrayList;
            if (baseObject != null)
            {
                List<CommandParameterCollection> list2 = new List<CommandParameterCollection>();
                foreach (PSObject obj2 in from paramCollection in baseObject.Cast<PSObject>()
                    where !(paramCollection.BaseObject is string)
                    select paramCollection)
                {
                    ArrayList source = obj2.BaseObject as ArrayList;
                    if (source != null)
                    {
                        CommandParameterCollection item = new CommandParameterCollection();
                        foreach (PSObject obj3 in from deserializedCommandParameter in source.Cast<PSObject>()
                            where !(deserializedCommandParameter.BaseObject is string)
                            select deserializedCommandParameter)
                        {
                            string str;
                            object obj4;
                            if (TryGetJobPropertyValue<string>(obj3, "Name", out str) && TryGetJobPropertyValue<object>(obj3, "Value", out obj4))
                            {
                                CommandParameter parameter = new CommandParameter(str, obj4);
                                item.Add(parameter);
                            }
                        }
                        list2.Add(item);
                    }
                }
                childProxyJob.StartParameters = list2;
            }
        }

        private void ProcessQueue()
        {
            bool flag = false;
            lock (this._inProgressSyncObject)
            {
                if (!this._pendingOperations.IsEmpty && !this._workerCreated)
                {
                    flag = true;
                    this._workerCreated = true;
                    this._inProgress = true;
                }
                else
                {
                    this._inProgress = false;
                }
            }
            if (flag)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcessQueueWorker));
            }
        }

        private void ProcessQueueWorker(object state)
        {
            QueueOperation operation;
            bool flag;
            object obj2 = null;
        Label_0000:
            flag = false;
            try
            {
                Monitor.Enter(obj2 = this._inProgressSyncObject, ref flag);
                if (!this._pendingOperations.TryDequeue(out operation))
                {
                    this._inProgress = false;
                    this._workerCreated = false;
                    return;
                }
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(obj2);
                }
            }
            switch (operation)
            {
                case QueueOperation.Stop:
                    try
                    {
                        this.DoStopAsync();
                        base.Finished.WaitOne();
                    }
                    catch (Exception exception)
                    {
                        this.OnStopJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
                    }
                    goto Label_0000;

                case QueueOperation.Suspend:
                    try
                    {
                        this.DoSuspendAsync();
                        this.JobSuspendedOrFinished.WaitOne();
                    }
                    catch (Exception exception2)
                    {
                        this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(exception2, false, null));
                    }
                    goto Label_0000;

                case QueueOperation.Resume:
                    try
                    {
                        this.DoResumeAsync();
                        this.JobRunningOrFinished.WaitOne();
                    }
                    catch (Exception exception3)
                    {
                        this.OnResumeJobCompleted(new AsyncCompletedEventArgs(exception3, false, null));
                    }
                    goto Label_0000;

                default:
                    goto Label_0000;
            }
        }

        public void ReceiveJob()
        {
            this.ReceiveJob(null, null);
        }

        public void ReceiveJob(EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged)
        {
            lock (base.SyncRoot)
            {
                if (!this._receiveIsValidCall)
                {
                    throw PSTraceSource.NewInvalidOperationException("PowerShellStrings", "JobProxyReceiveInvalid", new object[0]);
                }
                this._receiveIsValidCall = false;
                this._dataAddedHandler = dataAdded;
                this._stateChangedHandler = stateChanged;
            }
            this.RegisterChildEvents();
            this.ValidateAndDoSetJobState(JobState.Running, null);
            foreach (PSChildJobProxy proxy in base.ChildJobs)
            {
                proxy.DoSetJobState(JobState.Running, null);
            }
            this.AssignRunspaceOrRunspacePool(this._receivePowerShell);
            this.AddReceiveJobCommandToPowerShell(this._receivePowerShell, false);
            this._receivePowerShell.AddParameter("InstanceId", this._remoteJobInstanceId);
            this._receivePowerShell.BeginInvoke<PSObject, PSObject>(null, this._receivePowerShellOutput, null, new AsyncCallback(this.CleanupReceivePowerShell), null);
        }

        private void ReceivePowerShellInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            this._tracer.WriteMessage("PSJobProxy", "ReceivePowerShellInvocationStateChanged", this._remoteJobInstanceId, this, "receivePowerShell state changed to {0}", new string[] { e.InvocationStateInfo.State.ToString() });
            switch (e.InvocationStateInfo.State)
            {
                case PSInvocationState.Stopping:
                case PSInvocationState.Stopped:
                case PSInvocationState.Completed:
                    return;

                case PSInvocationState.Failed:
                {
                    JobState failed = JobState.Failed;
                    string str = (e.InvocationStateInfo.Reason == null) ? string.Empty : e.InvocationStateInfo.Reason.ToString();
                    this._tracer.WriteMessage("PSJobProxy", "ReceivePowerShellInvocationStateChanged", this._remoteJobInstanceId, this, "Setting job state to {0} old state was {1} and reason is {2}.", new string[] { failed.ToString(), base.JobStateInfo.State.ToString(), str });
                    this.DoSetJobState(failed, e.InvocationStateInfo.Reason);
                    return;
                }
            }
        }

        private void RegisterChildEvents()
        {
            if (!this._childEventsRegistered)
            {
                lock (base.SyncRoot)
                {
                    if (!this._childEventsRegistered)
                    {
                        this._childEventsRegistered = true;
                        if (this._dataAddedHandler != null)
                        {
                            foreach (PSChildJobProxy proxy in base.ChildJobs)
                            {
                                proxy.JobDataAdded += this._dataAddedHandler;
                            }
                        }
                        foreach (Job job in base.ChildJobs)
                        {
                            job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildProxyJobStateChanged);
                        }
                        if (this._stateChangedHandler != null)
                        {
                            foreach (PSChildJobProxy proxy2 in base.ChildJobs)
                            {
                                proxy2.StateChanged += this._stateChangedHandler;
                            }
                        }
                    }
                }
            }
        }

        private static string RemoveIdentifierInformation(string message, out Guid jobInstanceId, out string computerName)
        {
            jobInstanceId = Guid.Empty;
            computerName = string.Empty;
            string[] strArray = message.Split(new char[] { ':' }, 3);
            if (strArray.Length != 3)
            {
                return message;
            }
            if (!Guid.TryParse(strArray[0], out jobInstanceId))
            {
                jobInstanceId = Guid.Empty;
            }
            computerName = strArray[1];
            return strArray[2];
        }

        public void RemoveJob(bool removeRemoteJob)
        {
            this.RemoveJob(removeRemoteJob, false);
        }

        public void RemoveJob(bool removeRemoteJob, bool force)
        {
            if (!removeRemoteJob)
            {
                base.Dispose();
            }
            else
            {
                lock (base.SyncRoot)
                {
                    this.AssertNotDisposed();
                }
                try
                {
                    this.DoRemove(force);
                    lock (base.SyncRoot)
                    {
                        if (!this._removeCalled)
                        {
                            return;
                        }
                    }
                    this.RemoveComplete.WaitOne();
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    this._tracer.WriteMessage("PSJobProxy", "RemoveJob", this._remoteJobInstanceId, this, "Error", null);
                    this._tracer.TraceException(exception);
                    throw;
                }
            }
        }

        public void RemoveJobAsync(bool removeRemoteJob)
        {
            this.RemoveJobAsync(removeRemoteJob, false);
        }

        public void RemoveJobAsync(bool removeRemoteJob, bool force)
        {
            if (!removeRemoteJob)
            {
                base.Dispose();
                this.OnRemoveJobCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
            else
            {
                AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(force);
                new JobActionWorkerDelegate(this.JobActionWorker).BeginInvoke(asyncOp, ActionType.Remove, null, null);
            }
        }

        public override void ResumeJob()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Resume);
                }
                else
                {
                    this.DoResumeAsync();
                }
                this.JobRunningOrFinished.WaitOne();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "ResumeJob", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                throw;
            }
        }

        public override void ResumeJobAsync()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Resume);
                }
                else
                {
                    this.DoResumeAsync();
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "ResumeJobAsync", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                this.OnResumeJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
            }
        }

        private bool ShouldQueueOperation()
        {
            lock (this._inProgressSyncObject)
            {
                if (!this._inProgress)
                {
                    this._inProgress = true;
                    return false;
                }
                return true;
            }
        }

        private void SortDebug(DebugRecord record)
        {
            Guid guid;
            string str;
            string message = RemoveIdentifierInformation(record.Message, out guid, out str);
            if ((guid == Guid.Empty) || !this._childJobsMapping.ContainsKey(guid))
            {
                this.WriteDebug(message);
            }
            else
            {
                OriginInfo originInfo = new OriginInfo(str, Guid.Empty, ((PSChildJobProxy) this._childJobsMapping[guid]).InstanceId);
                ((PSChildJobProxy) this._childJobsMapping[guid]).Debug.Add(new RemotingDebugRecord(message, originInfo));
            }
        }

        private void SortError(ErrorRecord record)
        {
            Guid empty = Guid.Empty;
            string computerName = string.Empty;
            if (record.ErrorDetails != null)
            {
                record.ErrorDetails.RecommendedAction = RemoveIdentifierInformation(record.ErrorDetails.RecommendedAction, out empty, out computerName);
            }
            if ((empty == Guid.Empty) || !this._childJobsMapping.ContainsKey(empty))
            {
                this.WriteError(record);
            }
            else
            {
                OriginInfo originInfo = new OriginInfo(null, Guid.Empty, ((PSChildJobProxy) this._childJobsMapping[empty]).InstanceId);
                ((PSChildJobProxy) this._childJobsMapping[empty]).WriteError(new RemotingErrorRecord(record, originInfo));
            }
        }

        private void SortOutputObject(PSObject newObject)
        {
            PSPropertyInfo info = newObject.Properties[RemotingConstants.SourceJobInstanceId];
            Guid key = (info != null) ? ((Guid) info.Value) : Guid.Empty;
            if (((info == null) || (key == Guid.Empty)) || !this._childJobsMapping.ContainsKey(key))
            {
                base.Output.Add(newObject);
            }
            else
            {
                newObject.Properties.Remove(RemotingConstants.SourceJobInstanceId);
                newObject.Properties.Add(new PSNoteProperty(RemotingConstants.SourceJobInstanceId, ((PSChildJobProxy) this._childJobsMapping[key]).InstanceId));
                ((PSChildJobProxy) this._childJobsMapping[key]).Output.Add(newObject);
            }
        }

        private void SortProgress(ProgressRecord newRecord)
        {
            Guid guid;
            string str;
            newRecord.CurrentOperation = RemoveIdentifierInformation(newRecord.CurrentOperation, out guid, out str);
            if ((guid == Guid.Empty) || !this._childJobsMapping.ContainsKey(guid))
            {
                this.WriteProgress(newRecord);
            }
            else
            {
                OriginInfo originInfo = new OriginInfo(str, Guid.Empty, ((PSChildJobProxy) this._childJobsMapping[guid]).InstanceId);
                ((PSChildJobProxy) this._childJobsMapping[guid]).WriteProgress(new RemotingProgressRecord(newRecord, originInfo));
            }
        }

        private void SortVerbose(VerboseRecord record)
        {
            Guid guid;
            string str;
            string message = RemoveIdentifierInformation(record.Message, out guid, out str);
            if ((guid == Guid.Empty) || !this._childJobsMapping.ContainsKey(guid))
            {
                this.WriteVerbose(message);
            }
            else
            {
                OriginInfo originInfo = new OriginInfo(str, Guid.Empty, ((PSChildJobProxy) this._childJobsMapping[guid]).InstanceId);
                ((PSChildJobProxy) this._childJobsMapping[guid]).Verbose.Add(new RemotingVerboseRecord(message, originInfo));
            }
        }

        private void SortWarning(WarningRecord record)
        {
            Guid guid;
            string str;
            string message = RemoveIdentifierInformation(record.Message, out guid, out str);
            if ((guid == Guid.Empty) || !this._childJobsMapping.ContainsKey(guid))
            {
                this.WriteWarning(message);
            }
            else
            {
                OriginInfo originInfo = new OriginInfo(str, Guid.Empty, ((PSChildJobProxy) this._childJobsMapping[guid]).InstanceId);
                ((PSChildJobProxy) this._childJobsMapping[guid]).Warning.Add(new RemotingWarningRecord(message, originInfo));
            }
        }

        public override void StartJob()
        {
            this.StartJob(null, null, null);
        }

        public void StartJob(PSDataCollection<object> input)
        {
            this.StartJob(null, null, input);
        }

        public void StartJob(EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, PSDataCollection<object> input)
        {
            try
            {
                this.DoStartAsync(dataAdded, stateChanged, input);
                this._jobInitialiedWaitHandle.WaitOne();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "StartJob", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                throw;
            }
        }

        public override void StartJobAsync()
        {
            this.StartJobAsync(null, null, null);
        }

        public void StartJobAsync(PSDataCollection<object> input)
        {
            this.StartJobAsync(null, null, input);
        }

        public void StartJobAsync(EventHandler<JobDataAddedEventArgs> dataAdded, EventHandler<JobStateEventArgs> stateChanged, PSDataCollection<object> input)
        {
            try
            {
                this.DoStartAsync(dataAdded, stateChanged, input);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "StartJobAsync", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                this.OnStartJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
            }
        }

        public override void StopJob()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Stop);
                }
                else
                {
                    this.DoStopAsync();
                }
                base.Finished.WaitOne();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "StopJob", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                throw;
            }
        }

        public override void StopJob(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyJobControlNotSupported", new object[0]);
        }

        public override void StopJobAsync()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Stop);
                }
                else
                {
                    this.DoStopAsync();
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "StopJobAsync", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                this.OnStopJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
            }
        }

        public override void StopJobAsync(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyJobControlNotSupported", new object[0]);
        }

        public override void SuspendJob()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Suspend);
                }
                else
                {
                    this.DoSuspendAsync();
                }
                this.JobSuspendedOrFinished.WaitOne();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "SuspendJob", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                throw;
            }
        }

        public override void SuspendJob(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyJobControlNotSupported", new object[0]);
        }

        public override void SuspendJobAsync()
        {
            try
            {
                if (this.ShouldQueueOperation())
                {
                    this._pendingOperations.Enqueue(QueueOperation.Suspend);
                }
                else
                {
                    this.DoSuspendAsync();
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                this._tracer.WriteMessage("PSJobProxy", "SuspendJobAsync", this._remoteJobInstanceId, this, "Error", null);
                this._tracer.TraceException(exception);
                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(exception, false, null));
            }
        }

        public override void SuspendJobAsync(bool force, string reason)
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyJobControlNotSupported", new object[0]);
        }

        internal static bool TryGetJobPropertyValue<T>(PSObject o, string propertyName, out T propertyValue)
        {
            propertyValue = default(T);
            PSPropertyInfo info = o.Properties[propertyName];
            if ((info == null) || !(info.Value is T))
            {
                return false;
            }
            propertyValue = (T) info.Value;
            return true;
        }

        public override void UnblockJob()
        {
            throw PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyUnblockJobNotSupported", new object[0]);
        }

        public override void UnblockJobAsync()
        {
            this.OnUnblockJobCompleted(new AsyncCompletedEventArgs(PSTraceSource.NewNotSupportedException("PowerShellStrings", "ProxyUnblockJobNotSupported", new object[0]), false, null));
        }

        private void UnregisterChildEvents()
        {
            lock (base.SyncRoot)
            {
                if (this._childEventsRegistered)
                {
                    if (this._dataAddedHandler != null)
                    {
                        foreach (PSChildJobProxy proxy in base.ChildJobs)
                        {
                            proxy.JobDataAdded -= this._dataAddedHandler;
                        }
                    }
                    if (this._stateChangedHandler != null)
                    {
                        foreach (PSChildJobProxy proxy2 in base.ChildJobs)
                        {
                            proxy2.StateChanged -= this._stateChangedHandler;
                        }
                    }
                    foreach (Job job in base.ChildJobs)
                    {
                        job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleChildProxyJobStateChanged);
                    }
                    this._childEventsRegistered = false;
                }
            }
        }

        private void ValidateAndDoSetJobState(JobState state, Exception reason = null)
        {
            if (((this._previousState != JobState.Stopping) && (this._previousState != JobState.Suspending)) || (state != JobState.Running))
            {
                this.DoSetJobState(state, reason);
            }
        }

        public override bool HasMoreData
        {
            get
            {
                if (this._moreData && base.IsFinishedState(base.JobStateInfo.State))
                {
                    bool flag = base.ChildJobs.Any<Job>(t => t.HasMoreData);
                    bool flag2 = (((CollectionHasMoreData<PSObject>(base.Output) || CollectionHasMoreData<ErrorRecord>(base.Error)) || (CollectionHasMoreData<VerboseRecord>(base.Verbose) || CollectionHasMoreData<DebugRecord>(base.Debug))) || CollectionHasMoreData<WarningRecord>(base.Warning)) || CollectionHasMoreData<ProgressRecord>(base.Progress);
                    this._moreData = flag2 || flag;
                }
                return this._moreData;
            }
        }

        private ManualResetEvent JobRunningOrFinished
        {
            get
            {
                if (this._jobRunningOrFinished == null)
                {
                    lock (base.SyncRoot)
                    {
                        if (this._jobRunningOrFinished == null)
                        {
                            this.AssertNotDisposed();
                            this._jobRunningOrFinished = new ManualResetEvent(false);
                        }
                    }
                }
                return this._jobRunningOrFinished;
            }
        }

        private ManualResetEvent JobSuspendedOrFinished
        {
            get
            {
                if (this._jobSuspendedOrFinished == null)
                {
                    lock (base.SyncRoot)
                    {
                        if (this._jobSuspendedOrFinished == null)
                        {
                            this.AssertNotDisposed();
                            this._jobSuspendedOrFinished = new ManualResetEvent(false);
                        }
                    }
                }
                return this._jobSuspendedOrFinished;
            }
        }

        public override string Location
        {
            get
            {
                return this._remoteJobLocation;
            }
        }

        public Guid RemoteJobInstanceId
        {
            get
            {
                return this._remoteJobInstanceId;
            }
        }

        private ManualResetEvent RemoveComplete
        {
            get
            {
                if (this._removeComplete == null)
                {
                    lock (base.SyncRoot)
                    {
                        if (this._removeComplete == null)
                        {
                            this.AssertNotDisposed();
                            this._removeComplete = new ManualResetEvent(false);
                        }
                    }
                }
                return this._removeComplete;
            }
        }

        public bool RemoveRemoteJobOnCompletion
        {
            get
            {
                return this._removeRemoteJobOnCompletion;
            }
            set
            {
                this.AssertChangesCanBeAccepted();
                this._removeRemoteJobOnCompletion = value;
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this._runspace;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                lock (base.SyncRoot)
                {
                    this.AssertChangesCanBeAccepted();
                    this._runspacePool = null;
                    this._runspace = value;
                }
            }
        }

        public System.Management.Automation.Runspaces.RunspacePool RunspacePool
        {
            get
            {
                return this._runspacePool;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                lock (base.SyncRoot)
                {
                    this.AssertChangesCanBeAccepted();
                    this._runspace = null;
                    this._runspacePool = value;
                }
            }
        }

        public override string StatusMessage
        {
            get
            {
                return this._remoteJobStatusMessage;
            }
        }

        private enum ActionType
        {
            Remove
        }

        private class AsyncCompleteContainer
        {
            internal PSJobProxy.ActionType Action;
            internal AsyncCompletedEventArgs EventArgs;
        }

        private delegate void JobActionWorkerDelegate(AsyncOperation asyncOp, PSJobProxy.ActionType action);

        private enum QueueOperation
        {
            Stop,
            Suspend,
            Resume
        }
    }
}

