namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    internal class HttpWebResponseMessage : IODataResponseMessage
    {
        private readonly Func<Stream> getResponseStream;
        private readonly Dictionary<string, string> headers;
        private readonly int statusCode;

        internal HttpWebResponseMessage(HttpWebResponse httpResponse, Func<Stream> getResponseStream) : this(WebUtil.WrapResponseHeaders(httpResponse), (int) httpResponse.StatusCode, getResponseStream)
        {
        }

        internal HttpWebResponseMessage(Dictionary<string, string> headers, int statusCode, Func<Stream> getResponseStream)
        {
            this.headers = headers;
            this.statusCode = statusCode;
            this.getResponseStream = getResponseStream;
        }

        internal static void CheckAndFailForJsonContentType(string headerName, string headerValue)
        {
            if ((string.Equals(headerName, "Content-Type", StringComparison.Ordinal) && (headerValue != null)) && (headerValue.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) != -1))
            {
                throw new NotSupportedException("'application/json' is currently not supported in Content-Type header");
            }
        }

        public string GetHeader(string headerName)
        {
            string str;
            Util.CheckArgumentNullAndEmpty(headerName, "headerName");
            if (this.headers.TryGetValue(headerName, out str))
            {
                CheckAndFailForJsonContentType(headerName, str);
                return str;
            }
            return null;
        }

        public Stream GetStream()
        {
            return this.getResponseStream();
        }

        public void SetHeader(string headerName, string headerValue)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return this.headers;
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
                throw new NotImplementedException();
            }
        }
    }
}

