namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class MshExpressionResult
    {
        private System.Exception _exception;
        private MshExpression _resolvedExpression;
        private object _result;

        internal MshExpressionResult(object res, MshExpression re, System.Exception e)
        {
            this._result = res;
            this._resolvedExpression = re;
            this._exception = e;
        }

        internal System.Exception Exception
        {
            get
            {
                return this._exception;
            }
        }

        internal MshExpression ResolvedExpression
        {
            get
            {
                return this._resolvedExpression;
            }
        }

        internal object Result
        {
            get
            {
                return this._result;
            }
        }
    }
}

