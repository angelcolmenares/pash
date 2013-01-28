namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class RunspaceStateEventArgs : EventArgs
    {
        private System.Management.Automation.Runspaces.RunspaceStateInfo _runspaceStateInfo;

        internal RunspaceStateEventArgs(System.Management.Automation.Runspaces.RunspaceStateInfo runspaceStateInfo)
        {
            if (runspaceStateInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceStateInfo");
            }
            this._runspaceStateInfo = runspaceStateInfo;
        }

        public System.Management.Automation.Runspaces.RunspaceStateInfo RunspaceStateInfo
        {
            get
            {
                return this._runspaceStateInfo;
            }
        }
    }
}

