namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;

    internal sealed class RemoteSessionNegotiationEventArgs : EventArgs
    {
        private RemoteDataObject<PSObject> _remoteObject;
        private System.Management.Automation.Remoting.RemoteSessionCapability _remoteSessionCapability;

        internal RemoteSessionNegotiationEventArgs(System.Management.Automation.Remoting.RemoteSessionCapability remoteSessionCapability)
        {
            if (remoteSessionCapability == null)
            {
                throw PSTraceSource.NewArgumentNullException("remoteSessionCapability");
            }
            this._remoteSessionCapability = remoteSessionCapability;
        }

        internal RemoteDataObject<PSObject> RemoteData
        {
            get
            {
                return this._remoteObject;
            }
            set
            {
                this._remoteObject = value;
            }
        }

        internal System.Management.Automation.Remoting.RemoteSessionCapability RemoteSessionCapability
        {
            get
            {
                return this._remoteSessionCapability;
            }
        }
    }
}

