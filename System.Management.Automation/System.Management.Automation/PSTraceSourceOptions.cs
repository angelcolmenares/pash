namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum PSTraceSourceOptions
    {
        All = 0x7fff,
        Assert = 0x4000,
        Constructor = 1,
        Data = 0x1817,
        Delegates = 0x20,
        Dispose = 2,
        Error = 0x200,
        Errors = 640,
        Events = 0x40,
        Exception = 0x80,
        ExecutionFlow = 0x206f,
        Finalizer = 4,
        Lock = 0x100,
        Method = 8,
        None = 0,
        Property = 0x10,
        Scope = 0x2000,
        Verbose = 0x800,
        Warning = 0x400,
        WriteLine = 0x1000
    }
}

