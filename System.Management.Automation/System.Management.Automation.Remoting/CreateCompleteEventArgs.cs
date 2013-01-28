namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation.Runspaces;

    internal class CreateCompleteEventArgs : EventArgs
    {
        private readonly RunspaceConnectionInfo _connectionInfo;

        internal CreateCompleteEventArgs(RunspaceConnectionInfo connectionInfo)
        {
            this._connectionInfo = connectionInfo;
        }

        internal RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return this._connectionInfo;
            }
        }
    }
}

