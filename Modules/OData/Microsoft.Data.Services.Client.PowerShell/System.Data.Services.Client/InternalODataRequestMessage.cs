namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class InternalODataRequestMessage : IODataRequestMessage
    {
        private readonly IODataRequestMessage requestMessage;

        internal InternalODataRequestMessage(IODataRequestMessage requestMessage)
        {
            this.requestMessage = requestMessage;
        }

        public string GetHeader(string headerName)
        {
            return this.requestMessage.GetHeader(headerName);
        }

        public Stream GetStream()
        {
            throw new NotImplementedException();
        }

        public void SetHeader(string headerName, string headerValue)
        {
            HttpWebResponseMessage.CheckAndFailForJsonContentType(headerName, headerValue);
            this.requestMessage.SetHeader(headerName, headerValue);
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }
    }
}

