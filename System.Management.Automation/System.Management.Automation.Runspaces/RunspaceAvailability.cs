namespace System.Management.Automation.Runspaces
{
    using System;

    public enum RunspaceAvailability
    {
        None,
        Available,
        AvailableForNestedCommand,
        Busy
    }
}

