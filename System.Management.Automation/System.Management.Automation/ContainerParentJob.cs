namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public sealed class ContainerParentJob : Job2
    {
        private int _blockedChildJobsCount;
        private PSEventManager _eventManager;
        private readonly PSDataCollection<ErrorRecord> _executionError;
        private int _failedChildJobsCount;
        private int _finishedChildJobsCount;
        private int _isDisposed;
        private bool _moreData;
        private int _stoppedChildJobsCount;
        private int _suspendedChildJobsCount;
        private int _suspendingChildJobsCount;
        private readonly PowerShellTraceSource _tracer;
        private const int DisposedFalse = 0;
        private const int DisposedTrue = 1;
        private const string ResBaseName = "RemotingErrorIdStrings";
        private static Tracer StructuredTracer = new Tracer();
        private const string TraceClassName = "ContainerParentJob";

        public ContainerParentJob(string command) : base(command)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
        }

        public ContainerParentJob(string command, string name) : base(command, name)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
        }

        public ContainerParentJob(string command, string name, Guid instanceId) : base(command, name, instanceId)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
        }

        public ContainerParentJob(string command, string name, JobIdentifier jobId) : base(command, name, jobId)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
        }

        public ContainerParentJob(string command, string name, string jobType) : base(command, name)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
            base.PSJobTypeName = jobType;
        }

        public ContainerParentJob(string command, string name, Guid instanceId, string jobType) : base(command, name, instanceId)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
            base.PSJobTypeName = jobType;
        }

        public ContainerParentJob(string command, string name, JobIdentifier jobId, string jobType) : base(command, name, jobId)
        {
            this._moreData = true;
            this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
            this._executionError = new PSDataCollection<ErrorRecord>();
            base.PSJobTypeName = jobType;
        }

        public void AddChildJob(Job2 childJob)
        {
            JobStateInfo jobStateInfo;
            base.AssertNotDisposed();
            if (childJob == null)
            {
                throw new ArgumentNullException("childJob");
            }
            this._tracer.WriteMessage("ContainerParentJob", "AddChildJob", Guid.Empty, childJob, "Adding Child to Parent with InstanceId : ", new string[] { base.InstanceId.ToString() });
            lock (childJob.syncObject)
            {
                jobStateInfo = childJob.JobStateInfo;
                childJob.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleChildJobStateChanged);
            }
            base.ChildJobs.Add(childJob);
            this.ParentJobStateCalculation(new JobStateEventArgs(jobStateInfo, new JobStateInfo(JobState.NotStarted)));
        }

        internal static bool ComputeJobStateFromChildJobStates(string traceClassName, JobStateEventArgs e, ref int blockedChildJobsCount, ref int suspendedChildJobsCount, ref int suspendingChildJobsCount, ref int finishedChildJobsCount, ref int failedChildJobsCount, ref int stoppedChildJobsCount, int childJobsCount, out JobState computedJobState)
        {
            computedJobState = JobState.NotStarted;
            using (PowerShellTraceSource source = PowerShellTraceSourceFactory.GetTraceSource())
            {
                if (e.JobStateInfo.State == JobState.Blocked)
                {
                    Interlocked.Increment(ref blockedChildJobsCount);
                    source.WriteMessage(traceClassName, ": JobState is Blocked, at least one child job is blocked.");
                    computedJobState = JobState.Blocked;
                    return true;
                }
                if (e.PreviousJobStateInfo.State == JobState.Blocked)
                {
                    Interlocked.Decrement(ref blockedChildJobsCount);
                    if (blockedChildJobsCount == 0)
                    {
                        source.WriteMessage(traceClassName, ": JobState is unblocked, all child jobs are unblocked.");
                        computedJobState = JobState.Running;
                        return true;
                    }
                    return false;
                }
                if (e.PreviousJobStateInfo.State == JobState.Suspended)
                {
                    Interlocked.Decrement(ref suspendedChildJobsCount);
                }
                if (e.PreviousJobStateInfo.State == JobState.Suspending)
                {
                    Interlocked.Decrement(ref suspendingChildJobsCount);
                }
                if (e.JobStateInfo.State == JobState.Suspended)
                {
                    Interlocked.Increment(ref suspendedChildJobsCount);
                    if ((suspendedChildJobsCount + finishedChildJobsCount) == childJobsCount)
                    {
                        source.WriteMessage(traceClassName, ": JobState is suspended, all child jobs are suspended.");
                        computedJobState = JobState.Suspended;
                        return true;
                    }
                    return false;
                }
                if (e.JobStateInfo.State == JobState.Suspending)
                {
                    Interlocked.Increment(ref suspendingChildJobsCount);
                    if (((suspendedChildJobsCount + finishedChildJobsCount) + suspendingChildJobsCount) == childJobsCount)
                    {
                        source.WriteMessage(traceClassName, ": JobState is suspending, all child jobs are in suspending state.");
                        computedJobState = JobState.Suspending;
                        return true;
                    }
                    return false;
                }
                if (((e.JobStateInfo.State != JobState.Completed) && (e.JobStateInfo.State != JobState.Failed)) && (e.JobStateInfo.State != JobState.Stopped))
                {
                    if (e.JobStateInfo.State == JobState.Running)
                    {
                        computedJobState = JobState.Running;
                        return true;
                    }
                    return false;
                }
                if (e.JobStateInfo.State == JobState.Failed)
                {
                    Interlocked.Increment(ref failedChildJobsCount);
                }
                if (e.JobStateInfo.State == JobState.Stopped)
                {
                    Interlocked.Increment(ref stoppedChildJobsCount);
                }
                bool flag = false;
                int num = Interlocked.Increment(ref finishedChildJobsCount);
                if (num == childJobsCount)
                {
                    flag = true;
                }
                if (flag)
                {
                    if (failedChildJobsCount > 0)
                    {
                        source.WriteMessage(traceClassName, ": JobState is failed, at least one child job failed.");
                        computedJobState = JobState.Failed;
                        return true;
                    }
                    if (stoppedChildJobsCount > 0)
                    {
                        source.WriteMessage(traceClassName, ": JobState is stopped, stop is called.");
                        computedJobState = JobState.Stopped;
                        return true;
                    }
                    source.WriteMessage(traceClassName, ": JobState is completed.");
                    computedJobState = JobState.Completed;
                    return true;
                }
                if ((suspendedChildJobsCount + num) == childJobsCount)
                {
                    source.WriteMessage(traceClassName, ": JobState is suspended, all child jobs are suspended.");
                    computedJobState = JobState.Suspended;
                    return true;
                }
                if (((suspendingChildJobsCount + suspendedChildJobsCount) + num) == childJobsCount)
                {
                    source.WriteMessage(traceClassName, ": JobState is suspending, all child jobs are in suspending state.");
                    computedJobState = JobState.Suspending;
                    return true;
                }
            }
            return false;
        }

        private string ConstructLocation()
        {
            if ((base.ChildJobs == null) || (base.ChildJobs.Count == 0))
            {
                return string.Empty;
            }
            return (from job in base.ChildJobs select job.Location).Aggregate<string>((s1, s2) => (s1 + ',' + s2));
        }

        private string ConstructStatusMessage()
        {
            if ((base.ChildJobs == null) || (base.ChildJobs.Count == 0))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < base.ChildJobs.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.ChildJobs[i].StatusMessage))
                {
                    builder.Append(base.ChildJobs[i].StatusMessage);
                }
                if (i < (base.ChildJobs.Count - 1))
                {
                    builder.Append(",");
                }
            }
            return builder.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) != 1))
            {
                try
                {
                    this.UnregisterAllJobEvents();
                    this._executionError.Dispose();
                    foreach (Job job in base.ChildJobs)
                    {
                        this._tracer.WriteMessage("Disposing child job with id : " + job.Id);
                        job.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(true);
                }
            }
        }

        private void HandleChildJobStateChanged(object sender, JobStateEventArgs e)
        {
            this.ParentJobStateCalculation(e);
        }

        private void ParentJobStateCalculation(JobStateEventArgs e)
        {
            JobState state;
            if (ComputeJobStateFromChildJobStates("ContainerParentJob", e, ref this._blockedChildJobsCount, ref this._suspendedChildJobsCount, ref this._suspendingChildJobsCount, ref this._finishedChildJobsCount, ref this._failedChildJobsCount, ref this._stoppedChildJobsCount, base.ChildJobs.Count, out state))
            {
                if (state != base.JobStateInfo.State)
                {
                    if ((base.JobStateInfo.State == JobState.NotStarted) && (state == JobState.Running))
                    {
                        base.PSBeginTime = new DateTime?(DateTime.Now);
                    }
                    if (!base.IsFinishedState(base.JobStateInfo.State) && base.IsPersistentState(state))
                    {
                        base.PSEndTime = new DateTime?(DateTime.Now);
                    }
                    base.SetJobState(state);
                }
                if (this._finishedChildJobsCount == base.ChildJobs.Count)
                {
                    StructuredTracer.EndContainerParentJobExecution(base.InstanceId);
                }
            }
        }

        public override void ResumeJob()
        {
            AutoResetEvent completed;
            int resumedChildJobsCount;
            base.AssertNotDisposed();
            this._tracer.WriteMessage("ContainerParentJob", "ResumeJob", Guid.Empty, this, "Entering method", null);
            using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Job2) enumerator.Current) == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                    }
                }
            }
            if (base.ChildJobs.Count == 1)
            {
                Job2 targetObject = base.ChildJobs[0] as Job2;
                try
                {
                    this._tracer.WriteMessage("ContainerParentJob", "ResumeJob", Guid.Empty, this, "Single child job synchronously, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    targetObject.ResumeJob();
                }
                catch (Exception exception)
                {
                    this.ExecutionError.Add(new ErrorRecord(exception, "ContainerParentJobResumeError", ErrorCategory.InvalidResult, targetObject));
                    this._tracer.WriteMessage("ContainerParentJob", "ResumeJob", Guid.Empty, this, "Single child job threw exception, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    this._tracer.TraceException(exception);
                }
            }
            else
            {
                completed = new AutoResetEvent(false);
                resumedChildJobsCount = 0;
                EventHandler<AsyncCompletedEventArgs> handler = null;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        EventHandler<AsyncCompletedEventArgs> handler2 = null;
                        Job2 job = (Job2) enumerator2.Current;
                        if (handler2 == null)
                        {
                            handler2 = delegate (object sender, AsyncCompletedEventArgs e) {
                                this._tracer.WriteMessage("ContainerParentJob", "ResumeJob-Handler", Guid.Empty, this, "Finished resuming child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                if (e.Error != null)
                                {
                                    this.ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobResumeError", ErrorCategory.InvalidResult, job));
                                    this._tracer.WriteMessage("ContainerParentJob", "ResumeJob-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                    this._tracer.TraceException(e.Error);
                                }
                                Interlocked.Increment(ref resumedChildJobsCount);
                                if (resumedChildJobsCount == this.ChildJobs.Count)
                                {
                                    this._tracer.WriteMessage("ContainerParentJob", "ResumeJob-Handler", Guid.Empty, this, "Finished resuming all child jobs asynchronously", null);
                                    completed.Set();
                                }
                            };
                        }
                        handler = handler2;
                        job.ResumeJobCompleted += handler;
                        this._tracer.WriteMessage("ContainerParentJob", "ResumeJob", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        job.ResumeJobAsync();
                    }
                }
                completed.WaitOne();
                foreach (Job2 job3 in base.ChildJobs)
                {
                    job3.ResumeJobCompleted -= handler;
                }
                this._tracer.WriteMessage("ContainerParentJob", "ResumeJob", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void ResumeJobAsync()
        {
            int resumedChildJobsCount;
            if (this._isDisposed == 1)
            {
                this.OnResumeJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException("ContainerParentJob"), false, null));
            }
            else
            {
                this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync", Guid.Empty, this, "Entering method", null);
                using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Job2) enumerator.Current) == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                        }
                    }
                }
                resumedChildJobsCount = 0;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Job2 job = (Job2) enumerator2.Current;
                        EventHandler<AsyncCompletedEventArgs> eventHandler = null;
                        eventHandler = delegate (object sender, AsyncCompletedEventArgs e) {
                            Job2 job1 = sender as Job2;
                            this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync-Handler", Guid.Empty, this, "Finished resuming child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                            if (e.Error != null)
                            {
                                this.ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobResumeAsyncError", ErrorCategory.InvalidResult, job));
                                this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                this._tracer.TraceException(e.Error);
                            }
                            Interlocked.Increment(ref resumedChildJobsCount);
                            job1.ResumeJobCompleted -= eventHandler;
                            if (resumedChildJobsCount == this.ChildJobs.Count)
                            {
                                this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync-Handler", Guid.Empty, this, "Finished resuming all child jobs asynchronously", null);
                                this.OnResumeJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                            }
                        };
                        job.ResumeJobCompleted += eventHandler;
                        this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        job.ResumeJobAsync();
                    }
                }
                this._tracer.WriteMessage("ContainerParentJob", "ResumeJobAsync", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void StartJob()
        {
            AutoResetEvent completed;
            int startedChildJobsCount;
            base.AssertNotDisposed();
            this._tracer.WriteMessage("ContainerParentJob", "StartJob", Guid.Empty, this, "Entering method", null);
            StructuredTracer.BeginContainerParentJobExecution(base.InstanceId);
            using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Job2) enumerator.Current) == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                    }
                }
            }
            if (base.ChildJobs.Count == 1)
            {
                Job2 job2 = base.ChildJobs[0] as Job2;
                try
                {
                    this._tracer.WriteMessage("ContainerParentJob", "StartJob", Guid.Empty, this, "Single child job synchronously, child InstanceId: {0}", new string[] { job2.InstanceId.ToString() });
                    job2.StartJob();
                }
                catch (Exception exception)
                {
                    this.ExecutionError.Add(new ErrorRecord(exception, "ContainerParentJobStartError", ErrorCategory.InvalidResult, job2));
                    this._tracer.WriteMessage("ContainerParentJob", "StartJob", Guid.Empty, this, "Single child job threw exception, child InstanceId: {0}", new string[] { job2.InstanceId.ToString() });
                    this._tracer.TraceException(exception);
                }
            }
            else
            {
                completed = new AutoResetEvent(false);
                startedChildJobsCount = 0;
                EventHandler<AsyncCompletedEventArgs> handler = delegate (object sender, AsyncCompletedEventArgs e) {
                    Job2 targetObject = sender as Job2;
                    this._tracer.WriteMessage("ContainerParentJob", "StartJob-Handler", Guid.Empty, this, "Finished starting child job asynchronously, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    if (e.Error != null)
                    {
                        this.ExecutionError.Add(new ErrorRecord(e.Error, "ConainerParentJobStartError", ErrorCategory.InvalidResult, targetObject));
                        this._tracer.WriteMessage("ContainerParentJob", "StartJob-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                        this._tracer.TraceException(e.Error);
                    }
                    Interlocked.Increment(ref startedChildJobsCount);
                    if (startedChildJobsCount == this.ChildJobs.Count)
                    {
                        this._tracer.WriteMessage("ContainerParentJob", "StartJob-Handler", Guid.Empty, this, "Finished starting all child jobs asynchronously", null);
                        completed.Set();
                    }
                };
                foreach (Job2 job3 in base.ChildJobs)
                {
                    job3.StartJobCompleted += handler;
                    this._tracer.WriteMessage("ContainerParentJob", "StartJob", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job3.InstanceId.ToString() });
                    job3.StartJobAsync();
                }
                completed.WaitOne();
                foreach (Job2 job4 in base.ChildJobs)
                {
                    job4.StartJobCompleted -= handler;
                }
                this._tracer.WriteMessage("ContainerParentJob", "StartJob", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void StartJobAsync()
        {
            int startedChildJobsCount;
            EventHandler<AsyncCompletedEventArgs> eventHandler;
            if (this._isDisposed == 1)
            {
                this.OnStartJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException("ContainerParentJob"), false, null));
            }
            else
            {
                this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync", Guid.Empty, this, "Entering method", null);
                StructuredTracer.BeginContainerParentJobExecution(base.InstanceId);
                using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Job2) enumerator.Current) == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                        }
                    }
                }
                startedChildJobsCount = 0;
                eventHandler = null;
                eventHandler = delegate (object sender, AsyncCompletedEventArgs e) {
                    Job2 targetObject = sender as Job2;
                    this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync-Handler", Guid.Empty, this, "Finished starting child job asynchronously, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    if (e.Error != null)
                    {
                        this.ExecutionError.Add(new ErrorRecord(e.Error, "ConainerParentJobStartAsyncError", ErrorCategory.InvalidResult, targetObject));
                        this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                        this._tracer.TraceException(e.Error);
                    }
                    Interlocked.Increment(ref startedChildJobsCount);
                    targetObject.StartJobCompleted -= eventHandler;
                    if (startedChildJobsCount == this.ChildJobs.Count)
                    {
                        this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync-Handler", Guid.Empty, this, "Finished starting all child jobs asynchronously", null);
                        this.OnStartJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                    }
                };
                foreach (Job2 job2 in base.ChildJobs)
                {
                    job2.StartJobCompleted += eventHandler;
                    this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job2.InstanceId.ToString() });
                    job2.StartJobAsync();
                }
                this._tracer.WriteMessage("ContainerParentJob", "StartJobAsync", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void StopJob()
        {
            this.StopJobInternal(null, null);
        }

        public override void StopJob(bool force, string reason)
        {
            this.StopJobInternal(new bool?(force), reason);
        }

        public override void StopJobAsync()
        {
            this.StopJobAsyncInternal(null, null);
        }

        public override void StopJobAsync(bool force, string reason)
        {
            this.StopJobAsyncInternal(new bool?(force), reason);
        }

        private void StopJobAsyncInternal(bool? force, string reason)
        {
            int stoppedChildJobsCount;
            if (this._isDisposed == 1)
            {
                this.OnStopJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException("ContainerParentJob"), false, null));
            }
            else
            {
                this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync", Guid.Empty, this, "Entering method", null);
                using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Job2) enumerator.Current) == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                        }
                    }
                }
                stoppedChildJobsCount = 0;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Job2 job = (Job2) enumerator2.Current;
                        EventHandler<AsyncCompletedEventArgs> eventHandler = null;
                        eventHandler = delegate (object sender, AsyncCompletedEventArgs e) {
                            Job2 targetObject = sender as Job2;
                            this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync-Handler", Guid.Empty, this, "Finished stopping child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                            if (e.Error != null)
                            {
                                this.ExecutionError.Add(new ErrorRecord(e.Error, "ConainerParentJobStopAsyncError", ErrorCategory.InvalidResult, targetObject));
                                this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                this._tracer.TraceException(e.Error);
                            }
                            Interlocked.Increment(ref stoppedChildJobsCount);
                            targetObject.StopJobCompleted -= eventHandler;
                            if (stoppedChildJobsCount == this.ChildJobs.Count)
                            {
                                this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync-Handler", Guid.Empty, this, "Finished stopping all child jobs asynchronously", null);
                                this.OnStopJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                            }
                        };
                        job.StopJobCompleted += eventHandler;
                        this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        if (force.HasValue)
                        {
                            job.StopJobAsync(force.Value, reason);
                        }
                        else
                        {
                            job.StopJobAsync();
                        }
                    }
                }
                this._tracer.WriteMessage("ContainerParentJob", "StopJobAsync", Guid.Empty, this, "Exiting method", null);
            }
        }

        private void StopJobInternal(bool? force, string reason)
        {
            AutoResetEvent completed;
            int stoppedChildJobsCount;
            base.AssertNotDisposed();
            this._tracer.WriteMessage("ContainerParentJob", "StopJob", Guid.Empty, this, "Entering method", null);
            using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Job2) enumerator.Current) == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                    }
                }
            }
            if (base.ChildJobs.Count == 1)
            {
                Job2 targetObject = base.ChildJobs[0] as Job2;
                try
                {
                    this._tracer.WriteMessage("ContainerParentJob", "StopJob", Guid.Empty, this, "Single child job synchronously, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    if (force.HasValue)
                    {
                        targetObject.StopJob(force.Value, reason);
                    }
                    else
                    {
                        targetObject.StopJob();
                    }
                }
                catch (Exception exception)
                {
                    this.ExecutionError.Add(new ErrorRecord(exception, "ContainerParentJobStopError", ErrorCategory.InvalidResult, targetObject));
                    this._tracer.WriteMessage("ContainerParentJob", "StopJob", Guid.Empty, this, "Single child job threw exception, child InstanceId: {0}", new string[] { targetObject.InstanceId.ToString() });
                    this._tracer.TraceException(exception);
                }
            }
            else
            {
                completed = new AutoResetEvent(false);
                stoppedChildJobsCount = 0;
                EventHandler<AsyncCompletedEventArgs> handler = null;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        EventHandler<AsyncCompletedEventArgs> handler2 = null;
                        Job2 job = (Job2) enumerator2.Current;
                        if (handler2 == null)
                        {
                            handler2 = delegate (object sender, AsyncCompletedEventArgs e) {
                                this._tracer.WriteMessage("ContainerParentJob", "StopJob-Handler", Guid.Empty, this, "Finished stopping child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                if (e.Error != null)
                                {
                                    this.ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobStopError", ErrorCategory.InvalidResult, job));
                                    this._tracer.WriteMessage("ContainerParentJob", "StopJob-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                    this._tracer.TraceException(e.Error);
                                }
                                Interlocked.Increment(ref stoppedChildJobsCount);
                                if (stoppedChildJobsCount == this.ChildJobs.Count)
                                {
                                    this._tracer.WriteMessage("ContainerParentJob", "StopJob-Handler", Guid.Empty, this, "Finished stopping all child jobs asynchronously", null);
                                    completed.Set();
                                }
                            };
                        }
                        handler = handler2;
                        job.StopJobCompleted += handler;
                        this._tracer.WriteMessage("ContainerParentJob", "StopJob", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        if (force.HasValue)
                        {
                            job.StopJobAsync(force.Value, reason);
                        }
                        else
                        {
                            job.StopJobAsync();
                        }
                    }
                }
                completed.WaitOne();
                foreach (Job2 job3 in base.ChildJobs)
                {
                    job3.StopJobCompleted -= handler;
                }
                this._tracer.WriteMessage("ContainerParentJob", "StopJob", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void SuspendJob()
        {
            this.SuspendJobInternal(null, null);
        }

        public override void SuspendJob(bool force, string reason)
        {
            this.SuspendJobInternal(new bool?(force), reason);
        }

        public override void SuspendJobAsync()
        {
            this.SuspendJobAsyncInternal(null, null);
        }

        public override void SuspendJobAsync(bool force, string reason)
        {
            this.SuspendJobAsyncInternal(new bool?(force), reason);
        }

        private void SuspendJobAsyncInternal(bool? force, string reason)
        {
            int suspendedChildJobsCount;
            if (this._isDisposed == 1)
            {
                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException("ContainerParentJob"), false, null));
            }
            else
            {
                this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync", Guid.Empty, this, "Entering method", null);
                using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Job2) enumerator.Current) == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                        }
                    }
                }
                suspendedChildJobsCount = 0;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Job2 job = (Job2) enumerator2.Current;
                        EventHandler<AsyncCompletedEventArgs> eventHandler = null;
                        eventHandler = delegate (object sender, AsyncCompletedEventArgs e) {
                            Job2 job1 = sender as Job2;
                            this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync-Handler", Guid.Empty, this, "Finished suspending child job asynchronously, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                            if (e.Error != null)
                            {
                                this.ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobSuspendAsyncError", ErrorCategory.InvalidResult, job));
                                this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                                this._tracer.TraceException(e.Error);
                            }
                            Interlocked.Increment(ref suspendedChildJobsCount);
                            job1.SuspendJobCompleted -= eventHandler;
                            if (suspendedChildJobsCount == this.ChildJobs.Count)
                            {
                                this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync-Handler", Guid.Empty, this, "Finished suspending all child jobs asynchronously", null);
                                this.OnSuspendJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                            }
                        };
                        job.SuspendJobCompleted += eventHandler;
                        this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                        if (force.HasValue)
                        {
                            job.SuspendJobAsync(force.Value, reason);
                        }
                        else
                        {
                            job.SuspendJobAsync();
                        }
                    }
                }
                this._tracer.WriteMessage("ContainerParentJob", "SuspendJobAsync", Guid.Empty, this, "Exiting method", null);
            }
        }

        private void SuspendJobInternal(bool? force, string reason)
        {
            AutoResetEvent completed;
            int suspendedChildJobsCount;
            base.AssertNotDisposed();
            this._tracer.WriteMessage("ContainerParentJob", "SuspendJob", Guid.Empty, this, "Entering method", null);
            using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Job2) enumerator.Current) == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                    }
                }
            }
            if (base.ChildJobs.Count == 1)
            {
                Job2 targetObject = base.ChildJobs[0] as Job2;
                try
                {
                    this._tracer.WriteMessage("ContainerParentJob", "SuspendJob", Guid.Empty, this, "Single child job synchronously, child InstanceId: {0} force: {1}", new string[] { targetObject.InstanceId.ToString(), force.ToString() });
                    if (force.HasValue)
                    {
                        targetObject.SuspendJob(force.Value, reason);
                    }
                    else
                    {
                        targetObject.SuspendJob();
                    }
                }
                catch (Exception exception)
                {
                    this.ExecutionError.Add(new ErrorRecord(exception, "ContainerParentJobSuspendError", ErrorCategory.InvalidResult, targetObject));
                    this._tracer.WriteMessage("ContainerParentJob", "SuspendJob", Guid.Empty, this, "Single child job threw exception, child InstanceId: {0} force: {1}", new string[] { targetObject.InstanceId.ToString(), force.ToString() });
                    this._tracer.TraceException(exception);
                }
            }
            else
            {
                completed = new AutoResetEvent(false);
                suspendedChildJobsCount = 0;
                EventHandler<AsyncCompletedEventArgs> handler = null;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        EventHandler<AsyncCompletedEventArgs> handler2 = null;
                        Job2 job = (Job2) enumerator2.Current;
                        if (handler2 == null)
                        {
                            handler2 = delegate (object sender, AsyncCompletedEventArgs e) {
                                this._tracer.WriteMessage("ContainerParentJob", "SuspendJob-Handler", Guid.Empty, this, "Finished suspending child job asynchronously, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                                if (e.Error != null)
                                {
                                    this.ExecutionError.Add(new ErrorRecord(e.Error, "ContainerParentJobSuspendError", ErrorCategory.InvalidResult, job));
                                    this._tracer.WriteMessage("ContainerParentJob", "SuspendJob-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                                    this._tracer.TraceException(e.Error);
                                }
                                Interlocked.Increment(ref suspendedChildJobsCount);
                                if (suspendedChildJobsCount == this.ChildJobs.Count)
                                {
                                    this._tracer.WriteMessage("ContainerParentJob", "SuspendJob-Handler", Guid.Empty, this, "Finished suspending all child jobs asynchronously", null);
                                    completed.Set();
                                }
                            };
                        }
                        handler = handler2;
                        job.SuspendJobCompleted += handler;
                        this._tracer.WriteMessage("ContainerParentJob", "SuspendJob", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0} force: {1}", new string[] { job.InstanceId.ToString(), force.ToString() });
                        if (force.HasValue)
                        {
                            job.SuspendJobAsync(force.Value, reason);
                        }
                        else
                        {
                            job.SuspendJobAsync();
                        }
                    }
                }
                completed.WaitOne();
                foreach (Job2 job3 in base.ChildJobs)
                {
                    job3.SuspendJobCompleted -= handler;
                }
                this._tracer.WriteMessage("ContainerParentJob", "SuspendJob", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void UnblockJob()
        {
            AutoResetEvent completed;
            int unblockedChildJobsCount;
            base.AssertNotDisposed();
            this._tracer.WriteMessage("ContainerParentJob", "UnblockJob", Guid.Empty, this, "Entering method", null);
            using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Job2) enumerator.Current) == null)
                    {
                        throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                    }
                }
            }
            if (base.ChildJobs.Count == 1)
            {
                Job2 job2 = base.ChildJobs[0] as Job2;
                try
                {
                    this._tracer.WriteMessage("ContainerParentJob", "UnblockJob", Guid.Empty, this, "Single child job synchronously, child InstanceId: {0}", new string[] { job2.InstanceId.ToString() });
                    job2.UnblockJob();
                }
                catch (Exception exception)
                {
                    this.ExecutionError.Add(new ErrorRecord(exception, "ContainerParentJobUnblockError", ErrorCategory.InvalidResult, job2));
                    this._tracer.WriteMessage("ContainerParentJob", "UnblockJob", Guid.Empty, this, "Single child job threw exception, child InstanceId: {0}", new string[] { job2.InstanceId.ToString() });
                    this._tracer.TraceException(exception);
                }
            }
            else
            {
                completed = new AutoResetEvent(false);
                unblockedChildJobsCount = 0;
                EventHandler<AsyncCompletedEventArgs> handler = null;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        EventHandler<AsyncCompletedEventArgs> handler2 = null;
                        Job2 job = (Job2) enumerator2.Current;
                        if (handler2 == null)
                        {
                            handler2 = delegate (object sender, AsyncCompletedEventArgs e) {
                                Job2 targetObject = sender as Job2;
                                this._tracer.WriteMessage("ContainerParentJob", "UnblockJob-Handler", Guid.Empty, this, "Finished unblock child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                if (e.Error != null)
                                {
                                    this.ExecutionError.Add(new ErrorRecord(e.Error, "ConainerParentJobUnblockError", ErrorCategory.InvalidResult, targetObject));
                                    this._tracer.WriteMessage("ContainerParentJob", "UnblockJob-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                    this._tracer.TraceException(e.Error);
                                }
                                Interlocked.Increment(ref unblockedChildJobsCount);
                                if (unblockedChildJobsCount == this.ChildJobs.Count)
                                {
                                    this._tracer.WriteMessage("ContainerParentJob", "UnblockJob-Handler", Guid.Empty, this, "Finished unblock all child jobs asynchronously", null);
                                    completed.Set();
                                }
                            };
                        }
                        handler = handler2;
                        job.UnblockJobCompleted += handler;
                        this._tracer.WriteMessage("ContainerParentJob", "UnblockJob", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        job.UnblockJobAsync();
                    }
                }
                completed.WaitOne();
                foreach (Job2 job3 in base.ChildJobs)
                {
                    job3.UnblockJobCompleted -= handler;
                }
                this._tracer.WriteMessage("ContainerParentJob", "UnblockJob", Guid.Empty, this, "Exiting method", null);
            }
        }

        public override void UnblockJobAsync()
        {
            int unblockedChildJobsCount;
            if (this._isDisposed == 1)
            {
                this.OnUnblockJobCompleted(new AsyncCompletedEventArgs(new ObjectDisposedException("ContainerParentJob"), false, null));
            }
            else
            {
                this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync", Guid.Empty, this, "Entering method", null);
                using (IEnumerator<Job> enumerator = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Job2) enumerator.Current) == null)
                        {
                            throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", "JobActionInvalidWithNullChild", new object[0]);
                        }
                    }
                }
                unblockedChildJobsCount = 0;
                using (IEnumerator<Job> enumerator2 = base.ChildJobs.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Job2 job = (Job2) enumerator2.Current;
                        EventHandler<AsyncCompletedEventArgs> eventHandler = null;
                        eventHandler = delegate (object sender, AsyncCompletedEventArgs e) {
                            Job2 targetObject = sender as Job2;
                            this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync-Handler", Guid.Empty, this, "Finished unblock child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                            if (e.Error != null)
                            {
                                this.ExecutionError.Add(new ErrorRecord(e.Error, "ConainerParentJobUnblockError", ErrorCategory.InvalidResult, targetObject));
                                this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync-Handler", Guid.Empty, this, "Child job asynchronously had error, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                                this._tracer.TraceException(e.Error);
                            }
                            Interlocked.Increment(ref unblockedChildJobsCount);
                            targetObject.UnblockJobCompleted -= eventHandler;
                            if (unblockedChildJobsCount == this.ChildJobs.Count)
                            {
                                this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync-Handler", Guid.Empty, this, "Finished unblock all child jobs asynchronously", null);
                                this.OnUnblockJobCompleted(new AsyncCompletedEventArgs(null, false, null));
                            }
                        };
                        job.UnblockJobCompleted += eventHandler;
                        this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync", Guid.Empty, this, "Child job asynchronously, child InstanceId: {0}", new string[] { job.InstanceId.ToString() });
                        job.UnblockJobAsync();
                    }
                }
                this._tracer.WriteMessage("ContainerParentJob", "UnblockJobAsync", Guid.Empty, this, "Exiting method", null);
            }
        }

        private void UnregisterAllJobEvents()
        {
            if (this.EventManager == null)
            {
                this._tracer.WriteMessage("No events subscribed, skipping event unregistrations");
            }
            else
            {
                foreach (Job job in base.ChildJobs)
                {
                    this.UnregisterJobEvent(job);
                }
                this.UnregisterJobEvent(this);
                this._tracer.WriteMessage("Setting event manager to null");
                this.EventManager = null;
            }
        }

        private void UnregisterJobEvent(Job job)
        {
            string sourceIdentifier = job.InstanceId + ":StateChanged";
            this._tracer.WriteMessage("Unregistering StateChanged event for job ", job.InstanceId);
            foreach (PSEventSubscriber subscriber in from subscriber in this.EventManager.Subscribers
                where string.Equals(subscriber.SourceIdentifier, sourceIdentifier, StringComparison.OrdinalIgnoreCase)
                select subscriber)
            {
                this.EventManager.UnsubscribeEvent(subscriber);
                break;
            }
        }

        internal PSEventManager EventManager
        {
            get
            {
                return this._eventManager;
            }
            set
            {
                this._tracer.WriteMessage("Setting event manager for Job ", base.InstanceId);
                this._eventManager = value;
            }
        }

        internal PSDataCollection<ErrorRecord> ExecutionError
        {
            get
            {
                return this._executionError;
            }
        }

        public override bool HasMoreData
        {
            get
            {
                if (this._moreData && base.IsFinishedState(base.JobStateInfo.State))
                {
                    bool flag = false;
                    for (int i = 0; i < base.ChildJobs.Count; i++)
                    {
                        if (base.ChildJobs[i].HasMoreData)
                        {
                            flag = true;
                            break;
                        }
                    }
                    this._moreData = flag;
                }
                return this._moreData;
            }
        }

        public override string Location
        {
            get
            {
                return this.ConstructLocation();
            }
        }

        public override string StatusMessage
        {
            get
            {
                return this.ConstructStatusMessage();
            }
        }
    }
}

