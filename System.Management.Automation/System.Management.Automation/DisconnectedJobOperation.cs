namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class DisconnectedJobOperation : ExecutionCmdletHelper
    {
        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal DisconnectedJobOperation(Pipeline pipeline)
        {
            base.pipeline = pipeline;
            base.pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
        }

        private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs stateEventArgs)
        {
            switch (stateEventArgs.PipelineStateInfo.State)
            {
                case PipelineState.NotStarted:
                case PipelineState.Running:
                case PipelineState.Stopping:
                case PipelineState.Disconnected:
                    return;
            }
            this.SendStopComplete(stateEventArgs);
        }

        private void SendStopComplete(EventArgs eventArgs = null)
        {
            OperationStateEventArgs args = new OperationStateEventArgs {
                BaseEvent = eventArgs,
                OperationState = OperationState.StopComplete
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, args);
        }

        internal override void StartOperation()
        {
        }

        internal override void StopOperation()
        {
            if (((base.pipeline.PipelineStateInfo.State == PipelineState.Running) || (base.pipeline.PipelineStateInfo.State == PipelineState.Disconnected)) || (base.pipeline.PipelineStateInfo.State == PipelineState.NotStarted))
            {
                base.pipeline.StopAsync();
            }
            else
            {
                this.SendStopComplete(null);
            }
        }
    }
}

