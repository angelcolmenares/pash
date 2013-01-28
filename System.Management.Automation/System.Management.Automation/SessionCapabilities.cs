namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum SessionCapabilities
    {
        Language = 4,
        RemoteServer = 1,
        WorkflowServer = 2
    }
}

