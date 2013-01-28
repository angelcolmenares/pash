namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Web;

    internal class BatchServiceHost : IDataServiceHost2, IDataServiceHost, IDisposable
    {
        private readonly Uri absoluteRequestUri;
        private readonly Uri absoluteServiceUri;
        private readonly string contentId;
        private ODataBatchOperationResponseMessage operationMessage;
        private NameValueCollection queryParameters;
        private readonly WebHeaderCollection requestHeaders;
        private readonly string requestHttpMethod;
        private readonly Stream requestStream;
        private readonly WebHeaderCollection responseHeaders;
        private int responseStatusCode;
        private readonly ODataBatchWriter writer;

        internal BatchServiceHost(ODataBatchWriter writer)
        {
            this.writer = writer;
            this.requestHeaders = new WebHeaderCollection();
            this.responseHeaders = new WebHeaderCollection();
        }

        internal BatchServiceHost(Uri absoluteServiceUri, IODataRequestMessage operationMessage, string contentId, ODataBatchWriter writer, Version maxDataServiceVersion, Version minDataServiceVersion, Version dataServiceVersion) : this(writer)
        {
            this.absoluteServiceUri = absoluteServiceUri;
			this.absoluteRequestUri = RequestUriProcessor.GetAbsoluteUriFromReference(operationMessage.Url, absoluteServiceUri, dataServiceVersion);
            this.requestHttpMethod = operationMessage.Method;
            this.contentId = contentId;
            foreach (KeyValuePair<string, string> pair in operationMessage.Headers)
            {
                this.requestHeaders.Add(pair.Key, pair.Value);
            }
            if (string.IsNullOrEmpty(this.requestHeaders["MaxDataServiceVersion"]))
            {
                this.requestHeaders["MaxDataServiceVersion"] = maxDataServiceVersion.ToString();
            }
            if (string.IsNullOrEmpty(this.requestHeaders["MinDataServiceVersion"]))
            {
                this.requestHeaders["MinDataServiceVersion"] = minDataServiceVersion.ToString();
            }
            this.requestStream = operationMessage.GetStream();
        }

        public void Dispose()
        {
            if (this.requestStream != null)
            {
                this.requestStream.Dispose();
            }
        }

        internal ODataBatchOperationResponseMessage GetOperationResponseMessage()
        {
            return (this.operationMessage ?? (this.operationMessage = this.writer.CreateOperationResponseMessage()));
        }

        private void GetUriAndQueryParameters()
        {
            if (this.queryParameters == null)
            {
                this.queryParameters = HttpUtility.ParseQueryString(this.absoluteRequestUri.Query);
            }
        }

        string IDataServiceHost.GetQueryStringItem(string item)
        {
            this.GetUriAndQueryParameters();
            string[] values = this.queryParameters.GetValues(item);
            if ((values == null) || (values.Length == 0))
            {
                return null;
            }
            if (values.Length != 1)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceHost_MoreThanOneQueryParameterSpecifiedWithTheGivenName(item, this.absoluteRequestUri));
            }
            return values[0];
        }

        void IDataServiceHost.ProcessException(HandleExceptionArgs args)
        {
            WebUtil.CheckArgumentNull<HandleExceptionArgs>(args, "args");
            this.responseStatusCode = args.ResponseStatusCode;
            this.responseHeaders[System.Net.HttpResponseHeader.ContentType] = args.ResponseContentType;
            this.responseHeaders[System.Net.HttpResponseHeader.Allow] = args.ResponseAllowHeader;
        }

        internal string ContentId
        {
            get
            {
                return this.contentId;
            }
        }

        Uri IDataServiceHost.AbsoluteRequestUri
        {
            [DebuggerStepThrough]
            get
            {
                return this.absoluteRequestUri;
            }
        }

        Uri IDataServiceHost.AbsoluteServiceUri
        {
            [DebuggerStepThrough]
            get
            {
                return this.absoluteServiceUri;
            }
        }

        string IDataServiceHost.RequestAccept
        {
            get
            {
                return this.requestHeaders[HttpRequestHeader.Accept];
            }
        }

        string IDataServiceHost.RequestAcceptCharSet
        {
            get
            {
                return this.requestHeaders[HttpRequestHeader.AcceptCharset];
            }
        }

        string IDataServiceHost.RequestContentType
        {
            get
            {
                return this.requestHeaders[HttpRequestHeader.ContentType];
            }
        }

        string IDataServiceHost.RequestHttpMethod
        {
            [DebuggerStepThrough]
            get
            {
                return this.requestHttpMethod;
            }
        }

        string IDataServiceHost.RequestIfMatch
        {
            get
            {
                return this.requestHeaders[HttpRequestHeader.IfMatch];
            }
        }

        string IDataServiceHost.RequestIfNoneMatch
        {
            get
            {
                return this.requestHeaders[HttpRequestHeader.IfNoneMatch];
            }
        }

        string IDataServiceHost.RequestMaxVersion
        {
            get
            {
                return this.requestHeaders["MaxDataServiceVersion"];
            }
        }

        Stream IDataServiceHost.RequestStream
        {
            [DebuggerStepThrough]
            get
            {
                return this.requestStream;
            }
        }

        string IDataServiceHost.RequestVersion
        {
            get
            {
                return this.requestHeaders["DataServiceVersion"];
            }
        }

        string IDataServiceHost.ResponseCacheControl
        {
            get
            {
                return this.responseHeaders[System.Net.HttpResponseHeader.CacheControl];
            }
            set
            {
                this.responseHeaders[System.Net.HttpResponseHeader.CacheControl] = value;
            }
        }

        string IDataServiceHost.ResponseContentType
        {
            get
            {
                return this.responseHeaders[System.Net.HttpResponseHeader.ContentType];
            }
            set
            {
                this.responseHeaders[System.Net.HttpResponseHeader.ContentType] = value;
            }
        }

        string IDataServiceHost.ResponseETag
        {
            get
            {
                return this.responseHeaders[System.Net.HttpResponseHeader.ETag];
            }
            set
            {
                this.responseHeaders[System.Net.HttpResponseHeader.ETag] = value;
            }
        }

        string IDataServiceHost.ResponseLocation
        {
            get
            {
                return this.responseHeaders[System.Net.HttpResponseHeader.Location];
            }
            set
            {
                this.responseHeaders[System.Net.HttpResponseHeader.Location] = value;
            }
        }

        int IDataServiceHost.ResponseStatusCode
        {
            get
            {
                return this.responseStatusCode;
            }
            set
            {
                this.responseStatusCode = value;
            }
        }

        Stream IDataServiceHost.ResponseStream
        {
            get
            {
                throw System.Data.Services.Error.NotSupported();
            }
        }

        string IDataServiceHost.ResponseVersion
        {
            get
            {
                return this.responseHeaders["DataServiceVersion"];
            }
            set
            {
                this.responseHeaders["DataServiceVersion"] = value;
            }
        }

        WebHeaderCollection IDataServiceHost2.RequestHeaders
        {
            get
            {
                return this.requestHeaders;
            }
        }

        WebHeaderCollection IDataServiceHost2.ResponseHeaders
        {
            get
            {
                return this.responseHeaders;
            }
        }

        internal ODataBatchWriter Writer
        {
            get
            {
                return this.writer;
            }
        }
    }
}

