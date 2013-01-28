namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class ODataBatchOperationMessage : ODataMessage
    {
        private Func<Stream> contentStreamCreatorFunc;
        private ODataBatchOperationHeaders headers;
        private readonly IODataBatchOperationListener operationListener;
        private readonly IODataUrlResolver urlResolver;

        internal ODataBatchOperationMessage(Func<Stream> contentStreamCreatorFunc, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener, IODataUrlResolver urlResolver, bool writing) : base(writing, false, -1L)
        {
            this.contentStreamCreatorFunc = contentStreamCreatorFunc;
            this.operationListener = operationListener;
            this.headers = headers;
            this.urlResolver = urlResolver;
        }

        public override string GetHeader(string headerName)
        {
            string str;
            if ((this.headers != null) && this.headers.TryGetValue(headerName, out str))
            {
                return str;
            }
            return null;
        }

        public override Stream GetStream()
        {
            this.VerifyNotCompleted();
            this.operationListener.BatchOperationContentStreamRequested();
            Stream stream = this.contentStreamCreatorFunc();
            this.PartHeaderProcessingCompleted();
            return stream;
        }

        public override Task<Stream> GetStreamAsync()
        {
            this.VerifyNotCompleted();
            Task antecedentTask = this.operationListener.BatchOperationContentStreamRequestedAsync();
            Stream contentStream = this.contentStreamCreatorFunc();
            this.PartHeaderProcessingCompleted();
            return antecedentTask.FollowOnSuccessWith<Stream>(task => contentStream);
        }

        internal void PartHeaderProcessingCompleted()
        {
            this.contentStreamCreatorFunc = null;
        }

        internal Uri ResolveUrl(Uri baseUri, Uri payloadUri)
        {
            ExceptionUtils.CheckArgumentNotNull<Uri>(payloadUri, "payloadUri");
            if (this.urlResolver != null)
            {
                return this.urlResolver.ResolveUrl(baseUri, payloadUri);
            }
            return null;
        }

        public override void SetHeader(string headerName, string headerValue)
        {
            this.VerifyNotCompleted();
            base.VerifyCanSetHeader();
            if (headerValue == null)
            {
                if (this.headers != null)
                {
                    this.headers.Remove(headerName);
                }
            }
            else
            {
                if (this.headers == null)
                {
                    this.headers = new ODataBatchOperationHeaders();
                }
                this.headers[headerName] = headerValue;
            }
        }

        internal void VerifyNotCompleted()
        {
            if (this.contentStreamCreatorFunc == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataBatchOperationMessage_VerifyNotCompleted);
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return (this.headers ?? Enumerable.Empty<KeyValuePair<string, string>>());
            }
        }
    }
}

