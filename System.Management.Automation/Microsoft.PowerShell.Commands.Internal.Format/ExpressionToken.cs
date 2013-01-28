namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class ExpressionToken
    {
        internal string expressionValue;
        internal bool isScriptBlock;

        internal ExpressionToken()
        {
        }

        internal ExpressionToken(string expressionValue, bool isScriptBlock)
        {
            this.expressionValue = expressionValue;
            this.isScriptBlock = isScriptBlock;
        }
    }
}

