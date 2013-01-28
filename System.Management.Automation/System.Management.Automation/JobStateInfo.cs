namespace System.Management.Automation
{
    using System;

    public sealed class JobStateInfo
    {
        private Exception _reason;
        private JobState _state;

        public JobStateInfo(JobState state) : this(state, null)
        {
        }

        internal JobStateInfo(JobStateInfo jobStateInfo)
        {
            this._state = jobStateInfo.State;
            this._reason = jobStateInfo.Reason;
        }

        public JobStateInfo(JobState state, Exception reason)
        {
            this._state = state;
            this._reason = reason;
        }

        internal JobStateInfo Clone()
        {
            return new JobStateInfo(this);
        }

        public override string ToString()
        {
            return this._state.ToString();
        }

        public Exception Reason
        {
            get
            {
                return this._reason;
            }
        }

        public JobState State
        {
            get
            {
                return this._state;
            }
        }
    }
}

