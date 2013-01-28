namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.CompilerServices;

    internal abstract class ClientRemoteSession : RemoteSession
    {
        private ClientRemoteSessionContext _context = new ClientRemoteSessionContext();
        protected Version _serverProtocolVersion;
        private ClientRemoteSessionDataStructureHandler _sessionDSHandler;
        [TraceSource("CRSession", "ClientRemoteSession")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSession", "ClientRemoteSession");
        private System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal remoteRunspacePool;

        public abstract event EventHandler<RemoteSessionStateEventArgs> StateChanged;

        protected ClientRemoteSession()
        {
        }

        public abstract void CloseAsync();
        public abstract void ConnectAsync();
        public abstract void CreateAsync();
        public abstract void DisconnectAsync();
        internal System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal GetRunspacePool(Guid clientRunspacePoolId)
        {
            if ((this.remoteRunspacePool != null) && this.remoteRunspacePool.InstanceId.Equals(clientRunspacePoolId))
            {
                return this.remoteRunspacePool;
            }
            return null;
        }

        public abstract void ReconnectAsync();

        internal ClientRemoteSessionContext Context
        {
            get
            {
                return this._context;
            }
        }

        internal System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal RemoteRunspacePoolInternal
        {
            get
            {
                return this.remoteRunspacePool;
            }
            set
            {
                this.remoteRunspacePool = value;
            }
        }

        internal Version ServerProtocolVersion
        {
            get
            {
                return this._serverProtocolVersion;
            }
        }

        internal ClientRemoteSessionDataStructureHandler SessionDataStructureHandler
        {
            get
            {
                return this._sessionDSHandler;
            }
            set
            {
                this._sessionDSHandler = value;
            }
        }

        internal delegate void URIDirectionReported(Uri newURI);
    }
}

