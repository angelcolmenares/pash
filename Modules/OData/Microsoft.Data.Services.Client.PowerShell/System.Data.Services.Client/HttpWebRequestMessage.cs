namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;

    internal class HttpWebRequestMessage : IODataRequestMessage
    {
        private MemoryStream cachedRequestStream;
        private bool fireSendingRequestMethodCalled;
        private List<string> headersToReset;
        private readonly string httpMethod;
        private System.Net.HttpWebRequest httpRequest;
        private bool inSendingRequest2Event;
        private string originalContentTypeHeaderValue;
        private readonly RequestInfo requestInfo;
        private readonly Uri requestUrl;
        private bool sendingRequest2Fired;

        internal HttpWebRequestMessage(Uri requestUri, string httpMethod, RequestInfo requestInfo)
        {
            this.requestUrl = requestUri;
            this.httpMethod = httpMethod;
            this.requestInfo = requestInfo;
            this.httpRequest = CreateRequest(httpMethod, requestUri, requestInfo);
        }

        internal void Abort()
        {
            this.httpRequest.Abort();
        }

        internal void AddHeadersToReset(IEnumerable<string> headerNames)
        {
            if (this.headersToReset == null)
            {
                this.headersToReset = new List<string>();
            }
            this.headersToReset.AddRange(headerNames);
        }

        internal void AddHeadersToReset(string headerName)
        {
            this.AddHeadersToReset(new string[] { headerName });
        }

        internal void AfterSendingRequest2Event()
        {
            this.inSendingRequest2Event = false;
            this.sendingRequest2Fired = true;
        }

        internal void BeforeSendingRequest2Event()
        {
            this.inSendingRequest2Event = true;
            this.originalContentTypeHeaderValue = this.GetHeader("Content-Type");
        }

        internal IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            this.FireSendingRequest();
            return this.httpRequest.BeginGetRequestStream(callback, state);
        }

        internal IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            this.FireSendingRequest();
            return this.httpRequest.BeginGetResponse(callback, state);
        }

        private static System.Net.HttpWebRequest CreateRequest(string method, Uri requestUrl, RequestInfo requestInfo)
        {
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest) WebRequest.Create(requestUrl);
            if (requestInfo.Credentials != null)
            {
                request.Credentials = requestInfo.Credentials;
            }
            if (requestInfo.Timeout != 0)
            {
                TimeSpan span = new TimeSpan(0, 0, requestInfo.Timeout);
                request.Timeout = (int) Math.Min(2147483647.0, span.TotalMilliseconds);
            }
            request.KeepAlive = true;
            request.UserAgent = "Microsoft ADO.NET Data Services";
            if (((string.CompareOrdinal("GET", method) != 0) && requestInfo.UsePostTunneling) && (string.CompareOrdinal("POST", method) != 0))
            {
                request.Headers["X-HTTP-Method"] = method;
                method = "POST";
            }
            request.Method = method;
            request.Headers[HttpRequestHeader.AcceptCharset] = "UTF-8";
            return request;
        }

        internal Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return this.httpRequest.EndGetRequestStream(asyncResult);
        }

        internal HttpWebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return (HttpWebResponse) this.httpRequest.EndGetResponse(asyncResult);
        }

        private void FireSendingRequest()
        {
            if (!this.fireSendingRequestMethodCalled)
            {
                this.fireSendingRequestMethodCalled = true;
                Dictionary<string, string> cachedHeaders = null;
                if (this.requestInfo.HasSendingRequestEventHandlers)
                {
                    cachedHeaders = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (KeyValuePair<string, string> pair in this.Headers)
                    {
                        cachedHeaders.Add(pair.Key, pair.Value);
                    }
                    cachedHeaders.Add("Content-Length", this.httpRequest.ContentLength.ToString(CultureInfo.InvariantCulture));
                }
                if ((string.CompareOrdinal("GET", this.Method) != 0) && (string.CompareOrdinal("DELETE", this.Method) == 0))
                {
                    this.httpRequest.ContentType = null;
                    this.httpRequest.ContentLength = 0L;
                }
                if (this.requestInfo.HasSendingRequestEventHandlers)
                {
                    WebHeaderCollection requestHeaders = this.httpRequest.Headers;
                    SendingRequestEventArgs eventArgs = new SendingRequestEventArgs(this.httpRequest, requestHeaders);
                    this.requestInfo.FireSendingRequest(eventArgs);
                    if (!object.ReferenceEquals(eventArgs.Request, this.httpRequest))
                    {
                        this.httpRequest = (System.Net.HttpWebRequest) eventArgs.Request;
                    }
                    SetHeaderValues(this, cachedHeaders);
                }
                this.requestInfo.InternalSendRequest(this.httpRequest);
            }
        }

        public string GetHeader(string headerName)
        {
            Util.CheckArgumentNullAndEmpty(headerName, "headerName");
            string headerValue = GetHeaderValue(this.httpRequest, headerName);
            if (((this.sendingRequest2Fired && string.Equals(headerName, "Content-Type", StringComparison.Ordinal)) && (!string.Equals(headerValue, this.originalContentTypeHeaderValue, StringComparison.OrdinalIgnoreCase) && (headerValue != null))) && (headerValue.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) != -1))
            {
                throw new NotSupportedException("'application/json' is currently not supported in Content-Type header");
            }
            return headerValue;
        }

        private static string GetHeaderValue(System.Net.HttpWebRequest request, string headerName)
        {
            if (string.Equals(headerName, "Accept", StringComparison.Ordinal))
            {
                return request.Accept;
            }
            if (string.Equals(headerName, "Content-Type", StringComparison.Ordinal))
            {
                return request.ContentType;
            }
            if (string.Equals(headerName, "Content-Length", StringComparison.Ordinal))
            {
                return request.ContentLength.ToString(CultureInfo.InvariantCulture);
            }
            return request.Headers[headerName];
        }

        internal Stream GetRequestStream()
        {
            this.FireSendingRequest();
            return this.httpRequest.GetRequestStream();
        }

        internal HttpWebResponse GetResponse()
        {
            this.FireSendingRequest();
            return (HttpWebResponse) this.httpRequest.GetResponse();
        }

        public Stream GetStream()
        {
            if (this.inSendingRequest2Event)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ODataRequestMessage_GetStreamMethodNotSupported);
            }
            this.cachedRequestStream = new MemoryStream();
            return this.cachedRequestStream;
        }

        internal void SetContentLengthHeader()
        {
            this.SetHeader("Content-Length", (this.CachedRequestStream.Length - this.CachedRequestStream.Position).ToString(CultureInfo.InvariantCulture));
            if (this.headersToReset != null)
            {
                this.headersToReset.Add("Content-Length");
            }
            else
            {
                this.headersToReset = new List<string>(new string[] { "Content-Length" });
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            Util.CheckArgumentNullAndEmpty(headerName, "headerName");
            SetHeaderValue(this.httpRequest, headerName, headerValue);
        }

        private static void SetHeaderValue(System.Net.HttpWebRequest request, string headerName, string headerValue)
        {
            if (string.Equals(headerName, "Accept", StringComparison.Ordinal))
            {
                request.Accept = headerValue;
            }
            else if (string.Equals(headerName, "Content-Type", StringComparison.Ordinal))
            {
                request.ContentType = headerValue;
            }
            else if (string.Equals(headerName, "Content-Length", StringComparison.Ordinal))
            {
                request.ContentLength = long.Parse(headerValue, CultureInfo.InvariantCulture);
            }
            else
            {
                request.Headers[headerName] = headerValue;
            }
        }

        private static void SetHeaderValues(HttpWebRequestMessage requestMessage, Dictionary<string, string> cachedHeaders)
        {
            bool flag = true;
            System.Net.HttpWebRequest httpRequest = requestMessage.httpRequest;
            string method = requestMessage.Method;
            string str2 = null;
            cachedHeaders.TryGetValue("Content-Type", out str2);
            if (string.CompareOrdinal(method, "GET") != 0)
            {
                if (string.CompareOrdinal(method, "DELETE") == 0)
                {
                    httpRequest.ContentType = null;
                    httpRequest.ContentLength = 0L;
                }
                else
                {
                    httpRequest.ContentType = str2;
                }
                if (requestMessage.requestInfo.UsePostTunneling && (string.CompareOrdinal(method, "POST") != 0))
                {
                    httpRequest.Headers["X-HTTP-Method"] = method;
                    method = "POST";
                    flag = false;
                }
            }
            ICollection<string> allKeys = httpRequest.Headers.AllKeys;
            if (allKeys.Contains("If-Match"))
            {
                httpRequest.Headers.Remove(HttpRequestHeader.IfMatch);
            }
            if (flag && allKeys.Contains("X-HTTP-Method"))
            {
                httpRequest.Headers.Remove("X-HTTP-Method");
            }
            httpRequest.Method = method;
            if (requestMessage.HeadersToReset != null)
            {
                foreach (string str3 in requestMessage.HeadersToReset)
                {
                    SetHeaderValue(httpRequest, str3, cachedHeaders[str3]);
                }
            }
        }

        internal MemoryStream CachedRequestStream
        {
            get
            {
                this.cachedRequestStream.Position = 0L;
                return this.cachedRequestStream;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(this.httpRequest.Headers.Count);
                foreach (string str in this.httpRequest.Headers.AllKeys)
                {
                    string str2 = this.httpRequest.Headers[str];
                    list.Add(new KeyValuePair<string, string>(str, str2));
                }
                return list;
            }
        }

        internal IEnumerable<string> HeadersToReset
        {
            get
            {
                return (this.headersToReset ?? ((IEnumerable<string>) new string[0]));
            }
        }

        public System.Net.HttpWebRequest HttpWebRequest
        {
            get
            {
                return this.httpRequest;
            }
        }

        public string Method
        {
            get
            {
                return this.httpMethod;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public Uri Url
        {
            get
            {
                return this.requestUrl;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

