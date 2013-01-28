namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum VariablePathFlags
    {
        DriveQualified = 0x40,
        Function = 0x20,
        Global = 4,
        Local = 1,
        None = 0,
        Private = 8,
        Script = 2,
        Unqualified = 0x80,
        UnscopedVariableMask = 0x6f,
        Variable = 0x10
    }
}

