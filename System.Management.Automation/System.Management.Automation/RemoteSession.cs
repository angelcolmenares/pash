namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Remoting;

    internal abstract class RemoteSession
    {
        private System.Management.Automation.Remoting.BaseSessionDataStructureHandler _dsHandler;
        private Guid _instanceId = Guid.NewGuid ();

        protected RemoteSession()
        {

        }

        internal abstract void CompleteKeyExchange();
        internal abstract void StartKeyExchange();

        internal System.Management.Automation.Remoting.BaseSessionDataStructureHandler BaseSessionDataStructureHandler
        {
            get
            {
                return this._dsHandler;
            }
            set
            {
                this._dsHandler = value;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
        }

        internal abstract RemotingDestination MySelf { get; }
    }
}

