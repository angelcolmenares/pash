namespace System.Management.Automation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Remoting;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ThrottlingJob : Job
    {
        private Queue<Action> _actionsForUnblockingChildAdditions;
        private bool _alreadyDisabledFlowControlForPendingCmdletActionsQueue;
        private bool _alreadyDisabledFlowControlForPendingJobsQueue;
        private bool _alreadyWroteFlowControlBuffersHighMemoryUsageWarning;
        private object _alreadyWroteFlowControlBuffersHighMemoryUsageWarningLock;
        private CancellationTokenSource _cancellationTokenSource;
        private HashSet<string> _childJobLocations;
        private readonly bool _cmdletMode;
        private int _countOfAllChildJobs;
        private int _countOfBlockedChildJobs;
        private int _countOfFailedChildJobs;
        private int _countOfStoppedChildJobs;
        private int _countOfSuccessfullyCompletedChildJobs;
        private int _extraCapacityForRunningAllJobs;
        private int _extraCapacityForRunningQueryJobs;
        private static long _flowControlBuffersHighMemoryUsageThreshold = 0x7530L;
        private bool _inBoostModeToPreventQueryJobDeadlock;
        private bool _isStopping;
        private long _jobResultsCurrentCount;
        private SemaphoreSlim _jobResultsThrottlingSemaphore;
        private readonly object _lockObject;
        private int _maximumConcurrentChildJobs;
        private int _maxReadyToRunJobs;
        private bool _ownerWontSubmitNewChildJobs;
        private readonly int _progressActivityId;
        private readonly object _progressLock;
        private DateTime _progressReportLastTime;
        private readonly DateTime _progressStartTime;
        private Queue<StartableJob> _readyToRunQueryJobs;
        private Queue<StartableJob> _readyToRunRegularJobs;
        private readonly HashSet<Guid> _setOfChildJobsThatCanAddMoreChildJobs;
        private static readonly int MaximumReadyToRunJobs = 0x2710;

        internal event EventHandler<ThrottlingJobChildAddedEventArgs> ChildJobAdded;

        internal ThrottlingJob(string command, string jobName, string jobTypeName, int maximumConcurrentChildJobs, bool cmdletMode) : base(command, jobName)
        {
            this._progressStartTime = DateTime.UtcNow;
            this._progressLock = new object();
            this._progressReportLastTime = DateTime.MinValue;
            this._setOfChildJobsThatCanAddMoreChildJobs = new HashSet<Guid>();
            this._lockObject = new object();
            this._alreadyWroteFlowControlBuffersHighMemoryUsageWarningLock = new object();
            this._cancellationTokenSource = new CancellationTokenSource();
            this._childJobLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            base.Results.BlockingEnumerator = true;
            this._cmdletMode = cmdletMode;
            base.PSJobTypeName = jobTypeName;
            if (this._cmdletMode)
            {
                this._jobResultsThrottlingSemaphore = new SemaphoreSlim(ForwardingHelper.AggregationQueueMaxCapacity);
            }
            this._progressActivityId = new Random(this.GetHashCode()).Next();
            this.SetupThrottlingQueue(maximumConcurrentChildJobs);
        }

        internal void AddChildJobAndPotentiallyBlock(StartableJob childJob, ChildJobFlags flags)
        {
            using (ManualResetEventSlim slim = new ManualResetEventSlim(false))
            {
                if (childJob == null)
                {
                    throw new ArgumentNullException("childJob");
                }
                this.AddChildJobWithoutBlocking(childJob, flags, new Action(slim.Set));
                slim.Wait();
            }
        }

        internal void AddChildJobAndPotentiallyBlock(Cmdlet cmdlet, StartableJob childJob, ChildJobFlags flags)
        {
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                if (childJob == null)
                {
                    throw new ArgumentNullException("childJob");
                }
                this.AddChildJobWithoutBlocking(childJob, flags, new Action(source.Cancel));
                this.ForwardAllResultsToCmdlet(cmdlet, new CancellationToken?(source.Token));
            }
        }

        internal void AddChildJobWithoutBlocking(StartableJob childJob, ChildJobFlags flags, Action jobEnqueuedAction = null)
        {
            if (childJob == null)
            {
                throw new ArgumentNullException("childJob");
            }
            if (childJob.JobStateInfo.State != JobState.NotStarted)
            {
                throw new ArgumentException(RemotingErrorIdStrings.ThrottlingJobChildAlreadyRunning, "childJob");
            }
            base.AssertNotDisposed();
            JobStateInfo info = null;
            lock (this._lockObject)
            {
                if (this.IsEndOfChildJobs)
                {
                    throw new InvalidOperationException(RemotingErrorIdStrings.ThrottlingJobChildAddedAfterEndOfChildJobs);
                }
                if (this._isStopping)
                {
                    return;
                }
                if (this._countOfAllChildJobs == 0)
                {
                    info = new JobStateInfo(JobState.Running);
                }
                if (ChildJobFlags.CreatesChildJobs == (ChildJobFlags.CreatesChildJobs & flags))
                {
                    this._setOfChildJobsThatCanAddMoreChildJobs.Add(childJob.InstanceId);
                }
                base.ChildJobs.Add(childJob);
                this._childJobLocations.Add(childJob.Location);
                this._countOfAllChildJobs++;
                this.WriteWarningAboutHighUsageOfFlowControlBuffers((long) this.CountOfRunningOrReadyToRunChildJobs);
                if (this.CountOfRunningOrReadyToRunChildJobs > this._maxReadyToRunJobs)
                {
                    this._actionsForUnblockingChildAdditions.Enqueue(jobEnqueuedAction);
                }
                else if (jobEnqueuedAction != null)
                {
                    jobEnqueuedAction();
                }
            }
            if (info != null)
            {
                base.SetJobState(info.State, info.Reason);
            }
            this.ChildJobAdded.SafeInvoke<ThrottlingJobChildAddedEventArgs>(this, new ThrottlingJobChildAddedEventArgs(childJob));
            childJob.SetParentActivityIdGetter(new Func<int>(this.GetProgressActivityId));
            childJob.StateChanged += new EventHandler<JobStateEventArgs>(this.childJob_StateChanged);
            if (this._cmdletMode)
            {
                childJob.Results.DataAdded += new EventHandler<DataAddedEventArgs>(this.childJob_ResultsAdded);
            }
            this.EnqueueReadyToRunChildJob(childJob);
            this.ReportProgress(true);
        }

        private void childJob_ResultsAdded(object sender, DataAddedEventArgs e)
        {
            try
            {
                long currentCount = Interlocked.Increment(ref this._jobResultsCurrentCount);
                this.WriteWarningAboutHighUsageOfFlowControlBuffers(currentCount);
                this._jobResultsThrottlingSemaphore.Wait(this._cancellationTokenSource.Token);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void childJob_StateChanged(object sender, JobStateEventArgs e)
        {
            Job completedChildJob = (Job) sender;
            if ((e.PreviousJobStateInfo.State == JobState.Blocked) && (e.JobStateInfo.State != JobState.Blocked))
            {
                bool flag = false;
                lock (this._lockObject)
                {
                    this._countOfBlockedChildJobs--;
                    if (this._countOfBlockedChildJobs == 0)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.SetJobState(JobState.Running);
                }
            }
            switch (e.JobStateInfo.State)
            {
                case JobState.Completed:
                case JobState.Failed:
                case JobState.Stopped:
                    completedChildJob.StateChanged -= new EventHandler<JobStateEventArgs>(this.childJob_StateChanged);
                    this.MakeRoomForRunningOtherJobs(completedChildJob);
                    lock (this._lockObject)
                    {
                        if (e.JobStateInfo.State == JobState.Failed)
                        {
                            this._countOfFailedChildJobs++;
                        }
                        else if (e.JobStateInfo.State == JobState.Stopped)
                        {
                            this._countOfStoppedChildJobs++;
                        }
                        else if (e.JobStateInfo.State == JobState.Completed)
                        {
                            this._countOfSuccessfullyCompletedChildJobs++;
                        }
                        if (this._actionsForUnblockingChildAdditions.Count > 0)
                        {
                            Action action = this._actionsForUnblockingChildAdditions.Dequeue();
                            if (action != null)
                            {
                                action();
                            }
                        }
                        if (this._cmdletMode)
                        {
                            foreach (PSStreamObject obj2 in completedChildJob.Results.ReadAll())
                            {
                                base.Results.Add(obj2);
                            }
                            base.ChildJobs.Remove(completedChildJob);
                            this._setOfChildJobsThatCanAddMoreChildJobs.Remove(completedChildJob.InstanceId);
                            completedChildJob.Dispose();
                        }
                    }
                    this.ReportProgress(!this.IsThrottlingJobCompleted);
                    break;

                case JobState.Blocked:
                    lock (this._lockObject)
                    {
                        this._countOfBlockedChildJobs++;
                    }
                    base.SetJobState(JobState.Blocked);
                    break;
            }
            this.FigureOutIfThrottlingJobIsCompleted();
        }

        internal void DisableFlowControlForPendingCmdletActionsQueue()
        {
            if (this._cmdletMode && !this._alreadyDisabledFlowControlForPendingCmdletActionsQueue)
            {
                this._alreadyDisabledFlowControlForPendingCmdletActionsQueue = true;
                long num = 0x3fffffffL - this._jobResultsThrottlingSemaphore.CurrentCount;
                if ((num > 0L) && (num < 0x7fffffffL))
                {
                    this._jobResultsThrottlingSemaphore.Release((int) num);
                }
            }
        }

        internal void DisableFlowControlForPendingJobsQueue()
        {
            if (this._cmdletMode && !this._alreadyDisabledFlowControlForPendingJobsQueue)
            {
                this._alreadyDisabledFlowControlForPendingJobsQueue = true;
                lock (this._lockObject)
                {
                    this._maxReadyToRunJobs = 0x7fffffff;
                    while (this._actionsForUnblockingChildAdditions.Count > 0)
                    {
                        Action action = this._actionsForUnblockingChildAdditions.Dequeue();
                        if (action != null)
                        {
                            action();
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    List<Job> list;
                    this.StopJob();
                    lock (this._lockObject)
                    {
                        list = new List<Job>(base.ChildJobs);
                        base.ChildJobs.Clear();
                    }
                    foreach (Job job in list)
                    {
                        job.Dispose();
                    }
                    if (this._jobResultsThrottlingSemaphore != null)
                    {
                        this._jobResultsThrottlingSemaphore.Dispose();
                    }
                    this._cancellationTokenSource.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void EndOfChildJobs()
        {
            base.AssertNotDisposed();
            lock (this._lockObject)
            {
                this._ownerWontSubmitNewChildJobs = true;
            }
            this.FigureOutIfThrottlingJobIsCompleted();
        }

        private void EnqueueReadyToRunChildJob(StartableJob childJob)
        {
            lock (this._lockObject)
            {
                bool flag = this._setOfChildJobsThatCanAddMoreChildJobs.Contains(childJob.InstanceId);
                if ((flag && !this._inBoostModeToPreventQueryJobDeadlock) && (this._maximumConcurrentChildJobs == 1))
                {
                    this._inBoostModeToPreventQueryJobDeadlock = true;
                    this._extraCapacityForRunningAllJobs++;
                }
                if (flag)
                {
                    this._readyToRunQueryJobs.Enqueue(childJob);
                }
                else
                {
                    this._readyToRunRegularJobs.Enqueue(childJob);
                }
            }
            this.StartChildJobIfPossible();
        }

        private void FigureOutIfThrottlingJobIsCompleted()
        {
            JobStateInfo info = null;
            lock (this._lockObject)
            {
                if (this.IsThrottlingJobCompleted && !base.IsFinishedState(base.JobStateInfo.State))
                {
                    if (this._isStopping)
                    {
                        info = new JobStateInfo(JobState.Stopped, null);
                    }
                    else if (this._countOfFailedChildJobs > 0)
                    {
                        info = new JobStateInfo(JobState.Failed, null);
                    }
                    else if (this._countOfStoppedChildJobs > 0)
                    {
                        info = new JobStateInfo(JobState.Stopped, null);
                    }
                    else
                    {
                        info = new JobStateInfo(JobState.Completed);
                    }
                }
            }
            if (info != null)
            {
                base.SetJobState(info.State, info.Reason);
                base.CloseAllStreams();
            }
        }

        internal override void ForwardAllResultsToCmdlet(Cmdlet cmdlet)
        {
            this.ForwardAllResultsToCmdlet(cmdlet, null);
        }

        private void ForwardAllResultsToCmdlet(Cmdlet cmdlet, CancellationToken? cancellationToken)
        {
            base.AssertNotDisposed();
            ForwardingHelper.ForwardAllResultsToCmdlet(this, cmdlet, cancellationToken);
        }

        internal override void ForwardAvailableResultsToCmdlet(Cmdlet cmdlet)
        {
            base.AssertNotDisposed();
            base.ForwardAvailableResultsToCmdlet(cmdlet);
            foreach (Job job in this.GetChildJobsSnapshot())
            {
                job.ForwardAvailableResultsToCmdlet(cmdlet);
            }
        }

        private List<Job> GetChildJobsSnapshot()
        {
            lock (this._lockObject)
            {
                return new List<Job>(base.ChildJobs);
            }
        }

        internal int GetProgressActivityId()
        {
            lock (this._progressLock)
            {
                if (this._progressReportLastTime.Equals(DateTime.MinValue))
                {
                    try
                    {
                        this.ReportProgress(false);
                    }
                    catch (PSInvalidOperationException)
                    {
                        return -1;
                    }
                }
                return this._progressActivityId;
            }
        }

        private void MakeRoomForRunningOtherJobs(Job completedChildJob)
        {
            lock (this._lockObject)
            {
                this._extraCapacityForRunningAllJobs++;
                if (this._setOfChildJobsThatCanAddMoreChildJobs.Contains(completedChildJob.InstanceId))
                {
                    this._setOfChildJobsThatCanAddMoreChildJobs.Remove(completedChildJob.InstanceId);
                    this._extraCapacityForRunningQueryJobs++;
                    if (this._inBoostModeToPreventQueryJobDeadlock && (this._setOfChildJobsThatCanAddMoreChildJobs.Count == 0))
                    {
                        this._inBoostModeToPreventQueryJobDeadlock = false;
                        this._extraCapacityForRunningAllJobs--;
                    }
                }
            }
            this.StartChildJobIfPossible();
        }

        private void ReportProgress(bool minimizeFrequentUpdates)
        {
            lock (this._progressLock)
            {
                DateTime utcNow = DateTime.UtcNow;
                if (!minimizeFrequentUpdates || (((utcNow - this._progressStartTime) >= TimeSpan.FromSeconds(1.0)) && (this._progressReportLastTime.Equals(DateTime.MinValue) || ((utcNow - this._progressReportLastTime) >= TimeSpan.FromMilliseconds(200.0)))))
                {
                    double countOfFinishedChildJobs;
                    double num2;
                    int num3;
                    this._progressReportLastTime = utcNow;
                    lock (this._lockObject)
                    {
                        num2 = this._countOfAllChildJobs;
                        countOfFinishedChildJobs = this.CountOfFinishedChildJobs;
                    }
                    if (num2 >= 1.0)
                    {
                        num3 = (int) ((100.0 * countOfFinishedChildJobs) / num2);
                    }
                    else
                    {
                        num3 = -1;
                    }
                    num3 = Math.Max(-1, Math.Min(100, num3));
                    ProgressRecord progressRecord = new ProgressRecord(this._progressActivityId, base.Command, this.StatusMessage);
                    if (this.IsThrottlingJobCompleted)
                    {
                        if (this._progressReportLastTime.Equals(DateTime.MinValue))
                        {
                            goto Label_0192;
                        }
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        progressRecord.PercentComplete = 100;
                        progressRecord.SecondsRemaining = 0;
                    }
                    else
                    {
                        progressRecord.RecordType = ProgressRecordType.Processing;
                        progressRecord.PercentComplete = num3;
                        int? secondsRemaining = null;
                        if (num3 >= 0)
                        {
                            secondsRemaining = ProgressRecord.GetSecondsRemaining(this._progressStartTime, ((double) num3) / 100.0);
                        }
                        if (secondsRemaining.HasValue)
                        {
                            progressRecord.SecondsRemaining = secondsRemaining.Value;
                        }
                    }
                    this.WriteProgress(progressRecord);
                }
            Label_0192:;
            }
        }

        private void SetupThrottlingQueue(int maximumConcurrentChildJobs)
        {
            this._maximumConcurrentChildJobs = (maximumConcurrentChildJobs > 0) ? maximumConcurrentChildJobs : 0x7fffffff;
            if (this._cmdletMode)
            {
                this._maxReadyToRunJobs = MaximumReadyToRunJobs;
            }
            else
            {
                this._maxReadyToRunJobs = 0x7fffffff;
            }
            this._extraCapacityForRunningAllJobs = this._maximumConcurrentChildJobs;
            this._extraCapacityForRunningQueryJobs = Math.Max(1, this._extraCapacityForRunningAllJobs / 2);
            this._inBoostModeToPreventQueryJobDeadlock = false;
            this._readyToRunQueryJobs = new Queue<StartableJob>();
            this._readyToRunRegularJobs = new Queue<StartableJob>();
            this._actionsForUnblockingChildAdditions = new Queue<Action>();
        }

        private void StartChildJobIfPossible()
        {
            StartableJob job = null;
            lock (this._lockObject)
            {
                if (((this._readyToRunQueryJobs.Count > 0) && (this._extraCapacityForRunningQueryJobs > 0)) && (this._extraCapacityForRunningAllJobs > 0))
                {
                    this._extraCapacityForRunningQueryJobs--;
                    this._extraCapacityForRunningAllJobs--;
                    job = this._readyToRunQueryJobs.Dequeue();
                }
                else if ((this._readyToRunRegularJobs.Count > 0) && (this._extraCapacityForRunningAllJobs > 0))
                {
                    this._extraCapacityForRunningAllJobs--;
                    job = this._readyToRunRegularJobs.Dequeue();
                }
            }
            if (job != null)
            {
                job.StartJob();
            }
        }

        public override void StopJob()
        {
            List<Job> childJobsSnapshot = null;
            lock (this._lockObject)
            {
                if (!this._isStopping && !this.IsThrottlingJobCompleted)
                {
                    this._isStopping = true;
                    childJobsSnapshot = this.GetChildJobsSnapshot();
                }
            }
            if (childJobsSnapshot != null)
            {
                base.SetJobState(JobState.Stopping);
                this._cancellationTokenSource.Cancel();
                foreach (Job job in childJobsSnapshot)
                {
                    if (!job.IsFinishedState(job.JobStateInfo.State))
                    {
                        job.StopJob();
                    }
                }
                this.FigureOutIfThrottlingJobIsCompleted();
            }
            base.Finished.WaitOne();
        }

        private void WriteWarningAboutHighUsageOfFlowControlBuffers(long currentCount)
        {
            if (this._cmdletMode && (currentCount >= _flowControlBuffersHighMemoryUsageThreshold))
            {
                lock (this._alreadyWroteFlowControlBuffersHighMemoryUsageWarningLock)
                {
                    if (this._alreadyWroteFlowControlBuffersHighMemoryUsageWarning)
                    {
                        return;
                    }
                    this._alreadyWroteFlowControlBuffersHighMemoryUsageWarning = true;
                }
                string message = string.Format(CultureInfo.InvariantCulture, RemotingErrorIdStrings.ThrottlingJobFlowControlMemoryWarning, new object[] { base.Command });
                this.WriteWarning(message);
            }
        }

        private int CountOfFinishedChildJobs
        {
            get
            {
                lock (this._lockObject)
                {
                    return ((this._countOfFailedChildJobs + this._countOfStoppedChildJobs) + this._countOfSuccessfullyCompletedChildJobs);
                }
            }
        }

        private int CountOfRunningOrReadyToRunChildJobs
        {
            get
            {
                lock (this._lockObject)
                {
                    return (this._countOfAllChildJobs - this.CountOfFinishedChildJobs);
                }
            }
        }

        public override bool HasMoreData
        {
            get
            {
                if (!this.GetChildJobsSnapshot().Any<Job>(childJob => childJob.HasMoreData))
                {
                    return (base.Results.Count != 0);
                }
                return true;
            }
        }

        private bool IsEndOfChildJobs
        {
            get
            {
                lock (this._lockObject)
                {
                    return (this._isStopping || (this._ownerWontSubmitNewChildJobs && (this._setOfChildJobsThatCanAddMoreChildJobs.Count == 0)));
                }
            }
        }

        private bool IsThrottlingJobCompleted
        {
            get
            {
                lock (this._lockObject)
                {
                    return (this.IsEndOfChildJobs && (this._countOfAllChildJobs <= this.CountOfFinishedChildJobs));
                }
            }
        }

        public override string Location
        {
            get
            {
                lock (this._lockObject)
                {
                    return string.Join(", ", this._childJobLocations);
                }
            }
        }

        public override string StatusMessage
        {
            get
            {
                int countOfFinishedChildJobs;
                int num2;
                lock (this._lockObject)
                {
                    countOfFinishedChildJobs = this.CountOfFinishedChildJobs;
                    num2 = this._countOfAllChildJobs;
                }
                string str = num2.ToString(CultureInfo.CurrentUICulture);
                if (!this.IsEndOfChildJobs)
                {
                    str = str + "+";
                }
                return string.Format(CultureInfo.CurrentUICulture, RemotingErrorIdStrings.ThrottlingJobStatusMessage, new object[] { countOfFinishedChildJobs, str });
            }
        }

        [Flags]
        internal enum ChildJobFlags
        {
            None,
            CreatesChildJobs
        }

        private class ForwardingHelper : IDisposable
        {
            private readonly BlockingCollection<PSStreamObject> _aggregatedResults;
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            private bool _disposed;
            private readonly HashSet<Job> _monitoredJobs;
            private readonly object _myLock;
            private bool _stoppedMonitoringAllJobs;
            private readonly ThrottlingJob _throttlingJob;
            internal static readonly int AggregationQueueMaxCapacity = 0x2710;

            private ForwardingHelper(ThrottlingJob throttlingJob)
            {
                this._throttlingJob = throttlingJob;
                this._myLock = new object();
                this._monitoredJobs = new HashSet<Job>();
                this._aggregatedResults = new BlockingCollection<PSStreamObject>();
            }

            private void AggregateJobResults(PSDataCollection<PSStreamObject> resultsCollection)
            {
                lock (this._myLock)
                {
                    if ((this._disposed || this._stoppedMonitoringAllJobs) || (this._aggregatedResults.IsAddingCompleted || this._cancellationTokenSource.IsCancellationRequested))
                    {
                        return;
                    }
                }
                foreach (PSStreamObject obj2 in resultsCollection.ReadAll())
                {
                    bool flag = false;
                    try
                    {
                        lock (this._myLock)
                        {
                            if ((!this._disposed && !this._stoppedMonitoringAllJobs) && (!this._aggregatedResults.IsAddingCompleted && !this._cancellationTokenSource.IsCancellationRequested))
                            {
                                this._aggregatedResults.Add(obj2, this._cancellationTokenSource.Token);
                                flag = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (!flag)
                    {
                        this.StopMonitoringJob(this._throttlingJob);
                        try
                        {
                            this._throttlingJob.Results.Add(obj2);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }

            private void AttemptToPreserveAggregatedResults()
            {
                bool flag = false;
                foreach (PSStreamObject obj2 in this._aggregatedResults)
                {
                    if (!flag)
                    {
                        try
                        {
                            this._throttlingJob.Results.Add(obj2);
                        }
                        catch (PSInvalidOperationException)
                        {
                            flag = this._throttlingJob.IsFinishedState(this._throttlingJob.JobStateInfo.State);
                        }
                    }
                }
            }

            private void CancelForwarding()
            {
                this._cancellationTokenSource.Cancel();
                lock (this._myLock)
                {
                    this._aggregatedResults.CompleteAdding();
                }
            }

            private void CheckIfMonitoredJobIsComplete(Job job)
            {
                this.CheckIfMonitoredJobIsComplete(job, job.JobStateInfo.State);
            }

            private void CheckIfMonitoredJobIsComplete(Job job, JobState jobState)
            {
                if (job.IsFinishedState(jobState))
                {
                    lock (this._myLock)
                    {
                        this.StopMonitoringJob(job);
                    }
                }
            }

            private void CheckIfThrottlingJobIsComplete()
            {
                if (this._throttlingJob.IsThrottlingJobCompleted)
                {
                    List<PSDataCollection<PSStreamObject>> list = new List<PSDataCollection<PSStreamObject>>();
                    lock (this._myLock)
                    {
                        foreach (Job job in this._monitoredJobs)
                        {
                            list.Add(job.Results);
                        }
                        foreach (Job job2 in this._throttlingJob.GetChildJobsSnapshot())
                        {
                            list.Add(job2.Results);
                        }
                        list.Add(this._throttlingJob.Results);
                    }
                    foreach (PSDataCollection<PSStreamObject> datas in list)
                    {
                        this.AggregateJobResults(datas);
                    }
                    lock (this._myLock)
                    {
                        if (!this._disposed && !this._aggregatedResults.IsAddingCompleted)
                        {
                            this._aggregatedResults.CompleteAdding();
                        }
                    }
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                this._cancellationTokenSource.Cancel();
                lock (this._myLock)
                {
                    if (!this._disposed)
                    {
                        this.StopMonitoringAllJobs();
                        this._aggregatedResults.Dispose();
                        this._cancellationTokenSource.Dispose();
                        this._disposed = true;
                    }
                }
            }

            public static void ForwardAllResultsToCmdlet(ThrottlingJob throttlingJob, Cmdlet cmdlet, CancellationToken? cancellationToken)
            {
                WaitCallback callBack = null;
                using (ThrottlingJob.ForwardingHelper helper = new ThrottlingJob.ForwardingHelper(throttlingJob))
                {
                    try
                    {
                        throttlingJob.ChildJobAdded += new EventHandler<ThrottlingJobChildAddedEventArgs>(helper.ThrottlingJob_ChildJobAdded);
                        try
                        {
                            throttlingJob.StateChanged += new EventHandler<JobStateEventArgs>(helper.ThrottlingJob_StateChanged);
                            IDisposable disposable = null;
                            if (cancellationToken.HasValue)
                            {
                                disposable = cancellationToken.Value.Register(new Action(helper.CancelForwarding));
                            }
                            try
                            {
                                Thread.MemoryBarrier();
                                if (callBack == null)
                                {
                                    callBack = delegate (object param0) {
                                        helper.StartMonitoringJob(throttlingJob);
                                        foreach (Job job in throttlingJob.GetChildJobsSnapshot())
                                        {
                                            helper.StartMonitoringJob(job);
                                        }
                                        helper.CheckIfThrottlingJobIsComplete();
                                    };
                                }
                                ThreadPool.QueueUserWorkItem(callBack);
                                helper.ForwardResults(cmdlet);
                            }
                            finally
                            {
                                if (disposable != null)
                                {
                                    disposable.Dispose();
                                }
                            }
                        }
                        finally
                        {
                            throttlingJob.StateChanged -= new EventHandler<JobStateEventArgs>(helper.ThrottlingJob_StateChanged);
                        }
                    }
                    finally
                    {
                        throttlingJob.ChildJobAdded -= new EventHandler<ThrottlingJobChildAddedEventArgs>(helper.ThrottlingJob_ChildJobAdded);
                    }
                }
            }

            private void ForwardResults(Cmdlet cmdlet)
            {
                try
                {
                    foreach (PSStreamObject obj2 in this._aggregatedResults.GetConsumingEnumerable(this._throttlingJob._cancellationTokenSource.Token))
                    {
                        if (obj2 != null)
                        {
                            try
                            {
                                obj2.WriteStreamObject(cmdlet, false);
                            }
                            finally
                            {
                                if (this._throttlingJob._cmdletMode)
                                {
                                    Interlocked.Decrement(ref this._throttlingJob._jobResultsCurrentCount);
                                    this._throttlingJob._jobResultsThrottlingSemaphore.Release();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    this.StopMonitoringAllJobs();
                    this.AttemptToPreserveAggregatedResults();
                    throw;
                }
            }

            private void MonitoredJob_StateChanged(object sender, JobStateEventArgs e)
            {
                Job job = (Job) sender;
                this.CheckIfMonitoredJobIsComplete(job, e.JobStateInfo.State);
            }

            private void MonitoredJobResults_DataAdded(object sender, DataAddedEventArgs e)
            {
                PSDataCollection<PSStreamObject> resultsCollection = (PSDataCollection<PSStreamObject>) sender;
                this.AggregateJobResults(resultsCollection);
            }

            private void StartMonitoringJob(Job job)
            {
                lock (this._myLock)
                {
                    if ((this._disposed || this._stoppedMonitoringAllJobs) || this._monitoredJobs.Contains(job))
                    {
                        return;
                    }
                    this._monitoredJobs.Add(job);
                    job.Results.DataAdded += new EventHandler<DataAddedEventArgs>(this.MonitoredJobResults_DataAdded);
                    job.StateChanged += new EventHandler<JobStateEventArgs>(this.MonitoredJob_StateChanged);
                }
                this.AggregateJobResults(job.Results);
                this.CheckIfMonitoredJobIsComplete(job);
            }

            private void StopMonitoringAllJobs()
            {
                this._cancellationTokenSource.Cancel();
                lock (this._myLock)
                {
                    this._stoppedMonitoringAllJobs = true;
                    foreach (Job job in this._monitoredJobs.ToList<Job>())
                    {
                        this.StopMonitoringJob(job);
                    }
                    if (!this._disposed && !this._aggregatedResults.IsAddingCompleted)
                    {
                        this._aggregatedResults.CompleteAdding();
                    }
                }
            }

            private void StopMonitoringJob(Job job)
            {
                lock (this._myLock)
                {
                    if (this._monitoredJobs.Contains(job))
                    {
                        job.Results.DataAdded -= new EventHandler<DataAddedEventArgs>(this.MonitoredJobResults_DataAdded);
                        job.StateChanged -= new EventHandler<JobStateEventArgs>(this.MonitoredJob_StateChanged);
                        this._monitoredJobs.Remove(job);
                    }
                }
            }

            private void ThrottlingJob_ChildJobAdded(object sender, ThrottlingJobChildAddedEventArgs e)
            {
                this.StartMonitoringJob(e.AddedChildJob);
            }

            private void ThrottlingJob_StateChanged(object sender, JobStateEventArgs e)
            {
                this.CheckIfThrottlingJobIsComplete();
            }
        }
    }
}

