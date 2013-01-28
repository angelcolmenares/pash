namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces.Internal;

    internal abstract class ClientRemoteSessionDataStructureHandler : BaseSessionDataStructureHandler
    {
        internal abstract event EventHandler<RemoteSessionStateEventArgs> ConnectionStateChanged;

        internal abstract event EventHandler<RemoteDataEventArgs<string>> EncryptedSessionKeyReceived;

        internal abstract event EventHandler<RemoteSessionNegotiationEventArgs> NegotiationReceived;

        internal abstract event EventHandler<RemoteDataEventArgs<string>> PublicKeyRequestReceived;

        protected ClientRemoteSessionDataStructureHandler()
        {
        }

        internal abstract void CloseConnectionAsync();
        internal abstract void CreateAsync();
        internal abstract BaseClientCommandTransportManager CreateClientCommandTransportManager(ClientRemotePowerShell cmd, bool noInput);
        internal abstract void DisconnectAsync();
        internal abstract void ReconnectAsync();
        internal abstract void SendNegotiationAsync(RemoteSessionState sessionState);
        internal abstract void SendPublicKeyAsync(string localPublicKey);

        internal abstract ClientRemoteSessionDSHandlerStateMachine StateMachine { get; }

        internal abstract BaseClientSessionTransportManager TransportManager { get; }
    }
}

