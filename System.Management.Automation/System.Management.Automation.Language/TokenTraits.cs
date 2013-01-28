namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public static class TokenTraits
    {
        private static readonly TokenFlags[] _staticTokenFlags;
        private static readonly string[] _tokenText;

        static TokenTraits()
        {
            TokenFlags[] flagsArray = new TokenFlags[0x9b];
            flagsArray[8] = TokenFlags.ParseModeInvariant;
            flagsArray[9] = TokenFlags.ParseModeInvariant;
            flagsArray[10] = TokenFlags.ParseModeInvariant;
            flagsArray[11] = TokenFlags.ParseModeInvariant;
            flagsArray[12] = TokenFlags.ParseModeInvariant;
            flagsArray[13] = TokenFlags.ParseModeInvariant;
            flagsArray[14] = TokenFlags.ParseModeInvariant;
            flagsArray[15] = TokenFlags.ParseModeInvariant;
            flagsArray[0x10] = TokenFlags.ParseModeInvariant;
            flagsArray[0x11] = TokenFlags.ParseModeInvariant;
            flagsArray[0x12] = TokenFlags.ParseModeInvariant;
            flagsArray[0x13] = TokenFlags.ParseModeInvariant;
            flagsArray[0x15] = TokenFlags.ParseModeInvariant;
            flagsArray[0x16] = TokenFlags.ParseModeInvariant;
            flagsArray[0x17] = TokenFlags.ParseModeInvariant;
            flagsArray[0x18] = TokenFlags.ParseModeInvariant;
            flagsArray[0x19] = TokenFlags.ParseModeInvariant;
            flagsArray[0x1a] = TokenFlags.ParseModeInvariant | TokenFlags.BinaryOperator;
            flagsArray[0x1b] = TokenFlags.ParseModeInvariant | TokenFlags.BinaryOperator;
            flagsArray[0x1c] = TokenFlags.ParseModeInvariant | TokenFlags.SpecialOperator;
            flagsArray[0x1d] = TokenFlags.ParseModeInvariant | TokenFlags.SpecialOperator;
            flagsArray[30] = TokenFlags.ParseModeInvariant | TokenFlags.UnaryOperator;
            flagsArray[0x1f] = TokenFlags.PrefixOrPostfixOperator | TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator;
            flagsArray[0x20] = TokenFlags.PrefixOrPostfixOperator | TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator;
            flagsArray[0x21] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceMask;
            flagsArray[0x22] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.SpecialOperator;
            flagsArray[0x23] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.SpecialOperator;
            flagsArray[0x24] = TokenFlags.CanConstantFold | TokenFlags.UnaryOperator;
            flagsArray[0x25] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceMultiply;
            flagsArray[0x26] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceMultiply;
            flagsArray[0x27] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceMultiply;
            flagsArray[40] = TokenFlags.CanConstantFold | TokenFlags.UnaryOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceAdd;
            flagsArray[0x29] = TokenFlags.CanConstantFold | TokenFlags.UnaryOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceAdd;
            flagsArray[0x2a] = TokenFlags.AssignmentOperator;
            flagsArray[0x2b] = TokenFlags.AssignmentOperator;
            flagsArray[0x2c] = TokenFlags.AssignmentOperator;
            flagsArray[0x2d] = TokenFlags.AssignmentOperator;
            flagsArray[0x2e] = TokenFlags.AssignmentOperator;
            flagsArray[0x2f] = TokenFlags.AssignmentOperator;
            flagsArray[0x30] = TokenFlags.DisallowedInRestrictedMode;
            flagsArray[0x31] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.ParseModeInvariant;
            flagsArray[50] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceFormat;
            flagsArray[0x33] = TokenFlags.CanConstantFold | TokenFlags.UnaryOperator;
            flagsArray[0x34] = TokenFlags.CanConstantFold | TokenFlags.UnaryOperator;
            flagsArray[0x35] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceLogical;
            flagsArray[0x36] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceLogical;
            flagsArray[0x37] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceLogical;
            flagsArray[0x38] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceBitwise;
            flagsArray[0x39] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceBitwise;
            flagsArray[0x3a] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceBitwise;
            flagsArray[0x3b] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[60] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x3d] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x3e] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x3f] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x40] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x41] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x42] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x43] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x44] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x45] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[70] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x47] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x48] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x49] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4a] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4b] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4c] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4d] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4e] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x4f] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[80] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x51] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x52] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x53] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x54] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x55] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x56] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x57] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x58] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x59] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[90] = TokenFlags.CaseSensitiveOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x5b] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.CaseSensitiveOperator | TokenFlags.UnaryOperator | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x5c] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x5d] = TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x5e] = TokenFlags.DisallowedInRestrictedMode | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x5f] = TokenFlags.PrefixOrPostfixOperator | TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator;
            flagsArray[0x60] = TokenFlags.PrefixOrPostfixOperator | TokenFlags.DisallowedInRestrictedMode | TokenFlags.UnaryOperator;
            flagsArray[0x61] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x62] = TokenFlags.CanConstantFold | TokenFlags.BinaryOperator | TokenFlags.BinaryPrecedenceComparison;
            flagsArray[0x77] = TokenFlags.ScriptBlockBlockName | TokenFlags.Keyword;
            flagsArray[120] = TokenFlags.Keyword;
            flagsArray[0x79] = TokenFlags.Keyword;
            flagsArray[0x7a] = TokenFlags.Keyword;
            flagsArray[0x7b] = TokenFlags.Keyword;
            flagsArray[0x7c] = TokenFlags.Keyword;
            flagsArray[0x7d] = TokenFlags.Keyword;
            flagsArray[0x7e] = TokenFlags.Keyword;
            flagsArray[0x7f] = TokenFlags.ScriptBlockBlockName | TokenFlags.Keyword;
            flagsArray[0x80] = TokenFlags.Keyword;
            flagsArray[0x81] = TokenFlags.Keyword;
            flagsArray[130] = TokenFlags.ScriptBlockBlockName | TokenFlags.Keyword;
            flagsArray[0x83] = TokenFlags.Keyword;
            flagsArray[0x84] = TokenFlags.Keyword;
            flagsArray[0x85] = TokenFlags.Keyword;
            flagsArray[0x86] = TokenFlags.Keyword;
            flagsArray[0x87] = TokenFlags.Keyword;
            flagsArray[0x88] = TokenFlags.Keyword;
            flagsArray[0x89] = TokenFlags.Keyword;
            flagsArray[0x8a] = TokenFlags.Keyword;
            flagsArray[0x8b] = TokenFlags.Keyword;
            flagsArray[140] = TokenFlags.Keyword;
            flagsArray[0x8d] = TokenFlags.ScriptBlockBlockName | TokenFlags.Keyword;
            flagsArray[0x8e] = TokenFlags.Keyword;
            flagsArray[0x8f] = TokenFlags.Keyword;
            flagsArray[0x90] = TokenFlags.Keyword;
            flagsArray[0x91] = TokenFlags.Keyword;
            flagsArray[0x92] = TokenFlags.Keyword;
            flagsArray[0x93] = TokenFlags.Keyword;
            flagsArray[0x94] = TokenFlags.Keyword;
            flagsArray[0x95] = TokenFlags.Keyword;
            flagsArray[150] = TokenFlags.Keyword;
            flagsArray[0x97] = TokenFlags.Keyword;
            flagsArray[0x98] = TokenFlags.Keyword;
            flagsArray[0x99] = TokenFlags.Keyword;
            flagsArray[0x9a] = TokenFlags.Keyword;
            _staticTokenFlags = flagsArray;
            _tokenText = new string[] { 
                "unknown", "var", "@var", "param", "number", "label", "ident", "generic", "newline", "line continuation", "comment", "eof", "sqstr", "dqstr", "sq here string", "dq here string", 
                "(", ")", "{", "}", "[", "]", "@(", "@{", "$(", ";", "&&", "||", "&", "|", ",", "--", 
                "++", "..", "::", ".", "!", "*", "/", "%", "+", "-", "=", "+=", "-=", "*=", "/=", "%=", 
                "redirection", "<", "-f", "-not", "-bnot", "-and", "-or", "-xor", "-band", "-bor", "-bxor", "-join", "-eq", "-ne", "-ge", "-gt", 
                "-lt", "-le", "-ilike", "-inotlike", "-imatch", "-inotmatch", "-ireplace", "-icontains", "-inotcontains", "-iin", "-inotin", "-isplit", "-ceq", "-cne", "-cge", "-cgt", 
                "-clt", "-cle", "-clike", "-cnotlike", "-cmatch", "-cnotmatch", "-creplace", "-ccontains", "-cnotcontains", "-cin", "-cnotin", "-csplit", "-is", "-isnot", "-as", "++", 
                "--", "-shl", "-shr", "", "", "", "", "", "", "", "", "", "", "", "", "", 
                "", "", "", "", "", "", "", "begin", "break", "catch", "class", "continue", "data", "define", "do", "dynamicparam", 
                "else", "elseif", "end", "exit", "filter", "finally", "for", "foreach", "from", "function", "if", "in", "param", "process", "return", "switch", 
                "throw", "trap", "try", "until", "using", "var", "while", "workflow", "parallel", "sequence", "inlinescript"
             };
        }

        internal static int GetBinaryPrecedence(this TokenKind kind)
        {
            return (((int) _staticTokenFlags[(int) kind]) & 7);
        }

        public static TokenFlags GetTraits(this TokenKind kind)
        {
            return _staticTokenFlags[(int) kind];
        }

        public static bool HasTrait(this TokenKind kind, TokenFlags flag)
        {
            return ((kind.GetTraits() & flag) != TokenFlags.None);
        }

        public static string Text(this TokenKind kind)
        {
            return _tokenText[(int) kind];
        }
    }
}

