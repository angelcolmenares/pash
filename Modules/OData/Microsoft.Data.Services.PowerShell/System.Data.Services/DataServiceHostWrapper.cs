namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class DataServiceHostWrapper
    {
        private Uri absoluteRequestUri;
        private Uri absoluteServiceUri;
        private readonly IDataServiceHost host;
        private HttpVerbs httpVerb;
        private readonly string requestAccept;
        private readonly string requestAcceptCharSet;
        private readonly string requestContentType;
        private WebHeaderCollection requestHeaders;
        private string requestHttpMethod;
        private readonly string requestIfMatch;
        private readonly string requestIfNoneMatch;
        private Version requestMaxVersion;
        private Version requestMinVersion;
        private Stream requestStream;
        private Version requestVersion;
        private string requestVersionString;
        private WebHeaderCollection responseHeaders;
        private Stream responseStream;

        internal DataServiceHostWrapper(IDataServiceHost host)
        {
            this.host = host;
            this.httpVerb = HttpVerbs.None;
            this.requestAccept = host.RequestAccept;
            this.requestAcceptCharSet = host.RequestAcceptCharSet;
            this.requestContentType = host.RequestContentType;
            this.requestIfMatch = host.RequestIfMatch;
            this.requestIfNoneMatch = host.RequestIfNoneMatch;
        }

        private static Version GetMaxRequestVersionAllowed(Version maxProtocolVersion)
        {
            if (!(maxProtocolVersion == RequestDescription.Version1Dot0))
            {
                return maxProtocolVersion;
            }
            return RequestDescription.Version2Dot0;
        }

        private Version GetMinDataServiceVersion(Version maxProtocolVersion)
        {
            if (this.requestMinVersion == null)
            {
                this.TryGetMinDataServiceVersionFromWrappedHost(out this.requestMinVersion);
                if ((maxProtocolVersion < RequestDescription.Version3Dot0) || (this.requestMinVersion == null))
                {
                    this.requestMinVersion = RequestDescription.DataServiceDefaultResponseVersion;
                }
                else
                {
                    if (this.requestMinVersion > maxProtocolVersion)
                    {
                        throw DataServiceException.CreateBadRequestError(Strings.DataService_MinDSVGreaterThanMPV(this.requestMinVersion.ToString(2), maxProtocolVersion.ToString(2)));
                    }
                    if (!RequestDescription.IsKnownRequestVersion(this.requestMinVersion))
                    {
                        throw DataServiceException.CreateBadRequestError(Strings.DataService_InvalidMinDSV(this.requestMinVersion.ToString(2), KnownDataServiceVersionsToString(GetMaxRequestVersionAllowed(maxProtocolVersion))));
                    }
                }
            }
            return this.requestMinVersion;
        }

        internal string GetQueryStringItem(string item)
        {
            return this.host.GetQueryStringItem(item);
        }

        internal void InitializeRequestVersionHeaders(Version maxProtocolVersion)
        {
            Version maxRequestVersionAllowed = GetMaxRequestVersionAllowed(maxProtocolVersion);
            this.requestVersionString = this.host.RequestVersion;
            this.requestMinVersion = this.GetMinDataServiceVersion(maxRequestVersionAllowed);
            this.requestVersion = ValidateVersionHeader("DataServiceVersion", this.requestVersionString);
            this.requestMaxVersion = ValidateVersionHeader("MaxDataServiceVersion", this.host.RequestMaxVersion);
            if (this.requestVersion == null)
            {
                if (this.requestMaxVersion != null)
                {
                    if ((this.requestMaxVersion >= RequestDescription.Version2Dot0) && (maxProtocolVersion >= RequestDescription.Version2Dot0))
                    {
                        this.requestVersion = (this.requestMaxVersion < maxProtocolVersion) ? this.requestMaxVersion : maxProtocolVersion;
                    }
                    else
                    {
                        this.requestVersion = (maxProtocolVersion < RequestDescription.Version2Dot0) ? maxProtocolVersion : RequestDescription.Version2Dot0;
                    }
                }
                else
                {
                    this.requestVersion = maxProtocolVersion;
                }
                this.requestVersionString = this.requestVersion.ToString(2);
            }
            else
            {
                if (this.requestVersion > maxRequestVersionAllowed)
                {
                    throw DataServiceException.CreateBadRequestError(Strings.DataService_RequestVersionMustBeLessThanMPV(this.requestVersion, maxProtocolVersion));
                }
                if (!RequestDescription.IsKnownRequestVersion(this.requestVersion))
                {
                    throw DataServiceException.CreateBadRequestError(Strings.DataService_InvalidRequestVersion(this.requestVersion.ToString(2), KnownDataServiceVersionsToString(maxRequestVersionAllowed)));
                }
            }
            if (this.requestMaxVersion == null)
            {
                this.requestMaxVersion = maxProtocolVersion;
            }
            else if (this.requestMaxVersion < RequestDescription.DataServiceDefaultResponseVersion)
            {
                throw DataServiceException.CreateBadRequestError(Strings.DataService_MaxDSVTooLow(this.requestMaxVersion.ToString(2), RequestDescription.DataServiceDefaultResponseVersion.Major, RequestDescription.DataServiceDefaultResponseVersion.Minor));
            }
            if (this.requestMaxVersion < this.requestMinVersion)
            {
                throw DataServiceException.CreateBadRequestError(Strings.DataService_MaxDSVLowerThanMinDSV(this.requestMaxVersion.ToString(2), this.requestMinVersion.ToString(2)));
            }
        }

        private static string KnownDataServiceVersionsToString(Version maxRequestVersionAllowed)
        {
            StringBuilder builder = new StringBuilder();
            string str = string.Empty;
            for (int i = 0; i < RequestDescription.KnownDataServiceVersions.Length; i++)
            {
                Version version = RequestDescription.KnownDataServiceVersions[i];
                if (version > maxRequestVersionAllowed)
                {
                    break;
                }
                builder.Append(str);
                builder.Append(string.Format(CultureInfo.InvariantCulture, "'{0}'", new object[] { version.ToString() }));
                str = ", ";
            }
            return builder.ToString();
        }

        internal void ProcessException(HandleExceptionArgs args)
        {
            this.host.ProcessException(args);
        }

        internal bool TryGetMinDataServiceVersionFromWrappedHost(out Version minDataServiceVersion)
        {
            minDataServiceVersion = null;
            IDataServiceHost2 host = this.host as IDataServiceHost2;
            if (host != null)
            {
                if (host.RequestHeaders == null)
                {
                    throw new InvalidOperationException(Strings.DataServiceHost_RequestHeadersCannotBeNull);
                }
                minDataServiceVersion = ValidateVersionHeader("MinDataServiceVersion", host.RequestHeaders["MinDataServiceVersion"]);
            }
            return (minDataServiceVersion != null);
        }

        private static T ValidateAndCast<T>(object instance) where T: class
        {
            T local = instance as T;
            if (local == null)
            {
                throw new InvalidOperationException(Strings.DataServiceHost_FeatureRequiresIDataServiceHost2);
            }
            return local;
        }

        private static Version ValidateVersionHeader(string headerName, string headerValue)
        {
            KeyValuePair<Version, string> pair;
            if (string.IsNullOrEmpty(headerValue))
            {
                return null;
            }
            if (!HttpProcessUtility.TryReadVersion(headerValue, out pair))
            {
                throw DataServiceException.CreateBadRequestError(Strings.DataService_VersionCannotBeParsed(headerValue, headerName));
            }
            return pair.Key;
        }

        internal void VerifyQueryParameters()
        {
            if (this.host is System.Data.Services.HttpContextServiceHost)
            {
                ((System.Data.Services.HttpContextServiceHost) this.host).VerifyQueryParameters();
            }
        }

        internal Uri AbsoluteRequestUri
        {
            get
            {
                if (this.absoluteRequestUri == null)
                {
                    this.absoluteRequestUri = this.host.AbsoluteRequestUri;
                    if (this.absoluteRequestUri == null)
                    {
						throw new InvalidOperationException(Strings.RequestUriProcessor_AbsoluteRequestUriCannotBeNull);
                    }
                    if (!this.absoluteRequestUri.IsAbsoluteUri)
                    {
						throw new InvalidOperationException(Strings.RequestUriProcessor_AbsoluteRequestUriMustBeAbsolute);
                    }
                }
                return this.absoluteRequestUri;
            }
        }

        internal Uri AbsoluteServiceUri
        {
            get
            {
                if (this.absoluteServiceUri == null)
                {
                    this.absoluteServiceUri = this.host.AbsoluteServiceUri;
                    if (this.absoluteServiceUri == null)
                    {
						throw new InvalidOperationException(Strings.RequestUriProcessor_AbsoluteServiceUriCannotBeNull);
                    }
                    if (!this.absoluteServiceUri.IsAbsoluteUri)
                    {
						throw new InvalidOperationException(Strings.RequestUriProcessor_AbsoluteServiceUriMustBeAbsolute);
                    }
                    this.absoluteServiceUri = WebUtil.EnsureLastSegmentEmpty(this.absoluteServiceUri);
                }
                return this.absoluteServiceUri;
            }
        }

        internal System.Data.Services.BatchServiceHost BatchServiceHost
        {
            get
            {
                return (this.host as System.Data.Services.BatchServiceHost);
            }
        }

        internal System.Data.Services.HttpContextServiceHost HttpContextServiceHost
        {
            get
            {
                return (this.host as System.Data.Services.HttpContextServiceHost);
            }
        }

        internal HttpVerbs HttpVerb
        {
            get
            {
                if (this.httpVerb == HttpVerbs.None)
                {
                    switch (this.RequestHttpMethod)
                    {
                        case "GET":
                            this.httpVerb = HttpVerbs.GET;
                            goto Label_00EA;

                        case "POST":
                            this.httpVerb = HttpVerbs.POST;
                            goto Label_00EA;

                        case "PUT":
                            this.httpVerb = HttpVerbs.PUT;
                            goto Label_00EA;

                        case "MERGE":
                            this.httpVerb = HttpVerbs.MERGE;
                            goto Label_00EA;

                        case "PATCH":
                            this.httpVerb = HttpVerbs.PATCH;
                            goto Label_00EA;

                        case "DELETE":
                            this.httpVerb = HttpVerbs.DELETE;
                            goto Label_00EA;
                    }
                    throw DataServiceException.CreateMethodNotImplemented(Strings.DataService_NotImplementedException);
                }
            Label_00EA:
                return this.httpVerb;
            }
        }

        internal string RequestAccept
        {
            get
            {
                return this.requestAccept;
            }
        }

        internal string RequestAcceptCharSet
        {
            get
            {
                return this.requestAcceptCharSet;
            }
        }

        internal string RequestContentType
        {
            get
            {
                return this.requestContentType;
            }
        }

        internal WebHeaderCollection RequestHeaders
        {
            get
            {
                if (this.requestHeaders == null)
                {
                    IDataServiceHost2 host = ValidateAndCast<IDataServiceHost2>(this.host);
                    this.requestHeaders = host.RequestHeaders;
                    if (this.requestHeaders == null)
                    {
                        throw new InvalidOperationException(Strings.DataServiceHost_RequestHeadersCannotBeNull);
                    }
                }
                return this.requestHeaders;
            }
        }

        internal string RequestHttpMethod
        {
            get
            {
                if (string.IsNullOrEmpty(this.requestHttpMethod))
                {
                    this.requestHttpMethod = this.host.RequestHttpMethod;
                    if (string.IsNullOrEmpty(this.requestHttpMethod))
                    {
                        throw new InvalidOperationException(Strings.DataServiceHost_EmptyHttpMethod);
                    }
                }
                return this.requestHttpMethod;
            }
        }

        internal string RequestIfMatch
        {
            get
            {
                return this.requestIfMatch;
            }
        }

        internal string RequestIfNoneMatch
        {
            get
            {
                return this.requestIfNoneMatch;
            }
        }

        internal Version RequestMaxVersion
        {
            get
            {
                return this.requestMaxVersion;
            }
        }

        internal Version RequestMinVersion
        {
            get
            {
                return this.requestMinVersion;
            }
        }

        internal Stream RequestStream
        {
            get
            {
                if (this.requestStream == null)
                {
                    this.requestStream = this.host.RequestStream;
                    if (this.requestStream == null)
                    {
                        throw DataServiceException.CreateBadRequestError(Strings.BadRequest_NullRequestStream);
                    }
                }
                return this.requestStream;
            }
        }

        internal Version RequestVersion
        {
            get
            {
                return this.requestVersion;
            }
        }

        internal string RequestVersionString
        {
            get
            {
                return this.requestVersionString;
            }
        }

        internal string ResponseCacheControl
        {
            set
            {
                this.host.ResponseCacheControl = value;
            }
        }

        internal string ResponseContentType
        {
            get
            {
                return this.host.ResponseContentType;
            }
            set
            {
                this.host.ResponseContentType = value;
            }
        }

        internal string ResponseETag
        {
            get
            {
                return this.host.ResponseETag;
            }
            set
            {
                this.host.ResponseETag = value;
            }
        }

        internal WebHeaderCollection ResponseHeaders
        {
            get
            {
                if (this.responseHeaders == null)
                {
                    IDataServiceHost2 host = ValidateAndCast<IDataServiceHost2>(this.host);
                    this.responseHeaders = host.ResponseHeaders;
                    if (this.responseHeaders == null)
                    {
                        throw new InvalidOperationException(Strings.DataServiceHost_ResponseHeadersCannotBeNull);
                    }
                }
                return this.responseHeaders;
            }
        }

        internal string ResponseLocation
        {
            set
            {
                this.host.ResponseLocation = value;
            }
        }

        internal int ResponseStatusCode
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

        internal Stream ResponseStream
        {
            get
            {
                if (this.responseStream == null)
                {
                    this.responseStream = this.host.ResponseStream;
                    if (this.responseStream == null)
                    {
                        throw DataServiceException.CreateBadRequestError(Strings.BadRequest_NullResponseStream);
                    }
                }
                return this.responseStream;
            }
        }

        internal string ResponseVersion
        {
            set
            {
                this.host.ResponseVersion = value;
            }
        }

        internal bool? ReturnContentPreference
        {
            get
            {
                IDataServiceHost2 host = this.host as IDataServiceHost2;
                if (host == null)
                {
                    return null;
                }
                if (host.RequestHeaders == null)
                {
                    throw new InvalidOperationException(Strings.DataServiceHost_RequestHeadersCannotBeNull);
                }
                return HttpProcessUtility.GetReturnContentPreference(host.RequestHeaders["Prefer"]);
            }
        }
    }
}

