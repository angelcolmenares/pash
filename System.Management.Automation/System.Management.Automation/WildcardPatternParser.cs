namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Text;

    internal abstract class WildcardPatternParser
    {
        protected WildcardPatternParser()
        {
        }

        protected abstract void AppendAsterix();
        internal void AppendBracketExpression(string brackedExpressionContents, string bracketExpressionOperators, string pattern)
        {
            this.BeginBracketExpression();
            int num = 0;
            while (num < brackedExpressionContents.Length)
            {
                if (((num + 2) < brackedExpressionContents.Length) && (bracketExpressionOperators[num + 1] == '-'))
                {
                    char startOfCharacterRange = brackedExpressionContents[num];
                    char endOfCharacterRange = brackedExpressionContents[num + 2];
                    num += 3;
                    if (startOfCharacterRange > endOfCharacterRange)
                    {
                        throw NewWildcardPatternException(pattern);
                    }
                    this.AppendCharacterRangeToBracketExpression(startOfCharacterRange, endOfCharacterRange);
                }
                else
                {
                    this.AppendLiteralCharacterToBracketExpression(brackedExpressionContents[num]);
                    num++;
                }
            }
            this.EndBracketExpression();
        }

        protected abstract void AppendCharacterRangeToBracketExpression(char startOfCharacterRange, char endOfCharacterRange);
        protected abstract void AppendLiteralCharacter(char c);
        protected abstract void AppendLiteralCharacterToBracketExpression(char c);
        protected abstract void AppendQuestionMark();
        protected abstract void BeginBracketExpression();
        protected virtual void BeginWildcardPattern(WildcardPattern pattern)
        {
        }

        protected abstract void EndBracketExpression();
        protected virtual void EndWildcardPattern()
        {
        }

        internal static WildcardPatternException NewWildcardPatternException(string invalidPattern)
        {
            ParentContainsErrorRecordException exception = new ParentContainsErrorRecordException(StringUtil.Format(WildcardPatternStrings.InvalidPattern, invalidPattern));
            return new WildcardPatternException(new ErrorRecord(exception, "WildcardPattern_Invalid", ErrorCategory.InvalidArgument, null));
        }

        public static void Parse(WildcardPattern pattern, WildcardPatternParser parser)
        {
            parser.BeginWildcardPattern(pattern);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            StringBuilder builder = null;
            StringBuilder builder2 = null;
            foreach (char ch in pattern.Pattern)
            {
                if (flag3)
                {
                    if (((ch == ']') && !flag2) && !flag)
                    {
                        flag3 = false;
                        parser.AppendBracketExpression(builder.ToString(), builder2.ToString(), pattern.Pattern);
                        builder = null;
                        builder2 = null;
                    }
                    else if ((ch != '`') || flag)
                    {
                        builder.Append(ch);
                        builder2.Append(((ch == '-') && !flag) ? '-' : ' ');
                    }
                    flag2 = false;
                }
                else if ((ch == '*') && !flag)
                {
                    parser.AppendAsterix();
                }
                else if ((ch == '?') && !flag)
                {
                    parser.AppendQuestionMark();
                }
                else if ((ch == '[') && !flag)
                {
                    flag3 = true;
                    builder = new StringBuilder();
                    builder2 = new StringBuilder();
                    flag2 = true;
                }
                else if ((ch != '`') || flag)
                {
                    parser.AppendLiteralCharacter(ch);
                }
                flag = (ch == '`') && !flag;
            }
            if (flag3)
            {
                throw NewWildcardPatternException(pattern.Pattern);
            }
            if (flag && !pattern.Pattern.Equals("`", StringComparison.Ordinal))
            {
                parser.AppendLiteralCharacter(pattern.Pattern[pattern.Pattern.Length - 1]);
            }
            parser.EndWildcardPattern();
        }
    }
}

