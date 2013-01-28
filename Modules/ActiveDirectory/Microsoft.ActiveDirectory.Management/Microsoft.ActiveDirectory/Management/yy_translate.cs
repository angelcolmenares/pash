namespace Microsoft.ActiveDirectory.Management
{
    using System;
    using System.Globalization;

    internal class yy_translate
    {
        public static char translate(char c)
        {
            if (char.IsWhiteSpace(c))
            {
                return ' ';
            }
            if ((((((c == '\r') || (c == '@')) || ((c == '\n') || (c == '\t'))) || (((c == '\b') || (c == '|')) || ((c == ' ') || (c == ':')))) || ((((c == '+') || (c == '*')) || ((c == '-') || (c == '/'))) || (((c == '{') || (c == '}')) || ((c == '^') || (c == '='))))) || (((((c == '<') || (c == '>')) || ((c == '|') || (c == '&'))) || (((c == '$') || (c == '.')) || ((c == '?') || (c == '!')))) || ((((c == '%') || (c == '(')) || ((c == ')') || (c == '['))) || ((((c == ']') || (c == '\'')) || ((c == '"') || (c == '`'))) || (((c == ',') || (c == '#')) || (c == '_'))))))
            {
                return c;
            }
            if ((c >= 'a') && (c <= 'z'))
            {
                return c;
            }
            if ((c >= 'A') && (c <= 'Z'))
            {
                return c;
            }
            if ((c >= '0') && (c <= '9'))
            {
                return c;
            }
            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return 'w';

                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.Format:
                case UnicodeCategory.ConnectorPunctuation:
                    return '$';
            }
            return '~';
        }
    }
}

