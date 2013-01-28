namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class MshResolvedExpressionParameterAssociation
    {
        private MshParameter _originatingParameter;
        private MshExpression _resolvedExpression;
        [TraceSource("MshResolvedExpressionParameterAssociation", "MshResolvedExpressionParameterAssociation")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("MshResolvedExpressionParameterAssociation", "MshResolvedExpressionParameterAssociation");

        internal MshResolvedExpressionParameterAssociation(MshParameter parameter, MshExpression expression)
        {
            if (expression == null)
            {
                throw PSTraceSource.NewArgumentNullException("expression");
            }
            this._originatingParameter = parameter;
            this._resolvedExpression = expression;
        }

        internal MshParameter OriginatingParameter
        {
            get
            {
                return this._originatingParameter;
            }
        }

        internal MshExpression ResolvedExpression
        {
            get
            {
                return this._resolvedExpression;
            }
        }
    }
}

