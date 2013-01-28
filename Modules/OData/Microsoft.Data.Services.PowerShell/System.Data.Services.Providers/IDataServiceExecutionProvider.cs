namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.Linq.Expressions;

    internal interface IDataServiceExecutionProvider
    {
        object Execute(Expression requestExpression, DataServiceOperationContext context);
    }
}

