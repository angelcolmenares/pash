namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal sealed class ODataBatchOperationRequestMessage : IODataRequestMessageAsync, IODataRequestMessage, IODataUrlResolver
    {
        private readonly ODataBatchOperationMessage message;

        private ODataBatchOperationRequestMessage(Func<Stream> contentStreamCreatorFunc, string method, Uri requestUrl, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver, bool writing)
        {
            this.Method = method;
            this.Url = requestUrl;
            this.message = new ODataBatchOperationMessage(contentStreamCreatorFunc, headers, operationListener, urlResolver, writing);
        }

        internal static ODataBatchOperationRequestMessage CreateReadMessage(ODataBatchReaderStream batchReaderStream, string method, Uri requestUrl, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver)
        {
            return new ODataBatchOperationRequestMessage(() => ODataBatchUtils.CreateBatchOperationReadStream(batchReaderStream, headers, operationListener), method, requestUrl, headers, operationListener, urlResolver, false);
        }

        internal static ODataBatchOperationRequestMessage CreateWriteMessage(Stream outputStream, string method, Uri requestUrl, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver)
        {
            return new ODataBatchOperationRequestMessage(() => ODataBatchUtils.CreateBatchOperationWriteStream(outputStream, operationListener), method, requestUrl, null, operationListener, urlResolver, true);
        }

        public string GetHeader(string headerName)
        {
            return this.message.GetHeader(headerName);
        }

        public Stream GetStream()
        {
            return this.message.GetStream();
        }

        public Task<Stream> GetStreamAsync()
        {
            return this.message.GetStreamAsync();
        }

        Uri IODataUrlResolver.ResolveUrl(Uri baseUri, Uri payloadUri)
        {
            return this.message.ResolveUrl(baseUri, payloadUri);
        }

        public void SetHeader(string headerName, string headerValue)
        {
            this.message.SetHeader(headerName, headerValue);
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.message.Headers;
            }
        }

        public string Method { get; set; }

        internal ODataBatchOperationMessage OperationMessage
        {
            get
            {
                return this.message;
            }
        }

        public Uri Url { get; set; }
    }
}

