namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class PipelineStateEventArgs : EventArgs
    {
        private System.Management.Automation.Runspaces.PipelineStateInfo _pipelineStateInfo;

        internal PipelineStateEventArgs(System.Management.Automation.Runspaces.PipelineStateInfo pipelineStateInfo)
        {
            this._pipelineStateInfo = pipelineStateInfo;
        }

        public System.Management.Automation.Runspaces.PipelineStateInfo PipelineStateInfo
        {
            get
            {
                return this._pipelineStateInfo;
            }
        }
    }
}

