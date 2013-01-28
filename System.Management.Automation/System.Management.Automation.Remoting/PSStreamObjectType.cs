namespace System.Management.Automation.Remoting
{
    using System;

    internal enum PSStreamObjectType
    {
        BlockingError = 5,
        Debug = 8,
        Error = 2,
        Exception = 11,
        MethodExecutor = 3,
        Output = 1,
        Progress = 9,
        ShouldMethod = 6,
        Verbose = 10,
        Warning = 4,
        WarningRecord = 7
    }
}

