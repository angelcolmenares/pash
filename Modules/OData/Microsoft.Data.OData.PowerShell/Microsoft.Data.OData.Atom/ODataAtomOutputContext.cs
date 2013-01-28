namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    internal sealed class ODataAtomOutputContext : ODataOutputContext
    {
        private AsyncBufferedStream asynchronousOutputStream;
        private Stream messageOutputStream;
        private IODataOutputInStreamErrorListener outputInStreamErrorListener;
        private Stack<System.Xml.XmlWriter> xmlCustomizationWriters;
        private System.Xml.XmlWriter xmlRootWriter;
        private System.Xml.XmlWriter xmlWriter;

        private ODataAtomOutputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageWriterSettings, writingResponse, synchronous, model, urlResolver)
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
                this.xmlRootWriter = ODataAtomWriterUtils.CreateXmlWriter(asynchronousOutputStream, messageWriterSettings, encoding);
                this.xmlWriter = this.xmlRootWriter;
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

        internal static ODataOutputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataAtomOutputContext(format, message.GetStream(), encoding, messageWriterSettings, writingResponse, true, model, urlResolver);
        }

        internal static Task<ODataOutputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageWriterSettings messageWriterSettings, bool writingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<Stream, ODataOutputContext>(streamTask => new ODataAtomOutputContext(format, streamTask.Result, encoding, messageWriterSettings, writingResponse, false, model, urlResolver));
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
            ODataAtomCollectionWriter writer = new ODataAtomCollectionWriter(this, null, null);
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
            ODataAtomWriter writer = new ODataAtomWriter(this, false);
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
            ODataAtomWriter writer = new ODataAtomWriter(this, true);
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
                    this.xmlRootWriter.Flush();
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
                this.xmlWriter = null;
            }
        }

        internal void Flush()
        {
            this.xmlWriter.Flush();
        }

        internal Task FlushAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperationReturningTask(delegate {
                this.xmlWriter.Flush();
                return this.asynchronousOutputStream.FlushAsync();
            }).FollowOnSuccessWithTask(asyncBufferedStreamFlushTask => this.messageOutputStream.FlushAsync());
        }

        internal void InitializeWriterCustomization()
        {
            this.xmlCustomizationWriters = new Stack<System.Xml.XmlWriter>();
            this.xmlCustomizationWriters.Push(this.xmlRootWriter);
        }

        internal System.Xml.XmlWriter PopCustomWriter()
        {
            System.Xml.XmlWriter writer = this.xmlCustomizationWriters.Pop();
            this.xmlWriter = this.xmlCustomizationWriters.Peek();
            return writer;
        }

        internal void PushCustomWriter(System.Xml.XmlWriter customXmlWriter)
        {
            this.xmlCustomizationWriters.Push(customXmlWriter);
            this.xmlWriter = customXmlWriter;
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
            new ODataAtomEntityReferenceLinkSerializer(this).WriteEntityReferenceLink(link);
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
            new ODataAtomEntityReferenceLinkSerializer(this).WriteEntityReferenceLinks(links);
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
            new ODataAtomSerializer(this).WriteTopLevelError(error, includeDebugInformation);
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
            ODataAtomWriterUtils.WriteError(this.xmlWriter, error, includeDebugInformation, base.MessageWriterSettings.MessageQuotas.MaxNestingDepth);
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
            new ODataAtomPropertyAndValueSerializer(this).WriteTopLevelProperty(property);
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
            new ODataAtomServiceDocumentSerializer(this).WriteServiceDocument(defaultWorkspace);
        }

        internal System.Xml.XmlWriter XmlWriter
        {
            get
            {
                return this.xmlWriter;
            }
        }
    }
}

