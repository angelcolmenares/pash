namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Json;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    internal abstract class ODataInputContext : IDisposable
    {
        private bool disposed;
        private readonly ODataFormat format;
        private readonly ODataMessageReaderSettings messageReaderSettings;
        private readonly IEdmModel model;
        private readonly bool readingResponse;
        private readonly bool synchronous;
        private readonly IODataUrlResolver urlResolver;
        private readonly ODataVersion version;

        protected ODataInputContext(ODataFormat format, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "messageReaderSettings");
            this.format = format;
            this.messageReaderSettings = messageReaderSettings;
            this.version = version;
            this.readingResponse = readingResponse;
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

        internal virtual ODataBatchReader CreateBatchReader(string batchBoundary)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }

        internal virtual Task<ODataBatchReader> CreateBatchReaderAsync(string batchBoundary)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }

        internal virtual ODataCollectionReader CreateCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }

        internal virtual Task<ODataCollectionReader> CreateCollectionReaderAsync(IEdmTypeReference expectedItemTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }

        internal DuplicatePropertyNamesChecker CreateDuplicatePropertyNamesChecker()
        {
            return new DuplicatePropertyNamesChecker(this.MessageReaderSettings.ReaderBehavior.AllowDuplicatePropertyNames, this.ReadingResponse);
        }

        internal virtual ODataReader CreateEntryReader(IEdmEntityType expectedEntityType)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }

        internal virtual Task<ODataReader> CreateEntryReaderAsync(IEdmEntityType expectedEntityType)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }

        internal virtual ODataReader CreateFeedReader(IEdmEntityType expectedBaseEntityType)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }

        internal virtual Task<ODataReader> CreateFeedReaderAsync(IEdmEntityType expectedBaseEntityType)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }

        internal static ODataInputContext CreateInputContext(ODataFormat format, ODataPayloadKind readerPayloadKind, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            if (format == ODataFormat.Atom)
            {
                return ODataAtomInputContext.Create(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver);
            }
            if (format == ODataFormat.VerboseJson)
            {
                return ODataJsonInputContext.Create(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver);
            }
            if (format == ODataFormat.Metadata)
            {
                return ODataMetadataInputContext.Create(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver);
            }
            if ((format != ODataFormat.Batch) && (format != ODataFormat.RawValue))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataInputContext_CreateInputContext_UnrecognizedFormat));
            }
            return ODataRawInputContext.Create(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver, readerPayloadKind);
        }

        internal static Task<ODataInputContext> CreateInputContextAsync(ODataFormat format, ODataPayloadKind readerPayloadKind, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            if (format == ODataFormat.Atom)
            {
                return ODataAtomInputContext.CreateAsync(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver);
            }
            if (format == ODataFormat.VerboseJson)
            {
                return ODataJsonInputContext.CreateAsync(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver);
            }
            if ((format != ODataFormat.Batch) && (format != ODataFormat.RawValue))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataInputContext_CreateInputContext_UnrecognizedFormat));
            }
            return ODataRawInputContext.CreateAsync(format, message, encoding, messageReaderSettings, version, readingResponse, model, urlResolver, readerPayloadKind);
        }

        internal virtual ODataParameterReader CreateParameterReader(IEdmFunctionImport functionImport)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Parameter);
        }

        internal virtual Task<ODataParameterReader> CreateParameterReaderAsync(IEdmFunctionImport functionImport)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Parameter);
        }

        private ODataException CreatePayloadKindNotSupportedException(ODataPayloadKind payloadKind)
        {
            return new ODataException(Microsoft.Data.OData.Strings.ODataInputContext_UnsupportedPayloadKindForFormat(this.format.ToString(), payloadKind.ToString()));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.disposed = true;
            if (disposing)
            {
                this.DisposeImplementation();
            }
        }

        protected abstract void DisposeImplementation();
        internal virtual ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }

        internal virtual Task<ODataEntityReferenceLink> ReadEntityReferenceLinkAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }

        internal virtual ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }

        internal virtual Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }

        internal virtual ODataError ReadError()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual Task<ODataError> ReadErrorAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

        internal virtual IEdmModel ReadMetadataDocument()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.MetadataDocument);
        }

        internal virtual ODataProperty ReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }

        internal virtual Task<ODataProperty> ReadPropertyAsync(IEdmTypeReference expectedPropertyTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }

        internal virtual ODataWorkspace ReadServiceDocument()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }

        internal virtual Task<ODataWorkspace> ReadServiceDocumentAsync()
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }

        internal virtual object ReadValue(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }

        internal virtual Task<object> ReadValueAsync(IEdmPrimitiveTypeReference expectedPrimitiveTypeReference)
        {
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }

        internal void VerifyNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        internal ODataMessageReaderSettings MessageReaderSettings
        {
            get
            {
                return this.messageReaderSettings;
            }
        }

        internal IEdmModel Model
        {
            get
            {
                return this.model;
            }
        }

        internal bool ReadingResponse
        {
            get
            {
                return this.readingResponse;
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

        protected internal bool UseClientApiBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesClient);
            }
        }

        protected internal bool UseClientFormatBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient);
            }
        }

        protected internal bool UseDefaultApiBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.Default);
            }
        }

        protected internal bool UseDefaultFormatBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.FormatBehaviorKind == ODataBehaviorKind.Default);
            }
        }

        protected internal bool UseServerApiBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer);
            }
        }

        protected internal bool UseServerFormatBehavior
        {
            get
            {
                return (this.messageReaderSettings.ReaderBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesServer);
            }
        }

        internal ODataVersion Version
        {
            get
            {
                return this.version;
            }
        }
    }
}

