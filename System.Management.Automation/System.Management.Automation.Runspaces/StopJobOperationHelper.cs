namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    internal sealed class StopJobOperationHelper : IThrottleOperation
    {
        private Job job;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal StopJobOperationHelper(Job job)
        {
            this.job = job;
            this.job.StateChanged += new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
        }

        private void HandleJobStateChanged(object sender, JobStateEventArgs eventArgs)
        {
            if (this.job.IsFinishedState(this.job.JobStateInfo.State))
            {
                this.RaiseOperationCompleteEvent();
            }
        }

        private void RaiseOperationCompleteEvent()
        {
            this.job.StateChanged -= new EventHandler<JobStateEventArgs>(this.HandleJobStateChanged);
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StartComplete,
                BaseEvent = EventArgs.Empty
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
        }

        internal override void StartOperation()
        {
            if (this.job.IsFinishedState(this.job.JobStateInfo.State))
            {
                this.RaiseOperationCompleteEvent();
            }
            else
            {
                this.job.StopJob();
            }
        }

        internal override void StopOperation()
        {
        }
    }
}

