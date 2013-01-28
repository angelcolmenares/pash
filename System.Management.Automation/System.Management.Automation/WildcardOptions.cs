namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum WildcardOptions
    {
        Compiled = 1,
        CultureInvariant = 4,
        IgnoreCase = 2,
        None = 0
    }
}

