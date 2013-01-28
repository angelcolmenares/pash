namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.Linq.Expressions;

    internal class DataServiceExecutionProvider : IDataServiceExecutionProvider
    {
        public object Execute(Expression requestExpression, DataServiceOperationContext operationContext)
        {
            return ExpressionEvaluator.Evaluate(requestExpression);
        }
    }
}

