namespace System.Management.Automation.Language
{
    using System;

    [Flags]
    internal enum CharTraits
    {
        Digit = 0x80,
        ForceStartNewAssemblyNameSpecToken = 0x400,
        ForceStartNewToken = 0x200,
        ForceStartNewTokenAfterNumber = 0x800,
        HexDigit = 0x40,
        IdentifierStart = 2,
        MultiplierStart = 4,
        Newline = 0x20,
        None = 0,
        TypeSuffix = 8,
        VarNameFirst = 0x100,
        Whitespace = 0x10
    }
}

