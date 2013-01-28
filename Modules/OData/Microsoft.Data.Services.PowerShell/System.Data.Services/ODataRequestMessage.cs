namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class ODataRequestMessage : IODataRequestMessage, IODataUrlResolver
    {
        private readonly DataServiceHostWrapper host;

        internal ODataRequestMessage(DataServiceHostWrapper host)
        {
            this.host = host;
            this.ContentType = this.host.RequestContentType;
        }

        public string GetHeader(string headerName)
        {
            switch (headerName)
            {
                case "Content-Type":
                    return this.ContentType;

                case "DataServiceVersion":
                    return this.host.RequestVersionString;
            }
            throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
        }

        public Stream GetStream()
        {
            return this.host.RequestStream;
        }

        public Uri ResolveUrl(Uri baseUri, Uri payloadUri)
        {
            return payloadUri;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
        }

        internal string ContentType { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
        }

        public string Method
        {
            get
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
            set
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
        }

        public Uri Url
        {
            get
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
            set
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceException_GeneralError);
            }
        }
    }
}

