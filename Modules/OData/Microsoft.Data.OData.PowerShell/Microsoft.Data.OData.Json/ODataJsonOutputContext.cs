namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataJsonOutputContext : ODataOutputContext
    {
        private AsyncBufferedStream asynchronousOutputStream;
        private Microsoft.Data.OData.Json.JsonWriter jsonWriter;
        private Stream messageOutputStream;
        private IODataOutputInStreamErrorListener outputInStreamErrorListener;
        private TextWriter textWriter;

        private ODataJsonOutputContext(ODataFormat format, Microsoft.Data.OData.Json.JsonWriter jsonWriter, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
        {
            this.jsonWriter = jsonWriter;
        }

        private ODataJsonOutputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
        {
            try
            {
                Stream asynchronousOutputStream;
                this.messageOutputStream = messageStream;
                if (synchronous)
                {
                    asynchronousOutputStream = messageStream;
                }
                else
                {
                    this.asynchronousOutputStream = new AsyncBufferedStream(messageStream);
                    asynchronousOutputStream = this.asynchronousOutputStream;
                }
                this.textWriter = new StreamWriter(asynchronousOutputStream, encoding);
                this.jsonWriter = new Microsoft.Data.OData.Json.JsonWriter(this.textWriter, messageWriterSettings.Indent);
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception) && (messageStream != null))
                {
                    messageStream.Dispose();
                }
                throw;
            }
        }

        internal static ODataJsonOutputContext Create(ODataFormat format, Microsoft.Data.OData.Json.JsonWriter jsonWriter, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataJsonOutputContext(format, jsonWriter, messageWriterSettings, writingResponse, true, model, urlResolver);
        }

        internal static ODataOutputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataJsonOutputContext(format, message.GetStream(), encoding, messageWriterSettings, writingResponse, true, model, urlResolver);
        }

        internal static Task<ODataOutputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<Stream, ODataOutputContext>(streamTask => new ODataJsonOutputContext(format, streamTask.Result, encoding, messageWriterSettings, writingResponse, false, model, urlResolver));
        }

        internal override ODataCollectionWriter CreateODataCollectionWriter()
        {
            return this.CreateODataCollectionWriterImplementation();
        }

        internal override Task<ODataCollectionWriter> CreateODataCollectionWriterAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataCollectionWriter>(() => this.CreateODataCollectionWriterImplementation());
        }

        private ODataCollectionWriter CreateODataCollectionWriterImplementation()
        {
            ODataJsonCollectionWriter writer = new ODataJsonCollectionWriter(this, null, null);
            this.outputInStreamErrorListener = writer;
            return writer;
        }

        internal override ODataWriter CreateODataEntryWriter()
        {
            return this.CreateODataEntryWriterImplementation();
        }

        internal override Task<ODataWriter> CreateODataEntryWriterAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataWriter>(() => this.CreateODataEntryWriterImplementation());
        }

        private ODataWriter CreateODataEntryWriterImplementation()
        {
            ODataJsonWriter writer = new ODataJsonWriter(this, false);
            this.outputInStreamErrorListener = writer;
            return writer;
        }

        internal override ODataWriter CreateODataFeedWriter()
        {
            return this.CreateODataFeedWriterImplementation();
        }

        internal override Task<ODataWriter> CreateODataFeedWriterAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataWriter>(() => this.CreateODataFeedWriterImplementation());
        }

        private ODataWriter CreateODataFeedWriterImplementation()
        {
            ODataJsonWriter writer = new ODataJsonWriter(this, true);
            this.outputInStreamErrorListener = writer;
            return writer;
        }

        internal override ODataParameterWriter CreateODataParameterWriter(IEdmFunctionImport functionImport)
        {
            return this.CreateODataParameterWriterImplementation(functionImport);
        }

        internal override Task<ODataParameterWriter> CreateODataParameterWriterAsync(IEdmFunctionImport functionImport)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataParameterWriter>(() => this.CreateODataParameterWriterImplementation(functionImport));
        }

        private ODataParameterWriter CreateODataParameterWriterImplementation(IEdmFunctionImport functionImport)
        {
            ODataJsonParameterWriter writer = new ODataJsonParameterWriter(this, functionImport);
            this.outputInStreamErrorListener = writer;
            return writer;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (this.messageOutputStream != null)
                {
                    this.jsonWriter.Flush();
                    if (this.asynchronousOutputStream != null)
                    {
                        this.asynchronousOutputStream.FlushSync();
                        this.asynchronousOutputStream.Dispose();
                    }
                    this.messageOutputStream.Dispose();
                }
            }
            finally
            {
                this.messageOutputStream = null;
                this.asynchronousOutputStream = null;
                this.textWriter = null;
                this.jsonWriter = null;
            }
        }

        internal void Flush()
        {
            this.jsonWriter.Flush();
        }

        internal Task FlushAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.jsonWriter.Flush();
                return this.asynchronousOutputStream.FlushAsync();
            }).FollowOnSuccessWithTask(asyncBufferedStreamFlushTask => this.messageOutputStream.FlushAsync());
        }

        internal void VerifyNotDisposed()
        {
            if (this.messageOutputStream == null)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        internal override void WriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            this.WriteEntityReferenceLinkImplementation(link);
            this.Flush();
        }

        internal override Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink link)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteEntityReferenceLinkImplementation(link);
                return this.FlushAsync();
            });
        }

        private void WriteEntityReferenceLinkImplementation(ODataEntityReferenceLink link)
        {
            new ODataJsonEntityReferenceLinkSerializer(this).WriteEntityReferenceLink(link);
        }

        internal override void WriteEntityReferenceLinks(ODataEntityReferenceLinks links)
        {
            this.WriteEntityReferenceLinksImplementation(links);
            this.Flush();
        }

        internal override Task WriteEntityReferenceLinksAsync(ODataEntityReferenceLinks links)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteEntityReferenceLinksImplementation(links);
                return this.FlushAsync();
            });
        }

        private void WriteEntityReferenceLinksImplementation(ODataEntityReferenceLinks links)
        {
            new ODataJsonEntityReferenceLinkSerializer(this).WriteEntityReferenceLinks(links);
        }

        internal override void WriteError(ODataError error, bool includeDebugInformation)
        {
            this.WriteErrorImplementation(error, includeDebugInformation);
            this.Flush();
        }

        internal override Task WriteErrorAsync(ODataError error, bool includeDebugInformation)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteErrorImplementation(error, includeDebugInformation);
                return this.FlushAsync();
            });
        }

        private void WriteErrorImplementation(ODataError error, bool includeDebugInformation)
        {
            new ODataJsonSerializer(this).WriteTopLevelError(error, includeDebugInformation);
        }

        internal override void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            this.WriteInStreamErrorImplementation(error, includeDebugInformation);
            this.Flush();
        }

        internal override Task WriteInStreamErrorAsync(ODataError error, bool includeDebugInformation)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteInStreamErrorImplementation(error, includeDebugInformation);
                return this.FlushAsync();
            });
        }

        private void WriteInStreamErrorImplementation(ODataError error, bool includeDebugInformation)
        {
            if (this.outputInStreamErrorListener != null)
            {
                this.outputInStreamErrorListener.OnInStreamError();
            }
            ODataJsonWriterUtils.WriteError(this.jsonWriter, error, includeDebugInformation, base.MessageWriterSettings.MessageQuotas.MaxNestingDepth);
        }

        internal override void WriteProperty(ODataProperty property)
        {
            this.WritePropertyImplementation(property);
            this.Flush();
        }

        internal override Task WritePropertyAsync(ODataProperty property)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WritePropertyImplementation(property);
                return this.FlushAsync();
            });
        }

        private void WritePropertyImplementation(ODataProperty property)
        {
            new ODataJsonPropertyAndValueSerializer(this).WriteTopLevelProperty(property);
        }

        internal override void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            this.WriteServiceDocumentImplementation(defaultWorkspace);
            this.Flush();
        }

        internal override Task WriteServiceDocumentAsync(ODataWorkspace defaultWorkspace)
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.WriteServiceDocumentImplementation(defaultWorkspace);
                return this.FlushAsync();
            });
        }

        private void WriteServiceDocumentImplementation(ODataWorkspace defaultWorkspace)
        {
            new ODataJsonServiceDocumentSerializer(this).WriteServiceDocument(defaultWorkspace);
        }

        internal Microsoft.Data.OData.Json.JsonWriter JsonWriter
        {
            get
            {
                return this.jsonWriter;
            }
        }
    }
}

