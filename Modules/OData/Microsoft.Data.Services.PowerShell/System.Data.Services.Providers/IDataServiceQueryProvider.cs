namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal interface IDataServiceQueryProvider
    {
        object GetOpenPropertyValue(object target, string propertyName);
        IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target);
        object GetPropertyValue(object target, ResourceProperty resourceProperty);
        IQueryable GetQueryRootForResourceSet(ResourceSet resourceSet);
        ResourceType GetResourceType(object target);
        object InvokeServiceOperation(ServiceOperation serviceOperation, object[] parameters);

        object CurrentDataSource { get; set; }

        bool IsNullPropagationRequired { get; }
    }
}

