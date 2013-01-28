namespace System.Management.Automation.Remoting.Server
{
    using System;
    using System.Management.Automation.Internal;

    internal abstract class AbstractServerSessionTransportManager : AbstractServerTransportManager
    {
        protected AbstractServerSessionTransportManager(int fragmentSize, PSRemotingCryptoHelper cryptoHelper) : base(fragmentSize, cryptoHelper)
        {
        }

        internal abstract AbstractServerTransportManager GetCommandTransportManager(Guid powerShellCmdId);
        internal abstract void RemoveCommandTransportManager(Guid powerShellCmdId);
    }
}

