namespace System.Management.Automation.Remoting
{
    using System;

    internal abstract class BaseSessionDataStructureHandler
    {
        protected BaseSessionDataStructureHandler()
        {
        }

        internal abstract void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData);
    }
}

