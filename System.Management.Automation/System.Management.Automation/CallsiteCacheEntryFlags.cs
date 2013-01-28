namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum CallsiteCacheEntryFlags
    {
        Constructor = 4,
        None = 0,
        ParameterizedSetter = 2,
        Static = 1
    }
}

