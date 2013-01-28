namespace System.Data.Services.Providers
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Runtime.InteropServices;

    internal interface IDataServiceActionProvider
    {
        bool AdvertiseServiceAction(DataServiceOperationContext operationContext, ServiceAction serviceAction, object resourceInstance, bool resourceInstanceInFeed, ref ODataAction actionToSerialize);
        IDataServiceInvokable CreateInvokable(DataServiceOperationContext operationContext, ServiceAction serviceAction, object[] parameterTokens);
        IEnumerable<ServiceAction> GetServiceActions(DataServiceOperationContext operationContext);
        IEnumerable<ServiceAction> GetServiceActionsByBindingParameterType(DataServiceOperationContext operationContext, ResourceType bindingParameterType);
        bool TryResolveServiceAction(DataServiceOperationContext operationContext, string serviceActionName, out ServiceAction serviceAction);
    }
}

