namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;

    internal abstract class BaseClientSessionTransportManager : BaseClientTransportManager, IDisposable
    {
        protected BaseClientSessionTransportManager(Guid runspaceId, PSRemotingCryptoHelper cryptoHelper) : base(runspaceId, cryptoHelper)
        {
        }

        internal virtual BaseClientCommandTransportManager CreateClientCommandTransportManager(RunspaceConnectionInfo connectionInfo, ClientRemotePowerShell cmd, bool noInput)
        {
            throw new NotImplementedException();
        }

        internal virtual void DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        internal virtual void PrepareForRedirection()
        {
            throw new NotImplementedException();
        }

        internal virtual void ReconnectAsync()
        {
            throw new NotImplementedException();
        }

        internal virtual void Redirect(Uri newUri, RunspaceConnectionInfo connectionInfo)
        {
            throw new NotImplementedException();
        }

        internal virtual void RemoveCommandTransportManager(Guid powerShellCmdId)
        {
        }
    }
}

