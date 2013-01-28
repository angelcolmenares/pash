namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class BaseAsyncResult : IAsyncResult
    {
        private ODataRequestMessageWrapper abortable;
        private ManualResetEvent asyncWait;
        private bool asyncWaitDisposed;
        private object asyncWaitDisposeLock;
        private int completed;
        private int completedSynchronously = 1;
        private int done;
        private Exception failure;
        private const int False = 0;
        internal readonly string Method;
        protected PerRequest perRequest;
        internal readonly object Source;
        private const int True = 1;
        private readonly AsyncCallback userCallback;
        private bool userCompleted;
        private int userNotified;
        private readonly object userState;

        internal BaseAsyncResult(object source, string method, AsyncCallback callback, object state)
        {
            this.Source = source;
            this.Method = method;
            this.userCallback = callback;
            this.userState = state;
        }

        protected void AsyncEndGetRequestStream(IAsyncResult asyncResult)
        {
            AsyncStateBag asyncState = asyncResult.AsyncState as AsyncStateBag;
            PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            DataServiceContext context = (asyncState == null) ? null : asyncState.Context;
            try
            {
                this.CompleteCheck(request, InternalError.InvalidEndGetRequestCompleted);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                EqualRefCheck(this.perRequest, request, InternalError.InvalidEndGetRequestStream);
                Stream stream = Util.NullCheck<Stream>(WebUtil.EndGetRequestStream(Util.NullCheck<ODataRequestMessageWrapper>(request.Request, InternalError.InvalidEndGetRequestStreamRequest), asyncResult, context), InternalError.InvalidEndGetRequestStreamStream);
                request.RequestStream = stream;
                ContentStream requestContentStream = request.RequestContentStream;
                Util.NullCheck<ContentStream>(requestContentStream, InternalError.InvalidEndGetRequestStreamContent);
                Util.NullCheck<Stream>(requestContentStream.Stream, InternalError.InvalidEndGetRequestStreamContent);
                if (requestContentStream.IsKnownMemoryStream)
                {
                    MemoryStream stream3 = requestContentStream.Stream as MemoryStream;
                    byte[] buffer = stream3.GetBuffer();
                    int position = (int) stream3.Position;
                    int num2 = ((int) stream3.Length) - position;
                    if ((buffer == null) || (num2 == 0))
                    {
                        Error.ThrowInternalError(InternalError.InvalidEndGetRequestStreamContentLength);
                    }
                }
                request.RequestContentBufferValidLength = -1;
                Stream stream1 = requestContentStream.Stream;
                asyncResult = InvokeAsync(new AsyncAction(stream1.BeginRead), request.RequestContentBuffer, 0, request.RequestContentBuffer.Length, new AsyncCallback(this.AsyncRequestContentEndRead), asyncState);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
            }
            catch (Exception exception)
            {
                if (this.HandleFailure(request, exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        protected abstract void AsyncEndGetResponse(IAsyncResult asyncResult);
        private void AsyncEndWrite(IAsyncResult asyncResult)
        {
            AsyncStateBag asyncState = asyncResult.AsyncState as AsyncStateBag;
            PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            try
            {
                this.CompleteCheck(request, InternalError.InvalidEndWriteCompleted);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                EqualRefCheck(this.perRequest, request, InternalError.InvalidEndWrite);
                ContentStream requestContentStream = request.RequestContentStream;
                Util.NullCheck<ContentStream>(requestContentStream, InternalError.InvalidEndWriteStream);
                Util.NullCheck<Stream>(requestContentStream.Stream, InternalError.InvalidEndWriteStream);
                Util.NullCheck<Stream>(request.RequestStream, InternalError.InvalidEndWriteStream).EndWrite(asyncResult);
                if (!asyncResult.CompletedSynchronously)
                {
                    Stream stream = requestContentStream.Stream;
                    asyncResult = InvokeAsync(new AsyncAction(stream.BeginRead), request.RequestContentBuffer, 0, request.RequestContentBuffer.Length, new AsyncCallback(this.AsyncRequestContentEndRead), asyncState);
                    request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                }
            }
            catch (Exception exception)
            {
                if (this.HandleFailure(request, exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        private void AsyncRequestContentEndRead(IAsyncResult asyncResult)
        {
            AsyncStateBag asyncState = asyncResult.AsyncState as AsyncStateBag;
            PerRequest request = (asyncState == null) ? null : asyncState.PerRequest;
            try
            {
                this.CompleteCheck(request, InternalError.InvalidEndReadCompleted);
                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                EqualRefCheck(this.perRequest, request, InternalError.InvalidEndRead);
                ContentStream requestContentStream = request.RequestContentStream;
                Util.NullCheck<ContentStream>(requestContentStream, InternalError.InvalidEndReadStream);
                Util.NullCheck<Stream>(requestContentStream.Stream, InternalError.InvalidEndReadStream);
                Stream stream2 = Util.NullCheck<Stream>(request.RequestStream, InternalError.InvalidEndReadStream);
                int num = requestContentStream.Stream.EndRead(asyncResult);
                if (0 < num)
                {
                    bool flag = request.RequestContentBufferValidLength == -1;
                    request.RequestContentBufferValidLength = num;
                    if (!asyncResult.CompletedSynchronously || flag)
                    {
                        do
                        {
                            asyncResult = InvokeAsync(new AsyncAction(stream2.BeginWrite), request.RequestContentBuffer, 0, request.RequestContentBufferValidLength, new AsyncCallback(this.AsyncEndWrite), asyncState);
                            request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                            if ((asyncResult.CompletedSynchronously && !request.RequestCompleted) && !this.IsCompletedInternally)
                            {
                                Stream stream = requestContentStream.Stream;
                                asyncResult = InvokeAsync(new AsyncAction(stream.BeginRead), request.RequestContentBuffer, 0, request.RequestContentBuffer.Length, new AsyncCallback(this.AsyncRequestContentEndRead), asyncState);
                                request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                            }
                        }
                        while (((asyncResult.CompletedSynchronously && !request.RequestCompleted) && !this.IsCompletedInternally) && (request.RequestContentBufferValidLength > 0));
                    }
                }
                else
                {
                    request.RequestContentBufferValidLength = 0;
                    request.RequestStream = null;
                    stream2.Close();
                    ODataRequestMessageWrapper wrapper = Util.NullCheck<ODataRequestMessageWrapper>(request.Request, InternalError.InvalidEndWriteRequest);
                    asyncResult = InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetResponse), wrapper, new AsyncCallback(this.AsyncEndGetResponse), asyncState);
                    request.SetRequestCompletedSynchronously(asyncResult.CompletedSynchronously);
                }
            }
            catch (Exception exception)
            {
                if (this.HandleFailure(request, exception))
                {
                    throw;
                }
            }
            finally
            {
                this.HandleCompleted(request);
            }
        }

        protected virtual void CompleteCheck(PerRequest value, InternalError errorcode)
        {
            if ((value == null) || value.RequestCompleted)
            {
                Error.ThrowInternalError(errorcode);
            }
        }

        protected abstract void CompletedRequest();
        internal static T EndExecute<T>(object source, string method, IAsyncResult asyncResult) where T: BaseAsyncResult
        {
            Util.CheckArgumentNull<IAsyncResult>(asyncResult, "asyncResult");
            T local = asyncResult as T;
            if (((local == null) || (source != local.Source)) || (local.Method != method))
            {
                throw Error.Argument(Strings.Context_DidNotOriginateAsync, "asyncResult");
            }
            if (!local.IsCompleted)
            {
                local.AsyncWaitHandle.WaitOne();
            }
            if (Interlocked.Exchange(ref local.done, 1) != 0)
            {
                throw Error.Argument(Strings.Context_AsyncAlreadyDone, "asyncResult");
            }
            if (local.asyncWait != null)
            {
                Interlocked.CompareExchange(ref local.asyncWaitDisposeLock, new object(), null);
                lock (local.asyncWaitDisposeLock)
                {
                    local.asyncWaitDisposed = true;
                    Util.Dispose<ManualResetEvent>(local.asyncWait);
                }
            }
            if (local.IsAborted)
            {
                throw Error.InvalidOperation(Strings.Context_OperationCanceled);
            }
            if (local.Failure == null)
            {
                return local;
            }
            if (Util.IsKnownClientExcption(local.Failure))
            {
                throw local.Failure;
            }
            throw Error.InvalidOperation(Strings.DataServiceException_GeneralError, local.Failure);
        }

        protected static void EqualRefCheck(PerRequest actual, PerRequest expected, InternalError errorcode)
        {
            if (!object.ReferenceEquals(actual, expected))
            {
                Error.ThrowInternalError(errorcode);
            }
        }

        protected virtual void FinishCurrentChange(PerRequest pereq)
        {
            if (!pereq.RequestCompleted)
            {
                Error.ThrowInternalError(InternalError.SaveNextChangeIncomplete);
            }
            PerRequest perRequest = this.perRequest;
            if (perRequest != null)
            {
                EqualRefCheck(perRequest, pereq, InternalError.InvalidSaveNextChange);
            }
        }

        private static AsyncCallback GetDataServiceAsyncCallback(AsyncCallback callback)
        {
            return delegate (IAsyncResult asyncResult) {
                if (!asyncResult.CompletedSynchronously)
                {
                    callback(asyncResult);
                }
            };
        }

        internal void HandleCompleted()
        {
            if (this.IsCompletedInternally && (Interlocked.Exchange(ref this.userNotified, 1) == 0))
            {
                this.abortable = null;
                try
                {
                    if (CommonUtil.IsCatchableExceptionType(this.Failure))
                    {
                        this.CompletedRequest();
                    }
                }
                catch (Exception exception)
                {
                    if (this.HandleFailure(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.userCompleted = true;
                    this.SetAsyncWaitHandle();
                    if (((this.userCallback != null) && !(this.Failure is ThreadAbortException)) && !(this.Failure is StackOverflowException))
                    {
                        this.userCallback(this);
                    }
                }
            }
        }

        protected abstract void HandleCompleted(PerRequest pereq);
        internal bool HandleFailure(Exception e)
        {
            Interlocked.CompareExchange<Exception>(ref this.failure, e, null);
            this.SetCompleted();
            return !CommonUtil.IsCatchableExceptionType(e);
        }

        protected bool HandleFailure(PerRequest pereq, Exception e)
        {
            if (pereq != null)
            {
                if (this.IsAborted)
                {
                    pereq.SetAborted();
                }
                else
                {
                    pereq.SetComplete();
                }
            }
            return this.HandleFailure(e);
        }

        internal static IAsyncResult InvokeAsync(Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult> asyncAction, ODataRequestMessageWrapper request, AsyncCallback callback, object state)
        {
            return PostInvokeAsync(asyncAction(request, GetDataServiceAsyncCallback(callback), state), callback);
        }

        internal static IAsyncResult InvokeAsync(AsyncAction asyncAction, byte[] buffer, int offset, int length, AsyncCallback callback, object state)
        {
            return PostInvokeAsync(asyncAction(buffer, offset, length, GetDataServiceAsyncCallback(callback), state), callback);
        }

        private static IAsyncResult PostInvokeAsync(IAsyncResult asyncResult, AsyncCallback callback)
        {
            if (asyncResult.CompletedSynchronously)
            {
                callback(asyncResult);
            }
            return asyncResult;
        }

        internal void SetAborted()
        {
            Interlocked.Exchange(ref this.completed, 2);
        }

        private void SetAsyncWaitHandle()
        {
            if (this.asyncWait != null)
            {
                Interlocked.CompareExchange(ref this.asyncWaitDisposeLock, new object(), null);
                lock (this.asyncWaitDisposeLock)
                {
                    if (!this.asyncWaitDisposed)
                    {
                        this.asyncWait.Set();
                    }
                }
            }
        }

        internal void SetCompleted()
        {
            Interlocked.CompareExchange(ref this.completed, 1, 0);
        }

        internal void SetCompletedSynchronously(bool isCompletedSynchronously)
        {
            Interlocked.CompareExchange(ref this.completedSynchronously, isCompletedSynchronously ? 1 : 0, 1);
        }

        internal ODataRequestMessageWrapper Abortable
        {
            get
            {
                return this.abortable;
            }
            set
            {
                this.abortable = value;
                if ((value != null) && this.IsAborted)
                {
                    value.Abort();
                }
            }
        }

        public object AsyncState
        {
            get
            {
                return this.userState;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.asyncWait == null)
                {
                    Interlocked.CompareExchange<ManualResetEvent>(ref this.asyncWait, new ManualResetEvent(this.IsCompleted), null);
                    if (this.IsCompleted)
                    {
                        this.SetAsyncWaitHandle();
                    }
                }
                return this.asyncWait;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return (this.completedSynchronously == 1);
            }
        }

        internal Exception Failure
        {
            get
            {
                return this.failure;
            }
        }

        internal bool IsAborted
        {
            get
            {
                return (2 == this.completed);
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.userCompleted;
            }
        }

        internal bool IsCompletedInternally
        {
            get
            {
                return (0 != this.completed);
            }
        }

        internal delegate IAsyncResult AsyncAction(byte[] buffer, int offset, int length, AsyncCallback asyncCallback, object state);

        protected sealed class AsyncStateBag
        {
            internal readonly DataServiceContext Context;
            internal readonly System.Data.Services.Client.BaseAsyncResult.PerRequest PerRequest;

            internal AsyncStateBag(System.Data.Services.Client.BaseAsyncResult.PerRequest pereq, DataServiceContext context)
            {
                this.PerRequest = pereq;
                this.Context = context;
            }
        }

        internal sealed class ContentStream
        {
            private readonly bool isKnownMemoryStream;
            private readonly System.IO.Stream stream;

            public ContentStream(System.IO.Stream stream, bool isKnownMemoryStream)
            {
                this.stream = stream;
                this.isKnownMemoryStream = isKnownMemoryStream;
            }

            public bool IsKnownMemoryStream
            {
                get
                {
                    return this.isKnownMemoryStream;
                }
            }

            public System.IO.Stream Stream
            {
                get
                {
                    return this.stream;
                }
            }
        }

        protected sealed class PerRequest
        {
            private object disposeLock = new object();
            private const int False = 0;
            private bool isDisposed;
            private int requestCompletedSynchronously = 1;
            private byte[] requestContentBuffer;
            private int requestStatus;
            private const int True = 1;

            internal PerRequest()
            {
            }

            internal void Dispose()
            {
                if (!this.isDisposed)
                {
                    lock (this.disposeLock)
                    {
                        if (!this.isDisposed)
                        {
                            this.isDisposed = true;
                            if (this.ResponseStream != null)
                            {
                                this.ResponseStream.Dispose();
                                this.ResponseStream = null;
                            }
                            if (this.RequestContentStream != null)
                            {
                                if ((this.RequestContentStream.Stream != null) && this.RequestContentStream.IsKnownMemoryStream)
                                {
                                    this.RequestContentStream.Stream.Dispose();
                                }
                                this.RequestContentStream = null;
                            }
                            if (this.RequestStream != null)
                            {
                                try
                                {
                                    this.RequestStream.Dispose();
                                    this.RequestStream = null;
                                }
                                catch (WebException)
                                {
                                    if (!this.RequestAborted)
                                    {
                                        throw;
                                    }
                                }
                            }
                            System.Net.HttpWebResponse httpWebResponse = this.HttpWebResponse;
                            if (httpWebResponse != null)
                            {
                                httpWebResponse.Close();
                            }
                            this.Request = null;
                            this.SetComplete();
                        }
                    }
                }
            }

            internal void SetAborted()
            {
                Interlocked.Exchange(ref this.requestStatus, 2);
            }

            internal void SetComplete()
            {
                Interlocked.CompareExchange(ref this.requestStatus, 1, 0);
            }

            internal void SetRequestCompletedSynchronously(bool completedSynchronously)
            {
                Interlocked.CompareExchange(ref this.requestCompletedSynchronously, completedSynchronously ? 1 : 0, 1);
            }

            internal System.Net.HttpWebResponse HttpWebResponse { get; set; }

            internal ODataRequestMessageWrapper Request { get; set; }

            internal bool RequestAborted
            {
                get
                {
                    return (this.requestStatus == 2);
                }
            }

            internal bool RequestCompleted
            {
                get
                {
                    return (this.requestStatus != 0);
                }
            }

            internal bool RequestCompletedSynchronously
            {
                get
                {
                    return (this.requestCompletedSynchronously == 1);
                }
            }

            internal byte[] RequestContentBuffer
            {
                get
                {
                    if (this.requestContentBuffer == null)
                    {
                        this.requestContentBuffer = new byte[0x10000];
                    }
                    return this.requestContentBuffer;
                }
            }

            internal int RequestContentBufferValidLength { get; set; }

            internal BaseAsyncResult.ContentStream RequestContentStream { get; set; }

            internal Stream RequestStream { get; set; }

            internal Stream ResponseStream { get; set; }
        }
    }
}

