namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class PipelineStateInfo
    {
        private Exception _reason;
        private PipelineState _state;

        internal PipelineStateInfo(PipelineState state) : this(state, null)
        {
        }

        internal PipelineStateInfo(PipelineStateInfo pipelineStateInfo)
        {
            this._state = pipelineStateInfo.State;
            this._reason = pipelineStateInfo.Reason;
        }

        internal PipelineStateInfo(PipelineState state, Exception reason)
        {
            this._state = state;
            this._reason = reason;
        }

        internal PipelineStateInfo Clone()
        {
            return new PipelineStateInfo(this);
        }

        public Exception Reason
        {
            get
            {
                return this._reason;
            }
        }

        public PipelineState State
        {
            get
            {
                return this._state;
            }
        }
    }
}

