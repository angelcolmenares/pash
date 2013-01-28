namespace System.Management.Automation
{
    using System;
    using System.Text;

    internal class WildcardPatternToDosWildcardParser : WildcardPatternParser
    {
        private readonly StringBuilder result = new StringBuilder();

        protected override void AppendAsterix()
        {
            this.result.Append('*');
        }

        protected override void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange)
        {
        }

        protected override void AppendLiteralCharacter(char c)
        {
            this.result.Append(c);
        }

        protected override void AppendLiteralCharacterToBracketExpression(char c)
        {
        }

        protected override void AppendQuestionMark()
        {
            this.result.Append('?');
        }

        protected override void BeginBracketExpression()
        {
        }

        protected override void EndBracketExpression()
        {
            this.result.Append('?');
        }

        internal static string Parse(WildcardPattern wildcardPattern)
        {
            WildcardPatternToDosWildcardParser parser = new WildcardPatternToDosWildcardParser();
            WildcardPatternParser.Parse(wildcardPattern, parser);
            return parser.result.ToString();
        }
    }
}

