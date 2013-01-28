namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal abstract class ODataCollectionWriterCore : ODataCollectionWriter, IODataOutputInStreamErrorListener
    {
        private readonly CollectionWithoutExpectedTypeValidator collectionValidator;
        private Microsoft.Data.OData.DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;
        private readonly IEdmTypeReference expectedItemType;
        private readonly IODataReaderWriterListener listener;
        private readonly ODataOutputContext outputContext;
        private Stack<Scope> scopes = new Stack<Scope>();

        protected ODataCollectionWriterCore(ODataOutputContext outputContext, IEdmTypeReference expectedItemType, IODataReaderWriterListener listener)
        {
            this.outputContext = outputContext;
            this.expectedItemType = expectedItemType;
            this.listener = listener;
            this.scopes.Push(new Scope(CollectionWriterState.Start, null));
            if (this.expectedItemType == null)
            {
                this.collectionValidator = new CollectionWithoutExpectedTypeValidator(null);
            }
        }

        protected abstract void EndCollection();
        protected abstract void EndPayload();
        private void EnterScope(CollectionWriterState newState, object item)
        {
            this.InterceptException(() => this.ValidateTransition(newState));
            this.scopes.Push(new Scope(newState, item));
            this.NotifyListener(newState);
        }

        public sealed override void Flush()
        {
            this.VerifyCanFlush(true);
            try
            {
                this.FlushSynchronously();
            }
            catch
            {
                this.ReplaceScope(CollectionWriterState.Error, null);
                throw;
            }
        }

        public sealed override Task FlushAsync()
        {
            this.VerifyCanFlush(false);
            return this.FlushAsynchronously().FollowOnFaultWith(delegate (Task t) {
                this.ReplaceScope(CollectionWriterState.Error, null);
            });
        }

        protected abstract Task FlushAsynchronously();
        protected abstract void FlushSynchronously();
        private void InterceptException(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                if (!IsErrorState(this.State))
                {
                    this.EnterScope(CollectionWriterState.Error, this.scopes.Peek().Item);
                }
                throw;
            }
        }

        protected static bool IsErrorState(CollectionWriterState state)
        {
            return (state == CollectionWriterState.Error);
        }

        private void LeaveScope()
        {
            this.scopes.Pop();
            if (this.scopes.Count == 1)
            {
                this.scopes.Pop();
                this.scopes.Push(new Scope(CollectionWriterState.Completed, null));
                this.InterceptException(new Action(this.EndPayload));
                this.NotifyListener(CollectionWriterState.Completed);
            }
        }

        void IODataOutputInStreamErrorListener.OnInStreamError()
        {
            this.VerifyNotDisposed();
            if (this.State == CollectionWriterState.Completed)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromCompleted(this.State.ToString(), CollectionWriterState.Error.ToString()));
            }
            this.StartPayloadInStartState();
            this.EnterScope(CollectionWriterState.Error, this.scopes.Peek().Item);
        }

        private void NotifyListener(CollectionWriterState newState)
        {
            if (this.listener != null)
            {
                if (IsErrorState(newState))
                {
                    this.listener.OnException();
                }
                else if (newState == CollectionWriterState.Completed)
                {
                    this.listener.OnCompleted();
                }
            }
        }

        private void ReplaceScope(CollectionWriterState newState, ODataItem item)
        {
            this.ValidateTransition(newState);
            this.scopes.Pop();
            this.scopes.Push(new Scope(newState, item));
            this.NotifyListener(newState);
        }

        protected abstract void StartCollection(ODataCollectionStart collectionStart);
        protected abstract void StartPayload();
        private void StartPayloadInStartState()
        {
            if (this.scopes.Peek().State == CollectionWriterState.Start)
            {
                this.InterceptException(new Action(this.StartPayload));
            }
        }

        private void ValidateTransition(CollectionWriterState newState)
        {
            if (IsErrorState(this.State) || !IsErrorState(newState))
            {
                switch (this.State)
                {
                    case CollectionWriterState.Start:
                        if ((newState != CollectionWriterState.Collection) && (newState != CollectionWriterState.Completed))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_InvalidTransitionFromStart(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case CollectionWriterState.Collection:
                        if ((newState != CollectionWriterState.Item) && (newState != CollectionWriterState.Completed))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_InvalidTransitionFromCollection(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case CollectionWriterState.Item:
                        if (newState != CollectionWriterState.Completed)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_InvalidTransitionFromItem(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case CollectionWriterState.Completed:
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromCompleted(this.State.ToString(), newState.ToString()));

                    case CollectionWriterState.Error:
                        if (newState != CollectionWriterState.Error)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromError(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataCollectionWriterCore_ValidateTransition_UnreachableCodePath));
                }
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.outputContext.Synchronous)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_SyncCallOnAsyncWriter);
                }
            }
            else if (this.outputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_AsyncCallOnSyncWriter);
            }
        }

        private void VerifyCanFlush(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteEnd(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteItem(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteStart(bool synchronousCall, ODataCollectionStart collectionStart)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataCollectionStart>(collectionStart, "collection");
            string name = collectionStart.Name;
            if ((name != null) && (name.Length == 0))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_CollectionsMustNotHaveEmptyName);
            }
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        protected abstract void VerifyNotDisposed();
        protected abstract void WriteCollectionItem(object item, IEdmTypeReference expectedItemTypeReference);
        public sealed override void WriteEnd()
        {
            this.VerifyCanWriteEnd(true);
            this.WriteEndImplementation();
            if (this.scopes.Peek().State == CollectionWriterState.Completed)
            {
                this.Flush();
            }
        }

        public sealed override Task WriteEndAsync()
        {
            this.VerifyCanWriteEnd(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteEndImplementation)).FollowOnSuccessWithTask(delegate (Task task) {
                if (this.scopes.Peek().State == CollectionWriterState.Completed)
                {
                    return this.FlushAsync();
                }
                return TaskUtils.CompletedTask;
            });
        }

        private void WriteEndImplementation()
        {
            this.InterceptException(delegate {
                Scope scope = this.scopes.Peek();
                switch (scope.State)
                {
                    case CollectionWriterState.Start:
                    case CollectionWriterState.Completed:
                    case CollectionWriterState.Error:
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionWriterCore_WriteEndCalledInInvalidState(scope.State.ToString()));

                    case CollectionWriterState.Collection:
                        this.EndCollection();
                        break;

                    case CollectionWriterState.Item:
                        this.LeaveScope();
                        this.EndCollection();
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataCollectionWriterCore_WriteEnd_UnreachableCodePath));
                }
                this.LeaveScope();
            });
        }

        public sealed override void WriteItem(object item)
        {
            this.VerifyCanWriteItem(true);
            this.WriteItemImplementation(item);
        }

        public sealed override Task WriteItemAsync(object item)
        {
            this.VerifyCanWriteItem(false);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteItemImplementation(item);
            });
        }

        private void WriteItemImplementation(object item)
        {
            if (this.scopes.Peek().State != CollectionWriterState.Item)
            {
                this.EnterScope(CollectionWriterState.Item, item);
            }
            this.InterceptException(delegate {
                ValidationUtils.ValidateCollectionItem(item, true);
                this.WriteCollectionItem(item, this.expectedItemType);
            });
        }

        public sealed override void WriteStart(ODataCollectionStart collectionStart)
        {
            this.VerifyCanWriteStart(true, collectionStart);
            this.WriteStartImplementation(collectionStart);
        }

        public sealed override Task WriteStartAsync(ODataCollectionStart collection)
        {
            this.VerifyCanWriteStart(false, collection);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteStartImplementation(collection);
            });
        }

        private void WriteStartImplementation(ODataCollectionStart collectionStart)
        {
            this.StartPayloadInStartState();
            this.EnterScope(CollectionWriterState.Collection, collectionStart);
            this.InterceptException(() => this.StartCollection(collectionStart));
        }

        protected CollectionWithoutExpectedTypeValidator CollectionValidator
        {
            get
            {
                return this.collectionValidator;
            }
        }

        protected Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker
        {
            get
            {
                if (this.duplicatePropertyNamesChecker == null)
                {
                    this.duplicatePropertyNamesChecker = new Microsoft.Data.OData.DuplicatePropertyNamesChecker(this.outputContext.MessageWriterSettings.WriterBehavior.AllowDuplicatePropertyNames, this.outputContext.WritingResponse);
                }
                return this.duplicatePropertyNamesChecker;
            }
        }

        protected CollectionWriterState State
        {
            get
            {
                return this.scopes.Peek().State;
            }
        }

        internal enum CollectionWriterState
        {
            Start,
            Collection,
            Item,
            Completed,
            Error
        }

        private sealed class Scope
        {
            private readonly object item;
            private readonly ODataCollectionWriterCore.CollectionWriterState state;

            public Scope(ODataCollectionWriterCore.CollectionWriterState state, object item)
            {
                this.state = state;
                this.item = item;
            }

            public object Item
            {
                get
                {
                    return this.item;
                }
            }

            public ODataCollectionWriterCore.CollectionWriterState State
            {
                get
                {
                    return this.state;
                }
            }
        }
    }
}

