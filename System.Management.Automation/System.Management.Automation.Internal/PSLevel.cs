namespace System.Management.Automation.Internal
{
    using System;

    internal enum PSLevel : byte
    {
        Critical = 1,
        Debug = 20,
        Error = 2,
        Informational = 4,
        LogAlways = 0,
        Verbose = 5,
        Warning = 3
    }
}

