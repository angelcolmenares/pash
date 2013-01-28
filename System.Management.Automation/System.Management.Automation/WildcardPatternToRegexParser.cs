namespace System.Management.Automation
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class WildcardPatternToRegexParser : WildcardPatternParser
    {
        private const string regexChars = @"()[.?*{}^$+|\";
        private RegexOptions regexOptions;
        private StringBuilder regexPattern;

        protected override void AppendAsterix()
        {
            this.regexPattern.Append(".*");
        }

        protected override void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange)
        {
            AppendCharacterRangeToBracketExpression(this.regexPattern, startOfCharacterRange, endOfCharacterRange);
        }

        internal static void AppendCharacterRangeToBracketExpression(StringBuilder regexPattern, char startOfCharacterRange, char endOfCharacterRange)
        {
            AppendLiteralCharacterToBracketExpression(regexPattern, startOfCharacterRange);
            regexPattern.Append('-');
            AppendLiteralCharacterToBracketExpression(regexPattern, endOfCharacterRange);
        }

        protected override void AppendLiteralCharacter(char c)
        {
            AppendLiteralCharacter(this.regexPattern, c);
        }

        internal static void AppendLiteralCharacter(StringBuilder regexPattern, char c)
        {
            if (IsRegexChar(c))
            {
                regexPattern.Append('\\');
            }
            regexPattern.Append(c);
        }

        protected override void AppendLiteralCharacterToBracketExpression(char c)
        {
            AppendLiteralCharacterToBracketExpression(this.regexPattern, c);
        }

        internal static void AppendLiteralCharacterToBracketExpression(StringBuilder regexPattern, char c)
        {
            if (c == '[')
            {
                regexPattern.Append('[');
            }
            else if (c == ']')
            {
                regexPattern.Append(@"\]");
            }
            else if (c == '-')
            {
                regexPattern.Append(@"\x2d");
            }
            else
            {
                AppendLiteralCharacter(regexPattern, c);
            }
        }

        protected override void AppendQuestionMark()
        {
            this.regexPattern.Append('.');
        }

        protected override void BeginBracketExpression()
        {
            this.regexPattern.Append('[');
        }

        protected override void BeginWildcardPattern(WildcardPattern pattern)
        {
            this.regexPattern = new StringBuilder((pattern.Pattern.Length * 2) + 2);
            this.regexPattern.Append('^');
            this.regexOptions = TranslateWildcardOptionsIntoRegexOptions(pattern.Options);
        }

        protected override void EndBracketExpression()
        {
            this.regexPattern.Append(']');
        }

        protected override void EndWildcardPattern()
        {
            this.regexPattern.Append('$');
            string str = this.regexPattern.ToString();
            if (str.Equals("^.*$", StringComparison.Ordinal))
            {
                this.regexPattern.Remove(0, 4);
            }
            else
            {
                if (str.StartsWith("^.*", StringComparison.Ordinal))
                {
                    this.regexPattern.Remove(0, 3);
                }
                if (str.EndsWith(".*$", StringComparison.Ordinal))
                {
                    this.regexPattern.Remove(this.regexPattern.Length - 3, 3);
                }
            }
        }

        private static bool IsRegexChar(char ch)
        {
            for (int i = 0; i < @"()[.?*{}^$+|\".Length; i++)
            {
                if (ch == @"()[.?*{}^$+|\"[i])
                {
                    return true;
                }
            }
            return false;
        }

        public static Regex Parse(WildcardPattern wildcardPattern)
        {
            Regex regex;
            WildcardPatternToRegexParser parser = new WildcardPatternToRegexParser();
            WildcardPatternParser.Parse(wildcardPattern, parser);
            try
            {
                regex = new Regex(parser.regexPattern.ToString(), parser.regexOptions);
            }
            catch (ArgumentException)
            {
                throw WildcardPatternParser.NewWildcardPatternException(wildcardPattern.Pattern);
            }
            return regex;
        }

        internal static RegexOptions TranslateWildcardOptionsIntoRegexOptions(WildcardOptions options)
        {
            RegexOptions singleline = RegexOptions.Singleline;
            if ((options & WildcardOptions.Compiled) != WildcardOptions.None)
            {
                singleline |= RegexOptions.Compiled;
            }
            if ((options & WildcardOptions.IgnoreCase) != WildcardOptions.None)
            {
                singleline |= RegexOptions.IgnoreCase;
            }
            if ((options & WildcardOptions.CultureInvariant) == WildcardOptions.CultureInvariant)
            {
                singleline |= RegexOptions.CultureInvariant;
            }
            return singleline;
        }
    }
}

