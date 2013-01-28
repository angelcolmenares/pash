namespace System.Management.Automation.Internal
{
    using System;

    internal enum PSTask
    {
        CommandStart = 0x66,
        CommandStop = 0x67,
        CreateRunspace = 1,
        EngineStart = 100,
        EngineStop = 0x65,
        ExecuteCommand = 2,
        ExecutePipeline = 0x6a,
        None = 0,
        PowershellConsoleStartup = 4,
        ProviderStart = 0x68,
        ProviderStop = 0x69,
        ScheduledJob = 110,
        Serialization = 3
    }
}

