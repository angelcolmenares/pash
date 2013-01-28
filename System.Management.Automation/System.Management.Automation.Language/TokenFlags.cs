namespace System.Management.Automation.Language
{
    using System;

    [Flags]
    public enum TokenFlags
    {
        AssignmentOperator = 0x2000,
        AttributeName = 0x400000,
        BinaryOperator = 0x100,
        BinaryPrecedenceAdd = 4,
        BinaryPrecedenceBitwise = 2,
        BinaryPrecedenceComparison = 3,
        BinaryPrecedenceFormat = 6,
        BinaryPrecedenceLogical = 1,
        BinaryPrecedenceMask = 7,
        BinaryPrecedenceMultiply = 5,
        BinaryPrecedenceRange = 7,
        CanConstantFold = 0x800000,
        CaseSensitiveOperator = 0x400,
        CommandName = 0x80000,
        DisallowedInRestrictedMode = 0x20000,
        Keyword = 0x10,
        MemberName = 0x100000,
        None = 0,
        ParseModeInvariant = 0x8000,
        PrefixOrPostfixOperator = 0x40000,
        ScriptBlockBlockName = 0x20,
        SpecialOperator = 0x1000,
        TokenInError = 0x10000,
        TypeName = 0x200000,
        UnaryOperator = 0x200
    }
}

