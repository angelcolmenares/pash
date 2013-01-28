namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting.Server;

    internal abstract class ServerRemoteSessionDataStructureHandler : BaseSessionDataStructureHandler
    {
        internal abstract event EventHandler<RemoteDataEventArgs> CreateRunspacePoolReceived;

        internal abstract event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

        internal abstract event EventHandler<RemoteDataEventArgs<string>> PublicKeyReceived;

        internal abstract event EventHandler<EventArgs> SessionClosing;

        internal ServerRemoteSessionDataStructureHandler()
        {
        }

        internal abstract void CloseConnectionAsync(Exception reasonForClose);
        internal abstract void ConnectAsync();
        internal abstract void RaiseDataReceivedEvent(RemoteDataEventArgs arg);
        internal abstract void SendEncryptedSessionKey(string encryptedSessionKey);
        internal abstract void SendNegotiationAsync();
        internal abstract void SendRequestForPublicKey();

        internal abstract ServerRemoteSessionDSHandlerStateMachine StateMachine { get; }

        internal abstract AbstractServerSessionTransportManager TransportManager { get; }
    }
}

