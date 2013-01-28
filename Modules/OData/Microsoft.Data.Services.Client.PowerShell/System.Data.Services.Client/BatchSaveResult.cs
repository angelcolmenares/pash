namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class BatchSaveResult : BaseSaveResult
    {
        private ODataMessageReader batchMessageReader;
        private ODataBatchWriter batchWriter;
        private CurrentOperationResponse currentOperationResponse;
        private readonly DataServiceRequest[] Queries;
        private Stream responseStream;
        private byte[] streamCopyBuffer;
        private const int StreamCopyBufferSize = 0xfa0;

        internal BatchSaveResult(DataServiceContext context, string method, DataServiceRequest[] queries, SaveChangesOptions options, AsyncCallback callback, object state) : base(context, method, queries, options, callback, state)
        {
            this.Queries = queries;
            this.streamCopyBuffer = new byte[0xfa0];
        }

        internal void BatchBeginRequest()
        {
            BaseAsyncResult.PerRequest pereq = null;
            try
            {
                ODataRequestMessageWrapper request = this.GenerateBatchRequest();
                if (request != null)
                {
                    request.SetContentLengthHeader();
                    base.perRequest = pereq = new BaseAsyncResult.PerRequest();
                    pereq.Request = request;
                    pereq.RequestContentStream = new BaseAsyncResult.ContentStream(request.CachedRequestStream, true);
                    BaseAsyncResult.AsyncStateBag state = new BaseAsyncResult.AsyncStateBag(pereq, (DataServiceContext) base.Source);
                    this.responseStream = new MemoryStream();
                    IAsyncResult result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetRequestStream), request, new AsyncCallback(this.AsyncEndGetRequestStream), state);
                    pereq.SetRequestCompletedSynchronously(result.CompletedSynchronously);
                }
            }
            catch (Exception exception)
            {
                base.HandleFailure(pereq, exception);
                throw;
            }
            finally
            {
                this.HandleCompleted(pereq);
            }
        }

        internal void BatchRequest()
        {
            ODataRequestMessageWrapper request = this.GenerateBatchRequest();
            if (request != null)
            {
                request.SetContentLengthHeader();
                MemoryStream cachedRequestStream = request.CachedRequestStream;
                DataServiceContext source = (DataServiceContext) base.Source;
                using (Stream stream2 = WebUtil.GetRequestStream(request, source))
                {
                    byte[] buffer = cachedRequestStream.GetBuffer();
                    int position = (int) cachedRequestStream.Position;
                    int count = ((int) cachedRequestStream.Length) - position;
                    stream2.Write(buffer, position, count);
                }
                try
                {
                    base.batchResponse = WebUtil.GetResponse(request, source, false);
                }
                catch (WebException exception)
                {
                    WebUtil.GetHttpWebResponse(exception, ref this.batchResponse);
                    throw;
                }
                finally
                {
                    if (base.batchResponse != null)
                    {
                        this.responseStream = WebUtil.GetResponseStream(base.batchResponse, source);
                    }
                }
            }
        }

        private ODataRequestMessageWrapper CreateBatchRequest()
        {
            Uri requestUrl = Util.CreateUri(base.RequestInfo.BaseUriResolver.GetBaseUriWithSlash(), Util.CreateUri("$batch", UriKind.Relative));
            ODataRequestMessageWrapper requestMessage = new ODataRequestMessageWrapper("POST", requestUrl, base.RequestInfo);
            WebUtil.SetOperationVersionHeaders(requestMessage, Util.DataServiceVersion1, base.RequestInfo.MaxProtocolVersionAsVersion);
            return requestMessage;
        }

        protected override ODataRequestMessageWrapper CreateRequestMessage(Uri requestUri, string method)
        {
            return ODataRequestMessageWrapper.CreateBatchPartRequestMessage(this.batchWriter, method, requestUri, base.RequestInfo);
        }

        protected override void FinishCurrentChange(BaseAsyncResult.PerRequest pereq)
        {
            base.FinishCurrentChange(pereq);
            this.ResponseStream.Position = 0L;
            base.perRequest = null;
            base.SetCompleted();
        }

        private ODataRequestMessageWrapper GenerateBatchRequest()
        {
            if ((base.ChangedEntries.Count == 0) && (this.Queries == null))
            {
                base.SetCompleted();
                return null;
            }
            ODataRequestMessageWrapper requestMessage = this.CreateBatchRequest();
            using (ODataMessageWriter writer = Serializer.CreateMessageWriter(requestMessage, base.RequestInfo))
            {
                ODataUtils.SetHeadersForPayload(writer, ODataPayloadKind.Batch);
                requestMessage.FireSendingRequest2(null);
                this.batchWriter = writer.CreateODataBatchWriter();
                this.batchWriter.WriteStartBatch();
                if (this.Queries != null)
                {
                    for (int i = 0; i < this.Queries.Length; i++)
                    {
                        Uri requestUri = base.RequestInfo.BaseUriResolver.CreateAbsoluteUriIfNeeded(this.Queries[i].QueryComponents(base.RequestInfo.MaxProtocolVersion).Uri);
                        ODataRequestMessageWrapper wrapper2 = this.CreateRequestMessage(requestUri, "GET");
                        Version requestVersion = this.Queries[i].QueryComponents(base.RequestInfo.MaxProtocolVersion).Version;
                        WebUtil.SetOperationVersionHeaders(wrapper2, requestVersion, base.RequestInfo.MaxProtocolVersionAsVersion);
                        wrapper2.FireSendingRequest2(null);
                    }
                }
                else if (0 < base.ChangedEntries.Count)
                {
                    this.batchWriter.WriteStartChangeset();
                    ClientEdmModel model = ClientEdmModel.GetModel(base.RequestInfo.MaxProtocolVersion);
                    for (int j = 0; j < base.ChangedEntries.Count; j++)
                    {
                        Descriptor descriptor = base.ChangedEntries[j];
                        if (!descriptor.ContentGeneratedForSave)
                        {
                            ODataRequestMessageWrapper wrapper3;
                            EntityDescriptor entityDescriptor = descriptor as EntityDescriptor;
                            if (descriptor.DescriptorKind == DescriptorKind.Entity)
                            {
                                if (entityDescriptor.State != EntityStates.Added)
                                {
                                    if (((entityDescriptor.State == EntityStates.Unchanged) || (entityDescriptor.State == EntityStates.Modified)) && (entityDescriptor.SaveStream != null))
                                    {
                                        throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Context_BatchNotSupportedForMediaLink);
                                    }
                                }
                                else if (model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityDescriptor.Entity.GetType())).IsMediaLinkEntry || entityDescriptor.IsMediaLinkEntry)
                                {
                                    throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Context_BatchNotSupportedForMediaLink);
                                }
                            }
                            else if (descriptor.DescriptorKind == DescriptorKind.NamedStream)
                            {
                                throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Context_BatchNotSupportedForNamedStreams);
                            }
                            if (descriptor.DescriptorKind == DescriptorKind.Entity)
                            {
                                wrapper3 = base.CreateRequest(entityDescriptor);
                            }
                            else
                            {
                                wrapper3 = base.CreateRequest((LinkDescriptor) descriptor);
                            }
                            wrapper3.FireSendingRequest2(descriptor);
                            base.CreateChangeData(j, wrapper3);
                        }
                    }
                    this.batchWriter.WriteEndChangeset();
                }
                this.batchWriter.WriteEndBatch();
                this.batchWriter.Flush();
            }
            return requestMessage;
        }

        protected override MaterializeAtom GetMaterializer(EntityDescriptor entityDescriptor, ResponseInfo responseInfo)
        {
            return new MaterializeAtom(responseInfo, new QueryComponents(null, Util.DataServiceVersionEmpty, entityDescriptor.Entity.GetType(), null, null), null, this.currentOperationResponse.CreateResponseMessage(), ODataPayloadKind.Entry);
        }

        private DataServiceResponse HandleBatchResponse()
        {
            Func<Stream> func2 = null;
            Func<Stream> getResponseStream = null;
            DataServiceResponse response3;
            bool flag = true;
            try
            {
                Version version;
                ODataBatchReader reader;
                if ((base.batchResponse == null) || (base.batchResponse.StatusCode == HttpStatusCode.NoContent))
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Batch_ExpectedResponse(1));
                }
                if (func2 == null)
                {
                    func2 = () => this.ResponseStream;
                }
                Func<Stream> func = func2;
                BaseSaveResult.HandleResponse(base.RequestInfo, base.batchResponse.StatusCode, base.batchResponse.Headers["DataServiceVersion"], func, true, out version);
                if (this.ResponseStream == null)
                {
                    System.Data.Services.Client.Error.ThrowBatchExpectedResponse(InternalError.NullResponseStream);
                }
                HttpWebResponseMessage responseMessage = new HttpWebResponseMessage(base.batchResponse, func);
                ODataMessageReaderSettings settings = WebUtil.CreateODataMessageReaderSettings(base.RequestInfo.GetDeserializationInfo(null), null, false);
                this.batchMessageReader = new ODataMessageReader(responseMessage, settings);
                try
                {
                    reader = this.batchMessageReader.CreateODataBatchReader();
                }
                catch (Exception responseText)
                {
                    string str;
                    Encoding encoding;
                    HttpProcessUtility.ReadContentType(base.batchResponse.ContentType, out str, out encoding);
                    if (string.Equals("text/plain", str))
                    {
                        if (getResponseStream == null)
                        {
                            getResponseStream = () => WebUtil.GetResponseStream(base.batchResponse, (DataServiceContext) base.Source);
                        }
                        responseText = BaseSaveResult.GetResponseText(getResponseStream, base.batchResponse.StatusCode);
                    }
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Batch_ExpectedContentType(base.batchResponse.ContentType), responseText);
                }
                DataServiceResponse response = this.HandleBatchResponseInternal(reader);
                flag = false;
                response3 = response;
            }
            catch (DataServiceRequestException)
            {
                throw;
            }
            catch (InvalidOperationException exception3)
            {
                Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(base.batchResponse);
                int statusCode = (base.batchResponse == null) ? 500 : ((int) base.batchResponse.StatusCode);
                DataServiceResponse response2 = new DataServiceResponse(headers, statusCode, null, this.IsBatch);
                throw new DataServiceRequestException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, exception3, response2);
            }
            finally
            {
                if (flag)
                {
                    Util.Dispose<ODataMessageReader>(ref this.batchMessageReader);
                }
            }
            return response3;
        }

        private IEnumerable<OperationResponse> HandleBatchResponse(ODataBatchReader batchReader)
        {
            if (this.batchMessageReader == null)
            {
                goto Label_056D;
            }
            bool iteratorVariable0 = false;
            bool iteratorVariable1 = false;
            int index = 0;
            int iteratorVariable3 = 0;
            this.entryIndex = 0;
        Label_PostSwitchInIterator:;
            while (batchReader.Read())
            {
                Exception iteratorVariable4;
                switch (batchReader.State)
                {
                    case ODataBatchReaderState.Operation:
                    {
                        iteratorVariable4 = this.ProcessCurrentOperationResponse(batchReader);
                        if (iteratorVariable1)
                        {
                            break;
                        }
                        QueryOperationResponse iteratorVariable5 = null;
                        try
                        {
                            if (iteratorVariable4 == null)
                            {
                                DataServiceRequest query = this.Queries[index];
                                MaterializeAtom results = DataServiceRequest.Materialize(this.RequestInfo.GetDeserializationInfo(null), query.QueryComponents(this.RequestInfo.MaxProtocolVersion), null, this.currentOperationResponse.GetHeader("Content-Type"), this.currentOperationResponse.CreateResponseMessage(), query.PayloadKind);
                                iteratorVariable5 = QueryOperationResponse.GetInstance(query.ElementType, this.currentOperationResponse.Headers, query, results);
                            }
                        }
                        catch (ArgumentException exception)
                        {
                            iteratorVariable4 = exception;
                        }
                        catch (FormatException exception2)
                        {
                            iteratorVariable4 = exception2;
                        }
                        catch (InvalidOperationException exception3)
                        {
                            iteratorVariable4 = exception3;
                        }
                        if (iteratorVariable5 == null)
                        {
                            if (this.Queries == null)
                            {
                                throw iteratorVariable4;
                            }
                            DataServiceRequest request2 = this.Queries[index];
                            if (this.RequestInfo.IgnoreResourceNotFoundException && (this.currentOperationResponse.StatusCode == HttpStatusCode.NotFound))
                            {
                                iteratorVariable5 = QueryOperationResponse.GetInstance(request2.ElementType, this.currentOperationResponse.Headers, request2, MaterializeAtom.EmptyResults);
                            }
                            else
                            {
                                iteratorVariable5 = QueryOperationResponse.GetInstance(request2.ElementType, this.currentOperationResponse.Headers, request2, MaterializeAtom.EmptyResults);
                                iteratorVariable5.Error = iteratorVariable4;
                            }
                        }
                        iteratorVariable5.StatusCode = (int) this.currentOperationResponse.StatusCode;
                        index++;
                        yield return iteratorVariable5;
                        goto Label_PostSwitchInIterator;
                    }
                    case ODataBatchReaderState.ChangesetStart:
                    {
                        if ((this.IsBatch && iteratorVariable0) || (iteratorVariable3 != 0))
                        {
                            System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.UnexpectedBeginChangeSet);
                        }
                        iteratorVariable1 = true;
                        continue;
                    }
                    case ODataBatchReaderState.ChangesetEnd:
                    {
                        iteratorVariable0 = true;
                        iteratorVariable3 = 0;
                        iteratorVariable1 = false;
                        continue;
                    }
                    default:
                        goto Label_0491;
                }
                this.entryIndex = this.ValidateContentID(this.currentOperationResponse.Headers);
                try
                {
                    Descriptor descriptor = this.ChangedEntries[this.entryIndex];
                    iteratorVariable3 += this.SaveResultProcessed(descriptor);
                    if (iteratorVariable4 != null)
                    {
                        throw iteratorVariable4;
                    }
                    this.HandleOperationResponseHeaders(this.currentOperationResponse.StatusCode, this.currentOperationResponse.Headers);
                    this.HandleOperationResponse(descriptor, this.currentOperationResponse.Headers);
                }
                catch (Exception exception4)
                {
                    this.ChangedEntries[this.entryIndex].SaveError = exception4;
                    iteratorVariable4 = exception4;
                    if (!CommonUtil.IsCatchableExceptionType(exception4))
                    {
                        throw;
                    }
                }
                ChangeOperationResponse iteratorVariable6 = new ChangeOperationResponse(this.currentOperationResponse.Headers, this.ChangedEntries[this.entryIndex]) {
                    StatusCode = (int) this.currentOperationResponse.StatusCode
                };
                if (iteratorVariable4 != null)
                {
                    iteratorVariable6.Error = iteratorVariable4;
                }
                iteratorVariable3++;
                this.entryIndex++;
                yield return iteratorVariable6;
                goto Label_PostSwitchInIterator;
            Label_0491:
                System.Data.Services.Client.Error.ThrowBatchExpectedResponse(InternalError.UnexpectedBatchState);
            }
            if (((this.Queries == null) && ((!iteratorVariable0 || (0 < index)) || (this.ChangedEntries.Any<Descriptor>(o => (o.ContentGeneratedForSave && (o.SaveResultWasProcessed == 0))) && (!this.IsBatch || (this.ChangedEntries.FirstOrDefault<Descriptor>(o => (o.SaveError != null)) == null))))) || ((this.Queries != null) && (index != this.Queries.Length)))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Batch_IncompleteResponseCount);
            }
        Label_056D:
            yield break;
        }

        private DataServiceResponse HandleBatchResponseInternal(ODataBatchReader batchReader)
        {
            Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(base.batchResponse);
            IEnumerable<OperationResponse> enumerable = this.HandleBatchResponse(batchReader);
            if (this.Queries != null)
            {
                return new DataServiceResponse(headers, (int) base.batchResponse.StatusCode, enumerable, true);
            }
            List<OperationResponse> list = new List<OperationResponse>();
            DataServiceResponse response = new DataServiceResponse(headers, (int) base.batchResponse.StatusCode, list, true);
            Exception innerException = null;
            foreach (ChangeOperationResponse response2 in enumerable)
            {
                list.Add(response2);
                if ((innerException == null) && (response2.Error != null))
                {
                    innerException = response2.Error;
                }
            }
            if (innerException != null)
            {
                throw new DataServiceRequestException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, innerException, response);
            }
            return response;
        }

        protected override void HandleOperationResponse(HttpWebResponse response)
        {
            System.Data.Services.Client.Error.ThrowInternalError(InternalError.InvalidHandleOperationResponse);
        }

        protected override DataServiceResponse HandleResponse()
        {
            if (this.ResponseStream != null)
            {
                return this.HandleBatchResponse();
            }
            return new DataServiceResponse(null, 0, new List<OperationResponse>(0), true);
        }

        private Exception ProcessCurrentOperationResponse(ODataBatchReader batchReader)
        {
            MemoryStream stream2;
            Version version;
            IODataResponseMessage message = batchReader.CreateOperationResponseMessage();
            Stream input = message.GetStream();
            if (input == null)
            {
                System.Data.Services.Client.Error.ThrowBatchExpectedResponse(InternalError.NullResponseStream);
            }
            try
            {
                stream2 = new MemoryStream();
                WebUtil.CopyStream(input, stream2, ref this.streamCopyBuffer);
                stream2.Position = 0L;
            }
            finally
            {
                input.Dispose();
            }
            this.currentOperationResponse = new CurrentOperationResponse((HttpStatusCode) message.StatusCode, message.Headers, stream2);
            return BaseSaveResult.HandleResponse(base.RequestInfo, this.currentOperationResponse.StatusCode, this.currentOperationResponse.GetHeader("DataServiceVersion"), () => this.currentOperationResponse.ContentStream, false, out version);
        }

        private int ValidateContentID(Dictionary<string, string> contentHeaders)
        {
            string str;
            int result = 0;
            if (!contentHeaders.TryGetValue("Content-ID", out str) || !int.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
            {
                System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.ChangeResponseMissingContentID);
            }
            for (int i = 0; i < base.ChangedEntries.Count; i++)
            {
                if (base.ChangedEntries[i].ChangeOrder == result)
                {
                    return i;
                }
            }
            System.Data.Services.Client.Error.ThrowBatchUnexpectedContent(InternalError.ChangeResponseUnknownContentID);
            return -1;
        }

        internal override bool IsBatch
        {
            get
            {
                return true;
            }
        }

        protected override bool ProcessResponsePayload
        {
            get
            {
                return !this.currentOperationResponse.HasEmptyContent;
            }
        }

        protected override Stream ResponseStream
        {
            get
            {
                return this.responseStream;
            }
        }

        
        private sealed class CurrentOperationResponse
        {
            private readonly MemoryStream contentStream;
            private readonly Dictionary<string, string> headers;
            private readonly HttpStatusCode statusCode;

            public CurrentOperationResponse(HttpStatusCode statusCode, IEnumerable<KeyValuePair<string, string>> headers, MemoryStream contentStream)
            {
                this.statusCode = statusCode;
                this.contentStream = contentStream;
                this.headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    this.headers.Add(pair.Key, pair.Value);
                }
            }

            public IODataResponseMessage CreateResponseMessage()
            {
                if (!this.HasEmptyContent)
                {
                    return new HttpWebResponseMessage(this.headers, (int) this.statusCode, () => this.contentStream);
                }
                return null;
            }

            public string GetHeader(string headerName)
            {
                string str;
                this.headers.TryGetValue(headerName, out str);
                return str;
            }

            public Stream ContentStream
            {
                get
                {
                    return this.contentStream;
                }
            }

            public bool HasEmptyContent
            {
                get
                {
                    return (this.contentStream.Length == 0L);
                }
            }

            public Dictionary<string, string> Headers
            {
                get
                {
                    return this.headers;
                }
            }

            public HttpStatusCode StatusCode
            {
                get
                {
                    return this.statusCode;
                }
            }
        }
    }
}

