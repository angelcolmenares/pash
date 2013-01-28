namespace System.Management.Automation
{
    using System;

    public sealed class JobStateEventArgs : EventArgs
    {
        private readonly System.Management.Automation.JobStateInfo _jobStateInfo;
        private readonly System.Management.Automation.JobStateInfo _previousJobStateInfo;

        public JobStateEventArgs(System.Management.Automation.JobStateInfo jobStateInfo) : this(jobStateInfo, null)
        {
        }

        public JobStateEventArgs(System.Management.Automation.JobStateInfo jobStateInfo, System.Management.Automation.JobStateInfo previousJobStateInfo)
        {
            if (jobStateInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("jobStateInfo");
            }
            this._jobStateInfo = jobStateInfo;
            this._previousJobStateInfo = previousJobStateInfo;
        }

        public System.Management.Automation.JobStateInfo JobStateInfo
        {
            get
            {
                return this._jobStateInfo;
            }
        }

        public System.Management.Automation.JobStateInfo PreviousJobStateInfo
        {
            get
            {
                return this._previousJobStateInfo;
            }
        }
    }
}

