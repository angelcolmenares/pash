namespace System.Management.Automation.Tracing
{
    using System;

    [Flags]
    public enum PowerShellTraceKeywords : ulong
    {
        Cmdlets = 0x20L,
        Host = 0x10L,
        ManagedPlugIn = 0x100L,
        None = 0L,
        Pipeline = 2L,
        Protocol = 4L,
        Runspace = 1L,
        Serializer = 0x40L,
        Session = 0x80L,
        Transport = 8L,
        UseAlwaysAnalytic = 0x4000000000000000L,
        UseAlwaysDebug = 0x2000000000000000L,
        UseAlwaysOperational = 9223372036854775808L
    }
}

