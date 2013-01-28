namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;

    [Cmdlet("Wait", "Job", DefaultParameterSetName="SessionIdParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113422"), OutputType(new Type[] { typeof(System.Management.Automation.Job) })]
    public class WaitJobCommand : JobCmdletBase, IDisposable
    {
        private readonly HashSet<System.Management.Automation.Job> _blockedJobs = new HashSet<System.Management.Automation.Job>();
        private readonly object _disposableLock = new object();
        private Action _endProcessingAction;
        private readonly ManualResetEventSlim _endProcessingActionIsReady = new ManualResetEventSlim(false);
        private readonly object _endProcessingActionLock = new object();
        private readonly HashSet<System.Management.Automation.Job> _finishedJobs = new HashSet<System.Management.Automation.Job>();
        private bool _isDisposed;
        private readonly List<System.Management.Automation.Job> _jobsToWaitFor = new List<System.Management.Automation.Job>();
        private readonly object _jobTrackingLock = new object();
        private int _timeoutInSeconds = -1;
        private System.Timers.Timer _timer;
        private readonly object _timerLock = new object();
        private bool _warnNotTerminal;

        private void AddJobsThatNeedJobChangesTracking(IEnumerable<System.Management.Automation.Job> jobsToAdd)
        {
            lock (this._jobTrackingLock)
            {
                this._jobsToWaitFor.AddRange(jobsToAdd);
            }
        }

        protected override void BeginProcessing()
        {
            this.StartTimeoutTracking(this._timeoutInSeconds);
        }

        private void CleanUpEndProcessing()
        {
            this._endProcessingActionIsReady.Dispose();
        }

        private void CleanUpJobChangesTracking()
        {
            lock (this._jobTrackingLock)
            {
                foreach (System.Management.Automation.Job job in this._jobsToWaitFor)
                {
                    job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChangedEvent);
                }
            }
        }

        private void CleanUpTimoutTracking()
        {
            lock (this._timerLock)
            {
                if (this._timer != null)
                {
                    this._timer.Dispose();
                    this._timer = null;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this._disposableLock)
                {
                    if (!this._isDisposed)
                    {
                        this._isDisposed = true;
                        this.CleanUpTimoutTracking();
                        this.CleanUpJobChangesTracking();
                        this.CleanUpEndProcessing();
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            this.StartJobChangesTracking();
            this.InvokeEndProcesingAction();
            if (this._warnNotTerminal)
            {
                base.WriteWarning(RemotingErrorIdStrings.JobSuspendedDisconnectedWaitWithForce);
            }
        }

        private void EndProcessingBlockedJobsError()
        {
            Exception exception = new ArgumentException(RemotingErrorIdStrings.JobBlockedSoWaitJobCannotContinue);
            ErrorRecord errorRecord = new ErrorRecord(exception, "BlockedJobsDeadlockWithWaitJob", ErrorCategory.DeadlockDetected, this.GetOneBlockedJob());
            base.ThrowTerminatingError(errorRecord);
        }

        private void EndProcessingDoNothing()
        {
        }

        private void EndProcessingOutputAllFinishedJobs()
        {
            foreach (System.Management.Automation.Job job in this.GetFinishedJobs())
            {
                base.WriteObject(job);
            }
        }

        private void EndProcessingOutputSingleFinishedJob()
        {
            System.Management.Automation.Job sendToPipeline = this.GetFinishedJobs().FirstOrDefault<System.Management.Automation.Job>();
            if (sendToPipeline != null)
            {
                base.WriteObject(sendToPipeline);
            }
        }

        private List<System.Management.Automation.Job> GetFinishedJobs()
        {
            Func<System.Management.Automation.Job, bool> predicate = null;
            lock (this._jobTrackingLock)
            {
                if (predicate == null)
                {
                    predicate = j => ((this.Force == 0) && j.IsPersistentState(j.JobStateInfo.State)) || ((this.Force != 0) && j.IsFinishedState(j.JobStateInfo.State));
                }
                return this._jobsToWaitFor.Where<System.Management.Automation.Job>(predicate).ToList<System.Management.Automation.Job>();
            }
        }

        private System.Management.Automation.Job GetOneBlockedJob()
        {
            lock (this._jobTrackingLock)
            {
                return this._jobsToWaitFor.FirstOrDefault<System.Management.Automation.Job>(j => (j.JobStateInfo.State == JobState.Blocked));
            }
        }

        private void HandleJobStateChangedEvent(object source, JobStateEventArgs eventArgs)
        {
            System.Management.Automation.Job item = (System.Management.Automation.Job) source;
            lock (this._jobTrackingLock)
            {
                if (eventArgs.JobStateInfo.State == JobState.Blocked)
                {
                    this._blockedJobs.Add(item);
                }
                else
                {
                    this._blockedJobs.Remove(item);
                }
                if (((this.Force == 0) && item.IsPersistentState(eventArgs.JobStateInfo.State)) || ((this.Force != 0) && item.IsFinishedState(eventArgs.JobStateInfo.State)))
                {
                    if (!item.IsFinishedState(eventArgs.JobStateInfo.State))
                    {
                        this._warnNotTerminal = true;
                    }
                    this._finishedJobs.Add(item);
                }
                else
                {
                    this._finishedJobs.Remove(item);
                }
                if (this.Any.IsPresent)
                {
                    if (this._finishedJobs.Count > 0)
                    {
                        this.SetEndProcessingAction(new Action(this.EndProcessingOutputSingleFinishedJob));
                    }
                    else if (this._blockedJobs.Count == this._jobsToWaitFor.Count)
                    {
                        this.SetEndProcessingAction(new Action(this.EndProcessingBlockedJobsError));
                    }
                }
                else if (this._finishedJobs.Count == this._jobsToWaitFor.Count)
                {
                    this.SetEndProcessingAction(new Action(this.EndProcessingOutputAllFinishedJobs));
                }
                else if (this._blockedJobs.Count > 0)
                {
                    this.SetEndProcessingAction(new Action(this.EndProcessingBlockedJobsError));
                }
            }
        }

        private void InvokeEndProcesingAction()
        {
            Action action;
            this._endProcessingActionIsReady.Wait();
            lock (this._endProcessingActionLock)
            {
                action = this._endProcessingAction;
            }
            if (action != null)
            {
                action();
            }
        }

        protected override void ProcessRecord()
        {
            List<System.Management.Automation.Job> list;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "NameParameterSet"))
                {
                    if (parameterSetName == "InstanceIdParameterSet")
                    {
                        list = base.FindJobsMatchingByInstanceId(true, false, true, false);
                        goto Label_0097;
                    }
                    if (parameterSetName == "SessionIdParameterSet")
                    {
                        list = base.FindJobsMatchingBySessionId(true, false, true, false);
                        goto Label_0097;
                    }
                    if (parameterSetName == "StateParameterSet")
                    {
                        list = base.FindJobsMatchingByState(false);
                        goto Label_0097;
                    }
                    if (parameterSetName == "FilterParameterSet")
                    {
                        list = base.FindJobsMatchingByFilter(false);
                        goto Label_0097;
                    }
                }
                else
                {
                    list = base.FindJobsMatchingByName(true, false, true, false);
                    goto Label_0097;
                }
            }
            list = base.CopyJobsToList(this.Job, false, false);
        Label_0097:
            this.AddJobsThatNeedJobChangesTracking(list);
        }

        private void SetEndProcessingAction(Action endProcessingAction)
        {
            lock (this._endProcessingActionLock)
            {
                if (this._endProcessingAction == null)
                {
                    this._endProcessingAction = endProcessingAction;
                    this._endProcessingActionIsReady.Set();
                }
            }
        }

        private void StartJobChangesTracking()
        {
            lock (this._jobTrackingLock)
            {
                if (this._jobsToWaitFor.Count == 0)
                {
                    this.SetEndProcessingAction(new Action(this.EndProcessingDoNothing));
                }
                else
                {
                    foreach (System.Management.Automation.Job job in this._jobsToWaitFor)
                    {
                        job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleJobStateChangedEvent);
                        this.HandleJobStateChangedEvent(job, new JobStateEventArgs(job.JobStateInfo));
                    }
                }
            }
        }

        private void StartTimeoutTracking(int timeoutInSeconds)
        {
            ElapsedEventHandler handler = null;
            if (timeoutInSeconds == 0)
            {
                this.SetEndProcessingAction(new Action(this.EndProcessingDoNothing));
            }
            else if (timeoutInSeconds > 0)
            {
                lock (this._timerLock)
                {
                    System.Timers.Timer timer = new System.Timers.Timer {
                        Interval = timeoutInSeconds * 0x3e8,
                        AutoReset = false
                    };
                    this._timer = timer;
                    if (handler == null)
                    {
                        handler = (_, eventArgs) => this.SetEndProcessingAction(new Action(this.EndProcessingDoNothing));
                    }
                    this._timer.Elapsed += handler;
                    this._timer.Start();
                }
            }
        }

        protected override void StopProcessing()
        {
            this.SetEndProcessingAction(new Action(this.EndProcessingDoNothing));
        }

        [Parameter]
        public SwitchParameter Any { get; set; }

        public override string[] Command { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="JobParameterSet"), ValidateNotNullOrEmpty]
        public System.Management.Automation.Job[] Job { get; set; }

        [Parameter, Alias(new string[] { "TimeoutSec" }), ValidateRange(-1, 0x7fffffff)]
        public int Timeout
        {
            get
            {
                return this._timeoutInSeconds;
            }
            set
            {
                this._timeoutInSeconds = value;
            }
        }
    }
}

