namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class ODataResponseMessage : IODataResponseMessage
    {
        private string contentType;
        private readonly DataServiceHostWrapper host;
        private Stream responseStream;

        internal ODataResponseMessage(DataServiceHostWrapper host)
        {
            this.host = host;
        }

        public string GetHeader(string headerName)
        {
            return this.contentType;
        }

        public Stream GetStream()
        {
            return this.responseStream;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            switch (headerName)
            {
                case "Content-Type":
                    this.host.ResponseContentType = headerValue;
                    this.contentType = headerValue;
                    return;

                case "DataServiceVersion":
                    this.host.ResponseVersion = headerValue;
                    return;
            }
            throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
        }

        internal void SetStream(Stream stream)
        {
            this.responseStream = stream;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
        }

        public int StatusCode
        {
            get
            {
                return this.host.ResponseStatusCode;
            }
            set
            {
                this.host.ResponseStatusCode = value;
            }
        }
    }
}

