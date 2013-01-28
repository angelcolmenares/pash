namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    [OutputType(new Type[] { typeof(System.Management.Automation.Job) }), Cmdlet("Stop", "Job", SupportsShouldProcess=true, DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113413")]
    public class StopJobCommand : JobCmdletBase, IDisposable
    {
        private readonly List<System.Management.Automation.Job> _allJobsToStop = new List<System.Management.Automation.Job>();
        private readonly Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>> _cleanUpActions = new Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>>();
        private readonly List<ErrorRecord> _errorsToWrite = new List<ErrorRecord>();
        private bool _needToCheckForWaitingJobs;
        private readonly HashSet<Guid> _pendingJobs = new HashSet<Guid>();
        private readonly object _syncObject = new object();
        private readonly ManualResetEvent _waitForJobs = new ManualResetEvent(false);
        private System.Management.Automation.Job[] jobs;
        private bool passThru;

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
            foreach (ErrorRecord record in this._errorsToWrite)
            {
                base.WriteError(record);
            }
            if (this.passThru)
            {
                foreach (System.Management.Automation.Job job in this._allJobsToStop)
                {
                    base.WriteObject(job);
                }
            }
        }

        private void HandleStopJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        {
            System.Management.Automation.Job targetObject = sender as System.Management.Automation.Job;
            if (eventArgs.Error != null)
            {
                this._errorsToWrite.Add(new ErrorRecord(eventArgs.Error, "StopJobError", ErrorCategory.ReadError, targetObject));
            }
            ContainerParentJob job2 = targetObject as ContainerParentJob;
            if ((job2 != null) && (job2.ExecutionError.Count > 0))
            {
                foreach (ErrorRecord record in from e in job2.ExecutionError
                    where e.FullyQualifiedErrorId == "ContainerParentJobStopAsyncError"
                    select e)
                {
                    this._errorsToWrite.Add(record);
                }
            }
            bool flag = false;
            lock (this._syncObject)
            {
                if (this._pendingJobs.Contains(targetObject.InstanceId))
                {
                    this._pendingJobs.Remove(targetObject.InstanceId);
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
            this._allJobsToStop.AddRange(collection);
            foreach (System.Management.Automation.Job job in collection)
            {
                if (base.Stopping)
                {
                    break;
                }
                if (!job.IsFinishedState(job.JobStateInfo.State))
                {
                    string target = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemovePSJobWhatIfTarget, new object[] { job.Command, job.Id });
                    if (base.ShouldProcess(target, "Stop"))
                    {
                        Job2 key = job as Job2;
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
                            ContainerParentJob job3 = job as ContainerParentJob;
                            if ((job3 != null) && (job3.ExecutionError.Count > 0))
                            {
                                foreach (ErrorRecord record in from e in job3.ExecutionError
                                    where e.FullyQualifiedErrorId == "ContainerParentJobStopError"
                                    select e)
                                {
                                    base.WriteError(record);
                                }
                            }
                        }
                    }
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
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThru;
            }
            set
            {
                this.passThru = (bool) value;
            }
        }
    }
}

