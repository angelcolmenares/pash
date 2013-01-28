namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Data.Services.Providers;
    using System.Data.Services.Serializers;
    using System.IO;
    using System.Text;

    internal class ResponseBodyWriter
    {
        private readonly ODataFormat contentFormat;
        private readonly Encoding encoding;
        private readonly bool hasMoved;
        private readonly Stream mediaResourceStream;
        private readonly ODataMessageWriter messageWriter;
        private readonly ODataPayloadKind payloadKind;
        private readonly IEnumerator queryResults;
        private readonly RequestDescription requestDescription;
        private readonly IODataResponseMessage responseMessage;
        private readonly IDataService service;

        internal ResponseBodyWriter(bool hasMoved, IDataService service, IEnumerator queryResults, RequestDescription requestDescription, IODataResponseMessage responseMessage, ODataPayloadKind payloadKind)
        {
            this.hasMoved = hasMoved;
            this.service = service;
            this.queryResults = queryResults;
            this.requestDescription = requestDescription;
            this.responseMessage = responseMessage;
            this.payloadKind = payloadKind;
            this.encoding = HttpProcessUtility.EncodingFromAcceptCharset(this.service.OperationContext.Host.RequestAcceptCharSet);
            if ((((payloadKind == ODataPayloadKind.Entry) || (payloadKind == ODataPayloadKind.Feed)) || ((payloadKind == ODataPayloadKind.Property) || (payloadKind == ODataPayloadKind.Collection))) || (((payloadKind == ODataPayloadKind.EntityReferenceLink) || (payloadKind == ODataPayloadKind.EntityReferenceLinks)) || (((payloadKind == ODataPayloadKind.Error) || (payloadKind == ODataPayloadKind.ServiceDocument)) || (payloadKind == ODataPayloadKind.Parameter))))
            {
                DataServiceHostWrapper host = service.OperationContext.Host;
                if (WebUtil.GetEffectiveMaxResponseVersion(service.Configuration.DataServiceBehavior.MaxProtocolVersion, host.RequestMaxVersion) > RequestDescription.Version2Dot0)
                {
                    bool isEntityOrFeed = (payloadKind == ODataPayloadKind.Entry) || (payloadKind == ODataPayloadKind.Feed);
                    if (WebUtil.ResponseMediaTypeWouldBeJsonLight(host.RequestAccept, isEntityOrFeed))
                    {
                        requestDescription.VerifyAndRaiseResponseVersion(RequestDescription.Version3Dot0, service);
                        host.ResponseVersion = RequestDescription.Version3Dot0.ToString() + ";";
                    }
                }
            }
            if (this.requestDescription.TargetKind == RequestTargetKind.MediaResource)
            {
                this.mediaResourceStream = service.StreamProvider.GetReadStream(this.queryResults.Current, RequestDescription.GetStreamProperty(this.requestDescription), this.service.OperationContext);
            }
            else if (payloadKind != ODataPayloadKind.BinaryValue)
            {
                string requestAcceptCharSet = this.service.OperationContext.Host.RequestAcceptCharSet;
                if (string.IsNullOrEmpty(requestAcceptCharSet) || (requestAcceptCharSet == "*"))
                {
                    requestAcceptCharSet = "UTF-8";
                }
                if ((payloadKind == ODataPayloadKind.Value) && !string.IsNullOrEmpty(this.requestDescription.MimeType))
                {
                    this.messageWriter = CreateMessageWriter(this.AbsoluteServiceUri, this.service, this.requestDescription.ActualResponseVersion, responseMessage, ODataFormat.RawValue);
                }
                else
                {
                    this.messageWriter = CreateMessageWriter(this.AbsoluteServiceUri, this.service, this.requestDescription.ActualResponseVersion, responseMessage, this.service.OperationContext.Host.RequestAccept, requestAcceptCharSet);
                }
                try
                {
                    this.contentFormat = ODataUtils.SetHeadersForPayload(this.messageWriter, payloadKind);
                    if ((payloadKind == ODataPayloadKind.Value) && !string.IsNullOrEmpty(this.requestDescription.MimeType))
                    {
                        responseMessage.SetHeader("Content-Type", this.requestDescription.MimeType);
                    }
                }
                catch (ODataContentTypeException exception)
                {
                    throw new DataServiceException(0x19f, null, System.Data.Services.Strings.DataServiceException_UnsupportedMediaType, null, exception);
                }
                string headerValue = this.requestDescription.ResponseVersion.ToString() + ";";
                responseMessage.SetHeader("DataServiceVersion", headerValue);
            }
        }

        internal static ODataMessageWriter CreateMessageWriter(Uri serviceUri, IDataService dataService, Version responseVersion, IODataResponseMessage responseMessage, ODataFormat format)
        {
            return new ODataMessageWriter(responseMessage, CreateMessageWriterSettings(serviceUri, dataService.Provider, responseVersion, responseMessage is System.Data.Services.ODataResponseMessage, format), (dataService.OperationContext == null) ? null : dataService.Provider.GetMetadataModel(dataService.OperationContext));
        }

        internal static ODataMessageWriter CreateMessageWriter(Uri serviceUri, IDataService dataService, Version responseVersion, IODataResponseMessage responseMessage, string acceptHeaderValue, string acceptCharSetHeaderValue)
        {
            return new ODataMessageWriter(responseMessage, CreateMessageWriterSettings(serviceUri, dataService.Provider, responseVersion, responseMessage is System.Data.Services.ODataResponseMessage, acceptHeaderValue, acceptCharSetHeaderValue), (dataService.OperationContext == null) ? null : dataService.Provider.GetMetadataModel(dataService.OperationContext));
        }

        private static ODataMessageWriterSettings CreateMessageWriterSettings(Uri serviceUri, DataServiceProviderWrapper provider, Version responseVersion, bool disableDisposeStream, ODataFormat format)
        {
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings {
                BaseUri = serviceUri,
                Version = new ODataVersion?(CommonUtil.ConvertToODataVersion(responseVersion)),
                Indent = false,
                CheckCharacters = false,
                DisableMessageStreamDisposal = disableDisposeStream
            };
            settings.EnableWcfDataServicesServerBehavior(provider.IsV1Provider);
            settings.SetContentType(format);
            return settings;
        }

        private static ODataMessageWriterSettings CreateMessageWriterSettings(Uri serviceUri, DataServiceProviderWrapper provider, Version responseVersion, bool disableDisposeStream, string acceptHeaderValue, string acceptCharSetHeaderValue)
        {
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings {
                BaseUri = serviceUri,
                Version = new ODataVersion?(CommonUtil.ConvertToODataVersion(responseVersion)),
                Indent = false,
                CheckCharacters = false,
                DisableMessageStreamDisposal = disableDisposeStream
            };
            settings.EnableWcfDataServicesServerBehavior(provider.IsV1Provider);
            settings.SetContentType(acceptHeaderValue, acceptCharSetHeaderValue);
            return settings;
        }

        internal void Write(Stream stream)
        {
            IExceptionWriter exceptionWriter = null;
            Stream output = null;
            Serializer serializer = null;
            System.Data.Services.ODataResponseMessage responseMessage = this.responseMessage as System.Data.Services.ODataResponseMessage;
            if (responseMessage != null)
            {
                responseMessage.SetStream(stream);
            }
            try
            {
                BinarySerializer serializer2;
                MetadataSerializer serializer7;
                switch (this.payloadKind)
                {
                    case ODataPayloadKind.Feed:
                    case ODataPayloadKind.Entry:
                    {
                        EntitySerializer serializer6 = new EntitySerializer(this.requestDescription, this.AbsoluteServiceUri, this.service, this.service.OperationContext.Host.ResponseETag, this.messageWriter, this.contentFormat);
                        serializer = serializer6;
                        serializer6.WriteRequest(this.queryResults, this.hasMoved);
                        return;
                    }
                    case ODataPayloadKind.Property:
                    case ODataPayloadKind.EntityReferenceLink:
                    case ODataPayloadKind.EntityReferenceLinks:
                    case ODataPayloadKind.Collection:
                    {
                        NonEntitySerializer serializer5 = new NonEntitySerializer(this.requestDescription, this.AbsoluteServiceUri, this.service, this.messageWriter);
                        serializer = serializer5;
                        serializer5.WriteRequest(this.queryResults, this.hasMoved);
                        return;
                    }
                    case ODataPayloadKind.Value:
                        new TextSerializer(this.messageWriter).WriteRequest(this.queryResults.Current);
                        return;

                    case ODataPayloadKind.BinaryValue:
                        output = this.responseMessage.GetStream();
                        serializer2 = new BinarySerializer(output);
                        exceptionWriter = serializer2;
                        if (this.requestDescription.TargetKind != RequestTargetKind.MediaResource)
                        {
                            break;
                        }
                        if (this.mediaResourceStream != null)
                        {
                            serializer2.WriteRequest(this.mediaResourceStream, this.service.StreamProvider.StreamBufferSize);
                        }
                        return;

                    case ODataPayloadKind.ServiceDocument:
                        new ServiceDocumentSerializer(this.messageWriter).WriteServiceDocument(this.service.Provider);
                        return;

                    default:
                        goto Label_0194;
                }
                serializer2.WriteRequest(this.queryResults.Current);
                return;
            Label_0194:
                serializer7 = new MetadataSerializer(this.messageWriter);
                serializer7.WriteMetadataDocument(this.service);
            }
            catch (Exception exception)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                if (serializer != null)
                {
                    serializer.Flush();
                }
                string contentType = this.responseMessage.GetHeader("Content-Type").StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ? "application/json;odata=verbose" : "application/xml";
                if (this.messageWriter != null)
                {
                    ErrorHandler.HandleDuringWritingException(exception, this.service, contentType, this.messageWriter, stream, this.encoding);
                }
                else
                {
                    ErrorHandler.HandleDuringWritingException(exception, this.service, contentType, exceptionWriter);
                }
            }
            finally
            {
                WebUtil.Dispose(this.messageWriter);
                WebUtil.Dispose(this.queryResults);
                WebUtil.Dispose(this.mediaResourceStream);
                if ((output != null) && (responseMessage == null))
                {
                    output.Dispose();
                }
            }
        }

        internal Uri AbsoluteServiceUri
        {
            get
            {
                return this.service.OperationContext.AbsoluteServiceUri;
            }
        }

        internal DataServiceProviderWrapper Provider
        {
            get
            {
                return this.service.Provider;
            }
        }
    }
}

