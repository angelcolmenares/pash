namespace System.Management.Automation.Runspaces.Internal
{
    using System;

    internal enum PSConnectionRetryStatus
    {
        None,
        NetworkFailureDetected,
        ConnectionRetryAttempt,
        ConnectionRetrySucceeded,
        AutoDisconnectStarting,
        AutoDisconnectSucceeded,
        InternalErrorAbort
    }
}

