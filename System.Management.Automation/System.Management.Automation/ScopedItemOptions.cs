namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum ScopedItemOptions
    {
        AllScope = 8,
        Constant = 2,
        None = 0,
        Private = 4,
        ReadOnly = 1,
        Unspecified = 0x10
    }
}

