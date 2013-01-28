namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    internal sealed class ODataRequestMessage : ODataMessage, IODataRequestMessageAsync, IODataRequestMessage
    {
        private readonly IODataRequestMessage requestMessage;

        internal ODataRequestMessage(IODataRequestMessage requestMessage, bool writing, bool disableMessageStreamDisposal, long maxMessageSize) : base(writing, disableMessageStreamDisposal, maxMessageSize)
        {
            this.requestMessage = requestMessage;
        }

        public override string GetHeader(string headerName)
        {
            return this.requestMessage.GetHeader(headerName);
        }

        public override Stream GetStream()
        {
            return base.GetStream(new Func<Stream>(this.requestMessage.GetStream), true);
        }

        public override Task<Stream> GetStreamAsync()
        {
            IODataRequestMessageAsync requestMessage = this.requestMessage as IODataRequestMessageAsync;
            if (requestMessage == null)
            {
                throw new ODataException(Strings.ODataRequestMessage_AsyncNotAvailable);
            }
            return base.GetStreamAsync(new Func<Task<Stream>>(requestMessage.GetStreamAsync), true);
        }

        public override void SetHeader(string headerName, string headerValue)
        {
            base.VerifyCanSetHeader();
            this.requestMessage.SetHeader(headerName, headerValue);
        }

        public override IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.requestMessage.Headers;
            }
        }

        public string Method
        {
            get
            {
                return this.requestMessage.Method;
            }
            set
            {
                throw new ODataException(Strings.ODataMessage_MustNotModifyMessage);
            }
        }

        public Uri Url
        {
            get
            {
                return this.requestMessage.Url;
            }
            set
            {
                throw new ODataException(Strings.ODataMessage_MustNotModifyMessage);
            }
        }
    }
}

