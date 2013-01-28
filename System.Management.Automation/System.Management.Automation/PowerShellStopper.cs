namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    internal class PowerShellStopper : IDisposable
    {
        private EventHandler<PipelineStateEventArgs> eventHandler;
        private bool isDisposed;
        private PipelineBase pipeline;
        private PowerShell powerShell;

        internal PowerShellStopper(ExecutionContext context, PowerShell powerShell)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (powerShell == null)
            {
                throw new ArgumentNullException("powerShell");
            }
            this.powerShell = powerShell;
            if (((context.CurrentCommandProcessor != null) && (context.CurrentCommandProcessor.CommandRuntime != null)) && ((context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor != null) && (context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline != null)))
            {
                this.eventHandler = new EventHandler<PipelineStateEventArgs>(this.LocalPipeline_StateChanged);
                this.pipeline = context.CurrentCommandProcessor.CommandRuntime.PipelineProcessor.LocalPipeline;
                this.pipeline.StateChanged += this.eventHandler;
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                if (this.eventHandler != null)
                {
                    this.pipeline.StateChanged -= this.eventHandler;
                    this.eventHandler = null;
                }
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }

        private void LocalPipeline_StateChanged(object sender, PipelineStateEventArgs e)
        {
            if ((e.PipelineStateInfo.State == PipelineState.Stopping) && (this.powerShell.InvocationStateInfo.State == PSInvocationState.Running))
            {
                this.powerShell.Stop();
            }
        }
    }
}

