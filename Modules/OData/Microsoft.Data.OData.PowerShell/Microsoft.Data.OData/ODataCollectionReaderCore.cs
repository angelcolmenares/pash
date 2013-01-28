namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal abstract class ODataCollectionReaderCore : ODataCollectionReader
    {
        private readonly CollectionWithoutExpectedTypeValidator collectionValidator;
        private readonly IEdmTypeReference expectedItemTypeReference;
        private readonly ODataInputContext inputContext;
        private readonly IODataReaderWriterListener listener;
        private readonly Stack<Scope> scopes = new Stack<Scope>();

        protected ODataCollectionReaderCore(ODataInputContext inputContext, IEdmTypeReference expectedItemTypeReference, IODataReaderWriterListener listener)
        {
            this.inputContext = inputContext;
            this.expectedItemTypeReference = expectedItemTypeReference;
            if (this.expectedItemTypeReference == null)
            {
                this.collectionValidator = new CollectionWithoutExpectedTypeValidator(null);
            }
            this.listener = listener;
            this.EnterScope(ODataCollectionReaderState.Start, null);
        }

        protected void EnterScope(ODataCollectionReaderState state, object item)
        {
            this.EnterScope(state, item, false);
        }

        protected void EnterScope(ODataCollectionReaderState state, object item, bool isCollectionElementEmpty)
        {
            if (state == ODataCollectionReaderState.Value)
            {
                ValidationUtils.ValidateCollectionItem(item, true);
            }
            this.scopes.Push(new Scope(state, item, isCollectionElementEmpty));
            if (this.listener != null)
            {
                if (state == ODataCollectionReaderState.Exception)
                {
                    this.listener.OnException();
                }
                else if (state == ODataCollectionReaderState.Completed)
                {
                    this.listener.OnCompleted();
                }
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
                    this.EnterScope(ODataCollectionReaderState.Exception, null);
                }
                throw;
            }
            return local;
        }

        protected void PopScope(ODataCollectionReaderState state)
        {
            this.scopes.Pop();
        }

        public sealed override bool Read()
        {
            this.VerifyCanRead(true);
            return this.InterceptException<bool>(new Func<bool>(this.ReadSynchronously));
        }

        public sealed override Task<bool> ReadAsync()
        {
            this.VerifyCanRead(false);
            return this.ReadAsynchronously().FollowOnFaultWith<bool>(delegate (Task<bool> t) {
                this.EnterScope(ODataCollectionReaderState.Exception, null);
            });
        }

        protected Task<bool> ReadAsynchronously()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(new Func<bool>(this.ReadImplementation));
        }

        protected abstract bool ReadAtCollectionEndImplementation();
        protected abstract bool ReadAtCollectionStartImplementation();
        protected abstract bool ReadAtStartImplementation();
        protected abstract bool ReadAtValueImplementation();
        protected bool ReadImplementation()
        {
            switch (this.State)
            {
                case ODataCollectionReaderState.Start:
                    return this.ReadAtStartImplementation();

                case ODataCollectionReaderState.CollectionStart:
                    return this.ReadAtCollectionStartImplementation();

                case ODataCollectionReaderState.Value:
                    return this.ReadAtValueImplementation();

                case ODataCollectionReaderState.CollectionEnd:
                    return this.ReadAtCollectionEndImplementation();

                case ODataCollectionReaderState.Exception:
                case ODataCollectionReaderState.Completed:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataCollectionReader_ReadImplementation));
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataCollectionReader_ReadImplementation));
        }

        protected bool ReadSynchronously()
        {
            return this.ReadImplementation();
        }

        protected void ReplaceScope(ODataCollectionReaderState state, object item)
        {
            if (state == ODataCollectionReaderState.Value)
            {
                ValidationUtils.ValidateCollectionItem(item, true);
            }
            this.scopes.Pop();
            this.EnterScope(state, item);
        }

        private void VerifyAsynchronousCallAllowed()
        {
            if (this.inputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionReaderCore_AsyncCallOnSyncReader);
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                this.VerifySynchronousCallAllowed();
            }
            else
            {
                this.VerifyAsynchronousCallAllowed();
            }
        }

        private void VerifyCanRead(bool synchronousCall)
        {
            this.inputContext.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if ((this.State == ODataCollectionReaderState.Exception) || (this.State == ODataCollectionReaderState.Completed))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionReaderCore_ReadOrReadAsyncCalledInInvalidState(this.State));
            }
        }

        private void VerifySynchronousCallAllowed()
        {
            if (!this.inputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataCollectionReaderCore_SyncCallOnAsyncReader);
            }
        }

        protected CollectionWithoutExpectedTypeValidator CollectionValidator
        {
            get
            {
                return this.collectionValidator;
            }
        }

        protected IEdmTypeReference ExpectedItemTypeReference
        {
            get
            {
                return this.expectedItemTypeReference;
            }
        }

        protected bool IsCollectionElementEmpty
        {
            get
            {
                return this.scopes.Peek().IsCollectionElementEmpty;
            }
        }

        protected bool IsReadingNestedPayload
        {
            get
            {
                return (this.listener != null);
            }
        }

        public sealed override object Item
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().Item;
            }
        }

        public sealed override ODataCollectionReaderState State
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().State;
            }
        }

        protected sealed class Scope
        {
            private readonly bool isCollectionElementEmpty;
            private readonly object item;
            private readonly ODataCollectionReaderState state;

            public Scope(ODataCollectionReaderState state, object item) : this(state, item, false)
            {
            }

            public Scope(ODataCollectionReaderState state, object item, bool isCollectionElementEmpty)
            {
                this.state = state;
                this.item = item;
                this.isCollectionElementEmpty = isCollectionElementEmpty;
                bool flag1 = this.isCollectionElementEmpty;
            }

            public bool IsCollectionElementEmpty
            {
                get
                {
                    return this.isCollectionElementEmpty;
                }
            }

            public object Item
            {
                get
                {
                    return this.item;
                }
            }

            public ODataCollectionReaderState State
            {
                get
                {
                    return this.state;
                }
            }
        }
    }
}

