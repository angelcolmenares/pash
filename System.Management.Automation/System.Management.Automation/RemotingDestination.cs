namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum RemotingDestination : int
    {
        Client = 1,
        InvalidDestination = 0,
        Listener = 4,
        Server = 2
    }
}

