namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class ODataMessageReader : IDisposable
    {
        private string batchBoundary;
        private Encoding encoding;
        private ODataFormat format;
        private ODataInputContext inputContext;
        private bool isDisposed;
        private Microsoft.Data.OData.MediaTypeResolver mediaTypeResolver;
        private readonly ODataMessage message;
        private readonly IEdmModel model;
        private ODataPayloadKind readerPayloadKind;
        private readonly bool readingResponse;
        private bool readMethodCalled;
        private readonly ODataMessageReaderSettings settings;
        private readonly IODataUrlResolver urlResolver;
        private readonly ODataVersion version;

        public ODataMessageReader(IODataRequestMessage requestMessage) : this(requestMessage, new ODataMessageReaderSettings())
        {
        }

        public ODataMessageReader(IODataResponseMessage responseMessage) : this(responseMessage, new ODataMessageReaderSettings())
        {
        }

        public ODataMessageReader(IODataRequestMessage requestMessage, ODataMessageReaderSettings settings) : this(requestMessage, settings, null)
        {
        }

        public ODataMessageReader(IODataResponseMessage responseMessage, ODataMessageReaderSettings settings) : this(responseMessage, settings, null)
        {
        }

        public ODataMessageReader(IODataRequestMessage requestMessage, ODataMessageReaderSettings settings, IEdmModel model)
        {
            this.readerPayloadKind = ODataPayloadKind.Unsupported;
            ExceptionUtils.CheckArgumentNotNull<IODataRequestMessage>(requestMessage, "requestMessage");
            this.settings = (settings == null) ? new ODataMessageReaderSettings() : new ODataMessageReaderSettings(settings);
            ReaderValidationUtils.ValidateMessageReaderSettings(this.settings, false);
            this.readingResponse = false;
            this.message = new ODataRequestMessage(requestMessage, false, this.settings.DisableMessageStreamDisposal, this.settings.MessageQuotas.MaxReceivedMessageSize);
            this.urlResolver = requestMessage as IODataUrlResolver;
            this.version = ODataUtilsInternal.GetDataServiceVersion(this.message, this.settings.MaxProtocolVersion);
            this.model = model ?? EdmCoreModel.Instance;
            ODataVersionChecker.CheckVersionSupported(this.version, this.settings);
        }

        public ODataMessageReader(IODataResponseMessage responseMessage, ODataMessageReaderSettings settings, IEdmModel model)
        {
            this.readerPayloadKind = ODataPayloadKind.Unsupported;
            ExceptionUtils.CheckArgumentNotNull<IODataResponseMessage>(responseMessage, "responseMessage");
            this.settings = (settings == null) ? new ODataMessageReaderSettings() : new ODataMessageReaderSettings(settings);
            ReaderValidationUtils.ValidateMessageReaderSettings(this.settings, true);
            this.readingResponse = true;
            this.message = new ODataResponseMessage(responseMessage, false, this.settings.DisableMessageStreamDisposal, this.settings.MessageQuotas.MaxReceivedMessageSize);
            this.urlResolver = responseMessage as IODataUrlResolver;
            this.version = ODataUtilsInternal.GetDataServiceVersion(this.message, this.settings.MaxProtocolVersion);
            this.model = model ?? EdmCoreModel.Instance;
            ODataVersionChecker.CheckVersionSupported(this.version, this.settings);
        }

        private int ComparePayloadKindDetectionResult(ODataPayloadKindDetectionResult first, ODataPayloadKindDetectionResult second)
        {
            ODataPayloadKind payloadKind = first.PayloadKind;
            ODataPayloadKind kind2 = second.PayloadKind;
            if (payloadKind == kind2)
            {
                return 0;
            }
            if (first.PayloadKind >= second.PayloadKind)
            {
                return 1;
            }
            return -1;
        }

        public ODataBatchReader CreateODataBatchReader()
        {
            this.VerifyCanCreateODataBatchReader();
            return this.ReadFromInput<ODataBatchReader>(context => context.CreateBatchReader(this.batchBoundary), new ODataPayloadKind[] { ODataPayloadKind.Batch });
        }

        public Task<ODataBatchReader> CreateODataBatchReaderAsync()
        {
            this.VerifyCanCreateODataBatchReader();
            return this.ReadFromInputAsync<ODataBatchReader>(context => context.CreateBatchReaderAsync(this.batchBoundary), new ODataPayloadKind[] { ODataPayloadKind.Batch });
        }

        public ODataCollectionReader CreateODataCollectionReader()
        {
            return this.CreateODataCollectionReader(null);
        }

        public ODataCollectionReader CreateODataCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyCanCreateODataCollectionReader(expectedItemTypeReference);
            return this.ReadFromInput<ODataCollectionReader>(context => context.CreateCollectionReader(expectedItemTypeReference), new ODataPayloadKind[] { ODataPayloadKind.Collection });
        }

        public Task<ODataCollectionReader> CreateODataCollectionReaderAsync()
        {
            return this.CreateODataCollectionReaderAsync(null);
        }

        public Task<ODataCollectionReader> CreateODataCollectionReaderAsync(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyCanCreateODataCollectionReader(expectedItemTypeReference);
            return this.ReadFromInputAsync<ODataCollectionReader>(context => context.CreateCollectionReaderAsync(expectedItemTypeReference), new ODataPayloadKind[] { ODataPayloadKind.Collection });
        }

        public ODataReader CreateODataEntryReader()
        {
            return this.CreateODataEntryReader(null);
        }

        public ODataReader CreateODataEntryReader(IEdmEntityType expectedEntityType)
        {
            this.VerifyCanCreateODataEntryReader(expectedEntityType);
            return this.ReadFromInput<ODataReader>(context => context.CreateEntryReader(expectedEntityType), new ODataPayloadKind[] { ODataPayloadKind.Entry });
        }

        public Task<ODataReader> CreateODataEntryReaderAsync()
        {
            return this.CreateODataEntryReaderAsync(null);
        }

        public Task<ODataReader> CreateODataEntryReaderAsync(IEdmEntityType expectedEntityType)
        {
            this.VerifyCanCreateODataEntryReader(expectedEntityType);
            return this.ReadFromInputAsync<ODataReader>(context => context.CreateEntryReaderAsync(expectedEntityType), new ODataPayloadKind[] { ODataPayloadKind.Entry });
        }

        public ODataReader CreateODataFeedReader()
        {
            return this.CreateODataFeedReader(null);
        }

        public ODataReader CreateODataFeedReader(IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyCanCreateODataFeedReader(expectedBaseEntityType);
            return this.ReadFromInput<ODataReader>(context => context.CreateFeedReader(expectedBaseEntityType), new ODataPayloadKind[1]);
        }

        public Task<ODataReader> CreateODataFeedReaderAsync()
        {
            return this.CreateODataFeedReaderAsync(null);
        }

        public Task<ODataReader> CreateODataFeedReaderAsync(IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyCanCreateODataFeedReader(expectedBaseEntityType);
            return this.ReadFromInputAsync<ODataReader>(context => context.CreateFeedReaderAsync(expectedBaseEntityType), new ODataPayloadKind[1]);
        }

        public ODataParameterReader CreateODataParameterReader(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterReader(functionImport);
            return this.ReadFromInput<ODataParameterReader>(context => context.CreateParameterReader(functionImport), new ODataPayloadKind[] { ODataPayloadKind.Parameter });
        }

        public Task<ODataParameterReader> CreateODataParameterReaderAsync(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterReader(functionImport);
            return this.ReadFromInputAsync<ODataParameterReader>(context => context.CreateParameterReaderAsync(functionImport), new ODataPayloadKind[] { ODataPayloadKind.Parameter });
        }

        public IEnumerable<ODataPayloadKindDetectionResult> DetectPayloadKind()
        {
            IEnumerable<ODataPayloadKindDetectionResult> enumerable;
            MediaType type;
            if (this.settings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_PayloadKindDetectionInServerMode);
            }
            if (this.TryGetSinglePayloadKindResultFromContentType(out enumerable, out type))
            {
                return enumerable;
            }
            List<ODataPayloadKindDetectionResult> list = new List<ODataPayloadKindDetectionResult>();
            try
            {
                foreach (IGrouping<ODataFormat, ODataPayloadKindDetectionResult> grouping in from kvp in enumerable group kvp by kvp.Format)
                {
                    ODataPayloadKindDetectionInfo detectionInfo = new ODataPayloadKindDetectionInfo(type, this.settings, this.model, from pkg in grouping select pkg.PayloadKind);
                    IEnumerable<ODataPayloadKind> enumerable3 = this.readingResponse ? grouping.Key.DetectPayloadKind((IODataResponseMessage) this.message, detectionInfo) : grouping.Key.DetectPayloadKind((IODataRequestMessage) this.message, detectionInfo);
                    if (enumerable3 != null)
                    {
                        using (IEnumerator<ODataPayloadKind> enumerator2 = enumerable3.GetEnumerator())
                        {
                            Func<ODataPayloadKindDetectionResult, bool> predicate = null;
                            ODataPayloadKind kind;
                            while (enumerator2.MoveNext())
                            {
                                kind = enumerator2.Current;
                                if (predicate == null)
                                {
                                    predicate = pk => pk.PayloadKind == kind;
                                }
                                if (enumerable.Any<ODataPayloadKindDetectionResult>(predicate))
                                {
                                    list.Add(new ODataPayloadKindDetectionResult(kind, grouping.Key));
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.message.UseBufferingReadStream = false;
                this.message.BufferingReadStream.StopBuffering();
            }
            list.Sort(new Comparison<ODataPayloadKindDetectionResult>(this.ComparePayloadKindDetectionResult));
            return list;
        }

        public Task<IEnumerable<ODataPayloadKindDetectionResult>> DetectPayloadKindAsync()
        {
            IEnumerable<ODataPayloadKindDetectionResult> enumerable;
            MediaType type;
            if (this.settings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_PayloadKindDetectionInServerMode);
            }
            if (this.TryGetSinglePayloadKindResultFromContentType(out enumerable, out type))
            {
                return TaskUtils.GetCompletedTask<IEnumerable<ODataPayloadKindDetectionResult>>(enumerable);
            }
            List<ODataPayloadKindDetectionResult> detectedPayloadKinds = new List<ODataPayloadKindDetectionResult>();
            return Task.Factory.Iterate(this.GetPayloadKindDetectionTasks(type, enumerable, detectedPayloadKinds)).FollowAlwaysWith(delegate (Task t) {
                this.message.UseBufferingReadStream = false;
                this.message.BufferingReadStream.StopBuffering();
            }).FollowOnSuccessWith<IEnumerable<ODataPayloadKindDetectionResult>>(delegate (Task t) {
                detectedPayloadKinds.Sort(new Comparison<ODataPayloadKindDetectionResult>(this.ComparePayloadKindDetectionResult));
                return detectedPayloadKinds;
            });
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
                    if (this.inputContext != null)
                    {
                        this.inputContext.Dispose();
                    }
                }
                finally
                {
                    this.inputContext = null;
                }
                if (!this.settings.DisableMessageStreamDisposal && (this.message.BufferingReadStream != null))
                {
                    this.message.BufferingReadStream.Dispose();
                }
            }
        }

        private string GetContentTypeHeader()
        {
            string header = this.message.GetHeader("Content-Type");
            header = (header == null) ? null : header.Trim();
            if (string.IsNullOrEmpty(header))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_NoneOrEmptyContentTypeHeader);
            }
            return header;
        }

        internal ODataFormat GetFormat()
        {
            if (this.format == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_GetFormatCalledBeforeReadingStarted);
            }
            return this.format;
        }

        private IEnumerable<Task> GetPayloadKindDetectionTasks(MediaType contentType, IEnumerable<ODataPayloadKindDetectionResult> payloadKindsFromContentType, List<ODataPayloadKindDetectionResult> detectionResults)
        {
            IEnumerable<IGrouping<ODataFormat, ODataPayloadKindDetectionResult>> iteratorVariable0 = from kvp in payloadKindsFromContentType group kvp by kvp.Format;
            using (IEnumerator<IGrouping<ODataFormat, ODataPayloadKindDetectionResult>> iteratorVariable6 = iteratorVariable0.GetEnumerator())
            {
                Action<Task<IEnumerable<ODataPayloadKind>>> operation = null;
                IGrouping<ODataFormat, ODataPayloadKindDetectionResult> payloadKindGroup;
                while (iteratorVariable6.MoveNext())
                {
                    payloadKindGroup = iteratorVariable6.Current;
                    ODataPayloadKindDetectionInfo detectionInfo = new ODataPayloadKindDetectionInfo(contentType, this.settings, this.model, from pkg in payloadKindGroup select pkg.PayloadKind);
                    Task<IEnumerable<ODataPayloadKind>> antecedentTask = this.readingResponse ? payloadKindGroup.Key.DetectPayloadKindAsync((IODataResponseMessageAsync) this.message, detectionInfo) : payloadKindGroup.Key.DetectPayloadKindAsync((IODataRequestMessageAsync) this.message, detectionInfo);
                    if (operation == null)
                    {
                        operation = delegate (Task<IEnumerable<ODataPayloadKind>> t) {
                            IEnumerable<ODataPayloadKind> result = t.Result;
                            if (result != null)
                            {
                                using (IEnumerator<ODataPayloadKind> enumerator = result.GetEnumerator())
                                {
                                    Func<ODataPayloadKindDetectionResult, bool> predicate = null;
                                    while (enumerator.MoveNext())
                                    {
                                        ODataPayloadKind kind = enumerator.Current;
                                        if (predicate == null)
                                        {
                                            predicate = pk => pk.PayloadKind == kind;
                                        }
                                        if (payloadKindsFromContentType.Any<ODataPayloadKindDetectionResult>(predicate))
                                        {
                                            detectionResults.Add(new ODataPayloadKindDetectionResult(kind, payloadKindGroup.Key));
                                        }
                                    }
                                }
                            }
                        };
                    }
                    yield return antecedentTask.FollowOnSuccessWith<IEnumerable<ODataPayloadKind>>(operation);
                }
            }
        }

        private void ProcessContentType(params ODataPayloadKind[] payloadKinds)
        {
            MediaType type;
            string contentTypeHeader = this.GetContentTypeHeader();
            this.format = MediaTypeUtils.GetFormatFromContentType(contentTypeHeader, payloadKinds, this.MediaTypeResolver, out type, out this.encoding, out this.readerPayloadKind, out this.batchBoundary);
        }

        public ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            this.VerifyCanReadEntityReferenceLink();
            return this.ReadFromInput<ODataEntityReferenceLink>(context => context.ReadEntityReferenceLink(), new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLink });
        }

        public Task<ODataEntityReferenceLink> ReadEntityReferenceLinkAsync()
        {
            this.VerifyCanReadEntityReferenceLink();
            return this.ReadFromInputAsync<ODataEntityReferenceLink>(context => context.ReadEntityReferenceLinkAsync(), new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLink });
        }

        public ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            this.VerifyCanReadEntityReferenceLinks();
            return this.ReadFromInput<ODataEntityReferenceLinks>(context => context.ReadEntityReferenceLinks(), new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLinks });
        }

        public Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync()
        {
            this.VerifyCanReadEntityReferenceLinks();
            return this.ReadFromInputAsync<ODataEntityReferenceLinks>(context => context.ReadEntityReferenceLinksAsync(), new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLinks });
        }

        public ODataError ReadError()
        {
            this.VerifyCanReadError();
            return this.ReadFromInput<ODataError>(context => context.ReadError(), new ODataPayloadKind[] { ODataPayloadKind.Error });
        }

        public Task<ODataError> ReadErrorAsync()
        {
            this.VerifyCanReadError();
            return this.ReadFromInputAsync<ODataError>(context => context.ReadErrorAsync(), new ODataPayloadKind[] { ODataPayloadKind.Error });
        }

        private T ReadFromInput<T>(Func<ODataInputContext, T> readFunc, params ODataPayloadKind[] payloadKinds) where T: class
        {
            this.ProcessContentType(payloadKinds);
            this.inputContext = ODataInputContext.CreateInputContext(this.format, this.readerPayloadKind, this.message, this.encoding, this.settings, this.version, this.readingResponse, this.model, this.urlResolver);
            return readFunc(this.inputContext);
        }

        private Task<T> ReadFromInputAsync<T>(Func<ODataInputContext, Task<T>> readFunc, params ODataPayloadKind[] payloadKinds) where T: class
        {
            this.ProcessContentType(payloadKinds);
            return ODataInputContext.CreateInputContextAsync(this.format, this.readerPayloadKind, this.message, this.encoding, this.settings, this.version, this.readingResponse, this.model, this.urlResolver).FollowOnSuccessWithTask<ODataInputContext, T>(delegate (Task<ODataInputContext> createInputContextTask) {
                this.inputContext = createInputContextTask.Result;
                return readFunc(this.inputContext);
            });
        }

        public IEdmModel ReadMetadataDocument()
        {
            this.VerifyCanReadMetadataDocument();
            return this.ReadFromInput<IEdmModel>(context => context.ReadMetadataDocument(), new ODataPayloadKind[] { ODataPayloadKind.MetadataDocument });
        }

        public ODataProperty ReadProperty()
        {
            return this.ReadProperty(null);
        }

        public ODataProperty ReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyCanReadProperty(expectedPropertyTypeReference);
            return this.ReadFromInput<ODataProperty>(context => context.ReadProperty(expectedPropertyTypeReference), new ODataPayloadKind[] { ODataPayloadKind.Property });
        }

        public Task<ODataProperty> ReadPropertyAsync()
        {
            return this.ReadPropertyAsync(null);
        }

        public Task<ODataProperty> ReadPropertyAsync(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyCanReadProperty(expectedPropertyTypeReference);
            return this.ReadFromInputAsync<ODataProperty>(context => context.ReadPropertyAsync(expectedPropertyTypeReference), new ODataPayloadKind[] { ODataPayloadKind.Property });
        }

        public ODataWorkspace ReadServiceDocument()
        {
            this.VerifyCanReadServiceDocument();
            return this.ReadFromInput<ODataWorkspace>(context => context.ReadServiceDocument(), new ODataPayloadKind[] { ODataPayloadKind.ServiceDocument });
        }

        public Task<ODataWorkspace> ReadServiceDocumentAsync()
        {
            this.VerifyCanReadServiceDocument();
            return this.ReadFromInputAsync<ODataWorkspace>(context => context.ReadServiceDocumentAsync(), new ODataPayloadKind[] { ODataPayloadKind.ServiceDocument });
        }

        public object ReadValue(IEdmTypeReference expectedTypeReference)
        {
            ODataPayloadKind[] payloadKinds = this.VerifyCanReadValue(expectedTypeReference);
            return this.ReadFromInput<object>(context => context.ReadValue((IEdmPrimitiveTypeReference) expectedTypeReference), payloadKinds);
        }

        public Task<object> ReadValueAsync(IEdmTypeReference expectedTypeReference)
        {
            ODataPayloadKind[] payloadKinds = this.VerifyCanReadValue(expectedTypeReference);
            return this.ReadFromInputAsync<object>(context => context.ReadValueAsync((IEdmPrimitiveTypeReference) expectedTypeReference), payloadKinds);
        }

        private bool TryGetSinglePayloadKindResultFromContentType(out IEnumerable<ODataPayloadKindDetectionResult> payloadKindResults, out MediaType contentType)
        {
            if (this.message.UseBufferingReadStream == true)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_DetectPayloadKindMultipleTimes);
            }
            IList<ODataPayloadKindDetectionResult> list = MediaTypeUtils.GetPayloadKindsForContentType(this.GetContentTypeHeader(), this.MediaTypeResolver, out contentType);
            payloadKindResults = from r in list
                where ODataUtilsInternal.IsPayloadKindSupported(r.PayloadKind, !this.readingResponse)
                select r;
            if (payloadKindResults.Count<ODataPayloadKindDetectionResult>() > 1)
            {
                this.message.UseBufferingReadStream = true;
                return false;
            }
            return true;
        }

        private void VerifyCanCreateODataBatchReader()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
        }

        private void VerifyCanCreateODataCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (expectedItemTypeReference != null)
            {
                if (!this.model.IsUserModel())
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedItemTypeReference"), "expectedItemTypeReference");
                }
                if (!expectedItemTypeReference.IsODataPrimitiveTypeKind() && (expectedItemTypeReference.TypeKind() != EdmTypeKind.Complex))
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedCollectionTypeWrongKind(expectedItemTypeReference.TypeKind().ToString()), "expectedItemTypeReference");
                }
            }
        }

        private void VerifyCanCreateODataEntryReader(IEdmEntityType expectedEntityType)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if ((expectedEntityType != null) && !this.model.IsUserModel())
            {
                throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedEntityType"), "expectedEntityType");
            }
        }

        private void VerifyCanCreateODataFeedReader(IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if ((expectedBaseEntityType != null) && !this.model.IsUserModel())
            {
                throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedBaseEntityType"), "expectedBaseEntityType");
            }
        }

        private void VerifyCanCreateODataParameterReader(IEdmFunctionImport functionImport)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (this.readingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_ParameterPayloadInResponse);
            }
            ODataVersionChecker.CheckParameterPayload(this.version);
            if ((functionImport != null) && !this.model.IsUserModel())
            {
                throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_FunctionImportSpecifiedWithoutMetadata("functionImport"), "functionImport");
            }
        }

        private void VerifyCanReadEntityReferenceLink()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
        }

        private void VerifyCanReadEntityReferenceLinks()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (!this.readingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_EntityReferenceLinksInRequestNotAllowed);
            }
        }

        private void VerifyCanReadError()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (!this.readingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_ErrorPayloadInRequest);
            }
        }

        private void VerifyCanReadMetadataDocument()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (!this.readingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_MetadataDocumentInRequest);
            }
        }

        private void VerifyCanReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (expectedPropertyTypeReference != null)
            {
                if (!this.model.IsUserModel())
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedPropertyTypeReference"), "expectedPropertyTypeReference");
                }
                if (expectedPropertyTypeReference.IsODataEntityTypeKind())
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedPropertyTypeEntityKind, "expectedPropertyTypeReference");
                }
                if (expectedPropertyTypeReference.IsStream())
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedPropertyTypeStream, "expectedPropertyTypeReference");
                }
            }
        }

        private void VerifyCanReadServiceDocument()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (!this.readingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_ServiceDocumentInRequest);
            }
        }

        private ODataPayloadKind[] VerifyCanReadValue(IEdmTypeReference expectedTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();
            if (expectedTypeReference != null)
            {
                if (!expectedTypeReference.IsODataPrimitiveTypeKind())
                {
                    throw new ArgumentException(Microsoft.Data.OData.Strings.ODataMessageReader_ExpectedValueTypeWrongKind(expectedTypeReference.TypeKind().ToString()), "expectedTypeReference");
                }
                if (expectedTypeReference.IsBinary())
                {
                    return new ODataPayloadKind[] { ODataPayloadKind.BinaryValue };
                }
                return new ODataPayloadKind[] { ODataPayloadKind.Value };
            }
            return new ODataPayloadKind[] { ODataPayloadKind.Value, ODataPayloadKind.BinaryValue };
        }

        private void VerifyNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        private void VerifyReaderNotDisposedAndNotUsed()
        {
            this.VerifyNotDisposed();
            if (this.readMethodCalled)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_ReaderAlreadyUsed);
            }
            if ((this.message.BufferingReadStream != null) && this.message.BufferingReadStream.IsBuffering)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_PayloadKindDetectionRunning);
            }
            this.readMethodCalled = true;
        }

        private Microsoft.Data.OData.MediaTypeResolver MediaTypeResolver
        {
            get
            {
                if (this.mediaTypeResolver == null)
                {
                    bool shouldPlainAppJsonImplyVerboseJson = this.version < ODataVersion.V3;
                    bool shouldAppXmlAndAppAtomXmlBeInterchangeable = (this.version < ODataVersion.V3) && (this.settings.ReaderBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient);
                    this.mediaTypeResolver = new Microsoft.Data.OData.MediaTypeResolver(shouldPlainAppJsonImplyVerboseJson, shouldAppXmlAndAppAtomXmlBeInterchangeable);
                }
                return this.mediaTypeResolver;
            }
        }

        
    }
}

