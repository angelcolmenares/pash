namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting.Server;
    using System.Threading;

    internal class ServerRemoteSessionDSHandlerlImpl : ServerRemoteSessionDataStructureHandler
    {
        private ServerRemoteSession _session;
        private ServerRemoteSessionDSHandlerStateMachine _stateMachine;
        private AbstractServerSessionTransportManager _transportManager;

        internal override event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;

        internal override event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

        internal override event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;

        internal override event EventHandler<EventArgs> SessionClosing;

        internal ServerRemoteSessionDSHandlerlImpl(ServerRemoteSession session, AbstractServerSessionTransportManager transportManager)
        {
            this._session = session;
            this._stateMachine = new ServerRemoteSessionDSHandlerStateMachine(session);
            this._transportManager = transportManager;
            this._transportManager.DataReceived += new EventHandler<RemoteDataEventArgs>(session.DispatchInputQueueData);
        }

        internal override void CloseConnectionAsync(Exception reasonForClose)
        {
            this.SessionClosing.SafeInvoke<EventArgs>(this, EventArgs.Empty);
            this._transportManager.Close(reasonForClose);
            RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.CloseCompleted);
            this._stateMachine.RaiseEvent(fsmEventArg);
        }

        internal override void ConnectAsync()
        {
        }

        internal override void RaiseDataReceivedEvent(RemoteDataEventArgs dataArg)
        {
            if (dataArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataArg");
            }
            RemoteDataObject<PSObject> receivedData = dataArg.ReceivedData;
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
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerNotFoundCapabilityProperties, new object[] { exception2.Message, PSVersionInfo.BuildVersion, RemotingConstants.ProtocolVersion });
                    }
                    RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationReceived) {
                        RemoteSessionCapability = remoteSessionCapability
                    };
                    this._stateMachine.RaiseEvent(fsmEventArg);
                    if (this.NegotiationReceived != null)
                    {
                        RemoteSessionNegotiationEventArgs eventArgs = new RemoteSessionNegotiationEventArgs(remoteSessionCapability) {
                            RemoteData = receivedData
                        };
                        this.NegotiationReceived.SafeInvoke<RemoteSessionNegotiationEventArgs>(this, eventArgs);
                    }
                    return;
                }
                case RemotingDataType.CloseSession:
                {
                    PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientRequestedToCloseSession);
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, reason);
                    this._stateMachine.RaiseEvent(args);
                    return;
                }
                case RemotingDataType.CreateRunspacePool:
                    this.CreateRunspacePoolReceived.SafeInvoke<RemoteDataEventArgs>(this, dataArg);
                    return;

                case RemotingDataType.PublicKey:
                {
                    string publicKey = RemotingDecoder.GetPublicKey(receivedData.Data);
                    this.PublicKeyReceived.SafeInvoke<RemoteDataEventArgs<string>>(this, new RemoteDataEventArgs<string>(publicKey));
                    return;
                }
            }
            throw new PSRemotingDataStructureException(RemotingErrorIdStrings.ReceivedUnsupportedAction, new object[] { dataType });
        }

        internal override void RaiseKeyExchangeMessageReceived(RemoteDataObject<PSObject> receivedData)
        {
            this.RaiseDataReceivedEvent(new RemoteDataEventArgs(receivedData));
        }

        internal override void SendEncryptedSessionKey(string encryptedSessionKey)
        {
            this._transportManager.SendDataToClient<object>(RemotingEncoder.GenerateEncryptedSessionKeyResponse(Guid.Empty, encryptedSessionKey), true, false);
        }

        internal override void SendNegotiationAsync()
        {
            RemoteDataObject obj2 = RemotingEncoder.GenerateServerSessionCapability(this._session.Context.ServerCapability, Guid.Empty);
            RemoteSessionStateMachineEventArgs fsmEventArg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendCompleted);
            this._stateMachine.RaiseEvent(fsmEventArg);
            RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(obj2.Destination, obj2.DataType, obj2.RunspacePoolId, obj2.PowerShellId, (PSObject) obj2.Data);
            this._transportManager.SendDataToClient<PSObject>(data, false, false);
        }

        internal override void SendRequestForPublicKey()
        {
            this._transportManager.SendDataToClient<object>(RemotingEncoder.GeneratePublicKeyRequest(Guid.Empty), true, false);
        }

        internal override ServerRemoteSessionDSHandlerStateMachine StateMachine
        {
            get
            {
                return this._stateMachine;
            }
        }

        internal override AbstractServerSessionTransportManager TransportManager
        {
            get
            {
                return this._transportManager;
            }
        }
    }
}

