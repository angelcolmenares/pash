namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataMessageWriter : IDisposable
    {
        private string batchBoundary;
        private Encoding encoding;
        private ODataFormat format;
        private bool isDisposed;
        private readonly ODataMessage message;
        private readonly IEdmModel model;
        private ODataOutputContext outputContext;
        private readonly ODataMessageWriterSettings settings;
        private readonly IODataUrlResolver urlResolver;
        private bool writeErrorCalled;
        private bool writeMethodCalled;
        private ODataPayloadKind writerPayloadKind;
        private readonly bool writingResponse;

        public ODataMessageWriter(IODataRequestMessage requestMessage) : this(requestMessage, null)
        {
        }

        public ODataMessageWriter(IODataResponseMessage responseMessage) : this(responseMessage, null)
        {
        }

        public ODataMessageWriter(IODataRequestMessage requestMessage, ODataMessageWriterSettings settings) : this(requestMessage, settings, null)
        {
        }

        public ODataMessageWriter(IODataResponseMessage responseMessage, ODataMessageWriterSettings settings) : this(responseMessage, settings, null)
        {
        }

        public ODataMessageWriter(IODataRequestMessage requestMessage, ODataMessageWriterSettings settings, IEdmModel model)
        {
            this.writerPayloadKind = ODataPayloadKind.Unsupported;
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessage>(requestMessage, "requestMessage");
            this.settings = (settings == null) ? new ODataMessageWriterSettings() : new ODataMessageWriterSettings(settings);
            WriterValidationUtils.ValidateMessageWriterSettings(this.settings);
            this.writingResponse = false;
            this.message = new ODataRequestMessage(requestMessage, true, this.settings.DisableMessageStreamDisposal, -1L);
            this.urlResolver = requestMessage as IODataUrlResolver;
            this.model = model ?? EdmCoreModel.Instance;
        }

        public ODataMessageWriter(IODataResponseMessage responseMessage, ODataMessageWriterSettings settings, IEdmModel model)
        {
            this.writerPayloadKind = ODataPayloadKind.Unsupported;
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessage>(responseMessage, "responseMessage");
            this.settings = (settings == null) ? new ODataMessageWriterSettings() : new ODataMessageWriterSettings(settings);
            WriterValidationUtils.ValidateMessageWriterSettings(this.settings);
            this.writingResponse = true;
            this.message = new ODataResponseMessage(responseMessage, true, this.settings.DisableMessageStreamDisposal, -1L);
            this.urlResolver = responseMessage as IODataUrlResolver;
            this.model = model ?? EdmCoreModel.Instance;
        }

        public ODataBatchWriter CreateODataBatchWriter()
        {
            this.VerifyCanCreateODataBatchWriter();
            return this.WriteToOutput<ODataBatchWriter>(ODataPayloadKind.Batch, null, context => context.CreateODataBatchWriter(this.batchBoundary));
        }

        public Task<ODataBatchWriter> CreateODataBatchWriterAsync()
        {
            this.VerifyCanCreateODataBatchWriter();
            return this.WriteToOutputAsync<ODataBatchWriter>(ODataPayloadKind.Batch, null, context => context.CreateODataBatchWriterAsync(this.batchBoundary));
        }

        public ODataCollectionWriter CreateODataCollectionWriter()
        {
            this.VerifyCanCreateODataCollectionWriter();
            return this.WriteToOutput<ODataCollectionWriter>(ODataPayloadKind.Collection, null, context => context.CreateODataCollectionWriter());
        }

        public Task<ODataCollectionWriter> CreateODataCollectionWriterAsync()
        {
            this.VerifyCanCreateODataCollectionWriter();
            return this.WriteToOutputAsync<ODataCollectionWriter>(ODataPayloadKind.Collection, null, context => context.CreateODataCollectionWriterAsync());
        }

        public ODataWriter CreateODataEntryWriter()
        {
            this.VerifyCanCreateODataEntryWriter();
            return this.WriteToOutput<ODataWriter>(ODataPayloadKind.Entry, null, context => context.CreateODataEntryWriter());
        }

        public Task<ODataWriter> CreateODataEntryWriterAsync()
        {
            this.VerifyCanCreateODataEntryWriter();
            return this.WriteToOutputAsync<ODataWriter>(ODataPayloadKind.Entry, null, context => context.CreateODataEntryWriterAsync());
        }

        public ODataWriter CreateODataFeedWriter()
        {
            this.VerifyCanCreateODataFeedWriter();
            return this.WriteToOutput<ODataWriter>(ODataPayloadKind.Feed, null, context => context.CreateODataFeedWriter());
        }

        public Task<ODataWriter> CreateODataFeedWriterAsync()
        {
            this.VerifyCanCreateODataFeedWriter();
            return this.WriteToOutputAsync<ODataWriter>(ODataPayloadKind.Feed, null, context => context.CreateODataFeedWriterAsync());
        }

        public ODataParameterWriter CreateODataParameterWriter(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterWriter(functionImport);
            return this.WriteToOutput<ODataParameterWriter>(ODataPayloadKind.Parameter, new Action(this.VerifyODataParameterWriterHeaders), context => context.CreateODataParameterWriter(functionImport));
        }

        public Task<ODataParameterWriter> CreateODataParameterWriterAsync(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterWriter(functionImport);
            return this.WriteToOutputAsync<ODataParameterWriter>(ODataPayloadKind.Parameter, new Action(this.VerifyODataParameterWriterHeaders), context => context.CreateODataParameterWriterAsync(functionImport));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.isDisposed = true;
            if (disposing)
            {
                try
                {
                    if (this.outputContext != null)
                    {
                        this.outputContext.Dispose();
                    }
                }
                finally
                {
                    this.outputContext = null;
                }
            }
        	
        }

        private void EnsureODataFormatAndContentType()
        {
            string header = null;
            if (!this.settings.UseFormat.HasValue)
            {
                header = this.message.GetHeader("Content-Type");
                header = (header == null) ? null : header.Trim();
            }
            if (!string.IsNullOrEmpty(header))
            {
                ODataPayloadKind kind;
                MediaType type;
                this.format = MediaTypeUtils.GetFormatFromContentType(header, new ODataPayloadKind[] { this.writerPayloadKind }, MediaTypeResolver.DefaultMediaTypeResolver, out type, out this.encoding, out kind, out this.batchBoundary);
            }
            else
            {
                MediaType type2;
                this.format = MediaTypeUtils.GetContentTypeFromSettings(this.settings, this.writerPayloadKind, MediaTypeResolver.DefaultMediaTypeResolver, out type2, out this.encoding);
                if (this.writerPayloadKind == ODataPayloadKind.Batch)
                {
                    this.batchBoundary = ODataBatchWriterUtils.CreateBatchBoundary(this.writingResponse);
                    header = ODataBatchWriterUtils.CreateMultipartMixedContentType(this.batchBoundary);
                }
                else
                {
                    this.batchBoundary = null;
                    header = HttpUtils.BuildContentType(type2, this.encoding);
                }
                this.message.SetHeader("Content-Type", header);
            }
        }

        private void EnsureODataVersion()
        {
            if (!this.settings.Version.HasValue)
            {
                this.settings.Version = new ODataVersion?(ODataUtilsInternal.GetDataServiceVersion(this.message, ODataVersion.V3));
            }
            else
            {
                ODataUtilsInternal.SetDataServiceVersion(this.message, this.settings);
            }
            if ((((ODataVersion) this.settings.Version) >= ODataVersion.V3) && (this.settings.WriterBehavior.FormatBehaviorKind != ODataBehaviorKind.Default))
            {
                this.settings.WriterBehavior.UseDefaultFormatBehavior();
            }
        }

        internal ODataFormat SetHeaders(ODataPayloadKind payloadKind)
        {
            this.writerPayloadKind = payloadKind;
            this.EnsureODataVersion();
            this.EnsureODataFormatAndContentType();
            return this.format;
        }

        private void SetOrVerifyHeaders(ODataPayloadKind payloadKind)
        {
            this.VerifyPayloadKind(payloadKind);
            if (this.writerPayloadKind == ODataPayloadKind.Unsupported)
            {
                this.SetHeaders(payloadKind);
            }
        }

        private void VerifyCanCreateODataBatchWriter()
        {
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanCreateODataCollectionWriter()
        {
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanCreateODataEntryWriter()
        {
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanCreateODataFeedWriter()
        {
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanCreateODataParameterWriter(IEdmFunctionImport functionImport)
        {
            if (this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriter_CannotCreateParameterWriterOnResponseMessage);
            }
            if ((functionImport != null) && !this.model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotSpecifyFunctionImportWithoutModel);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataEntityReferenceLink>(link, "link");
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteEntityReferenceLinks(ODataEntityReferenceLinks links)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataEntityReferenceLinks>(links, "links");
            if (!this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_EntityReferenceLinksInRequestNotAllowed);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteInStreamError(ODataError error)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataError>(error, "error");
            this.VerifyNotDisposed();
            if (!this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_ErrorPayloadInRequest);
            }
            if (this.writeErrorCalled)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_WriteErrorAlreadyCalled);
            }
            this.writeErrorCalled = true;
            this.writeMethodCalled = true;
        }

        private void VerifyCanWriteMetadataDocument()
        {
            if (!this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_MetadataDocumentInRequest);
            }
            if (!this.model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotWriteMetadataWithoutModel);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteProperty(ODataProperty property)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataProperty>(property, "property");
            if (property.Value is ODataStreamReferenceValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotWriteStreamPropertyAsTopLevelProperty(property.Name));
            }
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataWorkspace>(defaultWorkspace, "defaultWorkspace");
            if (!this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_ServiceDocumentInRequest);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
        }

        private void VerifyCanWriteTopLevelError(ODataError error)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataError>(error, "error");
            if (!this.writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_ErrorPayloadInRequest);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
            this.writeErrorCalled = true;
        }

        private ODataPayloadKind VerifyCanWriteValue(object value)
        {
            if (value == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_CannotWriteNullInRawFormat);
            }
            this.VerifyWriterNotDisposedAndNotUsed();
            if (value is byte[])
            {
                return ODataPayloadKind.BinaryValue;
            }
            return ODataPayloadKind.Value;
        }

        private void VerifyEntityReferenceLinksHeaders(ODataEntityReferenceLinks links)
        {
            if (links.Count.HasValue)
            {
                ODataVersionChecker.CheckCount(this.settings.Version.Value);
            }
            if (links.NextPageLink != null)
            {
                ODataVersionChecker.CheckNextLink(this.settings.Version.Value);
            }
        }

        private void VerifyMetadataDocumentHeaders()
        {
            if (this.model.GetDataServiceVersion() == null)
            {
                Version version = this.settings.Version.Value.ToDataServiceVersion();
                this.model.SetDataServiceVersion(version);
            }
        }

        private void VerifyNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        private void VerifyODataParameterWriterHeaders()
        {
            ODataVersionChecker.CheckParameterPayload(this.settings.Version.Value);
        }

        private void VerifyPayloadKind(ODataPayloadKind payloadKindToWrite)
        {
            if ((this.writerPayloadKind != ODataPayloadKind.Unsupported) && (this.writerPayloadKind != payloadKindToWrite))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_IncompatiblePayloadKinds(this.writerPayloadKind, payloadKindToWrite));
            }
        }

        private void VerifyWriterNotDisposedAndNotUsed()
        {
            this.VerifyNotDisposed();
            if (this.writeMethodCalled)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageWriter_WriterAlreadyUsed);
            }
            this.writeMethodCalled = true;
        }

        public void WriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            this.VerifyCanWriteEntityReferenceLink(link);
            this.WriteToOutput(ODataPayloadKind.EntityReferenceLink, null, context => context.WriteEntityReferenceLink(link));
        }

        public Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink link)
        {
            this.VerifyCanWriteEntityReferenceLink(link);
            return this.WriteToOutputAsync(ODataPayloadKind.EntityReferenceLink, null, context => context.WriteEntityReferenceLinkAsync(link));
        }

        public void WriteEntityReferenceLinks(ODataEntityReferenceLinks links)
        {
            this.VerifyCanWriteEntityReferenceLinks(links);
            this.WriteToOutput(ODataPayloadKind.EntityReferenceLinks, () => this.VerifyEntityReferenceLinksHeaders(links), context => context.WriteEntityReferenceLinks(links));
        }

        public Task WriteEntityReferenceLinksAsync(ODataEntityReferenceLinks links)
        {
            this.VerifyCanWriteEntityReferenceLinks(links);
            return this.WriteToOutputAsync(ODataPayloadKind.EntityReferenceLinks, delegate {
                this.VerifyEntityReferenceLinksHeaders(links);
            }, context => context.WriteEntityReferenceLinksAsync(links));
        }

        public void WriteError(ODataError error, bool includeDebugInformation)
        {
            Action<ODataOutputContext> writeAction = null;
            if (this.outputContext == null)
            {
                this.VerifyCanWriteTopLevelError(error);
                if (writeAction == null)
                {
                    writeAction = context => context.WriteError(error, includeDebugInformation);
                }
                this.WriteToOutput(ODataPayloadKind.Error, null, writeAction);
            }
            else
            {
                this.VerifyCanWriteInStreamError(error);
                this.outputContext.WriteInStreamError(error, includeDebugInformation);
            }
        }

        public Task WriteErrorAsync(ODataError error, bool includeDebugInformation)
        {
            Func<ODataOutputContext, Task> writeAsyncAction = null;
            if (this.outputContext == null)
            {
                this.VerifyCanWriteTopLevelError(error);
                if (writeAsyncAction == null)
                {
                    writeAsyncAction = context => context.WriteErrorAsync(error, includeDebugInformation);
                }
                return this.WriteToOutputAsync(ODataPayloadKind.Error, null, writeAsyncAction);
            }
            this.VerifyCanWriteInStreamError(error);
            return this.outputContext.WriteInStreamErrorAsync(error, includeDebugInformation);
        }

        public void WriteMetadataDocument()
        {
            this.VerifyCanWriteMetadataDocument();
            this.WriteToOutput(ODataPayloadKind.MetadataDocument, new Action(this.VerifyMetadataDocumentHeaders), context => context.WriteMetadataDocument());
        }

        public void WriteProperty(ODataProperty property)
        {
            this.VerifyCanWriteProperty(property);
            this.WriteToOutput(ODataPayloadKind.Property, null, context => context.WriteProperty(property));
        }

        public Task WritePropertyAsync(ODataProperty property)
        {
            this.VerifyCanWriteProperty(property);
            return this.WriteToOutputAsync(ODataPayloadKind.Property, null, context => context.WritePropertyAsync(property));
        }

        public void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            //this.VerifyCanWriteServiceDocument(defaultWorkspace);
            this.WriteToOutput(ODataPayloadKind.ServiceDocument, null, context => context.WriteServiceDocument(defaultWorkspace));
        }

        public Task WriteServiceDocumentAsync(ODataWorkspace defaultWorkspace)
        {
            this.VerifyCanWriteServiceDocument(defaultWorkspace);
            return this.WriteToOutputAsync(ODataPayloadKind.ServiceDocument, null, context => context.WriteServiceDocumentAsync(defaultWorkspace));
        }

        private void WriteToOutput(ODataPayloadKind payloadKind, Action verifyHeaders, Action<ODataOutputContext> writeAction)
        {
            this.SetOrVerifyHeaders(payloadKind);
            if (verifyHeaders != null)
            {
                verifyHeaders();
            }
            this.outputContext = ODataOutputContext.CreateOutputContext(this.format, this.message, this.encoding, this.settings, this.writingResponse, this.model, this.urlResolver);
            writeAction(this.outputContext);
        }

        private TResult WriteToOutput<TResult>(ODataPayloadKind payloadKind, Action verifyHeaders, Func<ODataOutputContext, TResult> writeFunc)
        {
            this.SetOrVerifyHeaders(payloadKind);
            if (verifyHeaders != null)
            {
                verifyHeaders();
            }
            this.outputContext = ODataOutputContext.CreateOutputContext(this.format, this.message, this.encoding, this.settings, this.writingResponse, this.model, this.urlResolver);
            return writeFunc(this.outputContext);
        }

        private Task<TResult> WriteToOutputAsync<TResult>(ODataPayloadKind payloadKind, Action verifyHeaders, Func<ODataOutputContext, Task<TResult>> writeFunc)
        {
            this.SetOrVerifyHeaders(payloadKind);
            if (verifyHeaders != null)
            {
                verifyHeaders();
            }
            return ODataOutputContext.CreateOutputContextAsync(this.format, this.message, this.encoding, this.settings, this.writingResponse, this.model, this.urlResolver).FollowOnSuccessWithTask<ODataOutputContext, TResult>(delegate (Task<ODataOutputContext> createOutputContextTask) {
                this.outputContext = createOutputContextTask.Result;
                return writeFunc(this.outputContext);
            });
        }

        private Task WriteToOutputAsync(ODataPayloadKind payloadKind, Action verifyHeaders, Func<ODataOutputContext, Task> writeAsyncAction)
        {
            this.SetOrVerifyHeaders(payloadKind);
            if (verifyHeaders != null)
            {
                verifyHeaders();
            }
            return ODataOutputContext.CreateOutputContextAsync(this.format, this.message, this.encoding, this.settings, this.writingResponse, this.model, this.urlResolver).FollowOnSuccessWithTask<ODataOutputContext>(delegate (Task<ODataOutputContext> createOutputContextTask) {
                this.outputContext = createOutputContextTask.Result;
                return writeAsyncAction(this.outputContext);
            });
        }

        public void WriteValue(object value)
        {
            ODataPayloadKind payloadKind = this.VerifyCanWriteValue(value);
            this.WriteToOutput(payloadKind, null, context => context.WriteValue(value));
        }

        public Task WriteValueAsync(object value)
        {
            ODataPayloadKind payloadKind = this.VerifyCanWriteValue(value);
            return this.WriteToOutputAsync(payloadKind, null, context => context.WriteValueAsync(value));
        }
    }
}

