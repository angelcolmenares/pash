namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    internal sealed class ODataBatchOperationResponseMessage : IODataResponseMessageAsync, IODataResponseMessage, IODataUrlResolver
    {
        private readonly ODataBatchOperationMessage message;
        private int statusCode;

        private ODataBatchOperationResponseMessage(Func<Stream> contentStreamCreatorFunc, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver, bool writing)
        {
            this.message = new ODataBatchOperationMessage(contentStreamCreatorFunc, headers, operationListener, urlResolver, writing);
        }

        internal static ODataBatchOperationResponseMessage CreateReadMessage(ODataBatchReaderStream batchReaderStream, int statusCode, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver)
        {
            return new ODataBatchOperationResponseMessage(() => ODataBatchUtils.CreateBatchOperationReadStream(batchReaderStream, headers, operationListener), headers, operationListener, urlResolver, false) { statusCode = statusCode };
        }

        internal static ODataBatchOperationResponseMessage CreateWriteMessage(Stream outputStream, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver)
        {
            return new ODataBatchOperationResponseMessage(() => ODataBatchUtils.CreateBatchOperationWriteStream(outputStream, operationListener), null, operationListener, urlResolver, true);
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

        internal ODataBatchOperationMessage OperationMessage
        {
            get
            {
                return this.message;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.statusCode;
            }
            set
            {
                this.message.VerifyNotCompleted();
                this.statusCode = value;
            }
        }
    }
}

