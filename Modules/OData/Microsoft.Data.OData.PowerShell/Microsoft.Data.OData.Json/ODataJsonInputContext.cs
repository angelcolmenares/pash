namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataJsonInputContext : ODataInputContext
    {
        private BufferingJsonReader jsonReader;
        private TextReader textReader;

        internal ODataJsonInputContext(ODataFormat format, TextReader reader, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : base(format, messageReaderSettings, version, readingResponse, synchronous, model, urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFormat>(format, "format");
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(messageReaderSettings, "messageReaderSettings");
            try
            {
                this.textReader = reader;
                this.jsonReader = new BufferingJsonReader(this.textReader, base.UseServerFormatBehavior, messageReaderSettings.MessageQuotas.MaxNestingDepth);
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception) && (reader != null))
                {
                    reader.Dispose();
                }
                throw;
            }
        }

        internal ODataJsonInputContext(ODataFormat format, Stream messageStream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, bool synchronous, IEdmModel model, IODataUrlResolver urlResolver) : this(format, CreateTextReaderForMessageStreamConstructor(messageStream, encoding), messageReaderSettings, version, readingResponse, synchronous, model, urlResolver)
        {
        }

        internal static ODataInputContext Create(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return new ODataJsonInputContext(format, message.GetStream(), encoding, messageReaderSettings, version, readingResponse, true, model, urlResolver);
        }

        internal static Task<ODataInputContext> CreateAsync(ODataFormat format, ODataMessage message, Encoding encoding, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, bool readingResponse, IEdmModel model, IODataUrlResolver urlResolver)
        {
            return message.GetStreamAsync().FollowOnSuccessWith<Stream, ODataInputContext>(streamTask => new ODataJsonInputContext(format, streamTask.Result, encoding, messageReaderSettings, version, readingResponse, false, model, urlResolver));
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
            return new ODataJsonCollectionReader(this, expectedItemTypeReference, null);
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
            return new ODataJsonReader(this, expectedEntityType, false, null);
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
            return new ODataJsonReader(this, expectedBaseEntityType, true, null);
        }

        internal override ODataParameterReader CreateParameterReader(IEdmFunctionImport functionImport)
        {
            VerifyCanCreateParameterReader(functionImport);
            return this.CreateParameterReaderImplementation(functionImport);
        }

        internal override Task<ODataParameterReader> CreateParameterReaderAsync(IEdmFunctionImport functionImport)
        {
            VerifyCanCreateParameterReader(functionImport);
            return TaskUtils.GetTaskForSynchronousOperation<ODataParameterReader>(() => this.CreateParameterReaderImplementation(functionImport));
        }

        private ODataParameterReader CreateParameterReaderImplementation(IEdmFunctionImport functionImport)
        {
            return new ODataJsonParameterReader(this, functionImport);
        }

        private static TextReader CreateTextReaderForMessageStreamConstructor(Stream messageStream, Encoding encoding)
        {
            TextReader reader;
            try
            {
                reader = new StreamReader(messageStream, encoding);
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception) && (messageStream != null))
                {
                    messageStream.Dispose();
                }
                throw;
            }
            return reader;
        }

        internal IEnumerable<ODataPayloadKind> DetectPayloadKind()
        {
            ODataJsonPayloadKindDetectionDeserializer deserializer = new ODataJsonPayloadKindDetectionDeserializer(this);
            return deserializer.DetectPayloadKind();
        }

        protected override void DisposeImplementation()
        {
            try
            {
                if (this.textReader != null)
                {
                    this.textReader.Dispose();
                }
            }
            finally
            {
                this.textReader = null;
                this.jsonReader = null;
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
            ODataJsonEntityReferenceLinkDeserializer deserializer = new ODataJsonEntityReferenceLinkDeserializer(this);
            return deserializer.ReadEntityReferenceLink();
        }

        internal override ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            return this.ReadEntityReferenceLinksImplementation();
        }

        internal override Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataEntityReferenceLinks>(() => this.ReadEntityReferenceLinksImplementation());
        }

        private ODataEntityReferenceLinks ReadEntityReferenceLinksImplementation()
        {
            ODataJsonEntityReferenceLinkDeserializer deserializer = new ODataJsonEntityReferenceLinkDeserializer(this);
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
            ODataJsonErrorDeserializer deserializer = new ODataJsonErrorDeserializer(this);
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
            ODataJsonPropertyAndValueDeserializer deserializer = new ODataJsonPropertyAndValueDeserializer(this);
            return deserializer.ReadTopLevelProperty(expectedPropertyTypeReference);
        }

        internal override ODataWorkspace ReadServiceDocument()
        {
            return this.ReadServiceDocumentImplementation();
        }

        internal override Task<ODataWorkspace> ReadServiceDocumentAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<ODataWorkspace>(new Func<ODataWorkspace>(this.ReadServiceDocumentImplementation));
        }

        private ODataWorkspace ReadServiceDocumentImplementation()
        {
            ODataJsonServiceDocumentDeserializer deserializer = new ODataJsonServiceDocumentDeserializer(this);
            return deserializer.ReadServiceDocument();
        }

        private static void VerifyCanCreateParameterReader(IEdmFunctionImport functionImport)
        {
            if (functionImport == null)
            {
                throw new ArgumentNullException("functionImport", Microsoft.Data.OData.Strings.ODataJsonInputContext_FunctionImportCannotBeNullForCreateParameterReader("functionImport"));
            }
        }

        internal BufferingJsonReader JsonReader
        {
            get
            {
                return this.jsonReader;
            }
        }
    }
}

