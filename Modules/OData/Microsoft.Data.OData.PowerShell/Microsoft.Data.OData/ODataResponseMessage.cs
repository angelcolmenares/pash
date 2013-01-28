namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    internal sealed class ODataResponseMessage : ODataMessage, IODataResponseMessageAsync, IODataResponseMessage
    {
        private readonly IODataResponseMessage responseMessage;

        internal ODataResponseMessage(IODataResponseMessage responseMessage, bool writing, bool disableMessageStreamDisposal, long maxMessageSize) : base(writing, disableMessageStreamDisposal, maxMessageSize)
        {
            this.responseMessage = responseMessage;
        }

        public override string GetHeader(string headerName)
        {
            return this.responseMessage.GetHeader(headerName);
        }

        public override Stream GetStream()
        {
            return base.GetStream(new Func<Stream>(this.responseMessage.GetStream), false);
        }

        public override Task<Stream> GetStreamAsync()
        {
            IODataResponseMessageAsync responseMessage = this.responseMessage as IODataResponseMessageAsync;
            if (responseMessage == null)
            {
                throw new ODataException(Strings.ODataResponseMessage_AsyncNotAvailable);
            }
            return base.GetStreamAsync(new Func<Task<Stream>>(responseMessage.GetStreamAsync), false);
        }

        public override void SetHeader(string headerName, string headerValue)
        {
            base.VerifyCanSetHeader();
            this.responseMessage.SetHeader(headerName, headerValue);
        }

        public override IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.responseMessage.Headers;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.responseMessage.StatusCode;
            }
            set
            {
                throw new ODataException(Strings.ODataMessage_MustNotModifyMessage);
            }
        }
    }
}

