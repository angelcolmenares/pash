namespace Microsoft.Data.OData
{
    using System;

    internal static class ODataConstants
    {
        internal const string BatchContentTransferEncoding = "binary";
        internal const string BatchRequestBoundaryTemplate = "batch_{0}";
        internal const string BatchResponseBoundaryTemplate = "batchresponse_{0}";
        internal const string Charset = "charset";
        public const string ContentIdHeader = "Content-ID";
        internal const string ContentLengthHeader = "Content-Length";
        internal const string ContentTransferEncoding = "Content-Transfer-Encoding";
        public const string ContentTypeHeader = "Content-Type";
        public const string DataServiceVersionHeader = "DataServiceVersion";
        internal const int DefaultMaxEntityPropertyMappingsPerType = 100;
        internal const int DefaultMaxPartsPerBatch = 100;
        internal const long DefaultMaxReadMessageSize = 0x100000L;
        internal const int DefaultMaxRecursionDepth = 100;
        internal const int DefulatMaxOperationsPerChangeset = 0x3e8;
        internal const string HttpMultipartBoundary = "boundary";
        internal const string HttpQValueParameter = "q";
        internal const string HttpVersionInBatching = "HTTP/1.1";
        internal const string HttpWeakETagPrefix = "W/\"";
        internal const string HttpWeakETagSuffix = "\"";
        internal const ODataVersion MaxODataVersion = ODataVersion.V3;
        public const string MethodDelete = "DELETE";
        public const string MethodGet = "GET";
        public const string MethodMerge = "MERGE";
        public const string MethodPatch = "PATCH";
        public const string MethodPost = "POST";
        public const string MethodPut = "PUT";
        internal const ODataVersion ODataDefaultProtocolVersion = ODataVersion.V3;
        internal const string RequestChangeSetBoundaryTemplate = "changeset_{0}";
        internal const string ResponseChangeSetBoundaryTemplate = "changesetresponse_{0}";
    }
}

