namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;

    internal class ClientRemoteSessionContext
    {
        private RemoteSessionCapability _clientCapability;
        private Uri _remoteAddress;
        private RemoteSessionCapability _serverCapability;
        private string _shellName;
        private PSCredential _userCredential;

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

        internal Uri RemoteAddress
        {
            get
            {
                return this._remoteAddress;
            }
            set
            {
                this._remoteAddress = value;
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

        internal string ShellName
        {
            get
            {
                return this._shellName;
            }
            set
            {
                this._shellName = value;
            }
        }

        internal PSCredential UserCredential
        {
            get
            {
                return this._userCredential;
            }
            set
            {
                this._userCredential = value;
            }
        }
    }
}

