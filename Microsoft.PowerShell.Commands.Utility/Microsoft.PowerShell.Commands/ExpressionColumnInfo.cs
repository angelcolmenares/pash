namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal class ExpressionColumnInfo : Microsoft.PowerShell.Commands.ColumnInfo
    {
        private MshExpression expression;

        internal ExpressionColumnInfo(string staleObjectPropertyName, string displayName, MshExpression expression) : base(staleObjectPropertyName, displayName)
        {
            this.expression = expression;
        }

        internal override object GetValue(PSObject liveObject)
        {
            List<MshExpressionResult> values = this.expression.GetValues(liveObject);
            if (values.Count == 0)
            {
                return null;
            }
            MshExpressionResult result = values[0];
            if (result.Exception != null)
            {
                return null;
            }
            object obj2 = result.Result;
            if (obj2 != null)
            {
                return Microsoft.PowerShell.Commands.ColumnInfo.LimitString(obj2.ToString());
            }
            return string.Empty;
        }
    }
}

