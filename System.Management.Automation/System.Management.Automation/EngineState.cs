namespace System.Management.Automation
{
    using System;

    internal enum EngineState
    {
        None,
        Available,
        Degraded,
        OutOfService,
        Stopped
    }
}

