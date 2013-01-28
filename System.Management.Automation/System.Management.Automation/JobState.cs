namespace System.Management.Automation
{
    using System;

    public enum JobState
    {
        NotStarted,
        Running,
        Completed,
        Failed,
        Stopped,
        Blocked,
        Suspended,
        Disconnected,
        Suspending,
        Stopping
    }
}

