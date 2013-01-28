namespace System.Management.Automation.Remoting
{
    using System;

    internal class ServerRemoteSessionContext
    {
        private RemoteSessionCapability _clientCapability;
        private RemoteSessionCapability _serverCapability = RemoteSessionCapability.CreateServerCapability();
        private bool isNegotiationSucceeded;

        internal ServerRemoteSessionContext()
        {
        }

        internal RemoteSessionCapability ClientCapability
        {
            get
            {
                return this._clientCapability;
            }
            set
            {
                this._clientCapability = value;
            }
        }

        internal bool IsNegotiationSucceeded
        {
            get
            {
                return this.isNegotiationSucceeded;
            }
            set
            {
                this.isNegotiationSucceeded = value;
            }
        }

        internal RemoteSessionCapability ServerCapability
        {
            get
            {
                return this._serverCapability;
            }
            set
            {
                this._serverCapability = value;
            }
        }
    }
}

