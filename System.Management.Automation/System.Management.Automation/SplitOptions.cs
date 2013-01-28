namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum SplitOptions
    {
        CultureInvariant = 4,
        ExplicitCapture = 0x80,
        IgnoreCase = 0x40,
        IgnorePatternWhitespace = 8,
        Multiline = 0x10,
        RegexMatch = 2,
        SimpleMatch = 1,
        Singleline = 0x20
    }
}

