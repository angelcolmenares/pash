namespace System.Management.Automation.Internal
{
    using System;

    internal enum PSKeyword : ulong
    {
        Cmdlets = 0x20L,
        Host = 0x10L,
        ManagedPlugin = 0x100L,
        Pipeline = 2L,
        Protocol = 4L,
        Runspace = 1L,
        Serializer = 0x40L,
        Session = 0x80L,
        Transport = 8L,
        UseAlwaysAnalytic = 0x4000000000000000L,
        UseAlwaysOperational = 9223372036854775808L
    }
}

