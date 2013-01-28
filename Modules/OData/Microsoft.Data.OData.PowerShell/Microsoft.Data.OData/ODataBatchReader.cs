namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class ODataBatchReader : IODataBatchOperationListener
    {
        private ODataBatchReaderState batchReaderState;
        private readonly ODataBatchReaderStream batchStream;
        private string contentIdToAddOnNextRead;
        private uint currentBatchSize;
        private uint currentChangeSetSize;
        private readonly ODataRawInputContext inputContext;
        private OperationState operationState;
        private readonly bool synchronous;
        private readonly ODataBatchUrlResolver urlResolver;

        internal ODataBatchReader(ODataRawInputContext inputContext, string batchBoundary, Encoding batchEncoding, bool synchronous)
        {
            this.inputContext = inputContext;
            this.synchronous = synchronous;
            this.urlResolver = new ODataBatchUrlResolver(inputContext.UrlResolver);
            this.batchStream = new ODataBatchReaderStream(inputContext, batchBoundary, batchEncoding);
        }

        public ODataBatchOperationRequestMessage CreateOperationRequestMessage()
        {
            this.VerifyCanCreateOperationRequestMessage(true);
            return this.InterceptException<ODataBatchOperationRequestMessage>(new Func<ODataBatchOperationRequestMessage>(this.CreateOperationRequestMessageImplementation));
        }

        public Task<ODataBatchOperationRequestMessage> CreateOperationRequestMessageAsync()
        {
            this.VerifyCanCreateOperationRequestMessage(false);
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchOperationRequestMessage>(new Func<ODataBatchOperationRequestMessage>(this.CreateOperationRequestMessageImplementation)).FollowOnFaultWith<ODataBatchOperationRequestMessage>(delegate (Task<ODataBatchOperationRequestMessage> t) {
                this.State = ODataBatchReaderState.Exception;
            });
        }

        private ODataBatchOperationRequestMessage CreateOperationRequestMessageImplementation()
        {
            string str2;
            Uri uri;
            string str3;
            this.operationState = OperationState.MessageCreated;
            string requestLine = this.batchStream.ReadLine();
            while (requestLine.Length == 0)
            {
                requestLine = this.batchStream.ReadLine();
            }
            this.ParseRequestLine(requestLine, out str2, out uri);
            ODataBatchOperationHeaders headers = this.batchStream.ReadHeaders();
            ODataBatchOperationRequestMessage message = ODataBatchOperationRequestMessage.CreateReadMessage(this.batchStream, str2, uri, headers, this, this.urlResolver);
            if (headers.TryGetValue("Content-ID", out str3))
            {
                if ((str3 != null) && this.urlResolver.ContainsContentId(str3))
                {
                    throw new ODataException(Strings.ODataBatchReader_DuplicateContentIDsNotAllowed(str3));
                }
                this.contentIdToAddOnNextRead = str3;
            }
            return message;
        }

        public ODataBatchOperationResponseMessage CreateOperationResponseMessage()
        {
            this.VerifyCanCreateOperationResponseMessage(true);
            return this.InterceptException<ODataBatchOperationResponseMessage>(new Func<ODataBatchOperationResponseMessage>(this.CreateOperationResponseMessageImplementation));
        }

        public Task<ODataBatchOperationResponseMessage> CreateOperationResponseMessageAsync()
        {
            this.VerifyCanCreateOperationResponseMessage(false);
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchOperationResponseMessage>(new Func<ODataBatchOperationResponseMessage>(this.CreateOperationResponseMessageImplementation)).FollowOnFaultWith<ODataBatchOperationResponseMessage>(delegate (Task<ODataBatchOperationResponseMessage> t) {
                this.State = ODataBatchReaderState.Exception;
            });
        }

        private ODataBatchOperationResponseMessage CreateOperationResponseMessageImplementation()
        {
            this.operationState = OperationState.MessageCreated;
            string responseLine = this.batchStream.ReadLine();
            while (responseLine.Length == 0)
            {
                responseLine = this.batchStream.ReadLine();
            }
            int statusCode = this.ParseResponseLine(responseLine);
            ODataBatchOperationHeaders headers = this.batchStream.ReadHeaders();
            return ODataBatchOperationResponseMessage.CreateReadMessage(this.batchStream, statusCode, headers, this, this.urlResolver.BatchMessageUrlResolver);
        }

        private ODataBatchReaderState GetEndBoundaryState()
        {
            switch (this.batchReaderState)
            {
                case ODataBatchReaderState.Initial:
                    return ODataBatchReaderState.Completed;

                case ODataBatchReaderState.Operation:
                    if (this.batchStream.ChangeSetBoundary == null)
                    {
                        return ODataBatchReaderState.Completed;
                    }
                    return ODataBatchReaderState.ChangesetEnd;

                case ODataBatchReaderState.ChangesetStart:
                    return ODataBatchReaderState.ChangesetEnd;

                case ODataBatchReaderState.ChangesetEnd:
                    return ODataBatchReaderState.Completed;

                case ODataBatchReaderState.Completed:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReader_GetEndBoundary_Completed));

                case ODataBatchReaderState.Exception:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReader_GetEndBoundary_Exception));
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReader_GetEndBoundary_UnknownValue));
        }

        private void IncreaseBatchSize()
        {
            this.currentBatchSize++;
            if (this.currentBatchSize > this.inputContext.MessageReaderSettings.MessageQuotas.MaxPartsPerBatch)
            {
                throw new ODataException(Strings.ODataBatchReader_MaxBatchSizeExceeded(this.inputContext.MessageReaderSettings.MessageQuotas.MaxPartsPerBatch));
            }
        }

        private void IncreaseChangeSetSize()
        {
            this.currentChangeSetSize++;
            if (this.currentChangeSetSize > this.inputContext.MessageReaderSettings.MessageQuotas.MaxOperationsPerChangeset)
            {
                throw new ODataException(Strings.ODataBatchReader_MaxChangeSetSizeExceeded(this.inputContext.MessageReaderSettings.MessageQuotas.MaxOperationsPerChangeset));
            }
        }

        private T InterceptException<T>(Func<T> action)
        {
            T local;
            try
            {
                local = action();
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    this.State = ODataBatchReaderState.Exception;
                }
                throw;
            }
            return local;
        }

        void IODataBatchOperationListener.BatchOperationContentStreamDisposed()
        {
            this.operationState = OperationState.StreamDisposed;
        }

        void IODataBatchOperationListener.BatchOperationContentStreamRequested()
        {
            this.operationState = OperationState.StreamRequested;
        }

        Task IODataBatchOperationListener.BatchOperationContentStreamRequestedAsync()
        {
            this.operationState = OperationState.StreamRequested;
            return TaskUtils.CompletedTask;
        }

        private void ParseRequestLine(string requestLine, out string httpMethod, out Uri requestUri)
        {
            int index = requestLine.IndexOf(' ');
            if ((index <= 0) || ((requestLine.Length - 3) <= index))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidRequestLine(requestLine));
            }
            int num2 = requestLine.LastIndexOf(' ');
            if (((num2 < 0) || (((num2 - index) - 1) <= 0)) || ((requestLine.Length - 1) <= num2))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidRequestLine(requestLine));
            }
            httpMethod = requestLine.Substring(0, index);
            string uriString = requestLine.Substring(index + 1, (num2 - index) - 1);
            string strB = requestLine.Substring(num2 + 1);
            if (string.CompareOrdinal("HTTP/1.1", strB) != 0)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidHttpVersionSpecified(strB, "HTTP/1.1"));
            }
            HttpUtils.ValidateHttpMethod(httpMethod);
            if (this.batchStream.ChangeSetBoundary == null)
            {
                if (string.CompareOrdinal(httpMethod, "GET") != 0)
                {
                    throw new ODataException(Strings.ODataBatch_InvalidHttpMethodForQueryOperation(httpMethod));
                }
            }
            else if (string.CompareOrdinal(httpMethod, "GET") == 0)
            {
                throw new ODataException(Strings.ODataBatch_InvalidHttpMethodForChangeSetRequest(httpMethod));
            }
            requestUri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            requestUri = ODataBatchUtils.CreateOperationRequestUri(requestUri, this.inputContext.MessageReaderSettings.BaseUri, this.urlResolver);
        }

        private int ParseResponseLine(string responseLine)
        {
            int num3;
            int index = responseLine.IndexOf(' ');
            if ((index <= 0) || ((responseLine.Length - 3) <= index))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidResponseLine(responseLine));
            }
            int num2 = responseLine.IndexOf(' ', index + 1);
            if (((num2 < 0) || (((num2 - index) - 1) <= 0)) || ((responseLine.Length - 1) <= num2))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidResponseLine(responseLine));
            }
            string strB = responseLine.Substring(0, index);
            string s = responseLine.Substring(index + 1, (num2 - index) - 1);
            if (string.CompareOrdinal("HTTP/1.1", strB) != 0)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidHttpVersionSpecified(strB, "HTTP/1.1"));
            }
            if (!int.TryParse(s, out num3))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_NonIntegerHttpStatusCode(s));
            }
            return num3;
        }

        public bool Read()
        {
            this.VerifyCanRead(true);
            return this.InterceptException<bool>(new Func<bool>(this.ReadSynchronously));
        }

        public Task<bool> ReadAsync()
        {
            this.VerifyCanRead(false);
            return this.ReadAsynchronously().FollowOnFaultWith<bool>(delegate (Task<bool> t) {
                this.State = ODataBatchReaderState.Exception;
            });
        }

        private Task<bool> ReadAsynchronously()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(new Func<bool>(this.ReadImplementation));
        }

        private bool ReadImplementation()
        {
            switch (this.State)
            {
                case ODataBatchReaderState.Initial:
                    this.batchReaderState = this.SkipToNextPartAndReadHeaders();
                    break;

                case ODataBatchReaderState.Operation:
                    if (this.operationState == OperationState.None)
                    {
                        throw new ODataException(Strings.ODataBatchReader_NoMessageWasCreatedForOperation);
                    }
                    this.operationState = OperationState.None;
                    if (this.contentIdToAddOnNextRead != null)
                    {
                        this.urlResolver.AddContentId(this.contentIdToAddOnNextRead);
                        this.contentIdToAddOnNextRead = null;
                    }
                    this.batchReaderState = this.SkipToNextPartAndReadHeaders();
                    break;

                case ODataBatchReaderState.ChangesetStart:
                    this.batchReaderState = this.SkipToNextPartAndReadHeaders();
                    break;

                case ODataBatchReaderState.ChangesetEnd:
                    this.ResetChangeSetSize();
                    this.batchStream.ResetChangeSetBoundary();
                    this.batchReaderState = this.SkipToNextPartAndReadHeaders();
                    break;

                case ODataBatchReaderState.Completed:
                case ODataBatchReaderState.Exception:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReader_ReadImplementation));

                default:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReader_ReadImplementation));
            }
            return ((this.batchReaderState != ODataBatchReaderState.Completed) && (this.batchReaderState != ODataBatchReaderState.Exception));
        }

        private bool ReadSynchronously()
        {
            return this.ReadImplementation();
        }

        private void ResetChangeSetSize()
        {
            this.currentChangeSetSize = 0;
        }

        private ODataBatchReaderState SkipToNextPartAndReadHeaders()
        {
            bool flag;
            bool flag2;
            ODataBatchReaderState endBoundaryState;
            if (!this.batchStream.SkipToBoundary(out flag, out flag2))
            {
                if (this.batchStream.ChangeSetBoundary == null)
                {
                    return ODataBatchReaderState.Completed;
                }
                return ODataBatchReaderState.ChangesetEnd;
            }
            if (flag || flag2)
            {
                endBoundaryState = this.GetEndBoundaryState();
                if (endBoundaryState == ODataBatchReaderState.ChangesetEnd)
                {
                    this.urlResolver.Reset();
                }
                return endBoundaryState;
            }
            bool flag4 = this.batchStream.ChangeSetBoundary != null;
            bool flag5 = this.batchStream.ProcessPartHeader();
            if (flag4)
            {
                endBoundaryState = ODataBatchReaderState.Operation;
                this.IncreaseChangeSetSize();
                return endBoundaryState;
            }
            endBoundaryState = flag5 ? ODataBatchReaderState.ChangesetStart : ODataBatchReaderState.Operation;
            this.IncreaseBatchSize();
            return endBoundaryState;
        }

        private void ThrowODataException(string errorMessage)
        {
            this.State = ODataBatchReaderState.Exception;
            throw new ODataException(errorMessage);
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.synchronous)
                {
                    throw new ODataException(Strings.ODataBatchReader_SyncCallOnAsyncReader);
                }
            }
            else if (this.synchronous)
            {
                throw new ODataException(Strings.ODataBatchReader_AsyncCallOnSyncReader);
            }
        }

        private void VerifyCanCreateOperationRequestMessage(bool synchronousCall)
        {
            this.VerifyReaderReady();
            this.VerifyCallAllowed(synchronousCall);
            if (this.inputContext.ReadingResponse)
            {
                this.ThrowODataException(Strings.ODataBatchReader_CannotCreateRequestOperationWhenReadingResponse);
            }
            if (this.State != ODataBatchReaderState.Operation)
            {
                this.ThrowODataException(Strings.ODataBatchReader_InvalidStateForCreateOperationRequestMessage(this.State));
            }
            if (this.operationState != OperationState.None)
            {
                this.ThrowODataException(Strings.ODataBatchReader_OperationRequestMessageAlreadyCreated);
            }
        }

        private void VerifyCanCreateOperationResponseMessage(bool synchronousCall)
        {
            this.VerifyReaderReady();
            this.VerifyCallAllowed(synchronousCall);
            if (!this.inputContext.ReadingResponse)
            {
                this.ThrowODataException(Strings.ODataBatchReader_CannotCreateResponseOperationWhenReadingRequest);
            }
            if (this.State != ODataBatchReaderState.Operation)
            {
                this.ThrowODataException(Strings.ODataBatchReader_InvalidStateForCreateOperationResponseMessage(this.State));
            }
            if (this.operationState != OperationState.None)
            {
                this.ThrowODataException(Strings.ODataBatchReader_OperationResponseMessageAlreadyCreated);
            }
        }

        private void VerifyCanRead(bool synchronousCall)
        {
            this.VerifyReaderReady();
            this.VerifyCallAllowed(synchronousCall);
            if ((this.State == ODataBatchReaderState.Exception) || (this.State == ODataBatchReaderState.Completed))
            {
                throw new ODataException(Strings.ODataBatchReader_ReadOrReadAsyncCalledInInvalidState(this.State));
            }
        }

        private void VerifyReaderReady()
        {
            this.inputContext.VerifyNotDisposed();
            if (this.operationState == OperationState.StreamRequested)
            {
                throw new ODataException(Strings.ODataBatchReader_CannotUseReaderWhileOperationStreamActive);
            }
        }

        public ODataBatchReaderState State
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.batchReaderState;
            }
            private set
            {
                this.batchReaderState = value;
            }
        }

        private enum OperationState
        {
            None,
            MessageCreated,
            StreamRequested,
            StreamDisposed
        }
    }
}

