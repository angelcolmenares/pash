namespace System.Management.Automation
{
    using System;

    public enum PSInvocationState
    {
        NotStarted,
        Running,
        Stopping,
        Stopped,
        Completed,
        Failed,
        Disconnected
    }
}

