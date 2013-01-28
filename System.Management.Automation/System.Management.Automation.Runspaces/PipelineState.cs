namespace System.Management.Automation.Runspaces
{
    using System;

    public enum PipelineState
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

