namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal abstract class ODataReaderCore : ODataReader
    {
        private int currentEntryDepth;
        private readonly FeedWithoutExpectedTypeValidator feedValidator;
        private readonly ODataInputContext inputContext;
        private readonly IODataReaderWriterListener listener;
        private readonly bool readingFeed;
        private readonly Stack<Scope> scopes = new Stack<Scope>();

        protected ODataReaderCore(ODataInputContext inputContext, IEdmEntityType expectedEntityType, bool readingFeed, IODataReaderWriterListener listener)
        {
            this.inputContext = inputContext;
            this.readingFeed = readingFeed;
            this.listener = listener;
            this.currentEntryDepth = 0;
            if (this.readingFeed && this.inputContext.Model.IsUserModel())
            {
                this.feedValidator = new FeedWithoutExpectedTypeValidator();
            }
            this.EnterScope(new Scope(ODataReaderState.Start, null, expectedEntityType));
        }

        protected void ApplyEntityTypeNameFromPayload(string entityTypeNameFromPayload)
        {
            SerializationTypeNameAnnotation annotation;
            EdmTypeKind kind;
            IEdmEntityTypeReference reference = (IEdmEntityTypeReference) ReaderValidationUtils.ResolvePayloadTypeNameAndComputeTargetType(EdmTypeKind.Entity, null, this.CurrentEntityType.ToTypeReference(), entityTypeNameFromPayload, this.inputContext.Model, this.inputContext.MessageReaderSettings, this.inputContext.Version, () => EdmTypeKind.Entity, out kind, out annotation);
            IEdmEntityType type = null;
            ODataEntry currentEntry = this.CurrentEntry;
            if (reference != null)
            {
                type = reference.EntityDefinition();
                currentEntry.TypeName = type.ODataFullName();
                if (annotation != null)
                {
                    currentEntry.SetAnnotation<SerializationTypeNameAnnotation>(annotation);
                }
            }
            else if (entityTypeNameFromPayload != null)
            {
                currentEntry.TypeName = entityTypeNameFromPayload;
            }
            this.CurrentEntityType = type;
        }

        private void DecreaseEntryDepth()
        {
            this.currentEntryDepth--;
        }

        protected void EnterScope(Scope scope)
        {
            this.scopes.Push(scope);
            if (this.listener != null)
            {
                if (scope.State == ODataReaderState.Exception)
                {
                    this.listener.OnException();
                }
                else if (scope.State == ODataReaderState.Completed)
                {
                    this.listener.OnCompleted();
                }
            }
        }

        private void IncreaseEntryDepth()
        {
            this.currentEntryDepth++;
            if (this.currentEntryDepth > this.inputContext.MessageReaderSettings.MessageQuotas.MaxNestingDepth)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_MaxDepthOfNestedEntriesExceeded(this.inputContext.MessageReaderSettings.MessageQuotas.MaxNestingDepth));
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
                    this.EnterScope(new Scope(ODataReaderState.Exception, null, null));
                }
                throw;
            }
            return local;
        }

        protected void PopScope(ODataReaderState state)
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
                this.EnterScope(new Scope(ODataReaderState.Exception, null, null));
            });
        }

        private Task<bool> ReadAsynchronously()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(new Func<bool>(this.ReadImplementation));
        }

        protected abstract bool ReadAtEntityReferenceLink();
        protected abstract bool ReadAtEntryEndImplementation();
        protected abstract bool ReadAtEntryStartImplementation();
        protected abstract bool ReadAtFeedEndImplementation();
        protected abstract bool ReadAtFeedStartImplementation();
        protected abstract bool ReadAtNavigationLinkEndImplementation();
        protected abstract bool ReadAtNavigationLinkStartImplementation();
        protected abstract bool ReadAtStartImplementation();
        private bool ReadImplementation()
        {
            bool flag;
            switch (this.State)
            {
                case ODataReaderState.Start:
                    flag = this.ReadAtStartImplementation();
                    break;

                case ODataReaderState.FeedStart:
                    flag = this.ReadAtFeedStartImplementation();
                    break;

                case ODataReaderState.FeedEnd:
                    flag = this.ReadAtFeedEndImplementation();
                    break;

                case ODataReaderState.EntryStart:
                    this.IncreaseEntryDepth();
                    flag = this.ReadAtEntryStartImplementation();
                    break;

                case ODataReaderState.EntryEnd:
                    this.DecreaseEntryDepth();
                    flag = this.ReadAtEntryEndImplementation();
                    break;

                case ODataReaderState.NavigationLinkStart:
                    flag = this.ReadAtNavigationLinkStartImplementation();
                    break;

                case ODataReaderState.NavigationLinkEnd:
                    flag = this.ReadAtNavigationLinkEndImplementation();
                    break;

                case ODataReaderState.EntityReferenceLink:
                    flag = this.ReadAtEntityReferenceLink();
                    break;

                case ODataReaderState.Exception:
                case ODataReaderState.Completed:
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataReaderCore_NoReadCallsAllowed(this.State));

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataReaderCore_ReadImplementation));
            }
            if (((this.State == ODataReaderState.EntryStart) || (this.State == ODataReaderState.EntryEnd)) && (this.Item != null))
            {
                ReaderValidationUtils.ValidateEntry(this.CurrentEntry);
            }
            return flag;
        }

        private bool ReadSynchronously()
        {
            return this.ReadImplementation();
        }

        protected void ReplaceScope(Scope scope)
        {
            this.scopes.Pop();
            this.EnterScope(scope);
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.inputContext.Synchronous)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataReaderCore_SyncCallOnAsyncReader);
                }
            }
            else if (this.inputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataReaderCore_AsyncCallOnSyncReader);
            }
        }

        private void VerifyCanRead(bool synchronousCall)
        {
            this.inputContext.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if ((this.State == ODataReaderState.Exception) || (this.State == ODataReaderState.Completed))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataReaderCore_ReadOrReadAsyncCalledInInvalidState(this.State));
            }
        }

        protected ODataEntityReferenceLink CurrentEntityReferenceLink
        {
            get
            {
                return (ODataEntityReferenceLink) this.Item;
            }
        }

        protected IEdmEntityType CurrentEntityType
        {
            get
            {
                return this.scopes.Peek().EntityType;
            }
            set
            {
                this.scopes.Peek().EntityType = value;
            }
        }

        protected ODataEntry CurrentEntry
        {
            get
            {
                return (ODataEntry) this.Item;
            }
        }

        protected ODataFeed CurrentFeed
        {
            get
            {
                return (ODataFeed) this.Item;
            }
        }

        protected FeedWithoutExpectedTypeValidator CurrentFeedValidator
        {
            get
            {
                if (this.scopes.Count != 3)
                {
                    return null;
                }
                return this.feedValidator;
            }
        }

        protected ODataNavigationLink CurrentNavigationLink
        {
            get
            {
                return (ODataNavigationLink) this.Item;
            }
        }

        protected Scope CurrentScope
        {
            get
            {
                return this.scopes.Peek();
            }
        }

        protected bool IsExpandedLinkContent
        {
            get
            {
                return (this.scopes.Skip<Scope>(1).First<Scope>().State == ODataReaderState.NavigationLinkStart);
            }
        }

        protected bool IsReadingNestedPayload
        {
            get
            {
                return (this.listener != null);
            }
        }

        protected bool IsTopLevel
        {
            get
            {
                return (this.scopes.Count <= 2);
            }
        }

        public sealed override ODataItem Item
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().Item;
            }
        }

        protected Scope LinkParentEntityScope
        {
            get
            {
                return this.scopes.Skip<Scope>(1).First<Scope>();
            }
        }

        protected bool ReadingFeed
        {
            get
            {
                return this.readingFeed;
            }
        }

        public sealed override ODataReaderState State
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().State;
            }
        }

        protected class Scope
        {
            private readonly ODataItem item;
            private readonly ODataReaderState state;

            internal Scope(ODataReaderState state, ODataItem item, IEdmEntityType expectedEntityType)
            {
                this.state = state;
                this.item = item;
                this.EntityType = expectedEntityType;
            }

            internal IEdmEntityType EntityType { get; set; }

            internal ODataItem Item
            {
                get
                {
                    return this.item;
                }
            }

            internal ODataReaderState State
            {
                get
                {
                    return this.state;
                }
            }
        }
    }
}

