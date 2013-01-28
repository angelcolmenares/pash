namespace System.Management.Automation.Tracing
{
    using System;

    public enum PowerShellTraceTask
    {
        None,
        CreateRunspace,
        ExecuteCommand,
        Serialization,
        PowerShellConsoleStartup
    }
}

