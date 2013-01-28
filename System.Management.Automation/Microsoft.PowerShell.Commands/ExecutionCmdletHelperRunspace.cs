namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ExecutionCmdletHelperRunspace : ExecutionCmdletHelper
    {
        internal bool ShouldUseSteppablePipelineOnServer;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal ExecutionCmdletHelperRunspace(Pipeline pipeline)
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
            this.RaiseOperationCompleteEvent(stateEventArgs);
        }

        private void RaiseOperationCompleteEvent()
        {
            this.RaiseOperationCompleteEvent(null);
        }

        private void RaiseOperationCompleteEvent(EventArgs baseEventArgs)
        {
            if (base.pipeline != null)
            {
                base.pipeline.StateChanged -= new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
                base.pipeline.Dispose();
            }
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StopComplete,
                BaseEvent = baseEventArgs
            };
            if (this.OperationComplete != null)
            {
                this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
            }
        }

        internal override void StartOperation()
        {
            try
            {
                if (this.ShouldUseSteppablePipelineOnServer)
                {
                    RemotePipeline pipeline = base.pipeline as RemotePipeline;
                    pipeline.SetIsNested(true);
                    pipeline.SetIsSteppable(true);
                }
                base.pipeline.InvokeAsync();
            }
            catch (InvalidRunspaceStateException exception)
            {
                base.internalException = exception;
                this.RaiseOperationCompleteEvent();
            }
            catch (InvalidPipelineStateException exception2)
            {
                base.internalException = exception2;
                this.RaiseOperationCompleteEvent();
            }
            catch (InvalidOperationException exception3)
            {
                base.internalException = exception3;
                this.RaiseOperationCompleteEvent();
            }
        }

        internal override void StopOperation()
        {
            if (((base.pipeline.PipelineStateInfo.State == PipelineState.Running) || (base.pipeline.PipelineStateInfo.State == PipelineState.Disconnected)) || (base.pipeline.PipelineStateInfo.State == PipelineState.NotStarted))
            {
                base.pipeline.StopAsync();
            }
            else
            {
                this.RaiseOperationCompleteEvent();
            }
        }
    }
}

