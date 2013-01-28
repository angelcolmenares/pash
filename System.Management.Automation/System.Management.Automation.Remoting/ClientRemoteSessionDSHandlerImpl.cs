namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Threading;

    internal class ClientRemoteSessionDSHandlerImpl : ClientRemoteSessionDataStructureHandler, IDisposable
    {
        private RunspaceConnectionInfo _connectionInfo;
        private PSRemotingCryptoHelper _cryptoHelper;
        private Uri _redirectUri;
        private ClientRemoteSession _session;
        private ClientRemoteSessionDSHandlerStateMachine _stateMachine;
        [TraceSource("CRSDSHdlerImpl", "ClientRemoteSessionDSHandlerImpl")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSDSHdlerImpl", "ClientRemoteSessionDSHandlerImpl");
        private BaseClientSessionTransportManager _transportManager;
        private bool isCloseCalled;
        private int maxUriRedirectionCount;
        private const string resBaseName = "remotingerroridstrings";
        private object syncObject = new object();
        private ClientRemoteSession.URIDirectionReported uriRedirectionHandler;

        internal override event EventHandler<RemoteSessionStateEventArgs> ConnectionStateChanged;

        internal override event EventHandler<RemoteDataEventArgs<string>> EncryptedSessionKeyReceived;

        internal override event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

        internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyRequestReceived;

        internal ClientRemoteSessionDSHandlerImpl(ClientRemoteSession session, PSRemotingCryptoHelper cryptoHelper, RunspaceConnectionInfo connectionInfo, ClientRemoteSession.URIDirectionReported uriRedirectionHandler)
        {
            if (session == null)
            {
                throw PSTraceSource.NewArgumentNullException("session");
            }
            this._session = session;
            this._stateMachine = new ClientRemoteSessionDSHandlerStateMachine();
            this._stateMachine.StateChanged += new EventHandler<RemoteSessionStateEventArgs>(this.HandleStateChanged);
            this._connectionInfo = connectionInfo;
            this._cryptoHelper = cryptoHelper;
            if (this._connectionInfo is NewProcessConnectionInfo)
            {
                this._transportManager = new OutOfProcessClientSessionTransportManager(this._session.RemoteRunspacePoolInternal.InstanceId, (NewProcessConnectionInfo) this._connectionInfo, cryptoHelper);
            }
            else
            {
                this._transportManager = new WSManClientSessionTransportManager(this._session.RemoteRunspacePoolInternal.InstanceId, (WSManConnectionInfo) this._connectionInfo, cryptoHelper, this._session.RemoteRunspacePoolInternal.Name);
            }
            this._transportManager.DataReceived += new EventHandler<RemoteDataEventArgs>(this.DispatchInputQueueData);
            this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
            this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
            this._transportManager.DisconnectCompleted += new EventHandler<EventArgs>(this.HandleDisconnectComplete);
            this._transportManager.ReconnectCompleted += new EventHandler<EventArgs>(this.HandleReconnectComplete);
            this._transportManager.RobustConnectionNotification += new EventHandler<ConnectionStatusEventArgs>(this.HandleRobustConnectionNotification);
            WSManConnectionInfo info = connectionInfo as WSManConnectionInfo;
            if (info != null)
            {
                this.uriRedirectionHandler = uriRedirectionHandler;
                this.maxUriRedirectionCount = info.MaximumConnectionRedirectionCount;
            }
        }

        internal override void CloseConnectionAsync()
        {
            lock (this.syncObject)
            {
                if (!this.isCloseCalled)
                {
                    this._transportManager.CloseAsync();
                    this.isCloseCalled = true;
                }
            }
        }

        internal override void CreateAsync()
        {
            this._transportManager.CreateCompleted += new EventHandler<CreateCompleteEventArgs>(this.HandleCreateComplete);
            this._transportManager.CreateAsync();
        }

        internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(ClientRemotePowerShell cmd, bool noInput)
        {
            BaseClientCommandTransportManager manager = this._transportManager.CreateClientCommandTransportManager(this._connectionInfo, cmd, noInput);
            manager.DataReceived += new EventHandler<RemoteDataEventArgs>(this.DispatchInputQueueData);
            return manager;
        }

        internal override void DisconnectAsync()
        {
            this._transportManager.DisconnectAsync();
        }

        internal void DispatchInputQueueData(object sender, RemoteDataEventArgs dataArg)
        {
            if (dataArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataArg");
            }
            RemoteDataObject<PSObject> receivedData = dataArg.ReceivedData;
            if (receivedData == null)
            {
                throw PSTraceSource.NewArgumentException("dataArg");
            }
            RemotingDestination destination = receivedData.Destination;
			var d = receivedData.Data;
            if ((destination & (RemotingDestination.InvalidDestination | RemotingDestination.Client)) != (RemotingDestination.InvalidDestination | RemotingDestination.Client))
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RemotingDestinationNotForMe, new object[] { RemotingDestination.InvalidDestination | RemotingDestination.Client, destination });
            }
            switch (receivedData.TargetInterface)
            {
                case RemotingTargetInterface.Session:
                    this.ProcessSessionMessages(dataArg);
                    return;

                case RemotingTargetInterface.RunspacePool:
                case RemotingTargetInterface.PowerShell:
                {
                    RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.MessageReceived, null);
                    if (!this.StateMachine.CanByPassRaiseEvent(arg))
                    {
                        this.StateMachine.RaiseEvent(arg, false);
                        return;
                    }
                    this.ProcessNonSessionMessages(dataArg.ReceivedData);
                    return;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._transportManager.Dispose();
            }
        }

        private void HandleCloseComplete(object sender, EventArgs args)
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CloseCompleted);
            this._stateMachine.RaiseEvent(arg, false);
        }

        private void HandleConnectComplete(object sender, EventArgs args)
        {
        }

        private void HandleCreateComplete(object sender, EventArgs args)
        {
        }

        private void HandleDisconnectComplete(object sender, EventArgs args)
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectCompleted);
            this.StateMachine.RaiseEvent(arg, false);
        }

        private void HandleNegotiationSendingStateChange()
        {
            RemoteDataObject obj2 = RemotingEncoder.GenerateClientSessionCapability(this._session.Context.ClientCapability, this._session.RemoteRunspacePoolInternal.InstanceId);
            RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(obj2.Destination, obj2.DataType, obj2.RunspacePoolId, obj2.PowerShellId, (PSObject) obj2.Data);
            this._transportManager.DataToBeSentCollection.Add<PSObject>(data);
        }

        private void HandleReconnectComplete(object sender, EventArgs args)
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.ReconnectCompleted);
            this.StateMachine.RaiseEvent(arg, false);
        }

        private void HandleRobustConnectionNotification(object sender, ConnectionStatusEventArgs e)
        {
            RemoteSessionStateMachineEventArgs arg = null;
            switch (e.Notification)
            {
                case ConnectionStatus.AutoDisconnectStarting:
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.RCDisconnectStarted);
                    break;

                case ConnectionStatus.AutoDisconnectSucceeded:
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.DisconnectCompleted, new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RCAutoDisconnectingError, this._session.RemoteRunspacePoolInternal.ConnectionInfo.ComputerName)));
                    break;

                case ConnectionStatus.InternalErrorAbort:
                    arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError);
                    break;
            }
            if (arg != null)
            {
                this.StateMachine.RaiseEvent(arg, false);
            }
        }

        private void HandleStateChanged(object sender, RemoteSessionStateEventArgs arg)
        {
            if (arg == null)
            {
                throw PSTraceSource.NewArgumentNullException("arg");
            }
            if ((arg.SessionStateInfo.State == RemoteSessionState.NegotiationSending) || (arg.SessionStateInfo.State == RemoteSessionState.NegotiationSendingOnConnect))
            {
                this.HandleNegotiationSendingStateChange();
            }
            this.ConnectionStateChanged.SafeInvoke<RemoteSessionStateEventArgs>(this, arg);
            if ((arg.SessionStateInfo.State == RemoteSessionState.NegotiationSending) || (arg.SessionStateInfo.State == RemoteSessionState.NegotiationSendingOnConnect))
            {
                this.SendNegotiationAsync(arg.SessionStateInfo.State);
            }
            if (arg.SessionStateInfo.State == RemoteSessionState.Established)
            {
                WSManClientSessionTransportManager manager = this._transportManager as WSManClientSessionTransportManager;
                if (manager != null)
                {
                    manager.AdjustForProtocolVariations(this._session.ServerProtocolVersion);
                    manager.StartReceivingData();
                }
            }
            if (arg.SessionStateInfo.State == RemoteSessionState.ClosingConnection)
            {
                this.CloseConnectionAsync();
            }
            if (arg.SessionStateInfo.State == RemoteSessionState.Disconnecting)
            {
                this.DisconnectAsync();
            }
            if (arg.SessionStateInfo.State == RemoteSessionState.Reconnecting)
            {
                this.ReconnectAsync();
            }
        }

        private void HandleTransportCloseCompleteForRedirection(object source, EventArgs args)
        {
            this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
            this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
            this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
            this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
            this.PerformURIRedirectionStep2(this._redirectUri);
        }

        internal void HandleTransportError(object sender, TransportErrorOccuredEventArgs e)
        {
            PSRemotingTransportRedirectException exception = e.Exception as PSRemotingTransportRedirectException;
            if ((exception != null) && (this.maxUriRedirectionCount > 0))
            {
                Exception exception2 = null;
                try
                {
                    this.maxUriRedirectionCount--;
                    this.PerformURIRedirection(exception.RedirectLocation);
                    return;
                }
                catch (ArgumentNullException exception3)
                {
                    exception2 = exception3;
                }
                catch (UriFormatException exception4)
                {
                    exception2 = exception4;
                }
                if (exception2 != null)
                {
                    PSRemotingTransportException exception5 = new PSRemotingTransportException(PSRemotingErrorId.RedirectedURINotWellFormatted, RemotingErrorIdStrings.RedirectedURINotWellFormatted, new object[] { this._session.Context.RemoteAddress.OriginalString, exception.RedirectLocation }) {
                        TransportMessage = e.Exception.TransportMessage
                    };
                    e.Exception = exception5;
                }
            }
            RemoteSessionEvent connectFailed = RemoteSessionEvent.ConnectFailed;
            switch (e.ReportingTransportMethod)
            {
                case TransportMethodEnum.CreateShellEx:
                    connectFailed = RemoteSessionEvent.ConnectFailed;
                    break;

                case TransportMethodEnum.SendShellInputEx:
                case TransportMethodEnum.CommandInputEx:
                    connectFailed = RemoteSessionEvent.SendFailed;
                    break;

                case TransportMethodEnum.ReceiveShellOutputEx:
                case TransportMethodEnum.ReceiveCommandOutputEx:
                    connectFailed = RemoteSessionEvent.ReceiveFailed;
                    break;

                case TransportMethodEnum.CloseShellOperationEx:
                    connectFailed = RemoteSessionEvent.CloseFailed;
                    break;

                case TransportMethodEnum.DisconnectShellEx:
                    connectFailed = RemoteSessionEvent.DisconnectFailed;
                    break;

                case TransportMethodEnum.ReconnectShellEx:
                    connectFailed = RemoteSessionEvent.ReconnectFailed;
                    break;
            }
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(connectFailed, e.Exception);
            this._stateMachine.RaiseEvent(arg, false);
        }

        private void HandleTransportErrorForRedirection(object sender, TransportErrorOccuredEventArgs e)
        {
            this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
            this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
            this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleCloseComplete);
            this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
            this.HandleTransportError(sender, e);
        }

        private void PerformURIRedirection(string newURIString)
        {
            this._redirectUri = new Uri(newURIString);
            lock (this.syncObject)
            {
                if (!this.isCloseCalled)
                {
                    this._transportManager.CloseCompleted -= new EventHandler<EventArgs>(this.HandleCloseComplete);
                    this._transportManager.WSManTransportErrorOccured -= new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportError);
                    this._transportManager.CloseCompleted += new EventHandler<EventArgs>(this.HandleTransportCloseCompleteForRedirection);
                    this._transportManager.WSManTransportErrorOccured += new EventHandler<TransportErrorOccuredEventArgs>(this.HandleTransportErrorForRedirection);
                    this._transportManager.PrepareForRedirection();
                }
            }
        }

        private void PerformURIRedirectionStep2(Uri newURI)
        {
            lock (this.syncObject)
            {
                if (!this.isCloseCalled)
                {
                    if (this.uriRedirectionHandler != null)
                    {
                        this.uriRedirectionHandler(newURI);
                    }
                    this._transportManager.Redirect(newURI, this._connectionInfo);
                }
            }
        }

        internal void ProcessNonSessionMessages(RemoteDataObject<PSObject> rcvdData)
        {
            Guid runspacePoolId;
            if (rcvdData == null)
            {
                throw PSTraceSource.NewArgumentNullException("rcvdData");
            }
            switch (rcvdData.TargetInterface)
            {
                case RemotingTargetInterface.Session:
                    break;

                case RemotingTargetInterface.RunspacePool:
                {
                    runspacePoolId = rcvdData.RunspacePoolId;
                    RemoteRunspacePoolInternal runspacePool = this._session.GetRunspacePool(runspacePoolId);
                    if (runspacePool == null)
                    {
                        _trace.WriteLine("Client received data for Runspace (id: {0}), \r\n                            but the Runspace cannot be found", new object[] { runspacePoolId });
                        return;
                    }
                    runspacePool.DataStructureHandler.ProcessReceivedData(rcvdData);
                    return;
                }
                case RemotingTargetInterface.PowerShell:
                    runspacePoolId = rcvdData.RunspacePoolId;
                    this._session.GetRunspacePool(runspacePoolId).DataStructureHandler.DispatchMessageToPowerShell(rcvdData);
                    break;

                default:
                    return;
            }
        }

        private void ProcessSessionMessages(RemoteDataEventArgs arg)
        {
            if ((arg == null) || (arg.ReceivedData == null))
            {
                throw PSTraceSource.NewArgumentNullException("arg");
            }
            RemoteDataObject<PSObject> receivedData = arg.ReceivedData;
            RemotingTargetInterface targetInterface = receivedData.TargetInterface;
            RemotingDataType dataType = receivedData.DataType;
            switch (dataType)
            {
                case RemotingDataType.SessionCapability:
                {
                    RemoteSessionCapability remoteSessionCapability = null;
                    try
                    {
                        remoteSessionCapability = RemotingDecoder.GetSessionCapability(receivedData.Data);
                    }
                    catch (PSRemotingDataStructureException exception2)
                    {
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientNotFoundCapabilityProperties, new object[] { exception2.Message, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                    }
                    RemoteSessionStateMachineEventArgs args2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationReceived) {
                        RemoteSessionCapability = remoteSessionCapability
                    };
                    this._stateMachine.RaiseEvent(args2, false);
                    RemoteSessionNegotiationEventArgs eventArgs = new RemoteSessionNegotiationEventArgs(remoteSessionCapability);
                    this.NegotiationReceived.SafeInvoke<RemoteSessionNegotiationEventArgs>(this, eventArgs);
                    return;
                }
                case RemotingDataType.CloseSession:
                {
                    PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerRequestedToCloseSession);
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, reason);
                    this._stateMachine.RaiseEvent(args, false);
                    return;
                }
                case RemotingDataType.EncryptedSessionKey:
                {
                    string encryptedSessionKey = RemotingDecoder.GetEncryptedSessionKey(receivedData.Data);
                    this.EncryptedSessionKeyReceived.SafeInvoke<RemoteDataEventArgs<string>>(this, new RemoteDataEventArgs<string>(encryptedSessionKey));
                    return;
                }
                case RemotingDataType.PublicKeyRequest:
                    this.PublicKeyRequestReceived.SafeInvoke<RemoteDataEventArgs<string>>(this, new RemoteDataEventArgs<string>(string.Empty));
                    return;
            }
            throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ReceivedUnsupportedAction, new object[] { dataType });
        }

        internal override void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData)
        {
            this.ProcessSessionMessages(new RemoteDataEventArgs(receivedData));
        }

        internal override void ReconnectAsync()
        {
            this._transportManager.ReconnectAsync();
        }

        internal override void SendNegotiationAsync(RemoteSessionState sessionState)
        {
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendCompleted);
            this._stateMachine.RaiseEvent(arg, false);
            if (sessionState == RemoteSessionState.NegotiationSending)
            {
                this._transportManager.CreateAsync();
            }
            else if (sessionState == RemoteSessionState.NegotiationSendingOnConnect)
            {
                this._transportManager.ConnectCompleted += new EventHandler<EventArgs>(this.HandleConnectComplete);
                this._transportManager.ConnectAsync();
            }
        }

        internal override void SendPublicKeyAsync(string localPublicKey)
        {
            this._transportManager.DataToBeSentCollection.Add<object>(RemotingEncoder.GenerateMyPublicKey(this._session.RemoteRunspacePoolInternal.InstanceId, localPublicKey, RemotingDestination.InvalidDestination | RemotingDestination.Server));
        }

        internal override ClientRemoteSessionDSHandlerStateMachine StateMachine
        {
            get
            {
                return this._stateMachine;
            }
        }

        internal override BaseClientSessionTransportManager TransportManager
        {
            get
            {
                return this._transportManager;
            }
        }
    }
}

