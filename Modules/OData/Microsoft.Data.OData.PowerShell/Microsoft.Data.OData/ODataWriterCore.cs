namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal abstract class ODataWriterCore : ODataWriter, IODataOutputInStreamErrorListener
    {
        private int currentEntryDepth;
        private readonly FeedWithoutExpectedTypeValidator feedValidator;
        private readonly ODataOutputContext outputContext;
        private Stack<Scope> scopes = new Stack<Scope>();
        private readonly bool writingFeed;

        protected ODataWriterCore(ODataOutputContext outputContext, bool writingFeed)
        {
            this.outputContext = outputContext;
            this.writingFeed = writingFeed;
            if (this.writingFeed && this.outputContext.Model.IsUserModel())
            {
                this.feedValidator = new FeedWithoutExpectedTypeValidator();
            }
            this.scopes.Push(new Scope(WriterState.Start, null, false));
        }

        private void CheckForNavigationLinkWithContent(ODataPayloadKind contentPayloadKind)
        {
            Scope currentScope = this.CurrentScope;
            if ((currentScope.State == WriterState.NavigationLink) || (currentScope.State == WriterState.NavigationLinkWithContent))
            {
                Action action = null;
                ODataNavigationLink currentNavigationLink = (ODataNavigationLink) currentScope.Item;
                IEdmType navigationPropertyType = null;
                this.InterceptException(delegate {
                    navigationPropertyType = WriterValidationUtils.ValidateNavigationLink(currentNavigationLink, this.ParentEntryEntityType, new ODataPayloadKind?(contentPayloadKind));
                    ((NavigationLinkScope) this.CurrentScope).NavigationPropertyType = navigationPropertyType;
                });
                if (currentScope.State != WriterState.NavigationLinkWithContent)
                {
                    this.PromoteNavigationLinkScope();
                    if (!this.SkipWriting)
                    {
                        if (action == null)
                        {
                            action = delegate {
                                this.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, contentPayloadKind != ODataPayloadKind.EntityReferenceLink, new bool?(contentPayloadKind == ODataPayloadKind.Feed));
                                this.StartNavigationLinkWithContent(currentNavigationLink);
                            };
                        }
                        this.InterceptException(action);
                    }
                }
                else if (this.outputContext.WritingResponse || (currentNavigationLink.IsCollection != true))
                {
                    this.ThrowODataException(Microsoft.Data.OData.Strings.ODataWriterCore_MultipleItemsInNavigationLinkContent, currentNavigationLink);
                }
            }
            else if (contentPayloadKind == ODataPayloadKind.EntityReferenceLink)
            {
                this.ThrowODataException(Microsoft.Data.OData.Strings.ODataWriterCore_EntityReferenceLinkWithoutNavigationLink, null);
            }
        }

        protected abstract EntryScope CreateEntryScope(ODataEntry entry, bool skipWriting);
        protected abstract FeedScope CreateFeedScope(ODataFeed feed, bool skipWriting);
        private void DecreaseEntryDepth()
        {
            this.currentEntryDepth--;
        }

        protected abstract void EndEntry(ODataEntry entry);
        protected abstract void EndFeed(ODataFeed feed);
        protected abstract void EndNavigationLinkWithContent(ODataNavigationLink navigationLink);
        protected abstract void EndPayload();
        private void EnterScope(WriterState newState, ODataItem item)
        {
            this.InterceptException(() => this.ValidateTransition(newState));
            bool skipWriting = this.SkipWriting;
            Scope currentScope = this.CurrentScope;
            if (((currentScope.State == WriterState.Entry) && (newState == WriterState.NavigationLink)) && !skipWriting)
            {
                ProjectedPropertiesAnnotation projectedProperties = currentScope.Item.GetAnnotation<ProjectedPropertiesAnnotation>();
                ODataNavigationLink link = (ODataNavigationLink) item;
                skipWriting = projectedProperties.ShouldSkipProperty(link.Name);
            }
            else if ((currentScope.State == WriterState.Feed) && (newState == WriterState.Entry))
            {
                FeedScope scope1 = (FeedScope) currentScope;
                scope1.EntryCount++;
            }
            this.PushScope(newState, item, skipWriting);
        }

        public sealed override void Flush()
        {
            this.VerifyCanFlush(true);
            try
            {
                this.FlushSynchronously();
            }
			catch(Exception ex)
            {
                this.EnterScope(WriterState.Error, null);
                throw;
            }
        }

        public sealed override Task FlushAsync()
        {
            this.VerifyCanFlush(false);
            return this.FlushAsynchronously().FollowOnFaultWith(delegate (Task t) {
                this.EnterScope(WriterState.Error, null);
            });
        }

        protected abstract Task FlushAsynchronously();
        protected abstract void FlushSynchronously();
        private void IncreaseEntryDepth()
        {
            this.currentEntryDepth++;
            if (this.currentEntryDepth > this.outputContext.MessageWriterSettings.MessageQuotas.MaxNestingDepth)
            {
                this.ThrowODataException(Microsoft.Data.OData.Strings.ValidationUtils_MaxDepthOfNestedEntriesExceeded(this.outputContext.MessageWriterSettings.MessageQuotas.MaxNestingDepth), null);
            }
        }

        private void InterceptException(Action action)
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                if (!IsErrorState(this.State))
                {
                    this.EnterScope(WriterState.Error, this.CurrentScope.Item);
                }
                throw;
            }
        }

        protected static bool IsErrorState(WriterState state)
        {
            return (state == WriterState.Error);
        }

        private void LeaveScope()
        {
            this.scopes.Pop();
            if (this.scopes.Count == 1)
            {
                this.scopes.Pop();
                this.PushScope(WriterState.Completed, null, false);
                this.InterceptException(new Action(this.EndPayload));
            }
        }

        void IODataOutputInStreamErrorListener.OnInStreamError()
        {
            this.VerifyNotDisposed();
            if (this.State == WriterState.Completed)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromCompleted(this.State.ToString(), WriterState.Error.ToString()));
            }
            this.StartPayloadInStartState();
            this.EnterScope(WriterState.Error, this.CurrentScope.Item);
        }

        private void PromoteNavigationLinkScope()
        {
            this.ValidateTransition(WriterState.NavigationLinkWithContent);
            NavigationLinkScope other = (NavigationLinkScope) this.scopes.Pop();
            NavigationLinkScope item = new NavigationLinkScope(other);
            this.scopes.Push(item);
        }

        private void PushScope(WriterState state, ODataItem item, bool skipWriting)
        {
            Scope scope;
            switch (state)
            {
                case WriterState.Start:
                case WriterState.Completed:
					scope = new Scope(state, item, skipWriting);
					break;
                case WriterState.Error:
                    scope = new Scope(state, item, skipWriting);
                    break;

                case WriterState.Entry:
                    scope = this.CreateEntryScope((ODataEntry) item, skipWriting);
                    break;

                case WriterState.Feed:
                    scope = this.CreateFeedScope((ODataFeed) item, skipWriting);
                    break;

                case WriterState.NavigationLink:
                case WriterState.NavigationLinkWithContent:
                    scope = new NavigationLinkScope(state, (ODataNavigationLink) item, skipWriting);
                    break;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataWriterCore_Scope_Create_UnreachableCodePath));
            }
            this.scopes.Push(scope);
        }

        protected abstract void StartEntry(ODataEntry entry);
        protected abstract void StartFeed(ODataFeed feed);
        protected abstract void StartNavigationLinkWithContent(ODataNavigationLink navigationLink);
        protected abstract void StartPayload();
        private void StartPayloadInStartState()
        {
            if (this.State == WriterState.Start)
            {
                this.InterceptException(new Action(this.StartPayload));
            }
        }

        private void ThrowODataException(string errorMessage, ODataItem item)
        {
            this.EnterScope(WriterState.Error, item);
            throw new ODataException(errorMessage);
        }

        private void ValidateTransition(WriterState newState)
        {
            if (IsErrorState(this.State) || !IsErrorState(newState))
            {
                switch (this.State)
                {
                    case WriterState.Start:
                        if ((newState != WriterState.Feed) && (newState != WriterState.Entry))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromStart(this.State.ToString(), newState.ToString()));
                        }
                        if ((newState == WriterState.Feed) && !this.writingFeed)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_CannotWriteTopLevelFeedWithEntryWriter);
                        }
                        if ((newState != WriterState.Entry) || !this.writingFeed)
                        {
                            break;
                        }
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_CannotWriteTopLevelEntryWithFeedWriter);

                    case WriterState.Entry:
                        if (this.CurrentScope.Item == null)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromNullEntry(this.State.ToString(), newState.ToString()));
                        }
                        if (newState != WriterState.NavigationLink)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromEntry(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case WriterState.Feed:
                        if (newState != WriterState.Entry)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromFeed(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case WriterState.NavigationLink:
                        if (newState != WriterState.NavigationLinkWithContent)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidStateTransition(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case WriterState.NavigationLinkWithContent:
                        if ((newState != WriterState.Feed) && (newState != WriterState.Entry))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromExpandedLink(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    case WriterState.Completed:
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromCompleted(this.State.ToString(), newState.ToString()));

                    case WriterState.Error:
                        if (newState != WriterState.Error)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InvalidTransitionFromError(this.State.ToString(), newState.ToString()));
                        }
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataWriterCore_ValidateTransition_UnreachableCodePath));
                }
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.outputContext.Synchronous)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_SyncCallOnAsyncWriter);
                }
            }
            else if (this.outputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_AsyncCallOnSyncWriter);
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

        private void VerifyCanWriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink, bool synchronousCall)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataEntityReferenceLink>(entityReferenceLink, "entityReferenceLink");
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteStartEntry(bool synchronousCall, ODataEntry entry)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if (this.State != WriterState.NavigationLink)
            {
                ExceptionUtils.CheckArgumentNotNull<ODataEntry>(entry, "entry");
            }
        }

        private void VerifyCanWriteStartFeed(bool synchronousCall, ODataFeed feed)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFeed>(feed, "feed");
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            this.StartPayloadInStartState();
        }

        private void VerifyCanWriteStartNavigationLink(bool synchronousCall, ODataNavigationLink navigationLink)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataNavigationLink>(navigationLink, "navigationLink");
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        protected abstract void VerifyNotDisposed();
        protected abstract void WriteDeferredNavigationLink(ODataNavigationLink navigationLink);
        public sealed override void WriteEnd()
        {
            this.VerifyCanWriteEnd(true);
            this.WriteEndImplementation();
            if (this.CurrentScope.State == WriterState.Completed)
            {
                this.Flush();
            }
        }

        public sealed override Task WriteEndAsync()
        {
            this.VerifyCanWriteEnd(false);
            return TaskUtils.GetTaskForSynchronousOperation(new Action(this.WriteEndImplementation)).FollowOnSuccessWithTask(delegate (Task task) {
                if (this.CurrentScope.State == WriterState.Completed)
                {
                    return this.FlushAsync();
                }
                return TaskUtils.CompletedTask;
            });
        }

        private void WriteEndImplementation()
        {
            this.InterceptException(delegate {
                Scope currentScope = this.CurrentScope;
                switch (currentScope.State)
                {
                    case WriterState.Start:
                    case WriterState.Completed:
                    case WriterState.Error:
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_WriteEndCalledInInvalidState(currentScope.State.ToString()));

                    case WriterState.Entry:
                        if (!this.SkipWriting)
                        {
                            ODataEntry entry = (ODataEntry) currentScope.Item;
                            if (entry != null)
                            {
                                WriterValidationUtils.ValidateEntryAtEnd(entry);
                            }
                            this.EndEntry(entry);
                            this.DecreaseEntryDepth();
                        }
                        break;

                    case WriterState.Feed:
                        if (!this.SkipWriting)
                        {
                            ODataFeed item = (ODataFeed) currentScope.Item;
                            WriterValidationUtils.ValidateFeedAtEnd(item, !this.outputContext.WritingResponse, this.outputContext.Version);
                            this.EndFeed(item);
                        }
                        break;

                    case WriterState.NavigationLink:
                        if (!this.outputContext.WritingResponse)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_DeferredLinkInRequest);
                        }
                        if (!this.SkipWriting)
                        {
                            ODataNavigationLink navigationLink = (ODataNavigationLink) currentScope.Item;
                            ((NavigationLinkScope) this.CurrentScope).NavigationPropertyType = WriterValidationUtils.ValidateNavigationLink(navigationLink, this.ParentEntryEntityType, null);
                            this.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(navigationLink, false, navigationLink.IsCollection);
                            this.WriteDeferredNavigationLink(navigationLink);
                        }
                        break;

                    case WriterState.NavigationLinkWithContent:
                        if (!this.SkipWriting)
                        {
                            this.EndNavigationLinkWithContent((ODataNavigationLink) currentScope.Item);
                        }
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataWriterCore_WriteEnd_UnreachableCodePath));
                }
                this.LeaveScope();
            });
        }

        protected abstract void WriteEntityReferenceInNavigationLinkContent(ODataNavigationLink parentNavigationLink, ODataEntityReferenceLink entityReferenceLink);
        public sealed override void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink)
        {
            this.VerifyCanWriteEntityReferenceLink(entityReferenceLink, true);
            this.WriteEntityReferenceLinkImplementation(entityReferenceLink);
        }

        public sealed override Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink entityReferenceLink)
        {
            this.VerifyCanWriteEntityReferenceLink(entityReferenceLink, false);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteEntityReferenceLinkImplementation(entityReferenceLink);
            });
        }

        private void WriteEntityReferenceLinkImplementation(ODataEntityReferenceLink entityReferenceLink)
        {
            Action action = null;
            if (this.outputContext.WritingResponse)
            {
                this.ThrowODataException(Microsoft.Data.OData.Strings.ODataWriterCore_EntityReferenceLinkInResponse, null);
            }
            this.CheckForNavigationLinkWithContent(ODataPayloadKind.EntityReferenceLink);
            if (!this.SkipWriting)
            {
                if (action == null)
                {
                    action = delegate {
                        WriterValidationUtils.ValidateEntityReferenceLink(entityReferenceLink);
                        this.WriteEntityReferenceInNavigationLinkContent((ODataNavigationLink) this.CurrentScope.Item, entityReferenceLink);
                    };
                }
                this.InterceptException(action);
            }
        }

        public sealed override void WriteStart(ODataEntry entry)
        {
            this.VerifyCanWriteStartEntry(true, entry);
            this.WriteStartEntryImplementation(entry);
        }

        public sealed override void WriteStart(ODataFeed feed)
        {
            this.VerifyCanWriteStartFeed(true, feed);
            this.WriteStartFeedImplementation(feed);
        }

        public sealed override void WriteStart(ODataNavigationLink navigationLink)
        {
            this.VerifyCanWriteStartNavigationLink(true, navigationLink);
            this.WriteStartNavigationLinkImplementation(navigationLink);
        }

        public sealed override Task WriteStartAsync(ODataEntry entry)
        {
            this.VerifyCanWriteStartEntry(false, entry);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteStartEntryImplementation(entry);
            });
        }

        public sealed override Task WriteStartAsync(ODataFeed feed)
        {
            this.VerifyCanWriteStartFeed(false, feed);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteStartFeedImplementation(feed);
            });
        }

        public sealed override Task WriteStartAsync(ODataNavigationLink navigationLink)
        {
            this.VerifyCanWriteStartNavigationLink(false, navigationLink);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.WriteStartNavigationLinkImplementation(navigationLink);
            });
        }

        private void WriteStartEntryImplementation(ODataEntry entry)
        {
            Action action = null;
            this.StartPayloadInStartState();
            this.CheckForNavigationLinkWithContent(ODataPayloadKind.Entry);
            this.EnterScope(WriterState.Entry, entry);
            if (!this.SkipWriting)
            {
                this.IncreaseEntryDepth();
                if (action == null)
                {
                    action = delegate {
                        if (entry != null)
                        {
                            IEdmEntityType entityType = WriterValidationUtils.ValidateEntityTypeName(this.outputContext.Model, entry.TypeName);
                            bool validateMediaResource = this.outputContext.UseDefaultFormatBehavior || this.outputContext.UseServerFormatBehavior;
                            ValidationUtils.ValidateEntryMetadata(entry, entityType, this.outputContext.Model, validateMediaResource);
                            NavigationLinkScope parentNavigationLinkScope = this.ParentNavigationLinkScope;
                            if (parentNavigationLinkScope != null)
                            {
                                WriterValidationUtils.ValidateEntryInExpandedLink(entityType, parentNavigationLinkScope.NavigationPropertyType);
                            }
                            if (this.CurrentFeedValidator != null)
                            {
                                this.CurrentFeedValidator.ValidateEntry(entityType);
                            }
                            ((EntryScope) this.CurrentScope).EntityType = entityType;
                            WriterValidationUtils.ValidateEntryAtStart(entry);
                        }
                        this.StartEntry(entry);
                    };
                }
                this.InterceptException(action);
            }
        }

        private void WriteStartFeedImplementation(ODataFeed feed)
        {
            Action action = null;
            this.CheckForNavigationLinkWithContent(ODataPayloadKind.Feed);
            this.EnterScope(WriterState.Feed, feed);
            if (!this.SkipWriting)
            {
                if (action == null)
                {
                    action = delegate {
                        WriterValidationUtils.ValidateFeedAtStart(feed);
                        if (feed.Count.HasValue)
                        {
                            if (!this.IsTopLevel)
                            {
                                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_OnlyTopLevelFeedsSupportInlineCount);
                            }
                            if (!this.outputContext.WritingResponse)
                            {
                                this.ThrowODataException(Microsoft.Data.OData.Strings.ODataWriterCore_InlineCountInRequest, feed);
                            }
                            ODataVersionChecker.CheckCount(this.outputContext.Version);
                        }
                        this.StartFeed(feed);
                    };
                }
                this.InterceptException(action);
            }
        }

        private void WriteStartNavigationLinkImplementation(ODataNavigationLink navigationLink)
        {
            this.EnterScope(WriterState.NavigationLink, navigationLink);
        }

        private FeedWithoutExpectedTypeValidator CurrentFeedValidator
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

        protected Scope CurrentScope
        {
            get
            {
                return this.scopes.Peek();
            }
        }

        protected Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker
        {
            get
            {
                EntryScope currentScope;
                switch (this.State)
                {
                    case WriterState.Entry:
                        currentScope = (EntryScope) this.CurrentScope;
                        break;

                    case WriterState.NavigationLink:
                    case WriterState.NavigationLinkWithContent:
                        currentScope = (EntryScope) this.scopes.Skip<Scope>(1).First<Scope>();
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataWriterCore_DuplicatePropertyNamesChecker));
                }
                return currentScope.DuplicatePropertyNamesChecker;
            }
        }

        protected IEdmEntityType EntryEntityType
        {
            get
            {
                return ((EntryScope) this.CurrentScope).EntityType;
            }
        }

        protected int FeedScopeEntryCount
        {
            get
            {
                return ((FeedScope) this.CurrentScope).EntryCount;
            }
        }

        protected bool IsTopLevel
        {
            get
            {
                return (this.scopes.Count == 2);
            }
        }

        protected IEdmEntityType ParentEntryEntityType
        {
            get
            {
                EntryScope scope = (EntryScope) this.scopes.Skip<Scope>(1).First<Scope>();
                return scope.EntityType;
            }
        }

        protected ODataNavigationLink ParentNavigationLink
        {
            get
            {
                Scope scope = this.scopes.Skip<Scope>(1).FirstOrDefault<Scope>();
                if (scope != null)
                {
                    return (scope.Item as ODataNavigationLink);
                }
                return null;
            }
        }

        private NavigationLinkScope ParentNavigationLinkScope
        {
            get
            {
                IEnumerable<Scope> source = this.scopes.Skip<Scope>(1);
                Scope scope = source.First<Scope>();
                if (scope.State == WriterState.Start)
                {
                    return null;
                }
                if (scope.State == WriterState.Feed)
                {
                    scope = source.Skip<Scope>(1).First<Scope>();
                    if (scope.State == WriterState.Start)
                    {
                        return null;
                    }
                }
                if (scope.State != WriterState.NavigationLinkWithContent)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataWriterCore_ParentNavigationLinkScope));
                }
                return (NavigationLinkScope) scope;
            }
        }

        protected bool SkipWriting
        {
            get
            {
                return this.CurrentScope.SkipWriting;
            }
        }

        protected WriterState State
        {
            get
            {
                return this.CurrentScope.State;
            }
        }

        internal class EntryScope : ODataWriterCore.Scope
        {
            private readonly Microsoft.Data.OData.DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;
            private IEdmEntityType entityType;

            internal EntryScope(ODataEntry entry, bool skipWriting, bool writingResponse, ODataWriterBehavior writerBehavior) : base(ODataWriterCore.WriterState.Entry, entry, skipWriting)
            {
                if (entry != null)
                {
                    this.duplicatePropertyNamesChecker = new Microsoft.Data.OData.DuplicatePropertyNamesChecker(writerBehavior.AllowDuplicatePropertyNames, writingResponse);
                }
            }

            internal Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker
            {
                get
                {
                    return this.duplicatePropertyNamesChecker;
                }
            }

            internal IEdmEntityType EntityType
            {
                get
                {
                    return this.entityType;
                }
                set
                {
                    this.entityType = value;
                }
            }
        }

        internal abstract class FeedScope : ODataWriterCore.Scope
        {
            private int entryCount;

            internal FeedScope(ODataFeed feed, bool skipWriting) : base(ODataWriterCore.WriterState.Feed, feed, skipWriting)
            {
            }

            internal int EntryCount
            {
                get
                {
                    return this.entryCount;
                }
                set
                {
                    this.entryCount = value;
                }
            }
        }

        private sealed class NavigationLinkScope : ODataWriterCore.Scope
        {
            private IEdmType navigationPropertyType;

            internal NavigationLinkScope(ODataWriterCore.NavigationLinkScope other) : base(ODataWriterCore.WriterState.NavigationLinkWithContent, other.Item, other.SkipWriting)
            {
                this.navigationPropertyType = other.navigationPropertyType;
            }

            internal NavigationLinkScope(ODataWriterCore.WriterState writerState, ODataNavigationLink navLink, bool skipWriting) : base(writerState, navLink, skipWriting)
            {
            }

            internal IEdmType NavigationPropertyType
            {
                get
                {
                    return this.navigationPropertyType;
                }
                set
                {
                    this.navigationPropertyType = value;
                }
            }
        }

        internal class Scope
        {
            private readonly ODataItem item;
            private readonly bool skipWriting;
            private readonly ODataWriterCore.WriterState state;

            internal Scope(ODataWriterCore.WriterState state, ODataItem item, bool skipWriting)
            {
                this.state = state;
                this.item = item;
                this.skipWriting = skipWriting;
            }

            internal ODataItem Item
            {
                get
                {
                    return this.item;
                }
            }

            internal bool SkipWriting
            {
                get
                {
                    return this.skipWriting;
                }
            }

            internal ODataWriterCore.WriterState State
            {
                get
                {
                    return this.state;
                }
            }
        }

        internal enum WriterState
        {
            Start,
            Entry,
            Feed,
            NavigationLink,
            NavigationLinkWithContent,
            Completed,
            Error
        }
    }
}

