namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class MshExpressionFactory
    {
        private Dictionary<ExpressionToken, MshExpression> _expressionCache;

        internal MshExpression CreateFromExpressionToken(ExpressionToken et)
        {
            return this.CreateFromExpressionToken(et, null);
        }

        internal MshExpression CreateFromExpressionToken(ExpressionToken et, DatabaseLoadingInfo loadingInfo)
        {
            if (!et.isScriptBlock)
            {
                return new MshExpression(et.expressionValue);
            }
            if (this._expressionCache != null)
            {
                MshExpression expression;
                if (this._expressionCache.TryGetValue(et, out expression))
                {
                    return expression;
                }
            }
            else
            {
                this._expressionCache = new Dictionary<ExpressionToken, MshExpression>();
            }
            ScriptBlock scriptBlock = ScriptBlock.Create(et.expressionValue);
            scriptBlock.DebuggerStepThrough = true;
            if ((loadingInfo != null) && loadingInfo.isFullyTrusted)
            {
                scriptBlock.LanguageMode = 0;
            }
            MshExpression expression2 = new MshExpression(scriptBlock);
            this._expressionCache.Add(et, expression2);
            return expression2;
        }

        internal void VerifyScriptBlockText(string scriptText)
        {
            ScriptBlock.Create(scriptText);
        }
    }
}

