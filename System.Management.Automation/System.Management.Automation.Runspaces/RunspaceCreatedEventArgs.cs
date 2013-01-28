namespace System.Management.Automation.Runspaces
{
    using System;

    internal sealed class RunspaceCreatedEventArgs : EventArgs
    {
        private System.Management.Automation.Runspaces.Runspace runspace;

        internal RunspaceCreatedEventArgs(System.Management.Automation.Runspaces.Runspace runspace)
        {
            this.runspace = runspace;
        }

        internal System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this.runspace;
            }
        }
    }
}

