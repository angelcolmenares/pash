namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class RunspacePoolStateChangedEventArgs : EventArgs
    {
        private System.Management.Automation.RunspacePoolStateInfo stateInfo;

        internal RunspacePoolStateChangedEventArgs(System.Management.Automation.RunspacePoolStateInfo stateInfo)
        {
            this.stateInfo = stateInfo;
        }

        internal RunspacePoolStateChangedEventArgs(RunspacePoolState state)
        {
            this.stateInfo = new System.Management.Automation.RunspacePoolStateInfo(state, null);
        }

        public System.Management.Automation.RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                return this.stateInfo;
            }
        }
    }
}

