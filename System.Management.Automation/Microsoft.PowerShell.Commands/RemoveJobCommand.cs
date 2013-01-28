namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    [Cmdlet("Remove", "Job", SupportsShouldProcess=true, DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113377"), OutputType(new Type[] { typeof(System.Management.Automation.Job) }, ParameterSetName=new string[] { "JobParameterSet" })]
    public class RemoveJobCommand : JobCmdletBase, IDisposable
    {
        private readonly Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>> _cleanUpActions = new Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>>();
        private bool _needToCheckForWaitingJobs;
        private HashSet<Guid> _pendingJobs = new HashSet<Guid>();
        private readonly object _syncObject = new object();
        private readonly ManualResetEvent _waitForJobs = new ManualResetEvent(false);
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
                    job.StopJobCompleted -= this._cleanUpActions[job];
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
        }

        private void HandleStopJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        {
            System.Management.Automation.Job job = sender as System.Management.Automation.Job;
            this.RemoveJobAndDispose(job, true);
            bool flag = false;
            lock (this._syncObject)
            {
                if (this._pendingJobs.Contains(job.InstanceId))
                {
                    this._pendingJobs.Remove(job.InstanceId);
                }
                if (this._needToCheckForWaitingJobs && (this._pendingJobs.Count == 0))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this._waitForJobs.Set();
            }
        }

        protected override void ProcessRecord()
        {
            List<System.Management.Automation.Job> list = null;
            switch (base.ParameterSetName)
            {
                case "NameParameterSet":
                    list = base.FindJobsMatchingByName(false, false, true, !this.force);
                    break;

                case "InstanceIdParameterSet":
                    list = base.FindJobsMatchingByInstanceId(true, false, true, !this.force);
                    break;

                case "SessionIdParameterSet":
                    list = base.FindJobsMatchingBySessionId(true, false, true, !this.force);
                    break;

                case "CommandParameterSet":
                    list = base.FindJobsMatchingByCommand(false);
                    break;

                case "StateParameterSet":
                    list = base.FindJobsMatchingByState(false);
                    break;

                case "FilterParameterSet":
                    list = base.FindJobsMatchingByFilter(false);
                    break;

                default:
                    list = base.CopyJobsToList(this.jobs, false, !this.force);
                    break;
            }
            foreach (System.Management.Automation.Job job in list)
            {
                string message = base.GetMessage(RemotingErrorIdStrings.StopPSJobWhatIfTarget, new object[] { job.Command, job.Id });
                if (base.ShouldProcess(message, "Remove"))
                {
                    Job2 key = job as Job2;
                    if (!job.IsFinishedState(job.JobStateInfo.State))
                    {
                        if (key != null)
                        {
                            this._cleanUpActions.Add(key, new EventHandler<AsyncCompletedEventArgs>(this.HandleStopJobCompleted));
                            key.StopJobCompleted += new EventHandler<AsyncCompletedEventArgs>(this.HandleStopJobCompleted);
                            lock (this._syncObject)
                            {
                                if (!key.IsFinishedState(key.JobStateInfo.State) && !this._pendingJobs.Contains(key.InstanceId))
                                {
                                    this._pendingJobs.Add(key.InstanceId);
                                }
                            }
                            key.StopJobAsync();
                        }
                        else
                        {
                            job.StopJob();
                            this.RemoveJobAndDispose(job, false);
                        }
                    }
                    else
                    {
                        this.RemoveJobAndDispose(job, key != null);
                    }
                }
            }
        }

        private void RemoveJobAndDispose(System.Management.Automation.Job job, bool jobIsJob2)
        {
            try
            {
                if (jobIsJob2)
                {
                    base.JobManager.RemoveJob(job as Job2, this, true, false);
                }
                else
                {
                    base.JobRepository.Remove(job);
                }
                job.Dispose();
            }
            catch (ArgumentException exception)
            {
                ArgumentException exception2 = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CannotRemoveJob, new object[0]), exception);
                base.WriteError(new ErrorRecord(exception2, "CannotRemoveJob", ErrorCategory.InvalidOperation, job));
            }
        }

        protected override void StopProcessing()
        {
            this._waitForJobs.Set();
        }

        [Parameter(ParameterSetName="SessionIdParameterSet"), Parameter(ParameterSetName="InstanceIdParameterSet"), Parameter(ParameterSetName="FilterParameterSet"), Alias(new string[] { "F" }), Parameter(ParameterSetName="JobParameterSet"), Parameter(ParameterSetName="NameParameterSet")]
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
    }
}

