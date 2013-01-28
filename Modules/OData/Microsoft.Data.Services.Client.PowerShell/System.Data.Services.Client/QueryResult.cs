namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;

    internal class QueryResult : BaseAsyncResult
    {
        private byte[] asyncStreamCopyBuffer;
        private long contentLength;
        private string contentType;
        private HttpWebResponse httpWebResponse;
        private Stream outputResponseStream;
        internal readonly ODataRequestMessageWrapper Request;
        private BaseAsyncResult.ContentStream requestContentStream;
        internal readonly System.Data.Services.Client.RequestInfo RequestInfo;
        private ResponseInfo responseInfo;
        private bool responseStreamOwner;
        private static byte[] reusableAsyncCopyBuffer;
        internal readonly DataServiceRequest ServiceRequest;
        private HttpStatusCode statusCode;
        private bool usingBuffer;

        internal QueryResult(object source, string method, DataServiceRequest serviceRequest, ODataRequestMessageWrapper request, System.Data.Services.Client.RequestInfo requestInfo, AsyncCallback callback, object state) : base(source, method, callback, state)
        {
            this.ServiceRequest = serviceRequest;
            this.Request = request;
            this.RequestInfo = requestInfo;
            base.Abortable = request;
        }

        internal QueryResult(object source, string method, DataServiceRequest serviceRequest, ODataRequestMessageWrapper request, System.Data.Services.Client.RequestInfo requestInfo, AsyncCallback callback, object state, BaseAsyncResult.ContentStream requestContentStream) : this(source, method, serviceRequest, request, requestInfo, callback, state)
        {
            this.requestContentStream = requestContentStream;
        }

        protected override void AsyncEndGetResponse(IAsyncResult asyncResult)
        {
            BaseAsyncResult.AsyncStateBag asyncState = asyncResult.AsyncState as BaseAsyncResult.AsyncStateBag;
            BaseAsyncResult.PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            DataServiceContext context = (asyncState == null) ? null : asyncState.Context;
            try
            {
                if (base.IsAborted)
                {
                    if (request != null)
                    {
                        request.SetComplete();
                    }
                    base.SetCompleted();
                }
                else
                {
                    this.CompleteCheck(request, InternalError.InvalidEndGetResponseCompleted);
                    request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                    base.SetCompletedSynchronously(asyncResult.CompletedSynchronously);
                    HttpWebResponse response = WebUtil.EndGetResponse(Util.NullCheck<ODataRequestMessageWrapper>(request.Request, InternalError.InvalidEndGetResponseRequest), asyncResult, context);
                    request.HttpWebResponse = Util.NullCheck<HttpWebResponse>(response, InternalError.InvalidEndGetResponseResponse);
                    this.SetHttpWebResponse(request.HttpWebResponse);
                    Stream responseStream = null;
                    if (HttpStatusCode.NoContent != response.StatusCode)
                    {
                        responseStream = WebUtil.GetResponseStream(response, context);
                        request.ResponseStream = responseStream;
                    }
                    if ((responseStream != null) && responseStream.CanRead)
                    {
                        if (this.outputResponseStream == null)
                        {
                            this.outputResponseStream = Util.NullCheck<Stream>(this.GetAsyncResponseStreamCopy(), InternalError.InvalidAsyncResponseStreamCopy);
                        }
                        if (this.asyncStreamCopyBuffer == null)
                        {
                            this.asyncStreamCopyBuffer = Util.NullCheck<byte[]>(this.GetAsyncResponseStreamCopyBuffer(), InternalError.InvalidAsyncResponseStreamCopyBuffer);
                        }
                        this.ReadResponseStream(asyncState);
                    }
                    else
                    {
                        request.SetComplete();
                        base.SetCompleted();
                    }
                }
            }
            catch (Exception exception)
            {
                if (base.HandleFailure(exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        private void AsyncEndRead(IAsyncResult asyncResult)
        {
            BaseAsyncResult.AsyncStateBag asyncState = asyncResult.AsyncState as BaseAsyncResult.AsyncStateBag;
            BaseAsyncResult.PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            int count = 0;
            try
            {
                this.CompleteCheck(request, InternalError.InvalidEndReadCompleted);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                base.SetCompletedSynchronously(asyncResult.CompletedSynchronously);
                Stream stream = Util.NullCheck<Stream>(request.ResponseStream, InternalError.InvalidEndReadStream);
                Stream stream2 = Util.NullCheck<Stream>(this.outputResponseStream, InternalError.InvalidEndReadCopy);
                byte[] buffer = Util.NullCheck<byte[]>(this.asyncStreamCopyBuffer, InternalError.InvalidEndReadBuffer);
                count = stream.EndRead(asyncResult);
                this.usingBuffer = false;
                if (0 < count)
                {
                    stream2.Write(buffer, 0, count);
                }
                if (((0 < count) && (0 < buffer.Length)) && stream.CanRead)
                {
                    if (!asyncResult.CompletedSynchronously)
                    {
                        this.ReadResponseStream(asyncState);
                    }
                }
                else
                {
                    if (stream2.Position < stream2.Length)
                    {
                        ((MemoryStream) stream2).SetLength(stream2.Position);
                    }
                    request.SetComplete();
                    base.SetCompleted();
                }
            }
            catch (Exception exception)
            {
                if (base.HandleFailure(exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        internal void BeginExecuteQuery(DataServiceContext context)
        {
            IAsyncResult result = null;
            BaseAsyncResult.PerRequest pereq = new BaseAsyncResult.PerRequest();
            BaseAsyncResult.AsyncStateBag state = new BaseAsyncResult.AsyncStateBag(pereq, context);
            pereq.Request = this.Request;
            base.perRequest = pereq;
            try
            {
                if ((this.requestContentStream != null) && (this.requestContentStream.Stream != null))
                {
                    if (this.requestContentStream.IsKnownMemoryStream)
                    {
                        this.Request.SetContentLengthHeader();
                    }
                    base.perRequest.RequestContentStream = this.requestContentStream;
                    result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetRequestStream), this.Request, new AsyncCallback(this.AsyncEndGetRequestStream), state);
                }
                else
                {
                    result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetResponse), this.Request, new AsyncCallback(this.AsyncEndGetResponse), state);
                }
                pereq.SetRequestCompletedSynchronously(result.CompletedSynchronously);
                base.SetCompletedSynchronously(result.CompletedSynchronously);
            }
            catch (Exception exception)
            {
                base.HandleFailure(exception);
                throw;
            }
            finally
            {
                this.HandleCompleted(pereq);
            }
        }

        protected override void CompleteCheck(BaseAsyncResult.PerRequest pereq, InternalError errorcode)
        {
            if ((pereq == null) || ((pereq.RequestCompleted || base.IsCompletedInternally) && (!base.IsAborted && !pereq.RequestAborted)))
            {
                System.Data.Services.Client.Error.ThrowInternalError(errorcode);
            }
        }

        protected override void CompletedRequest()
        {
            byte[] asyncStreamCopyBuffer = this.asyncStreamCopyBuffer;
            this.asyncStreamCopyBuffer = null;
            if ((asyncStreamCopyBuffer != null) && !this.usingBuffer)
            {
                this.PutAsyncResponseStreamCopyBuffer(asyncStreamCopyBuffer);
            }
            if (this.responseStreamOwner && (this.outputResponseStream != null))
            {
                this.outputResponseStream.Position = 0L;
            }
            if (this.httpWebResponse != null)
            {
                Version version;
                this.httpWebResponse.Close();
                Exception e = BaseSaveResult.HandleResponse(this.RequestInfo, this.StatusCode, this.httpWebResponse.Headers["DataServiceVersion"], new Func<Stream>(this.GetResponseStream), false, out version);
                if (e != null)
                {
                    base.HandleFailure(e);
                }
                else
                {
                    this.responseInfo = this.RequestInfo.GetDeserializationInfo(null);
                }
            }
        }

        private MaterializeAtom CreateMaterializer(ProjectionPlan plan, ODataPayloadKind payloadKind)
        {
            HttpWebResponseMessage message = new HttpWebResponseMessage(this.httpWebResponse, () => this.GetResponseStream());
            return DataServiceRequest.Materialize(this.responseInfo, this.ServiceRequest.QueryComponents(this.responseInfo.MaxProtocolVersion), plan, this.ContentType, message, payloadKind);
        }

        internal static QueryResult EndExecuteQuery<TElement>(object source, string method, IAsyncResult asyncResult)
        {
            QueryResult result = null;
            try
            {
                result = BaseAsyncResult.EndExecute<QueryResult>(source, method, asyncResult);
            }
            catch (InvalidOperationException exception)
            {
                result = asyncResult as QueryResult;
                QueryOperationResponse response = result.GetResponse<TElement>(MaterializeAtom.EmptyResults);
                if (response != null)
                {
                    response.Error = exception;
                    throw new DataServiceQueryException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, exception, response);
                }
                throw;
            }
            return result;
        }

        internal void ExecuteQuery(DataServiceContext context)
        {
            try
            {
                if ((this.requestContentStream != null) && (this.requestContentStream.Stream != null))
                {
                    if (this.requestContentStream.IsKnownMemoryStream)
                    {
                        this.Request.SetContentLengthHeader();
                    }
                    using (Stream stream = WebUtil.GetRequestStream(this.Request, context))
                    {
                        int num;
                        byte[] buffer = new byte[0x10000];
                        do
                        {
                            num = this.requestContentStream.Stream.Read(buffer, 0, buffer.Length);
                            if (num > 0)
                            {
                                stream.Write(buffer, 0, num);
                            }
                        }
                        while (num > 0);
                    }
                }
                HttpWebResponse response = null;
                response = WebUtil.GetResponse(this.Request, context, true);
                this.SetHttpWebResponse(Util.NullCheck<HttpWebResponse>(response, InternalError.InvalidGetResponse));
                if (HttpStatusCode.NoContent != this.StatusCode)
                {
                    using (Stream stream2 = WebUtil.GetResponseStream(this.httpWebResponse, context))
                    {
                        if (stream2 != null)
                        {
                            Stream asyncResponseStreamCopy = this.GetAsyncResponseStreamCopy();
                            this.outputResponseStream = asyncResponseStreamCopy;
                            byte[] asyncResponseStreamCopyBuffer = this.GetAsyncResponseStreamCopyBuffer();
                            long num2 = WebUtil.CopyStream(stream2, asyncResponseStreamCopy, ref asyncResponseStreamCopyBuffer);
                            if (this.responseStreamOwner)
                            {
                                if (0L == num2)
                                {
                                    this.outputResponseStream = null;
                                }
                                else if (asyncResponseStreamCopy.Position < asyncResponseStreamCopy.Length)
                                {
                                    ((MemoryStream) asyncResponseStreamCopy).SetLength(asyncResponseStreamCopy.Position);
                                }
                            }
                            this.PutAsyncResponseStreamCopyBuffer(asyncResponseStreamCopyBuffer);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                base.HandleFailure(exception);
                throw;
            }
            finally
            {
                base.SetCompleted();
                this.CompletedRequest();
            }
            if (base.Failure != null)
            {
                throw base.Failure;
            }
        }

        protected virtual Stream GetAsyncResponseStreamCopy()
        {
            this.responseStreamOwner = true;
            long contentLength = this.contentLength;
            if ((0L < contentLength) && (contentLength <= 0x7fffffffL))
            {
                return new MemoryStream((int) contentLength);
            }
            return new MemoryStream();
        }

        protected virtual byte[] GetAsyncResponseStreamCopyBuffer()
        {
            return (Interlocked.Exchange<byte[]>(ref reusableAsyncCopyBuffer, null) ?? new byte[0x1f40]);
        }

        internal MaterializeAtom GetMaterializer(ProjectionPlan plan)
        {
            if (HttpStatusCode.NoContent != this.StatusCode)
            {
                return this.CreateMaterializer(plan, ODataPayloadKind.Unsupported);
            }
            return MaterializeAtom.EmptyResults;
        }

        internal QueryOperationResponse<TElement> GetResponse<TElement>(MaterializeAtom results)
        {
            if (this.httpWebResponse != null)
            {
                return new QueryOperationResponse<TElement>(WebUtil.WrapResponseHeaders(this.httpWebResponse), this.ServiceRequest, results) { StatusCode = (int) this.httpWebResponse.StatusCode };
            }
            return null;
        }

        internal Stream GetResponseStream()
        {
            return this.outputResponseStream;
        }

        internal QueryOperationResponse GetResponseWithType(MaterializeAtom results, Type elementType)
        {
            if (this.httpWebResponse != null)
            {
                Dictionary<string, string> headers = WebUtil.WrapResponseHeaders(this.httpWebResponse);
                QueryOperationResponse response = QueryOperationResponse.GetInstance(elementType, headers, this.ServiceRequest, results);
                response.StatusCode = (int) this.httpWebResponse.StatusCode;
                return response;
            }
            return null;
        }

        protected override void HandleCompleted(BaseAsyncResult.PerRequest pereq)
        {
            if (pereq != null)
            {
                base.SetCompletedSynchronously(pereq.RequestCompletedSynchronously);
                if (pereq.RequestCompleted)
                {
                    Interlocked.CompareExchange<BaseAsyncResult.PerRequest>(ref this.perRequest, null, pereq);
                    pereq.Dispose();
                }
            }
            base.HandleCompleted();
        }

        internal QueryOperationResponse<TElement> ProcessResult<TElement>(ProjectionPlan plan)
        {
            MaterializeAtom results = this.CreateMaterializer(plan, this.ServiceRequest.PayloadKind);
            return this.GetResponse<TElement>(results);
        }

        protected virtual void PutAsyncResponseStreamCopyBuffer(byte[] buffer)
        {
            reusableAsyncCopyBuffer = buffer;
        }

        private void ReadResponseStream(BaseAsyncResult.AsyncStateBag asyncStateBag)
        {
            BaseAsyncResult.PerRequest perRequest = asyncStateBag.PerRequest;
            IAsyncResult result = null;
            byte[] asyncStreamCopyBuffer = this.asyncStreamCopyBuffer;
            Stream responseStream = perRequest.ResponseStream;
            do
            {
                int offset = 0;
                int length = asyncStreamCopyBuffer.Length;
                this.usingBuffer = true;
                result = BaseAsyncResult.InvokeAsync(new BaseAsyncResult.AsyncAction(responseStream.BeginRead), asyncStreamCopyBuffer, offset, length, new AsyncCallback(this.AsyncEndRead), asyncStateBag);
                perRequest.SetRequestCompletedSynchronously(result.CompletedSynchronously);
                base.SetCompletedSynchronously(result.CompletedSynchronously);
            }
            while ((result.CompletedSynchronously && !perRequest.RequestCompleted) && (!base.IsCompletedInternally && responseStream.CanRead));
        }

        protected virtual void SetHttpWebResponse(HttpWebResponse response)
        {
            this.httpWebResponse = response;
            this.statusCode = response.StatusCode;
            this.contentLength = response.ContentLength;
            this.contentType = response.ContentType;
        }

        internal long ContentLength
        {
            get
            {
                return this.contentLength;
            }
        }

        internal string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        internal HttpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }
    }
}

