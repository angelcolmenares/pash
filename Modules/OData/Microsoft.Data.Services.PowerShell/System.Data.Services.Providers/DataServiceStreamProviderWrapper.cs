namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class DataServiceStreamProviderWrapper
    {
        private readonly IDataService dataService;
        private const int DefaultBufferSize = 0x10000;
        private IDataServiceStreamProvider streamProvider;

        public DataServiceStreamProviderWrapper(IDataService dataService)
        {
            this.dataService = dataService;
        }

        internal void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            InvokeApiCallAndValidateHeaders<bool>("IDataServiceStreamProvider.DeleteStream", delegate {
                this.LoadAndValidateStreamProvider().DeleteStream(entity, operationContext);
                return true;
            }, operationContext);
        }

        internal void DisposeProvider()
        {
            if (this.streamProvider != null)
            {
                WebUtil.Dispose(this.streamProvider);
                this.streamProvider = null;
            }
        }

        private static void GetETagFromHeaders(DataServiceOperationContext operationContext, out string etag, out bool? checkETagForEquality)
        {
            DataServiceHostWrapper host = operationContext.Host;
            if (string.IsNullOrEmpty(host.RequestIfMatch) && string.IsNullOrEmpty(host.RequestIfNoneMatch))
            {
                etag = null;
                checkETagForEquality = false;
            }
            else if (!string.IsNullOrEmpty(host.RequestIfMatch))
            {
                etag = host.RequestIfMatch;
                checkETagForEquality = true;
            }
            else
            {
                etag = host.RequestIfNoneMatch;
                checkETagForEquality = false;
            }
        }

        internal Stream GetReadStream(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext)
        {
            Stream stream;
            Func<Stream> apiCall = null;
            Func<Stream> func2 = null;
            string etagFromHeader;
            bool? checkETagForEquality;
            GetETagFromHeaders(operationContext, out etagFromHeader, out checkETagForEquality);
            try
            {
                if (streamProperty == null)
                {
                    if (apiCall == null)
                    {
                        apiCall = () => this.LoadAndValidateStreamProvider().GetReadStream(entity, etagFromHeader, checkETagForEquality, operationContext);
                    }
                    stream = InvokeApiCallAndValidateHeaders<Stream>("IDataServiceStreamProvider.GetReadStream", apiCall, operationContext);
                }
                else
                {
                    if (func2 == null)
                    {
                        func2 = () => this.LoadAndValidateStreamProvider2().GetReadStream(entity, streamProperty, etagFromHeader, checkETagForEquality, operationContext);
                    }
                    stream = InvokeApiCallAndValidateHeaders<Stream>("IDataServiceStreamProvider2.GetReadStream", func2, operationContext);
                }
            }
            catch (DataServiceException exception)
            {
                if (exception.StatusCode == 0x130)
                {
                    WebUtil.WriteETagValueInResponseHeader(null, this.GetStreamETag(entity, streamProperty, operationContext), operationContext.Host);
                }
                throw;
            }
            try
            {
                if (streamProperty == null)
                {
                    if ((stream == null) || !stream.CanRead)
                    {
                        throw new InvalidOperationException(Strings.DataService_InvalidStreamFromGetReadStream);
                    }
                }
                else if (stream == null)
                {
                    operationContext.ResponseStatusCode = 0xcc;
                }
                else if (!stream.CanRead)
                {
                    throw new InvalidOperationException(Strings.DataService_InvalidStreamFromGetReadStream);
                }
                WebUtil.WriteETagValueInResponseHeader(null, this.GetStreamETag(entity, streamProperty, operationContext), operationContext.Host);
            }
            catch
            {
                WebUtil.Dispose(stream);
                throw;
            }
            return stream;
        }

        internal Uri GetReadStreamUri(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext)
        {
            Uri uri;
            Func<Uri> apiCall = null;
            Func<Uri> func2 = null;
            if (streamProperty == null)
            {
                if (apiCall == null)
                {
                    apiCall = () => this.LoadAndValidateStreamProvider().GetReadStreamUri(entity, operationContext);
                }
                uri = InvokeApiCallAndValidateHeaders<Uri>("IDataServiceStreamProvider.GetReadStreamUri", apiCall, operationContext);
            }
            else
            {
                if (func2 == null)
                {
                    func2 = () => this.LoadAndValidateStreamProvider2().GetReadStreamUri(entity, streamProperty, operationContext);
                }
                uri = InvokeApiCallAndValidateHeaders<Uri>("IDataServiceStreamProvider2.GetReadStreamUri", func2, operationContext);
            }
            if ((uri != null) && !uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_GetReadStreamUriMustReturnAbsoluteUriOrNull);
            }
            return uri;
        }

        internal string GetStreamContentType(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext)
        {
            Func<string> apiCall = null;
            Func<string> func2 = null;
            if (streamProperty == null)
            {
                if (apiCall == null)
                {
                    apiCall = () => this.LoadAndValidateStreamProvider().GetStreamContentType(entity, operationContext);
                }
                string str = InvokeApiCallAndValidateHeaders<string>("IDataServiceStreamProvider.GetStreamContentType", apiCall, operationContext);
                if (string.IsNullOrEmpty(str))
                {
                    throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_GetStreamContentTypeReturnsEmptyOrNull);
                }
                return str;
            }
            if (func2 == null)
            {
                func2 = () => this.LoadAndValidateStreamProvider2().GetStreamContentType(entity, streamProperty, operationContext);
            }
            return InvokeApiCallAndValidateHeaders<string>("IDataServiceStreamProvider2.GetStreamContentType", func2, operationContext);
        }

        internal void GetStreamDescription(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext, out string etag, out Uri readStreamUri, out string contentType)
        {
            etag = this.GetStreamETag(entity, streamProperty, operationContext);
            readStreamUri = this.GetReadStreamUri(entity, streamProperty, operationContext);
            contentType = this.GetStreamContentType(entity, streamProperty, operationContext);
        }

        internal string GetStreamETag(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext)
        {
            string str;
            Func<string> apiCall = null;
            Func<string> func2 = null;
            if (streamProperty == null)
            {
                if (apiCall == null)
                {
                    apiCall = () => this.LoadAndValidateStreamProvider().GetStreamETag(entity, operationContext);
                }
                str = InvokeApiCallAndValidateHeaders<string>("IDataServiceStreamProvider.GetStreamETag", apiCall, operationContext);
            }
            else
            {
                if (func2 == null)
                {
                    func2 = () => this.LoadAndValidateStreamProvider2().GetStreamETag(entity, streamProperty, operationContext);
                }
                str = InvokeApiCallAndValidateHeaders<string>("IDataServiceStreamProvider2.GetStreamETag", func2, operationContext);
            }
            if (!WebUtil.IsETagValueValid(str, true))
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_GetStreamETagReturnedInvalidETagFormat);
            }
            return str;
        }

        internal Stream GetWriteStream(object entity, ResourceProperty streamProperty, DataServiceOperationContext operationContext)
        {
            Stream stream;
            Func<Stream> apiCall = null;
            Func<Stream> func2 = null;
            string etag;
            bool? checkETagForEquality;
            GetETagFromHeaders(operationContext, out etag, out checkETagForEquality);
            if (streamProperty == null)
            {
                if (apiCall == null)
                {
                    apiCall = () => this.LoadAndValidateStreamProvider().GetWriteStream(entity, etag, checkETagForEquality, operationContext);
                }
                stream = InvokeApiCallAndValidateHeaders<Stream>("IDataServiceStreamProvider.GetWriteStream", apiCall, operationContext);
            }
            else
            {
                if (func2 == null)
                {
                    func2 = () => this.LoadAndValidateStreamProvider2().GetWriteStream(entity, streamProperty, etag, checkETagForEquality, operationContext);
                }
                stream = InvokeApiCallAndValidateHeaders<Stream>("IDataServiceStreamProvider2.GetWriteStream", func2, operationContext);
            }
            if ((stream != null) && stream.CanWrite)
            {
                return stream;
            }
            WebUtil.Dispose(stream);
            throw new InvalidOperationException(Strings.DataService_InvalidStreamFromGetWriteStream);
        }

        private static T InvokeApiCallAndValidateHeaders<T>(string methodName, Func<T> apiCall, DataServiceOperationContext operationContext)
        {
            string responseContentType = operationContext.Host.ResponseContentType;
            string responseETag = operationContext.Host.ResponseETag;
            T local = apiCall();
            if ((operationContext.Host.ResponseContentType != responseContentType) || (operationContext.Host.ResponseETag != responseETag))
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_MustNotSetContentTypeAndEtag(methodName));
            }
            return local;
        }

        internal IDataServiceStreamProvider LoadAndValidateStreamProvider()
        {
            if (this.streamProvider == null)
            {
                this.LoadStreamProvider();
                if (this.streamProvider == null)
                {
                    throw new DataServiceException(500, Strings.DataServiceStreamProviderWrapper_MustImplementIDataServiceStreamProviderToSupportStreaming);
                }
            }
            return this.streamProvider;
        }

        internal IDataServiceStreamProvider2 LoadAndValidateStreamProvider2()
        {
            if (this.dataService.Configuration.DataServiceBehavior.MaxProtocolVersion < DataServiceProtocolVersion.V3)
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_MaxProtocolVersionMustBeV3OrAboveToSupportNamedStreams);
            }
            if (this.streamProvider == null)
            {
                this.LoadStreamProvider();
            }
            IDataServiceStreamProvider2 streamProvider = this.streamProvider as IDataServiceStreamProvider2;
            if (streamProvider == null)
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_MustImplementDataServiceStreamProvider2ToSupportNamedStreams);
            }
            return streamProvider;
        }

        private void LoadStreamProvider()
        {
            if (this.streamProvider == null)
            {
                if (this.dataService.Configuration.DataServiceBehavior.MaxProtocolVersion >= DataServiceProtocolVersion.V3)
                {
                    this.streamProvider = this.dataService.Provider.GetService<IDataServiceStreamProvider2>();
                }
                if (this.streamProvider == null)
                {
                    this.streamProvider = this.dataService.Provider.GetService<IDataServiceStreamProvider>();
                }
            }
        }

        internal ResourceType ResolveType(string entitySetName, IDataService service)
        {
            DataServiceOperationContext operationContext = service.OperationContext;
            string str = InvokeApiCallAndValidateHeaders<string>("IDataServiceStreamProvider.ResolveType", () => this.LoadAndValidateStreamProvider().ResolveType(entitySetName, operationContext), operationContext);
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_ResolveTypeMustReturnValidResourceTypeName);
            }
            ResourceType type = service.Provider.TryResolveResourceType(str);
            if (type == null)
            {
                throw new InvalidOperationException(Strings.DataServiceStreamProviderWrapper_ResolveTypeMustReturnValidResourceTypeName);
            }
            return type;
        }

        public int StreamBufferSize
        {
            get
            {
                int streamBufferSize = this.LoadAndValidateStreamProvider().StreamBufferSize;
                if (streamBufferSize <= 0)
                {
                    return 0x10000;
                }
                return streamBufferSize;
            }
        }
    }
}

