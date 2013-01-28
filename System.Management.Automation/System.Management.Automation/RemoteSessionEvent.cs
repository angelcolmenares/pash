namespace System.Management.Automation
{
    using System;

    internal enum RemoteSessionEvent
    {
        InvalidEvent,
        CreateSession,
        ConnectSession,
        NegotiationSending,
        NegotiationSendingOnConnect,
        NegotiationSendCompleted,
        NegotiationReceived,
        NegotiationCompleted,
        NegotiationPending,
        Close,
        CloseCompleted,
        CloseFailed,
        ConnectFailed,
        NegotiationFailed,
        NegotiationTimeout,
        SendFailed,
        ReceiveFailed,
        FatalError,
        MessageReceived,
        KeySent,
        KeySendFailed,
        KeyReceived,
        KeyReceiveFailed,
        KeyRequested,
        KeyRequestFailed,
        DisconnectStart,
        DisconnectCompleted,
        DisconnectFailed,
        ReconnectStart,
        ReconnectCompleted,
        ReconnectFailed,
        RCDisconnectStarted,
        MaxEvent
    }
}

