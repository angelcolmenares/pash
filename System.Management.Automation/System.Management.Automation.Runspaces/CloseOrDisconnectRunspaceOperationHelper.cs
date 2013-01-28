namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    internal sealed class CloseOrDisconnectRunspaceOperationHelper : IThrottleOperation
    {
        private RemoteRunspace remoteRunspace;

        internal override event EventHandler<OperationStateEventArgs> OperationComplete;

        internal CloseOrDisconnectRunspaceOperationHelper(RemoteRunspace remoteRunspace)
        {
            this.remoteRunspace = remoteRunspace;
            this.remoteRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
        }

        private void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs eventArgs)
        {
            switch (eventArgs.RunspaceStateInfo.State)
            {
                case RunspaceState.BeforeOpen:
                case RunspaceState.Opening:
                case RunspaceState.Opened:
                case RunspaceState.Closing:
                case RunspaceState.Disconnecting:
                    return;
            }
            this.RaiseOperationCompleteEvent();
        }

        private void RaiseOperationCompleteEvent()
        {
            this.remoteRunspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(this.HandleRunspaceStateChanged);
            OperationStateEventArgs eventArgs = new OperationStateEventArgs {
                OperationState = OperationState.StartComplete,
                BaseEvent = EventArgs.Empty
            };
            this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, eventArgs);
        }

        internal override void StartOperation()
        {
            if (((this.remoteRunspace.RunspaceStateInfo.State == RunspaceState.Closed) || (this.remoteRunspace.RunspaceStateInfo.State == RunspaceState.Broken)) || (this.remoteRunspace.RunspaceStateInfo.State == RunspaceState.Disconnected))
            {
                this.RaiseOperationCompleteEvent();
            }
            else if (this.remoteRunspace.CanDisconnect && (this.remoteRunspace.GetCurrentlyRunningPipeline() != null))
            {
                this.remoteRunspace.DisconnectAsync();
            }
            else
            {
                this.remoteRunspace.CloseAsync();
            }
        }

        internal override void StopOperation()
        {
        }
    }
}

