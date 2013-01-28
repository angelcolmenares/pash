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

    internal sealed class ODataAtomInputContext : ODataInputContext
    {
        private System.Xml.XmlReader baseXmlReader;
        private Stack<BufferingXmlReader> xmlCustomizationReaders;
        private BufferingXmlReader xmlReader;

        internal ODataAtomInputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageReaderSettings, version, readingResponse, synchronous, model, urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "messageReaderSettings");
            try
            {
                this.baseXmlReader = ODataAtomReaderUtils.CreateXmlReader(messageStream, encoding, messageReaderSettings);
                this.xmlReader = new BufferingXmlReader(this.baseXmlReader, null, messageReaderSettings.BaseUri, base.UseServerFormatBehavior && (base.Version < ODataVersion.V3), messageReaderSettings.MessageQuotas.MaxNestingDepth, messageReaderSettings.ReaderBehavior.ODataNamespace);
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

        internal static ODataInputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataAtomInputContext(format, message.GetStream(), encoding, messageReaderSettings, version, readingResponse, true, model, urlResolver);
        }

        internal static Task<ODataInputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<Stream, ODataInputContext>(streamTask => new ODataAtomInputContext(format, streamTask.Result, encoding, messageReaderSettings, version, readingResponse, false, model, urlResolver));
        }

        internal override ODataCollectionReader CreateCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            return this.CreateCollectionReaderImplementation(expectedItemTypeReference);
        }

        internal override Task<ODataCollectionReader> CreateCollectionReaderAsync(IEdmTypeReference expectedItemTypeReference)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataCollectionReader>(() => this.CreateCollectionReaderImplementation(expectedItemTypeReference));
        }

        private ODataCollectionReader CreateCollectionReaderImplementation(IEdmTypeReference expectedItemTypeReference)
        {
            return new ODataAtomCollectionReader(this, expectedItemTypeReference);
        }

        internal override ODataReader CreateEntryReader(IEdmEntityType expectedEntityType)
        {
            return this.CreateEntryReaderImplementation(expectedEntityType);
        }

        internal override Task<ODataReader> CreateEntryReaderAsync(IEdmEntityType expectedEntityType)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataReader>(() => this.CreateEntryReaderImplementation(expectedEntityType));
        }

        private ODataReader CreateEntryReaderImplementation(IEdmEntityType expectedEntityType)
        {
            return new ODataAtomReader(this, expectedEntityType, false);
        }

        internal override ODataReader CreateFeedReader(IEdmEntityType expectedBaseEntityType)
        {
            return this.CreateFeedReaderImplementation(expectedBaseEntityType);
        }

        internal override Task<ODataReader> CreateFeedReaderAsync(IEdmEntityType expectedBaseEntityType)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataReader>(() => this.CreateFeedReaderImplementation(expectedBaseEntityType));
        }

        private ODataReader CreateFeedReaderImplementation(IEdmEntityType expectedBaseEntityType)
        {
            return new ODataAtomReader(this, expectedBaseEntityType, true);
        }

        internal IEnumerable<ODataPayloadKind> DetectPayloadKind(ODataPayloadKindDetectionInfo detectionInfo)
        {
            ODataAtomPayloadKindDetectionDeserializer deserializer = new ODataAtomPayloadKindDetectionDeserializer(this);
            return deserializer.DetectPayloadKind(detectionInfo);
        }

        protected override void DisposeImplementation()
        {
            try
            {
                if (this.baseXmlReader != null)
                {
                    this.baseXmlReader.Dispose();
                }
            }
            finally
            {
                this.baseXmlReader = null;
                this.xmlReader = null;
            }
        }

        internal void InitializeReaderCustomization()
        {
            this.xmlCustomizationReaders = new Stack<BufferingXmlReader>();
            this.xmlCustomizationReaders.Push(this.xmlReader);
        }

        internal BufferingXmlReader PopCustomReader()
        {
            BufferingXmlReader reader = this.xmlCustomizationReaders.Pop();
            this.xmlReader = this.xmlCustomizationReaders.Peek();
            return reader;
        }

        internal void PushCustomReader(System.Xml.XmlReader customXmlReader, Uri xmlBaseUri)
        {
            if (!object.ReferenceEquals(this.xmlReader, customXmlReader))
            {
                BufferingXmlReader item = new BufferingXmlReader(customXmlReader, xmlBaseUri, base.MessageReaderSettings.BaseUri, false, base.MessageReaderSettings.MessageQuotas.MaxNestingDepth, base.MessageReaderSettings.ReaderBehavior.ODataNamespace);
                this.xmlCustomizationReaders.Push(item);
                this.xmlReader = item;
            }
            else
            {
                this.xmlCustomizationReaders.Push(this.xmlReader);
            }
        }

        internal override ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            return this.ReadEntityReferenceLinkImplementation();
        }

        internal override Task<ODataEntityReferenceLink> ReadEntityReferenceLinkAsync()
        {
            return TaskUtils.GetCompletedTask<ODataEntityReferenceLink>(this.ReadEntityReferenceLinkImplementation());
        }

        private ODataEntityReferenceLink ReadEntityReferenceLinkImplementation()
        {
            ODataAtomEntityReferenceLinkDeserializer deserializer = new ODataAtomEntityReferenceLinkDeserializer(this);
            return deserializer.ReadEntityReferenceLink();
        }

        internal override ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            return this.ReadEntityReferenceLinksImplementation();
        }

        internal override Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync()
        {
            return TaskUtils.GetCompletedTask<ODataEntityReferenceLinks>(this.ReadEntityReferenceLinksImplementation());
        }

        private ODataEntityReferenceLinks ReadEntityReferenceLinksImplementation()
        {
            ODataAtomEntityReferenceLinkDeserializer deserializer = new ODataAtomEntityReferenceLinkDeserializer(this);
            return deserializer.ReadEntityReferenceLinks();
        }

        internal override ODataError ReadError()
        {
            return this.ReadErrorImplementation();
        }

        internal override Task<ODataError> ReadErrorAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataError>(() => this.ReadErrorImplementation());
        }

        private ODataError ReadErrorImplementation()
        {
            ODataAtomErrorDeserializer deserializer = new ODataAtomErrorDeserializer(this);
            return deserializer.ReadTopLevelError();
        }

        internal override ODataProperty ReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            return this.ReadPropertyImplementation(expectedPropertyTypeReference);
        }

        internal override Task<ODataProperty> ReadPropertyAsync(IEdmTypeReference expectedPropertyTypeReference)
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataProperty>(() => this.ReadPropertyImplementation(expectedPropertyTypeReference));
        }

        private ODataProperty ReadPropertyImplementation(IEdmTypeReference expectedPropertyTypeReference)
        {
            ODataAtomPropertyAndValueDeserializer deserializer = new ODataAtomPropertyAndValueDeserializer(this);
            return deserializer.ReadTopLevelProperty(expectedPropertyTypeReference);
        }

        internal override ODataWorkspace ReadServiceDocument()
        {
            return this.ReadServiceDocumentImplementation();
        }

        internal override Task<ODataWorkspace> ReadServiceDocumentAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataWorkspace>(() => this.ReadServiceDocumentImplementation());
        }

        private ODataWorkspace ReadServiceDocumentImplementation()
        {
            ODataAtomServiceDocumentDeserializer deserializer = new ODataAtomServiceDocumentDeserializer(this);
            return deserializer.ReadServiceDocument();
        }

        internal BufferingXmlReader XmlReader
        {
            get
            {
                return this.xmlReader;
            }
        }
    }
}

