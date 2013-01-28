namespace System.Management.Automation.Runspaces
{
    using System;

    public enum RunspaceState
    {
        BeforeOpen,
        Opening,
        Opened,
        Closed,
        Closing,
        Broken,
        Disconnecting,
        Disconnected,
        Connecting
    }
}

