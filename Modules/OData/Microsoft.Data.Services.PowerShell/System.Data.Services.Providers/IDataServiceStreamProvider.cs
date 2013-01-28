namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.IO;

    internal interface IDataServiceStreamProvider
    {
        void DeleteStream(object entity, DataServiceOperationContext operationContext);
        Stream GetReadStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext);
        Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext);
        string GetStreamContentType(object entity, DataServiceOperationContext operationContext);
        string GetStreamETag(object entity, DataServiceOperationContext operationContext);
        Stream GetWriteStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext);
        string ResolveType(string entitySetName, DataServiceOperationContext operationContext);

        int StreamBufferSize { get; }
    }
}

