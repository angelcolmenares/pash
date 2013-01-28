namespace System.Management.Automation
{
    using System;

    internal static class SpecialCharacters
    {
        public const char emDash = '—';
        public const char enDash = '–';
        public const char horizontalBar = '―';
        public const char quoteDoubleLeft = '“';
        public const char quoteDoubleRight = '”';
        public const char quoteLowDoubleLeft = '„';
        public const char quoteReversed = '‛';
        public const char quoteSingleBase = '‚';
        public const char quoteSingleLeft = '‘';
        public const char quoteSingleRight = '’';

        public static char AsQuote(char c)
        {
            if (IsSingleQuote(c))
            {
                return '\'';
            }
            if (IsDoubleQuote(c))
            {
                return '"';
            }
            return c;
        }

        public static bool IsDash(char c)
        {
            if (((c != '–') && (c != '—')) && (c != '―'))
            {
                return (c == '-');
            }
            return true;
        }

        public static bool IsDelimiter(char c, char delimiter)
        {
            if (delimiter == '"')
            {
                return IsDoubleQuote(c);
            }
            if (delimiter == '\'')
            {
                return IsSingleQuote(c);
            }
            return (c == delimiter);
        }

        public static bool IsDoubleQuote(char c)
        {
            if (((c != '"') && (c != '“')) && (c != '”'))
            {
                return (c == '„');
            }
            return true;
        }

        public static bool IsQuote(char c)
        {
            if (!IsSingleQuote(c))
            {
                return IsDoubleQuote(c);
            }
            return true;
        }

        public static bool IsSingleQuote(char c)
        {
            if (((c != '‘') && (c != '’')) && ((c != '‚') && (c != '‛')))
            {
                return (c == '\'');
            }
            return true;
        }
    }
}

