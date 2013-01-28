namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Linq;

    internal interface IDataServicePagingProvider
    {
        object[] GetContinuationToken(IEnumerator enumerator);
        void SetContinuationToken(IQueryable query, ResourceType resourceType, object[] continuationToken);
    }
}

