namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Remoting.Server;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class ServerRemoteHost : PSHost, IHostSupportsInteractiveSession
    {
        private Guid _clientPowerShellId;
        private Guid _clientRunspacePoolId;
        private System.Management.Automation.Remoting.HostInfo _hostInfo;
        private Guid _instanceId = Guid.NewGuid();
        private ServerRemoteHostUserInterface _remoteHostUserInterface;
        private System.Management.Automation.Remoting.ServerMethodExecutor _serverMethodExecutor;
        private AbstractServerTransportManager _transportManager;

        internal ServerRemoteHost(Guid clientRunspacePoolId, Guid clientPowerShellId, System.Management.Automation.Remoting.HostInfo hostInfo, AbstractServerTransportManager transportManager)
        {
            this._clientRunspacePoolId = clientRunspacePoolId;
            this._clientPowerShellId = clientPowerShellId;
            this._hostInfo = hostInfo;
            this._transportManager = transportManager;
            this._serverMethodExecutor = new System.Management.Automation.Remoting.ServerMethodExecutor(clientRunspacePoolId, clientPowerShellId, this._transportManager);
            this._remoteHostUserInterface = hostInfo.IsHostUINull ? null : new ServerRemoteHostUserInterface(this);
        }

        public override void EnterNestedPrompt()
        {
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.EnterNestedPrompt);
        }

        public override void ExitNestedPrompt()
        {
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.ExitNestedPrompt);
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public void PopRunspace()
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.PopRunspace);
        }

        public void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace)
        {
			this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.PushRunspace, new object[] { runspace });
            //TODO: This is new stuff... MS didn't go there... throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.PushRunspace);
        }

        public override void SetShouldExit(int exitCode)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetShouldExit, new object[] { exitCode });
        }

        public override CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        internal System.Management.Automation.Remoting.HostInfo HostInfo
        {
            get
            {
                return this._hostInfo;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
        }

        public bool IsRunspacePushed
        {
            get
            {
                throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetIsRunspacePushed);
            }
        }

        public override string Name
        {
            get
            {
                return "ServerRemoteHost";
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetRunspace);
            }
        }

        internal System.Management.Automation.Remoting.ServerMethodExecutor ServerMethodExecutor
        {
            get
            {
                return this._serverMethodExecutor;
            }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return this._remoteHostUserInterface;
            }
        }

        public override System.Version Version
        {
            get
            {
                return RemotingConstants.HostVersion;
            }
        }
    }
}

