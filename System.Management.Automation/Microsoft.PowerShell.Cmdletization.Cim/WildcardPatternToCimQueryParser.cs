namespace Microsoft.PowerShell.Cmdletization.Cim
{
    using System;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class WildcardPatternToCimQueryParser : WildcardPatternParser
    {
        private bool needClientSideFiltering;
        private readonly StringBuilder result = new StringBuilder();

        protected override void AppendAsterix()
        {
            this.result.Append('%');
        }

        protected override void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange)
        {
            if (('[' <= startOfCharacterRange) && (startOfCharacterRange <= '^'))
            {
                startOfCharacterRange = 'Z';
                this.needClientSideFiltering = true;
            }
            if (('[' <= endOfCharacterRange) && (endOfCharacterRange <= '^'))
            {
                endOfCharacterRange = '_';
                this.needClientSideFiltering = true;
            }
            if (startOfCharacterRange == '-')
            {
                startOfCharacterRange = ',';
                this.needClientSideFiltering = true;
            }
            if (endOfCharacterRange == '-')
            {
                endOfCharacterRange = '.';
                this.needClientSideFiltering = true;
            }
            this.result.Append(startOfCharacterRange);
            this.result.Append('-');
            this.result.Append(endOfCharacterRange);
        }

        protected override void AppendLiteralCharacter(char c)
        {
            switch (c)
            {
                case '%':
                case '[':
                case '_':
                    this.BeginBracketExpression();
                    this.AppendLiteralCharacterToBracketExpression(c);
                    this.EndBracketExpression();
                    return;
            }
            this.result.Append(c);
        }

        protected override void AppendLiteralCharacterToBracketExpression(char c)
        {
            switch (c)
            {
                case '\\':
                case ']':
                case '^':
                case '-':
                    this.AppendCharacterRangeToBracketExpression(c, c);
                    return;
            }
            this.result.Append(c);
        }

        protected override void AppendQuestionMark()
        {
            this.result.Append('_');
        }

        protected override void BeginBracketExpression()
        {
            this.result.Append('[');
        }

        protected override void EndBracketExpression()
        {
            this.result.Append(']');
        }

        internal static string Parse(WildcardPattern wildcardPattern, out bool needsClientSideFiltering)
        {
            WildcardPatternToCimQueryParser parser = new WildcardPatternToCimQueryParser();
            WildcardPatternParser.Parse(wildcardPattern, parser);
            needsClientSideFiltering = parser.needClientSideFiltering;
            return parser.result.ToString();
        }
    }
}

