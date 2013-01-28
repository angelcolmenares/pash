namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class DataServiceExecutionProviderMethods
    {
        internal static readonly MethodInfo ApplyProjectionsMethodInfo = typeof(DataServiceExecutionProviderMethods).GetMethod("ApplyProjections", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo CreateServiceActionInvokableMethodInfo = typeof(DataServiceExecutionProviderMethods).GetMethod("CreateServiceActionInvokable", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo OfTypeMethodInfo = typeof(DataServiceExecutionProviderMethods).GetMethod("OfType", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo SetContinuationTokenMethodInfo = typeof(DataServiceExecutionProviderMethods).GetMethod("SetContinuationToken", BindingFlags.Public | BindingFlags.Static);

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static IQueryable ApplyProjections(object projectionProvider, IQueryable source, object rootProjectionNode)
        {
            return ((IProjectionProvider) projectionProvider).ApplyProjections(source, (RootProjectionNode) rootProjectionNode);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static IDataServiceInvokable CreateServiceActionInvokable(DataServiceOperationContext operationContext, IDataServiceActionProvider actionProvider, ServiceAction serviceAction, object[] parameterTokens)
        {
            WebUtil.CheckArgumentNull<DataServiceOperationContext>(operationContext, "operationContext");
            WebUtil.CheckArgumentNull<IDataServiceActionProvider>(actionProvider, "actionProvider");
            WebUtil.CheckArgumentNull<ServiceAction>(serviceAction, "serviceAction");
            IDataServiceInvokable invokable = actionProvider.CreateInvokable(operationContext, serviceAction, parameterTokens);
            WebUtil.CheckResourceExists(invokable != null, serviceAction.Name);
            return invokable;
        }

        public static IQueryable<TResult> OfType<TSource, TResult>(IQueryable<TSource> query, ResourceType resourceType)
        {
            if (query == null)
            {
                throw System.Data.Services.Error.ArgumentNull("query");
            }
            if (resourceType == null)
            {
                throw System.Data.Services.Error.ArgumentNull("resourceType");
            }
            return query.Provider.CreateQuery<TResult>(Expression.Call(null, DataServiceProviderMethods.OfTypeIQueryableMethodInfo.MakeGenericMethod(new Type[] { typeof(TSource), typeof(TResult) }), new Expression[] { query.Expression, Expression.Constant(resourceType) }));
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static IQueryable<TElement> SetContinuationToken<TElement>(IDataServicePagingProvider pagingProvider, IQueryable<TElement> query, ResourceType resourceType, object[] continuationToken)
        {
            WebUtil.CheckArgumentNull<IDataServicePagingProvider>(pagingProvider, "pagingProvider");
            pagingProvider.SetContinuationToken(query, resourceType, continuationToken);
            return query;
        }
    }
}

