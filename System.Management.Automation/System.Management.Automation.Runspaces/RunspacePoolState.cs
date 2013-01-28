namespace System.Management.Automation.Runspaces
{
    using System;

    public enum RunspacePoolState
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

