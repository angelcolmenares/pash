namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Threading;

    internal class ClientRemoteSessionImpl : ClientRemoteSession, IDisposable
    {
        private PSRemotingCryptoHelperClient _cryptoHelper;
        private RemotingDestination _mySelf;
        [TraceSource("CRSessionImpl", "ClientRemoteSessionImpl")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSessionImpl", "ClientRemoteSessionImpl");
        private ManualResetEvent _waitHandleForConfigurationReceived;

        public override event EventHandler<RemoteSessionStateEventArgs> StateChanged;

        internal ClientRemoteSessionImpl(RemoteRunspacePoolInternal rsPool, ClientRemoteSession.URIDirectionReported uriRedirectionHandler)
        {
            base.RemoteRunspacePoolInternal = rsPool;
            base.Context.RemoteAddress = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<Uri>(rsPool.ConnectionInfo, "ConnectionUri", null);
            this._cryptoHelper = new PSRemotingCryptoHelperClient();
            this._cryptoHelper.Session = this;
            base.Context.ClientCapability = RemoteSessionCapability.CreateClientCapability();
            base.Context.UserCredential = rsPool.ConnectionInfo.Credential;
            base.Context.ShellName = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(rsPool.ConnectionInfo, "ShellUri", string.Empty);
            this._mySelf = RemotingDestination.InvalidDestination | RemotingDestination.Client;
            base.SessionDataStructureHandler = new ClientRemoteSessionDSHandlerImpl(this, this._cryptoHelper, rsPool.ConnectionInfo, uriRedirectionHandler);
            base.BaseSessionDataStructureHandler = base.SessionDataStructureHandler;
            this._waitHandleForConfigurationReceived = new ManualResetEvent(false);
            base.SessionDataStructureHandler.NegotiationReceived += new EventHandler<RemoteSessionNegotiationEventArgs>(this.HandleNegotiationReceived);
            base.SessionDataStructureHandler.ConnectionStateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleConnectionStateChanged);
            base.SessionDataStructureHandler.EncryptedSessionKeyReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandleEncryptedSessionKeyReceived);
            base.SessionDataStructureHandler.PublicKeyRequestReceived += new EventHandler<RemoteDataEventArgs<string>>(this.HandlePublicKeyRequestReceived);
        }

        public override void CloseAsync()
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
            base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
        }

        internal override void CompleteKeyExchange()
        {
            this._cryptoHelper.CompleteKeyExchange();
        }

        public override void ConnectAsync()
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ConnectSession);
            base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
        }

        public override void CreateAsync()
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CreateSession);
            base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
        }

        public override void DisconnectAsync()
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectStart);
            base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._waitHandleForConfigurationReceived != null)
                {
                    this._waitHandleForConfigurationReceived.Close();
                    this._waitHandleForConfigurationReceived = null;
                }
                ((ClientRemoteSessionDSHandlerImpl) base.SessionDataStructureHandler).Dispose();
                base.SessionDataStructureHandler = null;
                this._cryptoHelper.Dispose();
                this._cryptoHelper = null;
            }
        }

        private void HandleConnectionStateChanged(object sender, RemoteSessionStateEventArgs arg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (arg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("arg");
                }
                if (arg.SessionStateInfo.State == RemoteSessionState.EstablishedAndKeyReceived)
                {
                    this.StartKeyExchange();
                }
                if (arg.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
                {
                    this.CompleteKeyExchange();
                }
                this.StateChanged.SafeInvoke<RemoteSessionStateEventArgs>(this, arg);
            }
        }

        private void HandleEncryptedSessionKeyReceived(object sender, RemoteDataEventArgs<string> eventArgs)
        {
            if (base.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeySent)
            {
                string data = eventArgs.Data;
                bool flag = this._cryptoHelper.ImportEncryptedSessionKey(data);
                RemoteSessionStateMachineEventArgs arg = null;
                if (!flag)
                {
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed);
                    base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
                }
                this.CompleteKeyExchange();
                arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceived);
                base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
            }
        }

        private void HandleNegotiationReceived(object sender, RemoteSessionNegotiationEventArgs arg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (arg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("arg");
                }
                if (arg.RemoteSessionCapability == null)
                {
                    throw PSTraceSource.NewArgumentException("arg");
                }
                base.Context.ServerCapability = arg.RemoteSessionCapability;
                try
                {
                    this.RunClientNegotiationAlgorithm(base.Context.ServerCapability);
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationCompleted);
                    base.SessionDataStructureHandler.StateMachine.RaiseEvent(args, false);
                }
                catch (PSRemotingDataStructureException exception)
                {
                    RemoteSessionStateMachineEventArgs args2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationFailed, exception);
                    base.SessionDataStructureHandler.StateMachine.RaiseEvent(args2, false);
                }
            }
        }

        private void HandlePublicKeyRequestReceived(object sender, RemoteDataEventArgs<string> eventArgs)
        {
			var state = base.SessionDataStructureHandler.StateMachine.State;
            if (state == RemoteSessionState.Established)
            {
                RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyRequested);
                base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
                this.StartKeyExchange();
            }
        }

        public override void ReconnectAsync()
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ReconnectStart);
            base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
        }

        private bool RunClientNegotiationAlgorithm(RemoteSessionCapability serverRemoteSessionCapability)
        {
            Version protocolVersion = serverRemoteSessionCapability.ProtocolVersion;
            base._serverProtocolVersion = protocolVersion;
            Version version2 = base.Context.ClientCapability.ProtocolVersion;
            if ((!version2.Equals(protocolVersion) && ((version2 != RemotingConstants.ProtocolVersionWin7RTM) || (protocolVersion != RemotingConstants.ProtocolVersionWin7RC))) && ((version2 != RemotingConstants.ProtocolVersionCurrent) || ((protocolVersion != RemotingConstants.ProtocolVersionWin7RC) && (protocolVersion != RemotingConstants.ProtocolVersionWin7RTM))))
            {
                PSRemotingDataStructureException exception = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNegotiationFailed, new object[] { "protocolversion", protocolVersion, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                throw exception;
            }
            Version pSVersion = serverRemoteSessionCapability.PSVersion;
            if (!base.Context.ClientCapability.PSVersion.Equals(pSVersion))
            {
                PSRemotingDataStructureException exception2 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNegotiationFailed, new object[] { "PSVersion", pSVersion.ToString(), PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                throw exception2;
            }
            Version serializationVersion = serverRemoteSessionCapability.SerializationVersion;
            if (!base.Context.ClientCapability.SerializationVersion.Equals(serializationVersion))
            {
                PSRemotingDataStructureException exception3 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNegotiationFailed, new object[] { "SerializationVersion", serializationVersion.ToString(), PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                throw exception3;
            }
            return true;
        }

        internal override void StartKeyExchange()
        {
            if ((base.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.Established) || (base.SessionDataStructureHandler.StateMachine.State == RemoteSessionState.EstablishedAndKeyRequested))
            {
                string publicKeyAsString = null;
                bool flag = false;
                RemoteSessionStateMachineEventArgs arg = null;
                Exception reason = null;
                try
                {
                    flag = this._cryptoHelper.ExportLocalPublicKey(out publicKeyAsString);
                }
                catch (PSCryptoException exception2)
                {
                    flag = false;
                    reason = exception2;
                }
                if (!flag)
                {
                    this.CompleteKeyExchange();
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySendFailed, reason);
                    base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
                }
                arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeySent);
                base.SessionDataStructureHandler.StateMachine.RaiseEvent(arg, false);
                base.SessionDataStructureHandler.SendPublicKeyAsync(publicKeyAsString);
            }
        }

        internal override RemotingDestination MySelf
        {
            get
            {
                return this._mySelf;
            }
        }
    }
}

