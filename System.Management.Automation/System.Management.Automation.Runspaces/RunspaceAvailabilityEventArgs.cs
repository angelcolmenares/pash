namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class RunspaceAvailabilityEventArgs : EventArgs
    {
        private System.Management.Automation.Runspaces.RunspaceAvailability _runspaceAvailability;

        internal RunspaceAvailabilityEventArgs(System.Management.Automation.Runspaces.RunspaceAvailability runspaceAvailability)
        {
            this._runspaceAvailability = runspaceAvailability;
        }

        public System.Management.Automation.Runspaces.RunspaceAvailability RunspaceAvailability
        {
            get
            {
                return this._runspaceAvailability;
            }
        }
    }
}

