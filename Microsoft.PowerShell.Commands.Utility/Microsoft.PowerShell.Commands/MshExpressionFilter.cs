namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;

    internal sealed class MshExpressionFilter
    {
        private WildcardPattern[] _wildcardPatterns;

        internal MshExpressionFilter(string[] wildcardPatternsStrings)
        {
            if (wildcardPatternsStrings == null)
            {
                throw new ArgumentNullException("wildcardPatternsStrings");
            }
            this._wildcardPatterns = new WildcardPattern[wildcardPatternsStrings.Length];
            for (int i = 0; i < wildcardPatternsStrings.Length; i++)
            {
                this._wildcardPatterns[i] = new WildcardPattern(wildcardPatternsStrings[i], WildcardOptions.IgnoreCase);
            }
        }

        internal bool IsMatch(MshExpression expression)
        {
            for (int i = 0; i < this._wildcardPatterns.Length; i++)
            {
                if (this._wildcardPatterns[i].IsMatch(expression.ToString()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

