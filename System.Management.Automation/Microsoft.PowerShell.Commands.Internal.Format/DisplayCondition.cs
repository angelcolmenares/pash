namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    internal static class DisplayCondition
    {
        internal static bool Evaluate(PSObject obj, MshExpression ex, out MshExpressionResult expressionResult)
        {
            expressionResult = null;
            List<MshExpressionResult> values = ex.GetValues(obj);
            if (values.Count == 0)
            {
                return false;
            }
            if (values[0].Exception != null)
            {
                expressionResult = values[0];
                return false;
            }
            return LanguagePrimitives.IsTrue(values[0].Result);
        }
    }
}

