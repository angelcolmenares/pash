namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ExecutionCmdletHelperComputerName : ExecutionCmdletHelper
    {
        private bool invokeAndDisconnect;
        private System.Management.Automation.RemoteRunspace remoteRunspace;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal ExecutionCmdletHelperComputerName(System.Management.Automation.RemoteRunspace remoteRunspace, Pipeline pipeline, bool invokeAndDisconnect = false)
        {
            this.invokeAndDisconnect = invokeAndDisconnect;
            this.remoteRunspace = remoteRunspace;
            remoteRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
            base.pipeline = pipeline;
            pipeline.StateChanged += new EventHandler<PipelineStateEventArgs>(this.HandlePipelineStateChanged);
        }

        private void HandlePipelineStateChanged(object sender, PipelineStateEventArgs stateEventArgs)
        {
            switch (stateEventArgs.PipelineStateInfo.State)
            {
                case PipelineState.Stopped:
                case PipelineState.Completed:
                case PipelineState.Failed:
                    if (this.remoteRunspace != null)
                    {
                        this.remoteRunspace.CloseAsync();
                    }
                    return;
            }
        }

        private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs stateEventArgs)
        {
            switch (stateEventArgs.RunspaceStateInfo.State)
            {
                case RunspaceState.Opened:
                    try
                    {
                        if (this.invokeAndDisconnect)
                        {
                            base.pipeline.InvokeAsyncAndDisconnect();
                        }
                        else
                        {
                            base.pipeline.InvokeAsync();
                        }
                    }
                    catch (InvalidPipelineStateException)
                    {
                        this.remoteRunspace.CloseAsync();
                    }
                    catch (InvalidRunspaceStateException exception)
                    {
                        base.internalException = exception;
                        this.remoteRunspace.CloseAsync();
                    }
                    return;

                case RunspaceState.Closed:
                    if (stateEventArgs.RunspaceStateInfo.Reason == null)
                    {
                        this.RaiseOperationCompleteEvent();
                        return;
                    }
                    this.RaiseOperationCompleteEvent(stateEventArgs);
                    return;

                case RunspaceState.Broken:
                    this.RaiseOperationCompleteEvent(stateEventArgs);
                    return;
            }
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
            if (this.remoteRunspace != null)
            {
                this.remoteRunspace.Dispose();
                this.remoteRunspace = null;
            }
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StopComplete,
                BaseEvent = baseEventArgs
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
        }

        internal override void StartOperation()
        {
            try
            {
                this.remoteRunspace.OpenAsync();
            }
            catch (PSRemotingTransportException exception)
            {
                base.internalException = exception;
                this.RaiseOperationCompleteEvent();
            }
        }

        internal override void StopOperation()
        {
            bool flag = false;
            if ((base.pipeline.PipelineStateInfo.State == PipelineState.Running) || (base.pipeline.PipelineStateInfo.State == PipelineState.NotStarted))
            {
                flag = true;
            }
            if (flag)
            {
                base.pipeline.StopAsync();
            }
            else
            {
                this.RaiseOperationCompleteEvent();
            }
        }

        internal System.Management.Automation.RemoteRunspace RemoteRunspace
        {
            get
            {
                return this.remoteRunspace;
            }
        }
    }
}

