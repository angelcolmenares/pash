namespace System.Management.Automation.Remoting
{
    using System;

    internal sealed class OperationStateEventArgs : EventArgs
    {
        private EventArgs baseEvent;
        private System.Management.Automation.Remoting.OperationState operationState;

        internal EventArgs BaseEvent
        {
            get
            {
                return this.baseEvent;
            }
            set
            {
                this.baseEvent = value;
            }
        }

        internal System.Management.Automation.Remoting.OperationState OperationState
        {
            get
            {
                return this.operationState;
            }
            set
            {
                this.operationState = value;
            }
        }
    }
}

