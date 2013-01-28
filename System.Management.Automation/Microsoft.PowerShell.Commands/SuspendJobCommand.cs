namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    [OutputType(new Type[] { typeof(System.Management.Automation.Job) }), Cmdlet("Suspend", "Job", SupportsShouldProcess=true, DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210613")]
    public class SuspendJobCommand : JobCmdletBase, IDisposable
    {
        private readonly List<System.Management.Automation.Job> _allJobsToSuspend = new List<System.Management.Automation.Job>();
        private readonly Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>> _cleanUpActions = new Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>>();
        private readonly List<ErrorRecord> _errorsToWrite = new List<ErrorRecord>();
        private bool _needToCheckForWaitingJobs;
        private readonly HashSet<Guid> _pendingJobs = new HashSet<Guid>();
        private readonly object _syncObject = new object();
        private bool _wait;
        private readonly ManualResetEvent _waitForJobs = new ManualResetEvent(false);
        private bool _warnInvalidState;
        private bool force;
        private System.Management.Automation.Job[] jobs;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Job2 job in this._cleanUpActions.Keys)
                {
                    job.SuspendJobCompleted -= this._cleanUpActions[job];
                }
                this._waitForJobs.Close();
            }
        }

        protected override void EndProcessing()
        {
            bool flag = false;
            lock (this._syncObject)
            {
                this._needToCheckForWaitingJobs = true;
                if (this._pendingJobs.Count > 0)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this._waitForJobs.WaitOne();
            }
            if (this._warnInvalidState)
            {
                base.WriteWarning(RemotingErrorIdStrings.SuspendJobInvalidJobState);
            }
            foreach (ErrorRecord record in this._errorsToWrite)
            {
                base.WriteError(record);
            }
            foreach (System.Management.Automation.Job job in this._allJobsToSuspend)
            {
                base.WriteObject(job);
            }
            base.EndProcessing();
        }

        private void HandleSuspendJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        {
            System.Management.Automation.Job job = sender as System.Management.Automation.Job;
            if ((eventArgs.Error != null) && (eventArgs.Error is InvalidJobStateException))
            {
                this._warnInvalidState = true;
            }
            this.ProcessExecutionErrorsAndReleaseWaitHandle(job);
        }

        private void noWait_Job2_StateChanged(object sender, JobStateEventArgs e)
        {
            System.Management.Automation.Job job = sender as System.Management.Automation.Job;
            switch (e.JobStateInfo.State)
            {
                case JobState.Completed:
                case JobState.Failed:
                case JobState.Stopped:
                case JobState.Suspended:
                case JobState.Suspending:
                    this.ProcessExecutionErrorsAndReleaseWaitHandle(job);
                    break;

                case JobState.Blocked:
                case JobState.Disconnected:
                    break;

                default:
                    return;
            }
        }

        private void ProcessExecutionErrorsAndReleaseWaitHandle(System.Management.Automation.Job job)
        {
            bool flag = false;
            lock (this._syncObject)
            {
                if (this._pendingJobs.Contains(job.InstanceId))
                {
                    this._pendingJobs.Remove(job.InstanceId);
                }
                else
                {
                    return;
                }
                if (this._needToCheckForWaitingJobs && (this._pendingJobs.Count == 0))
                {
                    flag = true;
                }
            }
            if (!this._wait)
            {
                job.StateChanged -= new EventHandler<JobStateEventArgs>(this.noWait_Job2_StateChanged);
                Job2 job2 = job as Job2;
                if (job2 != null)
                {
                    job2.SuspendJobCompleted -= new EventHandler<AsyncCompletedEventArgs>(this.HandleSuspendJobCompleted);
                }
            }
            ContainerParentJob job3 = job as ContainerParentJob;
            if ((job3 != null) && (job3.ExecutionError.Count > 0))
            {
                foreach (ErrorRecord record in from e in job3.ExecutionError
                    where e.FullyQualifiedErrorId == "ContainerParentJobSuspendAsyncError"
                    select e)
                {
                    if (record.Exception is InvalidJobStateException)
                    {
                        this._warnInvalidState = true;
                    }
                    else
                    {
                        this._errorsToWrite.Add(record);
                    }
                }
            }
            if (flag)
            {
                this._waitForJobs.Set();
            }
        }

        protected override void ProcessRecord()
        {
            List<System.Management.Automation.Job> collection = null;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "NameParameterSet"))
                {
                    if (parameterSetName == "InstanceIdParameterSet")
                    {
                        collection = base.FindJobsMatchingByInstanceId(true, false, true, false);
                        goto Label_00A2;
                    }
                    if (parameterSetName == "SessionIdParameterSet")
                    {
                        collection = base.FindJobsMatchingBySessionId(true, false, true, false);
                        goto Label_00A2;
                    }
                    if (parameterSetName == "StateParameterSet")
                    {
                        collection = base.FindJobsMatchingByState(false);
                        goto Label_00A2;
                    }
                    if (parameterSetName == "FilterParameterSet")
                    {
                        collection = base.FindJobsMatchingByFilter(false);
                        goto Label_00A2;
                    }
                }
                else
                {
                    collection = base.FindJobsMatchingByName(true, false, true, false);
                    goto Label_00A2;
                }
            }
            collection = base.CopyJobsToList(this.jobs, false, false);
        Label_00A2:
            this._allJobsToSuspend.AddRange(collection);
            foreach (System.Management.Automation.Job job in collection)
            {
                Job2 key = job as Job2;
                if (key == null)
                {
                    base.WriteError(new ErrorRecord(PSTraceSource.NewNotSupportedException("RemotingErrorIdStrings", "JobSuspendNotSupported", new object[] { job.Id }), "Job2OperationNotSupportedOnJob", ErrorCategory.InvalidType, job));
                }
                else
                {
                    string target = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemovePSJobWhatIfTarget, new object[] { job.Command, job.Id });
                    if (base.ShouldProcess(target, "Suspend"))
                    {
                        if (this._wait)
                        {
                            this._cleanUpActions.Add(key, new EventHandler<AsyncCompletedEventArgs>(this.HandleSuspendJobCompleted));
                        }
                        else
                        {
                            if (key.IsFinishedState(key.JobStateInfo.State) || (key.JobStateInfo.State == JobState.Stopping))
                            {
                                this._warnInvalidState = true;
                                goto Label_0277;
                            }
                            if ((key.JobStateInfo.State == JobState.Suspending) || (key.JobStateInfo.State == JobState.Suspended))
                            {
                                goto Label_0277;
                            }
                            key.StateChanged += new EventHandler<JobStateEventArgs>(this.noWait_Job2_StateChanged);
                        }
                        key.SuspendJobCompleted += new EventHandler<AsyncCompletedEventArgs>(this.HandleSuspendJobCompleted);
                        lock (this._syncObject)
                        {
                            if (!this._pendingJobs.Contains(key.InstanceId))
                            {
                                this._pendingJobs.Add(key.InstanceId);
                            }
                        }
                        if (!this._wait && ((key.IsFinishedState(key.JobStateInfo.State) || (key.JobStateInfo.State == JobState.Suspending)) || (key.JobStateInfo.State == JobState.Suspended)))
                        {
                            this.ProcessExecutionErrorsAndReleaseWaitHandle(key);
                        }
                        key.SuspendJobAsync(this.force, RemotingErrorIdStrings.ForceSuspendJob);
                    }
                Label_0277:;
                }
            }
        }

        protected override void StopProcessing()
        {
            this._waitForJobs.Set();
        }

        public override string[] Command
        {
            get
            {
                return null;
            }
        }

        [Parameter(ParameterSetName="StateParameterSet"), Parameter(ParameterSetName="FilterParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="JobParameterSet"), Alias(new string[] { "F" }), Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="NameParameterSet")]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="JobParameterSet")]
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
        public SwitchParameter Wait
        {
            get
            {
                return this._wait;
            }
            set
            {
                this._wait = (bool) value;
            }
        }
    }
}

