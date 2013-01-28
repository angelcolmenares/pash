namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Json;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    internal abstract class ODataOutputContext : IDisposable
    {
        private readonly ODataFormat format;
        private readonly ODataMessageWriterSettings messageWriterSettings;
        private readonly IEdmModel model;
        private readonly bool synchronous;
        private readonly IODataUrlResolver urlResolver;
        private readonly bool writingResponse;

        protected ODataOutputContext(ODataFormat format, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageWriterSettings>(messageWriterSettings, "messageWriterSettings");
            this.format = format;
            this.messageWriterSettings = messageWriterSettings;
            this.writingResponse = writingResponse;
            this.synchronous = synchronous;
            this.model = model;
            this.urlResolver = urlResolver;
        }

        [Conditional("DEBUG")]
        internal void AssertAsynchronous()
        {
        }

        [Conditional("DEBUG")]
        internal void AssertSynchronous()
        {
        }

        internal virtual ODataBatchWriter CreateODataBatchWriter(string batchBoundary)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }

        internal virtual Task<ODataBatchWriter> CreateODataBatchWriterAsync(string batchBoundary)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }

        internal virtual ODataCollectionWriter CreateODataCollectionWriter()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }

        internal virtual Task<ODataCollectionWriter> CreateODataCollectionWriterAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }

        internal virtual ODataWriter CreateODataEntryWriter()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }

        internal virtual Task<ODataWriter> CreateODataEntryWriterAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }

        internal virtual ODataWriter CreateODataFeedWriter()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }

        internal virtual Task<ODataWriter> CreateODataFeedWriterAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }

        internal virtual ODataParameterWriter CreateODataParameterWriter(IEdmFunctionImport functionImport)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual Task<ODataParameterWriter> CreateODataParameterWriterAsync(IEdmFunctionImport functionImport)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal static ODataOutputContext CreateOutputContext(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            if (format == ODataFormat.Atom)
            {
                return ODataAtomOutputContext.Create(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
            }
            if (format == ODataFormat.VerboseJson)
            {
                return ODataJsonOutputContext.Create(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
            }
            if (format == ODataFormat.Metadata)
            {
                return ODataMetadataOutputContext.Create(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
            }
            if ((format != ODataFormat.Batch) && (format != ODataFormat.RawValue))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataOutputContext_CreateOutputContext_UnrecognizedFormat));
            }
            return ODataRawOutputContext.Create(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
        }

        internal static Task<ODataOutputContext> CreateOutputContextAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            if (format == ODataFormat.Atom)
            {
                return ODataAtomOutputContext.CreateAsync(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
            }
            if (format == ODataFormat.VerboseJson)
            {
                return ODataJsonOutputContext.CreateAsync(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
            }
            if ((format != ODataFormat.Batch) && (format != ODataFormat.RawValue))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataOutputContext_CreateOutputContext_UnrecognizedFormat));
            }
            return ODataRawOutputContext.CreateAsync(format, message, encoding, messageWriterSettings, writingResponse, model, urlResolver);
        }

        private ODataException CreatePayloadKindNotSupportedException(ODataPayloadKind payloadKind)
        {
            return new ODataException(Microsoft.Data.OData.Strings.ODataOutputContext_UnsupportedPayloadKindForFormat(this.format.ToString(), payloadKind.ToString()));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        internal virtual void WriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }

        internal virtual Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink link)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }

        internal virtual void WriteEntityReferenceLinks(ODataEntityReferenceLinks links)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }

        internal virtual Task WriteEntityReferenceLinksAsync(ODataEntityReferenceLinks links)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }

        internal virtual void WriteError(ODataError error, bool includeDebugInformation)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual Task WriteErrorAsync(ODataError error, bool includeDebugInformation)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual Task WriteInStreamErrorAsync(ODataError error, bool includeDebugInformation)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual void WriteMetadataDocument()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.MetadataDocument);
        }

        internal virtual void WriteProperty(ODataProperty property)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }

        internal virtual Task WritePropertyAsync(ODataProperty property)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }

        internal virtual void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }

        internal virtual Task WriteServiceDocumentAsync(ODataWorkspace defaultWorkspace)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }

        internal virtual void WriteValue(object value)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }

        internal virtual Task WriteValueAsync(object value)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }

        internal ODataMessageWriterSettings MessageWriterSettings
        {
            get
            {
                return this.messageWriterSettings;
            }
        }

        internal IEdmModel Model
        {
            get
            {
                return this.model;
            }
        }

        internal bool Synchronous
        {
            get
            {
                return this.synchronous;
            }
        }

        internal IODataUrlResolver UrlResolver
        {
            get
            {
                return this.urlResolver;
            }
        }

        protected internal bool UseClientFormatBehavior
        {
            get
            {
                return (this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient);
            }
        }

        protected internal bool UseDefaultFormatBehavior
        {
            get
            {
                return (this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.Default);
            }
        }

        protected internal bool UseServerFormatBehavior
        {
            get
            {
                return (this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesServer);
            }
        }

        internal ODataVersion Version
        {
            get
            {
                return this.messageWriterSettings.Version.Value;
            }
        }

        internal bool WritingResponse
        {
            get
            {
                return this.writingResponse;
            }
        }
    }
}

