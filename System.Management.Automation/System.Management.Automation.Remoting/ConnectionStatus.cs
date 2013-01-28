namespace System.Management.Automation.Remoting
{
    using System;

    internal enum ConnectionStatus
    {
        AutoDisconnectStarting = 4,
        AutoDisconnectSucceeded = 5,
        ConnectionRetryAttempt = 2,
        ConnectionRetrySucceeded = 3,
        InternalErrorAbort = 6,
        NetworkFailureDetected = 1
    }
}

