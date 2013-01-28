namespace System.Management.Automation
{
    using System;

    internal enum RemoteSessionState
    {
        UndefinedState,
        Idle,
        Connecting,
        Connected,
        NegotiationSending,
        NegotiationSendingOnConnect,
        NegotiationSent,
        NegotiationReceived,
        NegotiationPending,
        ClosingConnection,
        Closed,
        Established,
        EstablishedAndKeySent,
        EstablishedAndKeyReceived,
        EstablishedAndKeyRequested,
        EstablishedAndKeyExchanged,
        Disconnecting,
        Disconnected,
        Reconnecting,
        RCDisconnecting,
        MaxState
    }
}

