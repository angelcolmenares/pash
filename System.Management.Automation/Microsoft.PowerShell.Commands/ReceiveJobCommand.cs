namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Cmdlet("Receive", "Job", DefaultParameterSetName="Location", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113372", RemotingCapability=RemotingCapability.SupportedByCommand)]
    public class ReceiveJobCommand : JobCmdletBase, IDisposable
    {
        private bool _autoRemoveJob;
        private readonly Dictionary<Guid, bool> _eventArgsWritten = new Dictionary<Guid, bool>();
        private bool _holdingResultsRef;
        private bool _isDisposed;
        private bool _isStopping;
        private readonly List<System.Management.Automation.Job> _jobsBeingAggregated = new List<System.Management.Automation.Job>();
        private readonly List<Guid> _jobsSpecifiedInParameters = new List<Guid>();
        private bool _outputJobFirst;
        private readonly PSDataCollection<PSStreamObject> _results = new PSDataCollection<PSStreamObject>();
        private readonly ReaderWriterLockSlim _resultsReaderWriterLock = new ReaderWriterLockSlim();
        private readonly object _syncObject = new object();
        private readonly PowerShellTraceSource _tracer = PowerShellTraceSourceFactory.GetTraceSource();
        private bool _wait;
        private readonly ManualResetEvent _writeExistingData = new ManualResetEvent(true);
        private bool _writeStateChangedEvents;
        private const string ClassNameTrace = "ReceiveJobCommand";
        private string[] computerNames;
        private bool flush = true;
        private System.Management.Automation.Job[] jobs;
        protected const string LocationParameterSet = "Location";
        private string[] locations;
        private bool recurse = true;
        private PSSession[] remoteRunspaceInfos;

        private void AddRemoveErrorToResults(System.Management.Automation.Job job, Exception ex)
        {
            ArgumentException exception = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CannotRemoveJob, new object[0]), ex);
            ErrorRecord record = new ErrorRecord(exception, "ReceiveJobAutoRemovalError", ErrorCategory.InvalidOperation, job);
            this._results.Add(new PSStreamObject(PSStreamObjectType.Error, record));
        }

        private void AggregateResultsFromJob(System.Management.Automation.Job job)
        {
            if (((this.Force != false) || !job.IsPersistentState(job.JobStateInfo.State)) && ((this.Force == false) || !job.IsFinishedState(job.JobStateInfo.State)))
            {
                job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
                if (((this.Force == false) && job.IsPersistentState(job.JobStateInfo.State)) || ((this.Force != false) && job.IsFinishedState(job.JobStateInfo.State)))
                {
                    job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
                }
                else
                {
                    this._tracer.WriteMessage("ReceiveJobCommand", "AggregateResultsFromJob", Guid.Empty, job, "BEGIN Adding job for aggregation", null);
                    this._jobsBeingAggregated.Add(job);
                    if (job.UsesResultsCollection)
                    {
                        job.Results.SourceId = job.InstanceId;
                        job.Results.DataAdded += new EventHandler<DataAddedEventArgs>(this.ResultsAdded);
                    }
                    else
                    {
                        job.Output.SourceId = job.InstanceId;
                        job.Error.SourceId = job.InstanceId;
                        job.Progress.SourceId = job.InstanceId;
                        job.Verbose.SourceId = job.InstanceId;
                        job.Warning.SourceId = job.InstanceId;
                        job.Debug.SourceId = job.InstanceId;
                        job.Output.DataAdded += new EventHandler<DataAddedEventArgs>(this.Output_DataAdded);
                        job.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
                        job.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.Progress_DataAdded);
                        job.Verbose.DataAdded += new EventHandler<DataAddedEventArgs>(this.Verbose_DataAdded);
                        job.Warning.DataAdded += new EventHandler<DataAddedEventArgs>(this.Warning_DataAdded);
                        job.Debug.DataAdded += new EventHandler<DataAddedEventArgs>(this.Debug_DataAdded);
                    }
                    this._tracer.WriteMessage("ReceiveJobCommand", "AggregateResultsFromJob", Guid.Empty, job, "END Adding job for aggregation", null);
                }
            }
        }

        private void AutoRemoveJobIfRequired(System.Management.Automation.Job job)
        {
            if ((this._autoRemoveJob && this._jobsSpecifiedInParameters.Contains(job.InstanceId)) && job.IsFinishedState(job.JobStateInfo.State))
            {
                if (job.HasMoreData)
                {
                    this._tracer.WriteMessage("ReceiveJobCommand", "AutoRemoveJobIfRequired", Guid.Empty, job, "Job has data and is being removed.", new string[0]);
                }
                Job2 job2 = job as Job2;
                if (job2 != null)
                {
                    try
                    {
                        base.JobManager.RemoveJob(job2, this, false, true);
                        job.Dispose();
                    }
                    catch (Exception exception)
                    {
                        this.AddRemoveErrorToResults(job2, exception);
                    }
                }
                else
                {
                    try
                    {
                        base.JobRepository.Remove(job);
                        job.Dispose();
                    }
                    catch (ArgumentException exception2)
                    {
                        this.AddRemoveErrorToResults(job, exception2);
                    }
                }
            }
        }

        protected override void BeginProcessing()
        {
            this.ValidateAutoRemove();
            this.ValidateWriteJobInResults();
            this.ValidateWriteEvents();
            this.ValidateForce();
        }

        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<DebugRecord> collection = sender as PSDataCollection<DebugRecord>;
                    DebugRecord data = this.GetData<DebugRecord>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Debug, data.Message, Guid.Empty));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && !this._isDisposed)
            {
                lock (this._syncObject)
                {
                    if (this._isDisposed)
                    {
                        return;
                    }
                    this._isDisposed = true;
                }
                if (this._jobsBeingAggregated != null)
                {
                    foreach (System.Management.Automation.Job job in this._jobsBeingAggregated)
                    {
                        if (job.UsesResultsCollection)
                        {
                            job.Results.DataAdded -= new EventHandler<DataAddedEventArgs>(this.ResultsAdded);
                        }
                        else
                        {
                            job.Output.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Output_DataAdded);
                            job.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
                            job.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Progress_DataAdded);
                            job.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Verbose_DataAdded);
                            job.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Warning_DataAdded);
                            job.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Debug_DataAdded);
                        }
                        job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
                    }
                }
                this._resultsReaderWriterLock.EnterWriteLock();
                try
                {
                    this._results.Complete();
                }
                finally
                {
                    this._resultsReaderWriterLock.ExitWriteLock();
                }
                this._resultsReaderWriterLock.Dispose();
                this._results.Clear();
                this._results.Dispose();
                this._writeExistingData.Set();
                this._writeExistingData.Close();
            }
        }

        private static void DoUnblockJob(System.Management.Automation.Job job)
        {
            if (job.ChildJobs.Count == 0)
            {
                PSRemotingChildJob job2 = job as PSRemotingChildJob;
                if (job2 != null)
                {
                    job2.UnblockJob();
                }
                else
                {
                    job.SetJobState(JobState.Running, null);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (!this._wait)
            {
                foreach (PSStreamObject obj3 in this._results)
                {
                    if (this._isStopping)
                    {
                        break;
                    }
                    obj3.WriteStreamObject(this, false, true);
                }
            }
            else
            {
                foreach (PSStreamObject obj2 in this._results)
                {
                    if (this._isStopping)
                    {
                        break;
                    }
                    obj2.WriteStreamObject(this, true, true);
                }
                this._eventArgsWritten.Clear();
            }
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<ErrorRecord> collection = sender as PSDataCollection<ErrorRecord>;
                    ErrorRecord data = this.GetData<ErrorRecord>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Error, data, Guid.Empty));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        private T GetData<T>(PSDataCollection<T> collection, int index)
        {
            if (!this.flush)
            {
                return collection[index];
            }
            Collection<T> collection2 = collection.ReadAndRemove(1);
            if (collection2.Count > 0)
            {
                return collection2[0];
            }
            return default(T);
        }

        private void HandleJobStateChanged(object sender, JobStateEventArgs e)
        {
            System.Management.Automation.Job job = sender as System.Management.Automation.Job;
            this._tracer.WriteMessage("ReceiveJobCommand", "HandleJobStateChanged", Guid.Empty, job, "BEGIN wait for write existing data", null);
            if (e.JobStateInfo.State != JobState.Running)
            {
                this._writeExistingData.WaitOne();
            }
            this._tracer.WriteMessage("ReceiveJobCommand", "HandleJobStateChanged", Guid.Empty, job, "END wait for write existing data", null);
            lock (this._syncObject)
            {
                if (!this._jobsBeingAggregated.Contains(job))
                {
                    this._tracer.WriteMessage("ReceiveJobCommand", "HandleJobStateChanged", Guid.Empty, job, "Returning because job is not in _jobsBeingAggregated", null);
                    return;
                }
            }
            if (e.JobStateInfo.State == JobState.Blocked)
            {
                DoUnblockJob(job);
            }
            if (((this.Force == 0) && job.IsPersistentState(e.JobStateInfo.State)) || ((this.Force != 0) && job.IsFinishedState(e.JobStateInfo.State)))
            {
                this.WriteReasonError(job);
                this.WriteJobStateInformationIfRequired(job, e);
                this.StopAggregateResultsFromJob(job);
            }
            else
            {
                this._tracer.WriteMessage("ReceiveJobCommand", "HandleJobStateChanged", Guid.Empty, job, "Returning because job state does not meet wait requirements (continue aggregating)", new string[0]);
            }
        }

        private void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<PSObject> collection = sender as PSDataCollection<PSObject>;
                    PSObject data = this.GetData<PSObject>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Output, data, Guid.Empty));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        protected override void ProcessRecord()
        {
            bool checkForRecurse = false;
            List<System.Management.Automation.Job> jobsToWrite = new List<System.Management.Automation.Job>();
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "Session"))
                {
                    if (parameterSetName == "ComputerName")
                    {
                        foreach (System.Management.Automation.Job job3 in this.jobs)
                        {
                            PSRemotingJob job4 = job3 as PSRemotingJob;
                            if (job4 == null)
                            {
                                string message = base.GetMessage(RemotingErrorIdStrings.ComputerNameParamNotSupported);
                                base.WriteError(new ErrorRecord(new ArgumentException(message), "ComputerNameParameterNotSupported", ErrorCategory.InvalidArgument, job3));
                            }
                            else
                            {
                                string[] resolvedComputerNames = null;
                                base.ResolveComputerNames(this.computerNames, out resolvedComputerNames);
                                foreach (string str3 in resolvedComputerNames)
                                {
                                    List<System.Management.Automation.Job> jobsForComputer = job4.GetJobsForComputer(str3);
                                    jobsToWrite.AddRange(jobsForComputer);
                                }
                            }
                        }
                    }
                    else if (parameterSetName == "Location")
                    {
                        if (this.locations == null)
                        {
                            jobsToWrite.AddRange(this.jobs);
                            checkForRecurse = true;
                        }
                        else
                        {
                            foreach (System.Management.Automation.Job job5 in this.jobs)
                            {
                                foreach (string str4 in this.locations)
                                {
                                    List<System.Management.Automation.Job> jobsForLocation = job5.GetJobsForLocation(str4);
                                    jobsToWrite.AddRange(jobsForLocation);
                                }
                            }
                        }
                    }
                    else if (parameterSetName == "InstanceIdParameterSet")
                    {
                        List<System.Management.Automation.Job> collection = base.FindJobsMatchingByInstanceId(true, false, true, false);
                        jobsToWrite.AddRange(collection);
                        checkForRecurse = true;
                    }
                    else if (parameterSetName == "SessionIdParameterSet")
                    {
                        List<System.Management.Automation.Job> list6 = base.FindJobsMatchingBySessionId(true, false, true, false);
                        jobsToWrite.AddRange(list6);
                        checkForRecurse = true;
                    }
                    else if (parameterSetName == "NameParameterSet")
                    {
                        List<System.Management.Automation.Job> list7 = base.FindJobsMatchingByName(true, false, true, false);
                        jobsToWrite.AddRange(list7);
                        checkForRecurse = true;
                    }
                }
                else
                {
                    foreach (System.Management.Automation.Job job in this.jobs)
                    {
                        PSRemotingJob job2 = job as PSRemotingJob;
                        if (job2 == null)
                        {
                            string str = base.GetMessage(RemotingErrorIdStrings.RunspaceParamNotSupported);
                            base.WriteError(new ErrorRecord(new ArgumentException(str), "RunspaceParameterNotSupported", ErrorCategory.InvalidArgument, job));
                        }
                        else
                        {
                            foreach (PSSession session in this.remoteRunspaceInfos)
                            {
                                List<System.Management.Automation.Job> jobsForRunspace = job2.GetJobsForRunspace(session);
                                jobsToWrite.AddRange(jobsForRunspace);
                            }
                        }
                    }
                }
            }
            if (this._wait)
            {
                this._writeExistingData.Reset();
                this.WriteJobsIfRequired(jobsToWrite);
                foreach (System.Management.Automation.Job job6 in jobsToWrite)
                {
                    this._jobsSpecifiedInParameters.Add(job6.InstanceId);
                }
                lock (this._syncObject)
                {
                    if (this._isDisposed || this._isStopping)
                    {
                        return;
                    }
                    if (!this._holdingResultsRef)
                    {
                        this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, (Job)null, "Adding Ref to results collection", null);
                        this._results.AddRef();
                        this._holdingResultsRef = true;
                    }
                }
                this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, null, "BEGIN Register for jobs", new string[0]);
                this.WriteResultsForJobsInCollection(jobsToWrite, checkForRecurse, true);
                this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, null, "END Register for jobs", new string[0]);
                lock (this._syncObject)
                {
                    if ((this._jobsBeingAggregated.Count == 0) && this._holdingResultsRef)
                    {
                        this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, (Job)null, "Removing Ref to results collection", null);
                        this._results.DecrementRef();
                        this._holdingResultsRef = false;
                    }
                }
                this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, null, "BEGIN Write existing job data", new string[0]);
                this.WriteResultsForJobsInCollection(jobsToWrite, checkForRecurse, false);
                this._tracer.WriteMessage("ReceiveJobCommand", "ProcessRecord", Guid.Empty, null, "END Write existing job data", new string[0]);
                this._writeExistingData.Set();
            }
            else
            {
                this.WriteResultsForJobsInCollection(jobsToWrite, checkForRecurse, false);
            }
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<ProgressRecord> collection = sender as PSDataCollection<ProgressRecord>;
                    ProgressRecord data = this.GetData<ProgressRecord>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Progress, data, collection.SourceId));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        private Collection<T> ReadAll<T>(PSDataCollection<T> psDataCollection)
        {
            if (this.flush)
            {
                return psDataCollection.ReadAll();
            }
            T[] array = new T[psDataCollection.Count];
            psDataCollection.CopyTo(array, 0);
            Collection<T> collection = new Collection<T>();
            foreach (T local in array)
            {
                collection.Add(local);
            }
            return collection;
        }

        private void ResultsAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            PSDataCollection<PSStreamObject> collection = sender as PSDataCollection<PSStreamObject>;
            PSStreamObject data = this.GetData<PSStreamObject>(collection, e.Index);
            if (data != null)
            {
                data.Id = collection.SourceId;
                this._results.Add(data);
            }
        }

        private void StopAggregateResultsFromJob(System.Management.Automation.Job job)
        {
            lock (this._syncObject)
            {
                this._tracer.WriteMessage("ReceiveJobCommand", "StopAggregateResultsFromJob", Guid.Empty, job, "Removing job from aggregation", null);
                this._jobsBeingAggregated.Remove(job);
                if ((this._jobsBeingAggregated.Count == 0) && this._holdingResultsRef)
                {
                    this._tracer.WriteMessage("ReceiveJobCommand", "StopAggregateResultsFromJob", Guid.Empty, (Job)null, "Removing Ref to results collection", (string)null);
                    this._results.DecrementRef();
                    this._holdingResultsRef = false;
                }
            }
            if (job.UsesResultsCollection)
            {
                job.Results.DataAdded -= new EventHandler<DataAddedEventArgs>(this.ResultsAdded);
            }
            else
            {
                job.Output.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Output_DataAdded);
                job.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
                job.Progress.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Progress_DataAdded);
                job.Verbose.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Verbose_DataAdded);
                job.Warning.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Warning_DataAdded);
                job.Debug.DataAdded -= new EventHandler<DataAddedEventArgs>(this.Debug_DataAdded);
            }
            job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
        }

        protected override void StopProcessing()
        {
            this._tracer.WriteMessage("ReceiveJobCommand", "StopProcessing", Guid.Empty, (Job)null, "Entered Stop Processing", null);
            lock (this._syncObject)
            {
                this._isStopping = true;
            }
            this._writeExistingData.Set();
            System.Management.Automation.Job[] jobArray = new System.Management.Automation.Job[this._jobsBeingAggregated.Count];
            for (int i = 0; i < this._jobsBeingAggregated.Count; i++)
            {
                jobArray[i] = this._jobsBeingAggregated[i];
            }
            foreach (System.Management.Automation.Job job in jobArray)
            {
                this.StopAggregateResultsFromJob(job);
            }
            this._resultsReaderWriterLock.EnterWriteLock();
            try
            {
                this._results.Complete();
            }
            finally
            {
                this._resultsReaderWriterLock.ExitWriteLock();
            }
            base.StopProcessing();
            this._tracer.WriteMessage("ReceiveJobCommand", "StopProcessing", Guid.Empty, (Job)null, "Exiting Stop Processing", (string)null);
        }

        private void ValidateAutoRemove()
        {
            if (this._autoRemoveJob && !this._wait)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "AutoRemoveCannotBeUsedWithoutWait", new object[0]);
            }
        }

        private void ValidateForce()
        {
            if ((this.Force != 0) && !this._wait)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "ForceCannotBeUsedWithoutWait", new object[0]);
            }
        }

        private void ValidateWait()
        {
            if (this._wait && !this.flush)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "BlockCannotBeUsedWithKeep", new object[0]);
            }
        }

        private void ValidateWriteEvents()
        {
            if (this._writeStateChangedEvents && !this._wait)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "WriteEventsCannotBeUsedWithoutWait", new object[0]);
            }
        }

        private void ValidateWriteJobInResults()
        {
            if (this._outputJobFirst && !this._wait)
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "WriteJobInResultsCannotBeUsedWithoutWait", new object[0]);
            }
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<VerboseRecord> collection = sender as PSDataCollection<VerboseRecord>;
                    VerboseRecord data = this.GetData<VerboseRecord>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Verbose, data.Message, Guid.Empty));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            lock (this._syncObject)
            {
                if (this._isDisposed)
                {
                    return;
                }
            }
            this._writeExistingData.WaitOne();
            this._resultsReaderWriterLock.EnterReadLock();
            try
            {
                if (this._results.IsOpen)
                {
                    PSDataCollection<WarningRecord> collection = sender as PSDataCollection<WarningRecord>;
                    WarningRecord data = this.GetData<WarningRecord>(collection, e.Index);
                    if (data != null)
                    {
                        this._results.Add(new PSStreamObject(PSStreamObjectType.Warning, data.Message, Guid.Empty));
                    }
                }
            }
            finally
            {
                this._resultsReaderWriterLock.ExitReadLock();
            }
        }

        private void WriteJobResults(System.Management.Automation.Job job)
        {
            if (job != null)
            {
                if (job.JobStateInfo.State == JobState.Disconnected)
                {
                    PSRemotingChildJob job2 = job as PSRemotingChildJob;
                    if ((job2 != null) && job2.DisconnectedAndBlocked)
                    {
                        return;
                    }
                }
                if (job.JobStateInfo.State == JobState.Blocked)
                {
                    DoUnblockJob(job);
                }
                if (!(job is Job2) && job.UsesResultsCollection)
                {
                    Collection<PSStreamObject> collection = this.ReadAll<PSStreamObject>(job.Results);
                    if (this._wait)
                    {
                        foreach (PSStreamObject obj2 in collection)
                        {
                            obj2.WriteStreamObject(this, job.Results.SourceId, false);
                        }
                    }
                    else
                    {
                        foreach (PSStreamObject obj3 in collection)
                        {
                            obj3.WriteStreamObject(this, false);
                        }
                    }
                }
                else
                {
                    foreach (PSObject obj4 in this.ReadAll<PSObject>(job.Output))
                    {
                        if (obj4 != null)
                        {
                            base.WriteObject(obj4);
                        }
                    }
                    foreach (ErrorRecord record in this.ReadAll<ErrorRecord>(job.Error))
                    {
                        if (record != null)
                        {
                            MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
                            if (commandRuntime != null)
                            {
                                commandRuntime.WriteError(record, true);
                            }
                        }
                    }
                    foreach (VerboseRecord record2 in this.ReadAll<VerboseRecord>(job.Verbose))
                    {
                        if (record2 != null)
                        {
                            MshCommandRuntime runtime2 = base.CommandRuntime as MshCommandRuntime;
                            if (runtime2 != null)
                            {
                                runtime2.WriteVerbose(record2, true);
                            }
                        }
                    }
                    foreach (DebugRecord record3 in this.ReadAll<DebugRecord>(job.Debug))
                    {
                        if (record3 != null)
                        {
                            MshCommandRuntime runtime3 = base.CommandRuntime as MshCommandRuntime;
                            if (runtime3 != null)
                            {
                                runtime3.WriteDebug(record3, true);
                            }
                        }
                    }
                    foreach (WarningRecord record4 in this.ReadAll<WarningRecord>(job.Warning))
                    {
                        if (record4 != null)
                        {
                            MshCommandRuntime runtime4 = base.CommandRuntime as MshCommandRuntime;
                            if (runtime4 != null)
                            {
                                runtime4.WriteWarning(record4, true);
                            }
                        }
                    }
                    foreach (ProgressRecord record5 in this.ReadAll<ProgressRecord>(job.Progress))
                    {
                        if (record5 != null)
                        {
                            MshCommandRuntime runtime5 = base.CommandRuntime as MshCommandRuntime;
                            if (runtime5 != null)
                            {
                                runtime5.WriteProgress(record5, true);
                            }
                        }
                    }
                }
                if (job.JobStateInfo.State == JobState.Failed)
                {
                    this.WriteReasonError(job);
                }
            }
        }

        private void WriteJobResultsRecursively(System.Management.Automation.Job job, bool registerInsteadOfWrite)
        {
            Hashtable duplicate = new Hashtable();
            this.WriteJobResultsRecursivelyHelper(duplicate, job, registerInsteadOfWrite);
            duplicate.Clear();
        }

        private void WriteJobResultsRecursivelyHelper(Hashtable duplicate, System.Management.Automation.Job job, bool registerInsteadOfWrite)
        {
            if (!duplicate.ContainsKey(job))
            {
                duplicate.Add(job, job);
                foreach (System.Management.Automation.Job job2 in job.ChildJobs)
                {
                    this.WriteJobResultsRecursivelyHelper(duplicate, job2, registerInsteadOfWrite);
                }
                if (registerInsteadOfWrite)
                {
                    this._eventArgsWritten[job.InstanceId] = false;
                    this.AggregateResultsFromJob(job);
                }
                else
                {
                    this.WriteJobResults(job);
                    this.WriteJobStateInformationIfRequired(job, null);
                }
            }
        }

        private void WriteJobsIfRequired(IEnumerable<System.Management.Automation.Job> jobsToWrite)
        {
            if (this._outputJobFirst)
            {
                foreach (System.Management.Automation.Job job in jobsToWrite)
                {
                    this._tracer.WriteMessage("ReceiveJobCommand", "WriteJobsIfRequired", Guid.Empty, job, "Writing job object as output", null);
                    base.WriteObject(job);
                }
            }
        }

        private void WriteJobStateInformation(System.Management.Automation.Job job, JobStateEventArgs args = null)
        {
            bool flag;
            this._eventArgsWritten.TryGetValue(job.InstanceId, out flag);
            if (flag)
            {
                this._tracer.WriteMessage("ReceiveJobCommand", "WriteJobStateInformation", Guid.Empty, job, "State information already written, skipping another write", null);
            }
            else
            {
                JobStateEventArgs args2 = args ?? new JobStateEventArgs(job.JobStateInfo);
                this._eventArgsWritten[job.InstanceId] = true;
                this._tracer.WriteMessage("ReceiveJobCommand", "WriteJobStateInformation", Guid.Empty, job, "Writing job state changed event args", null);
                PSObject obj2 = new PSObject(args2);
                obj2.Properties.Add(new PSNoteProperty(RemotingConstants.EventObject, true));
                this._results.Add(new PSStreamObject(PSStreamObjectType.Output, obj2, job.InstanceId));
            }
        }

        private void WriteJobStateInformationIfRequired(System.Management.Automation.Job job, JobStateEventArgs args = null)
        {
            if (this._writeStateChangedEvents && job.IsPersistentState(job.JobStateInfo.State))
            {
                this.WriteJobStateInformation(job, args);
            }
            this.AutoRemoveJobIfRequired(job);
        }

        private void WriteReasonError(System.Management.Automation.Job job)
        {
            PSRemotingChildJob job2 = job as PSRemotingChildJob;
            if ((job2 != null) && (job2.FailureErrorRecord != null))
            {
                this._results.Add(new PSStreamObject(PSStreamObjectType.Error, job2.FailureErrorRecord, job2.InstanceId));
            }
            else if (job.JobStateInfo.Reason != null)
            {
                Exception reason = job.JobStateInfo.Reason;
                Exception exception = reason;
                JobFailedException exception3 = reason as JobFailedException;
                if (exception3 != null)
                {
                    exception = exception3.Reason;
                }
                ErrorRecord record = new ErrorRecord(exception, "JobStateFailed", ErrorCategory.InvalidResult, null);
                if ((exception3 != null) && (exception3.DisplayScriptPosition != null))
                {
                    if (record.InvocationInfo == null)
                    {
                        record.SetInvocationInfo(new InvocationInfo(null, null));
                    }
                    record.InvocationInfo.DisplayScriptPosition = exception3.DisplayScriptPosition;
                }
                this._results.Add(new PSStreamObject(PSStreamObjectType.Error, record, job.InstanceId));
            }
        }

        private void WriteResultsForJobsInCollection(List<System.Management.Automation.Job> jobs, bool checkForRecurse, bool registerInsteadOfWrite)
        {
            foreach (System.Management.Automation.Job job in jobs)
            {
                if (base.JobManager.IsJobFromAdapter(job.InstanceId, "PSWorkflowJob") && (job.JobStateInfo.State == JobState.Stopped))
                {
                    MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
                    if (commandRuntime != null)
                    {
                        commandRuntime.WriteWarning(new WarningRecord(StringUtil.Format(RemotingErrorIdStrings.JobWasStopped, job.Name)), true);
                    }
                }
                if (checkForRecurse && this.recurse)
                {
                    this.WriteJobResultsRecursively(job, registerInsteadOfWrite);
                }
                else if (registerInsteadOfWrite)
                {
                    this._eventArgsWritten[job.InstanceId] = false;
                    this.AggregateResultsFromJob(job);
                }
                else
                {
                    this.WriteJobResults(job);
                    this.WriteJobStateInformationIfRequired(job, null);
                }
            }
        }

        [Parameter]
        public SwitchParameter AutoRemoveJob
        {
            get
            {
                return this._autoRemoveJob;
            }
            set
            {
                this._autoRemoveJob = (bool) value;
            }
        }

        public override string[] Command
        {
            get
            {
                return null;
            }
        }

        [ValidateNotNullOrEmpty, Alias(new string[] { "Cn" }), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName", Position=1)]
        public string[] ComputerName
        {
            get
            {
                return this.computerNames;
            }
            set
            {
                this.computerNames = value;
            }
        }

        public override Hashtable Filter
        {
            get
            {
                return null;
            }
        }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Session"), Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Location"), Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public System.Management.Automation.Job[] Job
        {
            get
            {
                return this.jobs;
            }
            set
            {
                this.jobs = value;
            }
        }

        [Parameter]
        public SwitchParameter Keep
        {
            get
            {
                return !this.flush;
            }
            set
            {
                this.flush = value == 0;
                this.ValidateWait();
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="Location", Position=1)]
        public string[] Location
        {
            get
            {
                return this.locations;
            }
            set
            {
                this.locations = value;
            }
        }

        [Parameter]
        public SwitchParameter NoRecurse
        {
            get
            {
                return !this.recurse;
            }
            set
            {
                this.recurse = value == 0;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Session", Position=1), ValidateNotNull]
        public PSSession[] Session
        {
            get
            {
                return this.remoteRunspaceInfos;
            }
            set
            {
                this.remoteRunspaceInfos = value;
            }
        }

        public override JobState State
        {
            get
            {
                return JobState.NotStarted;
            }
        }

        [Parameter]
        public SwitchParameter Wait
        {
            get
            {
                return this._wait;
            }
            set
            {
                this._wait = (bool) value;
                this.ValidateWait();
            }
        }

        [Parameter]
        public SwitchParameter WriteEvents
        {
            get
            {
                return this._writeStateChangedEvents;
            }
            set
            {
                this._writeStateChangedEvents = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter WriteJobInResults
        {
            get
            {
                return this._outputJobFirst;
            }
            set
            {
                this._outputJobFirst = (bool) value;
            }
        }
    }
}

