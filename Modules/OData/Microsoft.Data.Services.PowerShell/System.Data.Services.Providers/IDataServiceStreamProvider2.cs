namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.IO;

    internal interface IDataServiceStreamProvider2 : IDataServiceStreamProvider
    {
        Stream GetReadStream(object entity, ResourceProperty streamProperty, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext);
        Uri GetReadStreamUri(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext);
        string GetStreamContentType(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext);
        string GetStreamETag(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext);
        Stream GetWriteStream(object entity, ResourceProperty streamProperty, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext);
    }
}

