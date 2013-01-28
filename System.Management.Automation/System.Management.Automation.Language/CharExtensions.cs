namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class CharExtensions
    {
        private static readonly CharTraits[] _traits;

        static CharExtensions()
        {
            CharTraits[] traitsArray = new CharTraits[0x80];
            traitsArray[0] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken;
            traitsArray[9] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Whitespace;
            traitsArray[10] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Newline;
            traitsArray[11] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Whitespace;
            traitsArray[12] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Whitespace;
            traitsArray[13] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Newline;
            traitsArray[0x20] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken | CharTraits.Whitespace;
            traitsArray[0x21] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x23] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x24] = CharTraits.VarNameFirst;
            traitsArray[0x25] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x26] = CharTraits.ForceStartNewToken;
            traitsArray[40] = CharTraits.ForceStartNewToken;
            traitsArray[0x29] = CharTraits.ForceStartNewToken;
            traitsArray[0x2a] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x2b] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x2c] = CharTraits.ForceStartNewAssemblyNameSpecToken | CharTraits.ForceStartNewToken;
            traitsArray[0x2d] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x2e] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x2f] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x30] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x31] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[50] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x33] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x34] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x35] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x36] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x37] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x38] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x39] = CharTraits.VarNameFirst | CharTraits.Digit | CharTraits.HexDigit;
            traitsArray[0x3a] = CharTraits.VarNameFirst;
            traitsArray[0x3b] = CharTraits.ForceStartNewToken;
            traitsArray[60] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x3d] = CharTraits.ForceStartNewTokenAfterNumber | CharTraits.ForceStartNewAssemblyNameSpecToken;
            traitsArray[0x3e] = CharTraits.ForceStartNewTokenAfterNumber;
            traitsArray[0x3f] = CharTraits.VarNameFirst;
            traitsArray[0x41] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x42] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x43] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x44] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.TypeSuffix | CharTraits.IdentifierStart;
            traitsArray[0x45] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[70] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x47] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x48] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x49] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x4a] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x4b] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x4c] = CharTraits.VarNameFirst | CharTraits.TypeSuffix | CharTraits.IdentifierStart;
            traitsArray[0x4d] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x4e] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x4f] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[80] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x51] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x52] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x53] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x54] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x55] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x56] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x57] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x58] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x59] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[90] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x5d] = CharTraits.ForceStartNewTokenAfterNumber | CharTraits.ForceStartNewAssemblyNameSpecToken;
            traitsArray[0x5e] = CharTraits.VarNameFirst;
            traitsArray[0x5f] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x61] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x62] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x63] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[100] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.TypeSuffix | CharTraits.IdentifierStart;
            traitsArray[0x65] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x66] = CharTraits.VarNameFirst | CharTraits.HexDigit | CharTraits.IdentifierStart;
            traitsArray[0x67] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x68] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x69] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x6a] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x6b] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x6c] = CharTraits.VarNameFirst | CharTraits.TypeSuffix | CharTraits.IdentifierStart;
            traitsArray[0x6d] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[110] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x6f] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x70] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x71] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x72] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x73] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x74] = CharTraits.VarNameFirst | CharTraits.MultiplierStart | CharTraits.IdentifierStart;
            traitsArray[0x75] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x76] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x77] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[120] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x79] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x7a] = CharTraits.VarNameFirst | CharTraits.IdentifierStart;
            traitsArray[0x7b] = CharTraits.ForceStartNewToken;
            traitsArray[0x7c] = CharTraits.ForceStartNewToken;
            traitsArray[0x7d] = CharTraits.ForceStartNewToken;
            _traits = traitsArray;
        }

        internal static bool ForceStartNewToken(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.ForceStartNewToken) != CharTraits.None);
            }
            return c.IsWhitespace();
        }

        internal static bool ForceStartNewTokenAfterNumber(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.ForceStartNewTokenAfterNumber) != CharTraits.None);
            }
            return c.IsDash();
        }

        internal static bool ForceStartNewTokenInAssemblyNameSpec(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.ForceStartNewAssemblyNameSpecToken) != CharTraits.None);
            }
            return c.IsWhitespace();
        }

        internal static bool IsDash(this char c)
        {
            if (((c != '-') && (c != '–')) && (c != '—'))
            {
                return (c == '―');
            }
            return true;
        }

        internal static bool IsDecimalDigit(this char c)
        {
            return ((c < '\x0080') && ((_traits[c] & CharTraits.Digit) != CharTraits.None));
        }

        internal static bool IsDoubleQuote(this char c)
        {
            if (((c != '"') && (c != '“')) && (c != '”'))
            {
                return (c == '„');
            }
            return true;
        }

        internal static bool IsHexDigit(this char c)
        {
            return ((c < '\x0080') && ((_traits[c] & CharTraits.HexDigit) != CharTraits.None));
        }

        internal static bool IsIndentifierFollow(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & (CharTraits.Digit | CharTraits.IdentifierStart)) != CharTraits.None);
            }
            return char.IsLetterOrDigit(c);
        }

        internal static bool IsIndentifierStart(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.IdentifierStart) != CharTraits.None);
            }
            return char.IsLetter(c);
        }

        internal static bool IsMultiplierStart(this char c)
        {
            return ((c < '\x0080') && ((_traits[c] & CharTraits.MultiplierStart) != CharTraits.None));
        }

        internal static bool IsSingleQuote(this char c)
        {
            if (((c != '\'') && (c != '‘')) && ((c != '’') && (c != '‚')))
            {
                return (c == '‛');
            }
            return true;
        }

        internal static bool IsTypeSuffix(this char c)
        {
            return ((c < '\x0080') && ((_traits[c] & CharTraits.TypeSuffix) != CharTraits.None));
        }

        internal static bool IsVariableStart(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.VarNameFirst) != CharTraits.None);
            }
            return char.IsLetterOrDigit(c);
        }

        internal static bool IsWhitespace(this char c)
        {
            if (c < '\x0080')
            {
                return ((_traits[c] & CharTraits.Whitespace) != CharTraits.None);
            }
            if (c > 'Ā')
            {
                return char.IsSeparator(c);
            }
            if (c != '\x00a0')
            {
                return (c == '\x0085');
            }
            return true;
        }
    }
}

