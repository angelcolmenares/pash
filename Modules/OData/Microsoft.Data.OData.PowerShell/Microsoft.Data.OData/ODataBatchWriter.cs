namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    internal sealed class ODataBatchWriter : IODataBatchOperationListener, IODataOutputInStreamErrorListener
    {
        private readonly string batchBoundary;
        private bool batchStartBoundaryWritten;
        private string changeSetBoundary;
        private bool changesetStartBoundaryWritten;
        private uint currentBatchSize;
        private uint currentChangeSetSize;
        private string currentOperationContentId;
        private ODataBatchOperationRequestMessage currentOperationRequestMessage;
        private ODataBatchOperationResponseMessage currentOperationResponseMessage;
        private readonly ODataRawOutputContext rawOutputContext;
        private BatchWriterState state;
        private readonly ODataBatchUrlResolver urlResolver;

        internal ODataBatchWriter(ODataRawOutputContext rawOutputContext, string batchBoundary)
        {
            ExceptionUtils.CheckArgumentNotNull<string>(batchBoundary, "batchBoundary");
            this.rawOutputContext = rawOutputContext;
            this.batchBoundary = batchBoundary;
            this.urlResolver = new ODataBatchUrlResolver(rawOutputContext.UrlResolver);
            this.rawOutputContext.InitializeTextWriter();
        }

        public ODataBatchOperationRequestMessage CreateOperationRequestMessage(string method, Uri uri)
        {
            this.VerifyCanCreateOperationRequestMessage(true, method, uri);
            return this.CreateOperationRequestMessageImplementation(method, uri);
        }

        public Task<ODataBatchOperationRequestMessage> CreateOperationRequestMessageAsync(string method, Uri uri)
        {
            this.VerifyCanCreateOperationRequestMessage(false, method, uri);
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchOperationRequestMessage>(() => this.CreateOperationRequestMessageImplementation(method, uri));
        }

        private ODataBatchOperationRequestMessage CreateOperationRequestMessageImplementation(string method, Uri uri)
        {
            if (this.changeSetBoundary == null)
            {
                this.InterceptException(new Action(this.IncreaseBatchSize));
            }
            else
            {
                this.InterceptException(new Action(this.IncreaseChangeSetSize));
            }
            this.WritePendingMessageData(true);
            if (this.currentOperationContentId != null)
            {
                this.urlResolver.AddContentId(this.currentOperationContentId);
            }
            this.InterceptException(delegate {
                uri = ODataBatchUtils.CreateOperationRequestUri(uri, this.rawOutputContext.MessageWriterSettings.BaseUri, this.urlResolver);
            });
            this.CurrentOperationRequestMessage = ODataBatchOperationRequestMessage.CreateWriteMessage(this.rawOutputContext.OutputStream, method, uri, this, this.urlResolver);
            this.SetState(BatchWriterState.OperationCreated);
            this.WriteStartBoundaryForOperation();
            ODataBatchWriterUtils.WriteRequestPreamble(this.rawOutputContext.TextWriter, method, uri);
            return this.CurrentOperationRequestMessage;
        }

        public ODataBatchOperationResponseMessage CreateOperationResponseMessage()
        {
            this.VerifyCanCreateOperationResponseMessage(true);
            return this.CreateOperationResponseMessageImplementation();
        }

        public Task<ODataBatchOperationResponseMessage> CreateOperationResponseMessageAsync()
        {
            this.VerifyCanCreateOperationResponseMessage(false);
            return TaskUtils.GetTaskForSynchronousOperation<ODataBatchOperationResponseMessage>(new Func<ODataBatchOperationResponseMessage>(this.CreateOperationResponseMessageImplementation));
        }

        private ODataBatchOperationResponseMessage CreateOperationResponseMessageImplementation()
        {
            this.WritePendingMessageData(true);
            this.CurrentOperationResponseMessage = ODataBatchOperationResponseMessage.CreateWriteMessage(this.rawOutputContext.OutputStream, this, this.urlResolver.BatchMessageUrlResolver);
            this.SetState(BatchWriterState.OperationCreated);
            this.WriteStartBoundaryForOperation();
            ODataBatchWriterUtils.WriteResponsePreamble(this.rawOutputContext.TextWriter);
            return this.CurrentOperationResponseMessage;
        }

        private void DisposeBatchWriterAndSetContentStreamRequestedState()
        {
            this.rawOutputContext.CloseTextWriter();
            this.SetState(BatchWriterState.OperationStreamRequested);
        }

        public void Flush()
        {
            this.VerifyCanFlush(true);
            try
            {
                this.rawOutputContext.Flush();
            }
            catch
            {
                this.SetState(BatchWriterState.Error);
                throw;
            }
        }

        public Task FlushAsync()
        {
            this.VerifyCanFlush(false);
            return this.rawOutputContext.FlushAsync().FollowOnFaultWith(delegate (Task t) {
                this.SetState(BatchWriterState.Error);
            });
        }

        private void IncreaseBatchSize()
        {
            this.currentBatchSize++;
            if (this.currentBatchSize > this.rawOutputContext.MessageWriterSettings.MessageQuotas.MaxPartsPerBatch)
            {
                throw new ODataException(Strings.ODataBatchWriter_MaxBatchSizeExceeded(this.rawOutputContext.MessageWriterSettings.MessageQuotas.MaxPartsPerBatch));
            }
        }

        private void IncreaseChangeSetSize()
        {
            this.currentChangeSetSize++;
            if (this.currentChangeSetSize > this.rawOutputContext.MessageWriterSettings.MessageQuotas.MaxOperationsPerChangeset)
            {
                throw new ODataException(Strings.ODataBatchWriter_MaxChangeSetSizeExceeded(this.rawOutputContext.MessageWriterSettings.MessageQuotas.MaxOperationsPerChangeset));
            }
        }

        private void InterceptException(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                if (!IsErrorState(this.state))
                {
                    this.SetState(BatchWriterState.Error);
                }
                throw;
            }
        }

        private static bool IsErrorState(BatchWriterState state)
        {
            return (state == BatchWriterState.Error);
        }

        void IODataBatchOperationListener.BatchOperationContentStreamDisposed()
        {
            this.SetState(BatchWriterState.OperationStreamDisposed);
            this.CurrentOperationRequestMessage = null;
            this.CurrentOperationResponseMessage = null;
            this.rawOutputContext.InitializeTextWriter();
        }

        void IODataBatchOperationListener.BatchOperationContentStreamRequested()
        {
            this.StartBatchOperationContent();
            this.rawOutputContext.FlushBuffers();
            this.DisposeBatchWriterAndSetContentStreamRequestedState();
        }

        Task IODataBatchOperationListener.BatchOperationContentStreamRequestedAsync()
        {
            this.StartBatchOperationContent();
            return this.rawOutputContext.FlushBuffersAsync().FollowOnSuccessWith(delegate (Task task) {
                this.DisposeBatchWriterAndSetContentStreamRequestedState();
            });
        }

        void IODataOutputInStreamErrorListener.OnInStreamError()
        {
            this.rawOutputContext.VerifyNotDisposed();
            this.SetState(BatchWriterState.Error);
            this.rawOutputContext.TextWriter.Flush();
            throw new ODataException(Strings.ODataBatchWriter_CannotWriteInStreamErrorForBatch);
        }

        private void RememberContentIdHeader(string contentId)
        {
            this.currentOperationContentId = contentId;
            if ((contentId != null) && this.urlResolver.ContainsContentId(contentId))
            {
                throw new ODataException(Strings.ODataBatchWriter_DuplicateContentIDsNotAllowed(contentId));
            }
        }

        private void ResetChangeSetSize()
        {
            this.currentChangeSetSize = 0;
        }

        private void SetState(BatchWriterState newState)
        {
            this.InterceptException(() => this.ValidateTransition(newState));
            switch (newState)
            {
                case BatchWriterState.ChangeSetStarted:
                    this.changeSetBoundary = ODataBatchWriterUtils.CreateChangeSetBoundary(this.rawOutputContext.WritingResponse);
                    break;

                case BatchWriterState.ChangeSetCompleted:
                    this.changeSetBoundary = null;
                    break;
            }
            this.state = newState;
        }

        private void StartBatchOperationContent()
        {
            this.WritePendingMessageData(false);
            this.rawOutputContext.TextWriter.Flush();
        }

        private void ThrowODataException(string errorMessage)
        {
            this.SetState(BatchWriterState.Error);
            throw new ODataException(errorMessage);
        }

        private void ValidateTransition(BatchWriterState newState)
        {
            if (IsErrorState(this.state) || !IsErrorState(newState))
            {
                if ((newState == BatchWriterState.ChangeSetStarted) && (this.changeSetBoundary != null))
                {
                    throw new ODataException(Strings.ODataBatchWriter_CannotStartChangeSetWithActiveChangeSet);
                }
                if ((newState == BatchWriterState.ChangeSetCompleted) && (this.changeSetBoundary == null))
                {
                    throw new ODataException(Strings.ODataBatchWriter_CannotCompleteChangeSetWithoutActiveChangeSet);
                }
                if ((newState == BatchWriterState.BatchCompleted) && (this.changeSetBoundary != null))
                {
                    throw new ODataException(Strings.ODataBatchWriter_CannotCompleteBatchWithActiveChangeSet);
                }
                switch (this.state)
                {
                    case BatchWriterState.Start:
                        if (newState != BatchWriterState.BatchStarted)
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromStart);
                        }
                        break;

                    case BatchWriterState.BatchStarted:
                        if (((newState != BatchWriterState.ChangeSetStarted) && (newState != BatchWriterState.OperationCreated)) && (newState != BatchWriterState.BatchCompleted))
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromBatchStarted);
                        }
                        break;

                    case BatchWriterState.ChangeSetStarted:
                        if ((newState != BatchWriterState.OperationCreated) && (newState != BatchWriterState.ChangeSetCompleted))
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromChangeSetStarted);
                        }
                        break;

                    case BatchWriterState.OperationCreated:
                        if ((((newState != BatchWriterState.OperationCreated) && (newState != BatchWriterState.OperationStreamRequested)) && ((newState != BatchWriterState.ChangeSetStarted) && (newState != BatchWriterState.ChangeSetCompleted))) && (newState != BatchWriterState.BatchCompleted))
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromOperationCreated);
                        }
                        break;

                    case BatchWriterState.OperationStreamRequested:
                        if (newState != BatchWriterState.OperationStreamDisposed)
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromOperationContentStreamRequested);
                        }
                        break;

                    case BatchWriterState.OperationStreamDisposed:
                        if (((newState != BatchWriterState.OperationCreated) && (newState != BatchWriterState.ChangeSetStarted)) && ((newState != BatchWriterState.ChangeSetCompleted) && (newState != BatchWriterState.BatchCompleted)))
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromOperationContentStreamDisposed);
                        }
                        break;

                    case BatchWriterState.ChangeSetCompleted:
                        if (((newState != BatchWriterState.BatchCompleted) && (newState != BatchWriterState.ChangeSetStarted)) && (newState != BatchWriterState.OperationCreated))
                        {
                            throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromChangeSetCompleted);
                        }
                        break;

                    case BatchWriterState.BatchCompleted:
                        throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromBatchCompleted);

                    case BatchWriterState.Error:
                        if (newState != BatchWriterState.Error)
                        {
                            throw new ODataException(Strings.ODataWriterCore_InvalidTransitionFromError(this.state.ToString(), newState.ToString()));
                        }
                        break;

                    default:
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchWriter_ValidateTransition_UnreachableCodePath));
                }
            }
        }

        private void ValidateWriterReady()
        {
            this.rawOutputContext.VerifyNotDisposed();
            if (this.state == BatchWriterState.OperationStreamRequested)
            {
                throw new ODataException(Strings.ODataBatchWriter_InvalidTransitionFromOperationContentStreamRequested);
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.rawOutputContext.Synchronous)
                {
                    throw new ODataException(Strings.ODataBatchWriter_SyncCallOnAsyncWriter);
                }
            }
            else if (this.rawOutputContext.Synchronous)
            {
                throw new ODataException(Strings.ODataBatchWriter_AsyncCallOnSyncWriter);
            }
        }

        private void VerifyCanCreateOperationRequestMessage(bool synchronousCall, string method, Uri uri)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
            if (this.rawOutputContext.WritingResponse)
            {
                this.ThrowODataException(Strings.ODataBatchWriter_CannotCreateRequestOperationWhenWritingResponse);
            }
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(method, "method");
            if (this.changeSetBoundary == null)
            {
                if (string.CompareOrdinal(method, "GET") != 0)
                {
                    this.ThrowODataException(Strings.ODataBatch_InvalidHttpMethodForQueryOperation(method));
                }
            }
            else if (string.CompareOrdinal(method, "GET") == 0)
            {
                this.ThrowODataException(Strings.ODataBatch_InvalidHttpMethodForChangeSetRequest(method));
            }
            ExceptionUtils.CheckArgumentNotNull<Uri>(uri, "uri");
        }

        private void VerifyCanCreateOperationResponseMessage(bool synchronousCall)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
            if (!this.rawOutputContext.WritingResponse)
            {
                this.ThrowODataException(Strings.ODataBatchWriter_CannotCreateResponseOperationWhenWritingRequest);
            }
        }

        private void VerifyCanFlush(bool synchronousCall)
        {
            this.rawOutputContext.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if (this.state == BatchWriterState.OperationStreamRequested)
            {
                this.ThrowODataException(Strings.ODataBatchWriter_FlushOrFlushAsyncCalledInStreamRequestedState);
            }
        }

        private void VerifyCanWriteEndBatch(bool synchronousCall)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteEndChangeset(bool synchronousCall)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteStartBatch(bool synchronousCall)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteStartChangeset(bool synchronousCall)
        {
            this.ValidateWriterReady();
            this.VerifyCallAllowed(synchronousCall);
        }

        public void WriteEndBatch()
        {
            this.VerifyCanWriteEndBatch(true);
            this.WriteEndBatchImplementation();
            this.Flush();
        }

        public Task WriteEndBatchAsync()
        {
            this.VerifyCanWriteEndBatch(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteEndBatchImplementation)).FollowOnSuccessWithTask(task => this.FlushAsync());
        }

        private void WriteEndBatchImplementation()
        {
            this.WritePendingMessageData(true);
            this.SetState(BatchWriterState.BatchCompleted);
            ODataBatchWriterUtils.WriteEndBoundary(this.rawOutputContext.TextWriter, this.batchBoundary, !this.batchStartBoundaryWritten);
            this.rawOutputContext.TextWriter.WriteLine();
        }

        public void WriteEndChangeset()
        {
            this.VerifyCanWriteEndChangeset(true);
            this.WriteEndChangesetImplementation();
        }

        public Task WriteEndChangesetAsync()
        {
            this.VerifyCanWriteEndChangeset(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteEndChangesetImplementation));
        }

        private void WriteEndChangesetImplementation()
        {
            this.WritePendingMessageData(true);
            string changeSetBoundary = this.changeSetBoundary;
            this.SetState(BatchWriterState.ChangeSetCompleted);
            ODataBatchWriterUtils.WriteEndBoundary(this.rawOutputContext.TextWriter, changeSetBoundary, !this.changesetStartBoundaryWritten);
            this.urlResolver.Reset();
            this.currentOperationContentId = null;
        }

        private void WritePendingMessageData(bool reportMessageCompleted)
        {
            if (this.CurrentOperationMessage != null)
            {
                if (this.CurrentOperationResponseMessage != null)
                {
                    int statusCode = this.CurrentOperationResponseMessage.StatusCode;
                    string statusMessage = HttpUtils.GetStatusMessage(statusCode);
                    this.rawOutputContext.TextWriter.WriteLine("{0} {1} {2}", "HTTP/1.1", statusCode, statusMessage);
                }
                bool flag = (this.CurrentOperationRequestMessage != null) && (this.changeSetBoundary != null);
                string contentId = null;
                IEnumerable<KeyValuePair<string, string>> headers = this.CurrentOperationMessage.Headers;
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        string key = pair.Key;
                        string str4 = pair.Value;
                        this.rawOutputContext.TextWriter.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { key, str4 }));
                        if (flag && (string.CompareOrdinal("Content-ID", key) == 0))
                        {
                            contentId = str4;
                        }
                    }
                }
                if (flag)
                {
                    this.RememberContentIdHeader(contentId);
                }
                this.rawOutputContext.TextWriter.WriteLine();
                if (reportMessageCompleted)
                {
                    this.CurrentOperationMessage.PartHeaderProcessingCompleted();
                    this.CurrentOperationRequestMessage = null;
                    this.CurrentOperationResponseMessage = null;
                }
            }
        }

        public void WriteStartBatch()
        {
            this.VerifyCanWriteStartBatch(true);
            this.WriteStartBatchImplementation();
        }

        public Task WriteStartBatchAsync()
        {
            this.VerifyCanWriteStartBatch(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteStartBatchImplementation));
        }

        private void WriteStartBatchImplementation()
        {
            this.SetState(BatchWriterState.BatchStarted);
        }

        private void WriteStartBoundaryForOperation()
        {
            if (this.changeSetBoundary == null)
            {
                ODataBatchWriterUtils.WriteStartBoundary(this.rawOutputContext.TextWriter, this.batchBoundary, !this.batchStartBoundaryWritten);
                this.batchStartBoundaryWritten = true;
            }
            else
            {
                ODataBatchWriterUtils.WriteStartBoundary(this.rawOutputContext.TextWriter, this.changeSetBoundary, !this.changesetStartBoundaryWritten);
                this.changesetStartBoundaryWritten = true;
            }
        }

        public void WriteStartChangeset()
        {
            this.VerifyCanWriteStartChangeset(true);
            this.WriteStartChangesetImplementation();
        }

        public Task WriteStartChangesetAsync()
        {
            this.VerifyCanWriteStartChangeset(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteStartChangesetImplementation));
        }

        private void WriteStartChangesetImplementation()
        {
            this.WritePendingMessageData(true);
            this.SetState(BatchWriterState.ChangeSetStarted);
            this.ResetChangeSetSize();
            this.InterceptException(new Action(this.IncreaseBatchSize));
            ODataBatchWriterUtils.WriteStartBoundary(this.rawOutputContext.TextWriter, this.batchBoundary, !this.batchStartBoundaryWritten);
            this.batchStartBoundaryWritten = true;
            ODataBatchWriterUtils.WriteChangeSetPreamble(this.rawOutputContext.TextWriter, this.changeSetBoundary);
            this.changesetStartBoundaryWritten = false;
        }

        private ODataBatchOperationMessage CurrentOperationMessage
        {
            get
            {
                if (this.currentOperationRequestMessage != null)
                {
                    return this.currentOperationRequestMessage.OperationMessage;
                }
                if (this.currentOperationResponseMessage != null)
                {
                    return this.currentOperationResponseMessage.OperationMessage;
                }
                return null;
            }
        }

        private ODataBatchOperationRequestMessage CurrentOperationRequestMessage
        {
            get
            {
                return this.currentOperationRequestMessage;
            }
            set
            {
                this.currentOperationRequestMessage = value;
            }
        }

        private ODataBatchOperationResponseMessage CurrentOperationResponseMessage
        {
            get
            {
                return this.currentOperationResponseMessage;
            }
            set
            {
                this.currentOperationResponseMessage = value;
            }
        }

        private enum BatchWriterState
        {
            Start,
            BatchStarted,
            ChangeSetStarted,
            OperationCreated,
            OperationStreamRequested,
            OperationStreamDisposed,
            ChangeSetCompleted,
            BatchCompleted,
            Error
        }
    }
}

