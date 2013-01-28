namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    internal class HttpContextServiceHost : IDataServiceHost2, IDataServiceHost
    {
        private Uri absoluteServiceUri;
        private bool errorFound;
        private readonly Stream incomingMessageBody;

        internal HttpContextServiceHost(Stream messageBody)
        {
            this.incomingMessageBody = messageBody;
        }

        private static bool MustNotReturnMessageBody(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.NoContent:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.NotModified:
                    return true;
            }
            return false;
        }

        string IDataServiceHost.GetQueryStringItem(string item)
        {
            WebUtil.CheckArgumentNull<string>(item, "item");
            NameValueCollection queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            string[] values = queryParameters.GetValues(item);
            if ((values == null) || (values.Length == 0))
            {
                string str = null;
                foreach (string str2 in queryParameters.Keys)
                {
                    if ((str2 != null) && StringComparer.OrdinalIgnoreCase.Equals(str2.Trim(), item))
                    {
                        if (str != null)
                        {
                            throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_AmbiguousItemName(item, str, str2));
                        }
                        str = str2;
                        values = queryParameters.GetValues(str2);
                    }
                }
                if ((values == null) || (values.Length == 0))
                {
                    return null;
                }
            }
            if (values.Length != 1)
            {
                throw DataServiceException.CreateSyntaxError();
            }
            return values[0];
        }

        void IDataServiceHost.ProcessException(HandleExceptionArgs args)
        {
            WebUtil.CheckArgumentNull<HandleExceptionArgs>(args, "args");
            this.errorFound = true;
            if (!args.ResponseWritten)
            {
                ((IDataServiceHost) this).ResponseStatusCode = args.ResponseStatusCode;
                ((IDataServiceHost) this).ResponseContentType = args.ResponseContentType;
                if (args.ResponseAllowHeader != null)
                {
                    WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.Allow] = args.ResponseAllowHeader;
                }
            }
        }

        internal void VerifyQueryParameters()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
            NameValueCollection queryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            for (int i = 0; i < queryParameters.Count; i++)
            {
                string key = queryParameters.GetKey(i);
                if (key == null)
                {
                    string[] values = queryParameters.GetValues(i);
                    if (values != null)
                    {
                        for (int j = 0; j < values.Length; j++)
                        {
                            string str2 = values[j].Trim();
                            if ((str2.Length > 0) && (str2[0] == '$'))
                            {
                                throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_QueryParameterMustBeSpecifiedOnce(str2));
                            }
                        }
                    }
                }
                else
                {
                    key = key.Trim();
                    if (!set.Add(key))
                    {
                        throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_QueryParameterMustBeSpecifiedOnce(key));
                    }
                    if ((key.Length > 0) && (key[0] == '$'))
                    {
                        if ((((key != "$expand") && (key != "$filter")) && ((key != "$orderby") && (key != "$skip"))) && (((key != "$skiptoken") && (key != "$inlinecount")) && ((key != "$top") && (key != "$select"))))
                        {
                            throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_UnknownQueryParameter(key));
                        }
                        string[] strArray2 = queryParameters.GetValues(i);
                        if ((strArray2 == null) || (strArray2.Length != 1))
                        {
                            throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_QueryParameterMustBeSpecifiedOnce(key));
                        }
                    }
                }
            }
        }

        internal bool ErrorFound
        {
            get
            {
                return this.errorFound;
            }
        }

        private string HostHeader
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host];
            }
        }

        Uri IDataServiceHost.AbsoluteRequestUri {
			get {

				Uri absoluteRequestUri = null;
				object obj2;
				if (OperationContext.Current.IncomingMessageProperties.TryGetValue ("MicrosoftDataServicesRequestUri", out obj2)) {
					absoluteRequestUri = obj2 as Uri;
					if (absoluteRequestUri == null) {
						throw new InvalidOperationException (Strings.HttpContextServiceHost_IncomingMessagePropertyMustBeValidUriInstance ("MicrosoftDataServicesRequestUri"));
					}
				}
				if (absoluteRequestUri == null) {
					UriTemplateMatch uriTemplateMatch = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
					absoluteRequestUri = WebUtil.ApplyHostHeader (uriTemplateMatch.RequestUri, this.HostHeader);
				}
	            
				return absoluteRequestUri;
			}
        }

        Uri IDataServiceHost.AbsoluteServiceUri
        {
            get
            {
                if (this.absoluteServiceUri == null)
                {
                    object obj2;
                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue("MicrosoftDataServicesRootUri", out obj2))
                    {
                        this.absoluteServiceUri = obj2 as Uri;
                        if (this.absoluteServiceUri == null)
                        {
                            throw new InvalidOperationException(Strings.HttpContextServiceHost_IncomingMessagePropertyMustBeValidUriInstance("MicrosoftDataServicesRootUri"));
                        }
                    }
                    if (this.absoluteServiceUri == null)
                    {
                        UriTemplateMatch uriTemplateMatch = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
                        this.absoluteServiceUri = WebUtil.ApplyHostHeader(uriTemplateMatch.BaseUri, this.HostHeader);
                    }
                    if (!string.IsNullOrEmpty(this.absoluteServiceUri.Fragment))
                    {
                        throw new InvalidOperationException(Strings.HttpContextServiceHost_IncomingTemplateMatchFragment(this.absoluteServiceUri));
                    }
                    if (!string.IsNullOrEmpty(this.absoluteServiceUri.Query))
                    {
                        throw new InvalidOperationException(Strings.HttpContextServiceHost_IncomingTemplateMatchQuery(this.absoluteServiceUri));
                    }
                    this.absoluteServiceUri = WebUtil.EnsureLastSegmentEmpty(this.absoluteServiceUri);
                }
                return this.absoluteServiceUri;
            }
        }

        string IDataServiceHost.RequestAccept
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Accept;
            }
        }

        string IDataServiceHost.RequestAcceptCharSet
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.AcceptCharset];
            }
        }

        string IDataServiceHost.RequestContentType
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.ContentType;
            }
        }

        string IDataServiceHost.RequestHttpMethod
        {
            get
            {
                string[] values = WebOperationContext.Current.IncomingRequest.Headers.GetValues("X-HTTP-Method");
                if ((values == null) || (values.Length == 0))
                {
                    return WebOperationContext.Current.IncomingRequest.Method;
                }
                if (values.Length != 1)
                {
                    throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_XMethodIncorrectCount(values.Length));
                }
                string strB = values[0];
                if (string.CompareOrdinal("POST", WebOperationContext.Current.IncomingRequest.Method) != 0)
                {
                    throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_XMethodNotUsingPost);
                }
                if (((string.CompareOrdinal("DELETE", strB) != 0) && (string.CompareOrdinal("PUT", strB) != 0)) && ((string.CompareOrdinal("MERGE", strB) != 0) && (string.CompareOrdinal("PATCH", strB) != 0)))
                {
                    throw DataServiceException.CreateBadRequestError(Strings.HttpContextServiceHost_XMethodIncorrectValue(strB));
                }
                return strB;
            }
        }

        string IDataServiceHost.RequestIfMatch
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.IfMatch];
            }
        }

        string IDataServiceHost.RequestIfNoneMatch
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.IfNoneMatch];
            }
        }

        string IDataServiceHost.RequestMaxVersion
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers["MaxDataServiceVersion"];
            }
        }

        Stream IDataServiceHost.RequestStream
        {
            [DebuggerStepThrough]
            get
            {
                return this.incomingMessageBody;
            }
        }

        string IDataServiceHost.RequestVersion
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers["DataServiceVersion"];
            }
        }

        string IDataServiceHost.ResponseCacheControl
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.CacheControl];
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.CacheControl] = value;
            }
        }

        string IDataServiceHost.ResponseContentType
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }

        string IDataServiceHost.ResponseETag
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.ETag;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ETag = value;
            }
        }

        string IDataServiceHost.ResponseLocation
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.Location];
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.Location] = value;
            }
        }

        int IDataServiceHost.ResponseStatusCode
        {
            get
            {
                return (int) WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                HttpStatusCode statusCode = (HttpStatusCode) value;
                WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode;
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = MustNotReturnMessageBody(statusCode);
            }
        }

        Stream IDataServiceHost.ResponseStream
        {
            get
            {
                throw Error.NotSupported();
            }
        }

        string IDataServiceHost.ResponseVersion
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.Headers["DataServiceVersion"];
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.Headers["DataServiceVersion"] = value;
            }
        }

        WebHeaderCollection IDataServiceHost2.RequestHeaders
        {
            [DebuggerStepThrough]
            get
            {
                return WebOperationContext.Current.IncomingRequest.Headers;
            }
        }

        WebHeaderCollection IDataServiceHost2.ResponseHeaders
        {
            [DebuggerStepThrough]
            get
            {
                return WebOperationContext.Current.OutgoingResponse.Headers;
            }
        }
    }
}

