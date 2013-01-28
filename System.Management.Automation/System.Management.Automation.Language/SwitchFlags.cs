namespace System.Management.Automation.Language
{
    using System;

    [Flags]
    public enum SwitchFlags
    {
        CaseSensitive = 0x10,
        Exact = 8,
        File = 1,
        None = 0,
        Parallel = 0x20,
        Regex = 2,
        Wildcard = 4
    }
}

