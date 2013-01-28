namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Cmdlet("Resume", "Job", SupportsShouldProcess=true, DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210611"), OutputType(new Type[] { typeof(System.Management.Automation.Job) })]
    public class ResumeJobCommand : JobCmdletBase, IDisposable
    {
        private readonly List<System.Management.Automation.Job> _allJobsToResume = new List<System.Management.Automation.Job>();
        private readonly Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>> _cleanUpActions = new Dictionary<Job2, EventHandler<AsyncCompletedEventArgs>>();
        private readonly List<ErrorRecord> _errorsToWrite = new List<ErrorRecord>();
        private bool _needToCheckForWaitingJobs;
        private readonly HashSet<Guid> _pendingJobs = new HashSet<Guid>();
        private readonly object _syncObject = new object();
        private readonly ManualResetEvent _waitForJobs = new ManualResetEvent(false);
        private bool _warnInvalidState;
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
                    job.ResumeJobCompleted -= this._cleanUpActions[job];
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
            if ((this.Wait != 0) && flag)
            {
                this._waitForJobs.WaitOne();
            }
            if (this._warnInvalidState)
            {
                base.WriteWarning(RemotingErrorIdStrings.ResumeJobInvalidJobState);
            }
            foreach (ErrorRecord record in this._errorsToWrite)
            {
                base.WriteError(record);
            }
            foreach (System.Management.Automation.Job job in this._allJobsToResume)
            {
                base.WriteObject(job);
            }
            base.EndProcessing();
        }

        private void HandleResumeJobCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        {
            System.Management.Automation.Job job = sender as System.Management.Automation.Job;
            if ((eventArgs.Error != null) && (eventArgs.Error is InvalidJobStateException))
            {
                this._warnInvalidState = true;
            }
            ContainerParentJob job2 = job as ContainerParentJob;
            if ((job2 != null) && (job2.ExecutionError.Count > 0))
            {
                foreach (ErrorRecord record in from e in job2.ExecutionError
                    where e.FullyQualifiedErrorId == "ContainerParentJobResumeAsyncError"
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
                job2.ExecutionError.Clear();
            }
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
            this._allJobsToResume.AddRange(collection);
            foreach (System.Management.Automation.Job job in collection)
            {
                Job2 key = job as Job2;
                if (key == null)
                {
                    base.WriteError(new ErrorRecord(PSTraceSource.NewNotSupportedException("RemotingErrorIdStrings", "JobResumeNotSupported", new object[] { job.Id }), "Job2OperationNotSupportedOnJob", ErrorCategory.InvalidType, job));
                }
                else
                {
                    string target = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemovePSJobWhatIfTarget, new object[] { job.Command, job.Id });
                    if (base.ShouldProcess(target, "Resume"))
                    {
                        this._cleanUpActions.Add(key, new EventHandler<AsyncCompletedEventArgs>(this.HandleResumeJobCompleted));
                        key.ResumeJobCompleted += new EventHandler<AsyncCompletedEventArgs>(this.HandleResumeJobCompleted);
                        lock (this._syncObject)
                        {
                            if (!this._pendingJobs.Contains(key.InstanceId))
                            {
                                this._pendingJobs.Add(key.InstanceId);
                            }
                        }
                        key.ResumeJobAsync();
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

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="JobParameterSet"), ValidateNotNullOrEmpty]
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

        [Parameter(ParameterSetName="__AllParameterSets")]
        public SwitchParameter Wait { get; set; }
    }
}

