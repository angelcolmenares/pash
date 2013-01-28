namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum RemoteStreamOptions
    {
        AddInvocationInfo = 15,
        AddInvocationInfoToDebugRecord = 4,
        AddInvocationInfoToErrorRecord = 1,
        AddInvocationInfoToVerboseRecord = 8,
        AddInvocationInfoToWarningRecord = 2
    }
}

