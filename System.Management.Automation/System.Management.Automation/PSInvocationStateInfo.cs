namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    public sealed class PSInvocationStateInfo
    {
        private Exception exceptionReason;
        private PSInvocationState executionState;

        internal PSInvocationStateInfo(PipelineStateInfo pipelineStateInfo)
        {
            this.executionState = (PSInvocationState) pipelineStateInfo.State;
            this.exceptionReason = pipelineStateInfo.Reason;
        }

        internal PSInvocationStateInfo(PSInvocationState state, Exception reason)
        {
            this.executionState = state;
            this.exceptionReason = reason;
        }

        internal PSInvocationStateInfo Clone()
        {
            return new PSInvocationStateInfo(this.executionState, this.exceptionReason);
        }

        public Exception Reason
        {
            get
            {
                return this.exceptionReason;
            }
        }

        public PSInvocationState State
        {
            get
            {
                return this.executionState;
            }
        }
    }
}

