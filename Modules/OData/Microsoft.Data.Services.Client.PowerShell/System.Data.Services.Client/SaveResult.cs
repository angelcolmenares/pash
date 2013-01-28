namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Materialization;
    using System.Data.Services.Client.Metadata;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class SaveResult : BaseSaveResult
    {
        private CachedResponse cachedResponse;
        private readonly List<CachedResponse> cachedResponses;
        private HttpWebResponse httpWebResponse;
        private MemoryStream inMemoryResponseStream;

        internal SaveResult(DataServiceContext context, string method, SaveChangesOptions options, AsyncCallback callback, object state) : base(context, method, null, options, callback, state)
        {
            this.cachedResponses = new List<CachedResponse>();
        }

        internal void BeginCreateNextChange()
        {
            HttpWebResponse response;
            this.inMemoryResponseStream = new MemoryStream();
            BaseAsyncResult.PerRequest pereq = null;
            IAsyncResult result = null;
        Label_000F:
            response = null;
            ODataRequestMessageWrapper requestMessage = null;
            try
            {
                if (base.perRequest != null)
                {
                    base.SetCompleted();
                    System.Data.Services.Client.Error.ThrowInternalError(InternalError.InvalidBeginNextChange);
                }
                requestMessage = this.CreateNextRequest();
                if (requestMessage == null)
                {
                    base.Abortable = null;
                }
                if ((requestMessage != null) || (base.entryIndex < base.ChangedEntries.Count))
                {
                    if (base.ChangedEntries[base.entryIndex].ContentGeneratedForSave)
                    {
                        goto Label_0191;
                    }
                    base.Abortable = requestMessage;
                    BaseAsyncResult.ContentStream stream = this.CreateNonBatchChangeData(base.entryIndex, requestMessage);
                    base.perRequest = pereq = new BaseAsyncResult.PerRequest();
                    pereq.Request = requestMessage;
                    BaseAsyncResult.AsyncStateBag state = new BaseAsyncResult.AsyncStateBag(pereq, (DataServiceContext) base.Source);
                    if ((stream == null) || (stream.Stream == null))
                    {
                        result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetResponse), requestMessage, new AsyncCallback(this.AsyncEndGetResponse), state);
                    }
                    else
                    {
                        if (stream.IsKnownMemoryStream)
                        {
                            requestMessage.SetContentLengthHeader();
                        }
                        pereq.RequestContentStream = stream;
                        result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetRequestStream), requestMessage, new AsyncCallback(this.AsyncEndGetRequestStream), state);
                    }
                    pereq.SetRequestCompletedSynchronously(result.CompletedSynchronously);
                    base.SetCompletedSynchronously(pereq.RequestCompletedSynchronously);
                }
                else
                {
                    base.SetCompleted();
                    if (base.CompletedSynchronously)
                    {
                        this.HandleCompleted(pereq);
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                WebUtil.GetHttpWebResponse(exception, ref response);
                this.HandleOperationException(exception, response);
                this.HandleCompleted(pereq);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            if (((pereq != null) && pereq.RequestCompleted) && (pereq.RequestCompletedSynchronously && !base.IsCompletedInternally))
            {
                this.FinishCurrentChange(pereq);
            }
        Label_0191:
            if (((pereq == null) || (pereq.RequestCompleted && pereq.RequestCompletedSynchronously)) && !base.IsCompletedInternally)
            {
                goto Label_000F;
            }
        }

        private ODataRequestMessageWrapper CheckAndProcessMediaEntryPost(EntityDescriptor entityDescriptor)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(base.RequestInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityDescriptor.Entity.GetType()));
            if (!clientTypeAnnotation.IsMediaLinkEntry && !entityDescriptor.IsMediaLinkEntry)
            {
                return null;
            }
            if ((clientTypeAnnotation.MediaDataMember == null) && (entityDescriptor.SaveStream == null))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_MLEWithoutSaveStream(clientTypeAnnotation.ElementTypeName));
            }
            ODataRequestMessageWrapper mediaResourceRequest = null;
            if (clientTypeAnnotation.MediaDataMember != null)
            {
                string contentType = null;
                int length = 0;
                if (clientTypeAnnotation.MediaDataMember.MimeTypeProperty == null)
                {
                    contentType = "application/octet-stream";
                }
                else
                {
                    object obj2 = clientTypeAnnotation.MediaDataMember.MimeTypeProperty.GetValue(entityDescriptor.Entity);
                    string str2 = (obj2 != null) ? obj2.ToString() : null;
                    if (string.IsNullOrEmpty(str2))
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_NoContentTypeForMediaLink(clientTypeAnnotation.ElementTypeName, clientTypeAnnotation.MediaDataMember.MimeTypeProperty.PropertyName));
                    }
                    contentType = str2;
                }
                object propertyValue = clientTypeAnnotation.MediaDataMember.GetValue(entityDescriptor.Entity);
                if (propertyValue == null)
                {
                    base.mediaResourceRequestStream = null;
                }
                else
                {
                    byte[] bytes = propertyValue as byte[];
                    if (bytes == null)
                    {
                        string str3;
                        Encoding encoding;
                        HttpProcessUtility.ReadContentType(contentType, out str3, out encoding);
                        if (encoding == null)
                        {
                            encoding = Encoding.UTF8;
                            contentType = contentType + ";charset=UTF-8";
                        }
                        bytes = encoding.GetBytes(ClientConvert.ToString(propertyValue));
                    }
                    length = bytes.Length;
                    base.mediaResourceRequestStream = new MemoryStream(bytes, 0, bytes.Length, false, true);
                }
                mediaResourceRequest = this.CreateMediaResourceRequest(entityDescriptor.GetResourceUri(base.RequestInfo.BaseUriResolver, false), "POST", Util.DataServiceVersion1, clientTypeAnnotation.MediaDataMember == null, true);
                mediaResourceRequest.SetHeader("Content-Length", length.ToString(CultureInfo.InvariantCulture));
                mediaResourceRequest.SetHeader("Content-Type", contentType);
                mediaResourceRequest.AddHeadersToReset("Content-Length");
                mediaResourceRequest.AddHeadersToReset("Content-Type");
            }
            else
            {
                mediaResourceRequest = this.CreateMediaResourceRequest(entityDescriptor.GetResourceUri(base.RequestInfo.BaseUriResolver, false), "POST", Util.DataServiceVersion1, clientTypeAnnotation.MediaDataMember == null, true);
                this.SetupMediaResourceRequest(mediaResourceRequest, entityDescriptor.SaveStream, null);
            }
            entityDescriptor.State = EntityStates.Modified;
            return mediaResourceRequest;
        }

        private ODataRequestMessageWrapper CheckAndProcessMediaEntryPut(EntityDescriptor entityDescriptor)
        {
            if (entityDescriptor.SaveStream == null)
            {
                return null;
            }
            Uri latestEditStreamUri = entityDescriptor.GetLatestEditStreamUri();
            if (latestEditStreamUri == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_SetSaveStreamWithoutEditMediaLink);
            }
            ODataRequestMessageWrapper mediaResourceRequest = this.CreateMediaResourceRequest(latestEditStreamUri, "PUT", Util.DataServiceVersion1, true, false);
            this.SetupMediaResourceRequest(mediaResourceRequest, entityDescriptor.SaveStream, entityDescriptor.GetLatestStreamETag());
            return mediaResourceRequest;
        }

        private void CheckContinueOnError()
        {
            if (!Util.IsFlagSet(base.Options, SaveChangesOptions.ContinueOnError))
            {
                base.SetCompleted();
            }
            else
            {
                base.streamRequestKind = BaseSaveResult.StreamRequestKind.None;
                base.ChangedEntries[base.entryIndex].ContentGeneratedForSave = true;
            }
        }

        private ODataRequestMessageWrapper CreateMediaResourceRequest(Uri requestUri, string method, Version version, bool sendChunked, bool applyResponsePreference)
        {
            ODataRequestMessageWrapper requestMessage = this.CreateRequestMessage(requestUri, method);
            requestMessage.SendChunked = sendChunked;
            requestMessage.SetHeader("Content-Type", "*/*");
            if (applyResponsePreference)
            {
                BaseSaveResult.ApplyPreferences(requestMessage, method, base.RequestInfo.AddAndUpdateResponsePreference, ref version);
            }
            WebUtil.SetOperationVersionHeaders(requestMessage, version, base.RequestInfo.MaxProtocolVersionAsVersion);
            return requestMessage;
        }

        private ODataRequestMessageWrapper CreateNamedStreamRequest(StreamDescriptor namedStreamInfo)
        {
            Uri latestEditLink = namedStreamInfo.GetLatestEditLink();
            if (latestEditLink == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_SetSaveStreamWithoutNamedStreamEditLink(namedStreamInfo.Name));
            }
            ODataRequestMessageWrapper mediaResourceRequest = this.CreateMediaResourceRequest(latestEditLink, "PUT", Util.DataServiceVersion3, true, false);
            this.SetupMediaResourceRequest(mediaResourceRequest, namedStreamInfo.SaveStream, namedStreamInfo.GetLatestETag());
            return mediaResourceRequest;
        }

        internal void CreateNextChange()
        {
            ODataRequestMessageWrapper requestMessage = null;
            do
            {
                HttpWebResponse response = null;
                try
                {
                    requestMessage = this.CreateNextRequest();
                    if (((requestMessage != null) || (base.entryIndex < base.ChangedEntries.Count)) && !base.ChangedEntries[base.entryIndex].ContentGeneratedForSave)
                    {
                        BaseAsyncResult.ContentStream stream = this.CreateNonBatchChangeData(base.entryIndex, requestMessage);
                        if ((stream != null) && (stream.Stream != null))
                        {
                            if (stream.IsKnownMemoryStream)
                            {
                                requestMessage.SetContentLengthHeader();
                            }
                            using (Stream stream2 = WebUtil.GetRequestStream(requestMessage, (DataServiceContext) base.Source))
                            {
                                int num;
                                byte[] buffer = new byte[0x10000];
                                do
                                {
                                    num = stream.Stream.Read(buffer, 0, buffer.Length);
                                    if (num > 0)
                                    {
                                        stream2.Write(buffer, 0, num);
                                    }
                                }
                                while (num > 0);
                            }
                        }
                        response = WebUtil.GetResponse(requestMessage, (DataServiceContext) base.Source, false);
                        this.HandleOperationResponse(response);
                        base.HandleOperationResponseHeaders(response.StatusCode, WebUtil.WrapResponseHeaders(response));
                        this.HandleOperationResponseData(response);
                        base.perRequest = null;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    WebUtil.GetHttpWebResponse(exception, ref response);
                    this.HandleOperationException(exception, response);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
            while ((base.entryIndex < base.ChangedEntries.Count) && !base.IsCompletedInternally);
        }

        private ODataRequestMessageWrapper CreateNextRequest()
        {
            bool flag = base.streamRequestKind == BaseSaveResult.StreamRequestKind.None;
            if (base.entryIndex < base.ChangedEntries.Count)
            {
                Descriptor descriptor = base.ChangedEntries[base.entryIndex];
                if (descriptor.DescriptorKind == DescriptorKind.Entity)
                {
                    EntityDescriptor descriptor2 = (EntityDescriptor) descriptor;
                    descriptor2.CloseSaveStream();
                    if ((base.streamRequestKind == BaseSaveResult.StreamRequestKind.PutMediaResource) && (EntityStates.Unchanged == descriptor2.State))
                    {
                        descriptor2.ContentGeneratedForSave = true;
                        flag = true;
                    }
                }
                else if (descriptor.DescriptorKind == DescriptorKind.NamedStream)
                {
                    ((StreamDescriptor) descriptor).CloseSaveStream();
                }
            }
            if (flag)
            {
                base.entryIndex++;
            }
            ODataRequestMessageWrapper wrapper = null;
            if (base.entryIndex < base.ChangedEntries.Count)
            {
                Descriptor descriptor3 = base.ChangedEntries[base.entryIndex];
                if (descriptor3.DescriptorKind == DescriptorKind.Entity)
                {
                    EntityDescriptor entityDescriptor = (EntityDescriptor) descriptor3;
                    if (((EntityStates.Unchanged == descriptor3.State) || (EntityStates.Modified == descriptor3.State)) && ((wrapper = this.CheckAndProcessMediaEntryPut(entityDescriptor)) != null))
                    {
                        base.streamRequestKind = BaseSaveResult.StreamRequestKind.PutMediaResource;
                    }
                    else if ((EntityStates.Added == descriptor3.State) && ((wrapper = this.CheckAndProcessMediaEntryPost(entityDescriptor)) != null))
                    {
                        base.streamRequestKind = BaseSaveResult.StreamRequestKind.PostMediaResource;
                        entityDescriptor.StreamState = EntityStates.Added;
                    }
                    else
                    {
                        base.streamRequestKind = BaseSaveResult.StreamRequestKind.None;
                        wrapper = base.CreateRequest(entityDescriptor);
                    }
                }
                else if (descriptor3.DescriptorKind == DescriptorKind.NamedStream)
                {
                    wrapper = this.CreateNamedStreamRequest((StreamDescriptor) descriptor3);
                }
                else
                {
                    wrapper = base.CreateRequest((LinkDescriptor) descriptor3);
                }
                if (wrapper != null)
                {
                    wrapper.FireSendingRequest2(descriptor3);
                }
            }
            return wrapper;
        }

        protected BaseAsyncResult.ContentStream CreateNonBatchChangeData(int index, ODataRequestMessageWrapper requestMessage)
        {
            Descriptor descriptor = base.ChangedEntries[index];
            if ((descriptor.DescriptorKind == DescriptorKind.Entity) && (base.streamRequestKind != BaseSaveResult.StreamRequestKind.None))
            {
                if (base.streamRequestKind != BaseSaveResult.StreamRequestKind.None)
                {
                    return new BaseAsyncResult.ContentStream(base.mediaResourceRequestStream, false);
                }
            }
            else
            {
                if (descriptor.DescriptorKind == DescriptorKind.NamedStream)
                {
                    descriptor.ContentGeneratedForSave = true;
                    return new BaseAsyncResult.ContentStream(base.mediaResourceRequestStream, false);
                }
                if (base.CreateChangeData(index, requestMessage))
                {
                    return new BaseAsyncResult.ContentStream(requestMessage.CachedRequestStream, true);
                }
            }
            return null;
        }

        protected override ODataRequestMessageWrapper CreateRequestMessage(Uri requestUri, string method)
        {
            return new ODataRequestMessageWrapper(method, requestUri, base.RequestInfo);
        }

        protected override void FinishCurrentChange(BaseAsyncResult.PerRequest pereq)
        {
            base.FinishCurrentChange(pereq);
            if (this.ResponseStream.Position != 0L)
            {
                this.ResponseStream.Position = 0L;
                this.HandleOperationResponseData(this.httpWebResponse, this.ResponseStream);
            }
            else
            {
                this.HandleOperationResponseData(this.httpWebResponse, null);
            }
            pereq.Dispose();
            base.perRequest = null;
            if (!pereq.RequestCompletedSynchronously && !base.IsCompletedInternally)
            {
                this.BeginCreateNextChange();
            }
        }

        protected override MaterializeAtom GetMaterializer(EntityDescriptor entityDescriptor, ResponseInfo responseInfo)
        {
            return new MaterializeAtom(responseInfo, new ODataEntry[] { this.cachedResponse.Entry }, entityDescriptor.Entity.GetType());
        }

        private void HandleOperationException(InvalidOperationException e, HttpWebResponse response)
        {
            Func<Stream> getResponseStream = null;
            Descriptor descriptor = base.ChangedEntries[base.entryIndex];
            Dictionary<string, string> headers = null;
            HttpStatusCode internalServerError = HttpStatusCode.InternalServerError;
            Version parsedResponseVersion = null;
            if (response != null)
            {
                headers = WebUtil.WrapResponseHeaders(response);
                base.HandleOperationResponseHeaders(response.StatusCode, headers);
                if (getResponseStream == null)
                {
                    getResponseStream = () => WebUtil.GetResponseStream(response, (DataServiceContext) this.Source);
                }
                e = BaseSaveResult.HandleResponse(base.RequestInfo, response.StatusCode, response.Headers["DataServiceVersion"], getResponseStream, false, out parsedResponseVersion);
                internalServerError = response.StatusCode;
            }
            else
            {
                headers = new Dictionary<string, string>(StringComparer.Ordinal);
                headers.Add("Content-Type", "text/plain");
                if (e.GetType() != typeof(DataServiceClientException))
                {
                    e = new DataServiceClientException(e.Message, e);
                }
            }
            this.cachedResponses.Add(new CachedResponse(descriptor, headers, internalServerError, parsedResponseVersion, null, e));
            base.perRequest = null;
            this.CheckContinueOnError();
        }

        protected override void HandleOperationResponse(HttpWebResponse response)
        {
            this.httpWebResponse = response;
        }

        private void HandleOperationResponseData(HttpWebResponse response)
        {
            using (Stream stream = WebUtil.GetResponseStream(response, (DataServiceContext) base.Source))
            {
                if (stream != null)
                {
                    using (MemoryStream stream2 = new MemoryStream())
                    {
                        if (WebUtil.CopyStream(stream, stream2, ref this.buildBatchBuffer) != 0L)
                        {
                            stream2.Position = 0L;
                            this.HandleOperationResponseData(response, stream2);
                        }
                        else
                        {
                            this.HandleOperationResponseData(response, null);
                        }
                    }
                }
            }
        }

        private void HandleOperationResponseData(HttpWebResponse response, Stream responseStream)
        {
            Version version;
            Func<Stream> getResponseStream = null;
            Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(response);
            Descriptor descriptor = base.ChangedEntries[base.entryIndex];
            MaterializerEntry entry = null;
            Exception exception = BaseSaveResult.HandleResponse(base.RequestInfo, response.StatusCode, response.Headers["DataServiceVersion"], () => responseStream, false, out version);
            if (((responseStream != null) && (descriptor.DescriptorKind == DescriptorKind.Entity)) && (exception == null))
            {
                EntityDescriptor entityDescriptor = (EntityDescriptor) descriptor;
                if (((entityDescriptor.State == EntityStates.Added) || (entityDescriptor.StreamState == EntityStates.Added)) || ((entityDescriptor.State == EntityStates.Modified) || (entityDescriptor.StreamState == EntityStates.Modified)))
                {
                    try
                    {
                        ResponseInfo responseInfo = base.CreateResponseInfo(entityDescriptor);
                        if (getResponseStream == null)
                        {
                            getResponseStream = () => responseStream;
                        }
                        HttpWebResponseMessage message = new HttpWebResponseMessage(response, getResponseStream);
                        entry = ODataReaderEntityMaterializer.ParseSingleEntityPayload(message, responseInfo, entityDescriptor.Entity.GetType());
                        entityDescriptor.TransientEntityDescriptor = entry.EntityDescriptor;
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                        if (!CommonUtil.IsCatchableExceptionType(exception2))
                        {
                            throw;
                        }
                    }
                }
            }
            this.cachedResponses.Add(new CachedResponse(descriptor, headers, response.StatusCode, version, (entry != null) ? entry.Entry : null, exception));
            if (exception != null)
            {
                descriptor.SaveError = exception;
            }
        }

        protected override DataServiceResponse HandleResponse()
        {
            List<OperationResponse> list = new List<OperationResponse>((this.cachedResponses != null) ? this.cachedResponses.Count : 0);
            DataServiceResponse response = new DataServiceResponse(null, -1, list, false);
            Exception innerException = null;
            try
            {
                foreach (CachedResponse response2 in this.cachedResponses)
                {
                    Descriptor descriptor = response2.Descriptor;
                    base.SaveResultProcessed(descriptor);
                    OperationResponse item = new ChangeOperationResponse(response2.Headers, descriptor) {
                        StatusCode = (int) response2.StatusCode
                    };
                    if (response2.Exception != null)
                    {
                        item.Error = response2.Exception;
                        if (innerException == null)
                        {
                            innerException = response2.Exception;
                        }
                    }
                    else
                    {
                        this.cachedResponse = response2;
                        base.HandleOperationResponse(descriptor, response2.Headers);
                    }
                    list.Add(item);
                }
            }
            catch (InvalidOperationException exception2)
            {
                innerException = exception2;
            }
            if (innerException != null)
            {
                throw new DataServiceRequestException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, innerException, response);
            }
            return response;
        }

        private void SetupMediaResourceRequest(ODataRequestMessageWrapper mediaResourceRequest, DataServiceSaveStream saveStream, string etag)
        {
            IEnumerable<string> keys;
            base.mediaResourceRequestStream = saveStream.Stream;
            WebUtil.ApplyHeadersToRequest(saveStream.Args.Headers, mediaResourceRequest, true);
            Dictionary<string, string> headers = saveStream.Args.Headers;
            if (headers.ContainsKey("Accept"))
            {
                keys = new List<string>(headers.Count - 1);
                foreach (string str in headers.Keys)
                {
                    if (str != "Accept")
                    {
                        ((List<string>) keys).Add(str);
                    }
                }
            }
            else
            {
                keys = headers.Keys;
            }
            mediaResourceRequest.AddHeadersToReset(keys);
            if (etag != null)
            {
                mediaResourceRequest.SetHeader("If-Match", etag);
                mediaResourceRequest.AddHeadersToReset("If-Match");
            }
        }

        internal override bool IsBatch
        {
            get
            {
                return false;
            }
        }

        protected override bool ProcessResponsePayload
        {
            get
            {
                return (this.cachedResponse.Entry != null);
            }
        }

        protected override Stream ResponseStream
        {
            get
            {
                return this.inMemoryResponseStream;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CachedResponse
        {
            public readonly Dictionary<string, string> Headers;
            public readonly HttpStatusCode StatusCode;
            public readonly System.Version Version;
            public readonly ODataEntry Entry;
            public readonly System.Exception Exception;
            public readonly System.Data.Services.Client.Descriptor Descriptor;
            internal CachedResponse(System.Data.Services.Client.Descriptor descriptor, Dictionary<string, string> headers, HttpStatusCode statusCode, System.Version responseVersion, ODataEntry entry, System.Exception exception)
            {
                this.Descriptor = descriptor;
                this.Entry = entry;
                this.Exception = exception;
                this.Headers = headers;
                this.StatusCode = statusCode;
                this.Version = responseVersion;
            }
        }
    }
}

