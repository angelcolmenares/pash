namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal sealed class ODataAtomReader : ODataReaderCore
    {
        private ODataAtomEntryAndFeedDeserializer atomEntryAndFeedDeserializer;
        private Stack<ODataAtomEntryAndFeedDeserializer> atomEntryAndFeedDeserializersStack;
        private readonly ODataAtomInputContext atomInputContext;

        internal ODataAtomReader(ODataAtomInputContext atomInputContext, IEdmEntityType expectedEntityType, bool readingFeed) : base(atomInputContext, expectedEntityType, readingFeed, null)
        {
            this.atomInputContext = atomInputContext;
            this.atomEntryAndFeedDeserializer = new ODataAtomEntryAndFeedDeserializer(atomInputContext);
            if (this.atomInputContext.MessageReaderSettings.ReaderBehavior.EntryXmlCustomizationCallback != null)
            {
                this.atomInputContext.InitializeReaderCustomization();
                this.atomEntryAndFeedDeserializersStack = new Stack<ODataAtomEntryAndFeedDeserializer>();
                this.atomEntryAndFeedDeserializersStack.Push(this.atomEntryAndFeedDeserializer);
            }
        }

        private void EndEntry()
        {
            IODataAtomReaderEntryState currentEntryState = this.CurrentEntryState;
            ODataEntry entry = currentEntryState.Entry;
            if (entry != null)
            {
                if (currentEntryState.CachedEpm != null)
                {
                    AtomScope currentScope = (AtomScope) base.CurrentScope;
                    if (currentScope.HasAtomEntryMetadata)
                    {
                        EpmSyndicationReader.ReadEntryEpm(currentEntryState, this.atomInputContext);
                    }
                    if (currentScope.HasEpmCustomReaderValueCache)
                    {
                        EpmCustomReader.ReadEntryEpm(currentEntryState, this.atomInputContext);
                    }
                }
                if (currentEntryState.AtomEntryMetadata != null)
                {
                    entry.SetAnnotation<AtomEntryMetadata>(currentEntryState.AtomEntryMetadata);
                }
                IEdmEntityType entityType = currentEntryState.EntityType;
                if ((!currentEntryState.MediaLinkEntry.HasValue && (entityType != null)) && this.atomInputContext.Model.HasDefaultStream(entityType))
                {
                    ODataAtomEntryAndFeedDeserializer.EnsureMediaResource(currentEntryState, true);
                }
                bool validateMediaResource = this.atomInputContext.UseDefaultFormatBehavior || this.atomInputContext.UseServerFormatBehavior;
                ValidationUtils.ValidateEntryMetadata(entry, entityType, this.atomInputContext.Model, validateMediaResource);
            }
            this.ReplaceScope(ODataReaderState.EntryEnd);
        }

        private void EnterScope(ODataReaderState state, ODataItem item, IEdmEntityType expectedEntityType)
        {
            base.EnterScope(new AtomScope(state, item, expectedEntityType));
        }

        protected override bool ReadAtEntityReferenceLink()
        {
            base.PopScope(ODataReaderState.EntityReferenceLink);
            this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
            return true;
        }

        protected override bool ReadAtEntryEndImplementation()
        {
            bool isTopLevel = base.IsTopLevel;
            bool isExpandedLinkContent = base.IsExpandedLinkContent;
            bool emptyInline = base.CurrentEntry == null;
            base.PopScope(ODataReaderState.EntryEnd);
            if (!emptyInline)
            {
                bool flag4 = false;
                if (this.atomInputContext.MessageReaderSettings.ReaderBehavior.EntryXmlCustomizationCallback != null)
                {
                    XmlReader objB = this.atomInputContext.PopCustomReader();
                    if (!object.ReferenceEquals(this.atomInputContext.XmlReader, objB))
                    {
                        flag4 = true;
                        this.atomEntryAndFeedDeserializersStack.Pop();
                        this.atomEntryAndFeedDeserializer = this.atomEntryAndFeedDeserializersStack.Peek();
                    }
                }
                if (!flag4)
                {
                    this.atomEntryAndFeedDeserializer.ReadEntryEnd();
                }
            }
            bool flag5 = true;
            if (isTopLevel)
            {
                this.atomEntryAndFeedDeserializer.ReadPayloadEnd();
                this.ReplaceScope(ODataReaderState.Completed);
                return false;
            }
            if (isExpandedLinkContent)
            {
                this.atomEntryAndFeedDeserializer.ReadNavigationLinkContentAfterExpansion(emptyInline);
                this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
                return flag5;
            }
            if (this.atomEntryAndFeedDeserializer.ReadFeedContent(this.CurrentFeedState, base.IsExpandedLinkContent))
            {
                this.ReadEntryStart();
                return flag5;
            }
            this.ReplaceScope(ODataReaderState.FeedEnd);
            return flag5;
        }

        protected override bool ReadAtEntryStartImplementation()
        {
            if (base.CurrentEntry == null)
            {
                this.EndEntry();
            }
            else if ((this.atomEntryAndFeedDeserializer.XmlReader.NodeType == XmlNodeType.EndElement) || this.CurrentEntryState.EntryElementEmpty)
            {
                this.EndEntry();
            }
            else if (this.atomInputContext.UseServerApiBehavior)
            {
                ODataAtomReaderNavigationLinkDescriptor navigationLinkDescriptor = this.atomEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState);
                if (navigationLinkDescriptor == null)
                {
                    this.EndEntry();
                }
                else
                {
                    this.StartNavigationLink(navigationLinkDescriptor);
                }
            }
            else
            {
                this.StartNavigationLink(this.CurrentEntryState.FirstNavigationLinkDescriptor);
            }
            return true;
        }

        protected override bool ReadAtFeedEndImplementation()
        {
            bool isTopLevel = base.IsTopLevel;
            bool emptyInline = this.atomEntryAndFeedDeserializer.IsReaderOnInlineEndElement();
            if (!emptyInline)
            {
                this.atomEntryAndFeedDeserializer.ReadFeedEnd();
            }
            base.PopScope(ODataReaderState.FeedEnd);
            if (isTopLevel)
            {
                this.atomEntryAndFeedDeserializer.ReadPayloadEnd();
                this.ReplaceScope(ODataReaderState.Completed);
                return false;
            }
            this.atomEntryAndFeedDeserializer.ReadNavigationLinkContentAfterExpansion(emptyInline);
            this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
            return true;
        }

        protected override bool ReadAtFeedStartImplementation()
        {
            if ((this.atomEntryAndFeedDeserializer.XmlReader.NodeType == XmlNodeType.EndElement) || this.CurrentFeedState.FeedElementEmpty)
            {
                IODataAtomReaderFeedState currentFeedState = this.CurrentFeedState;
                ODataFeed currentFeed = base.CurrentFeed;
                if (this.atomInputContext.MessageReaderSettings.EnableAtomMetadataReading)
                {
                    currentFeed.SetAnnotation<AtomFeedMetadata>(currentFeedState.AtomFeedMetadata);
                }
                this.ReplaceScope(ODataReaderState.FeedEnd);
            }
            else
            {
                this.ReadEntryStart();
            }
            return true;
        }

        protected override bool ReadAtNavigationLinkEndImplementation()
        {
            this.atomEntryAndFeedDeserializer.ReadNavigationLinkEnd();
            base.PopScope(ODataReaderState.NavigationLinkEnd);
            ODataAtomReaderNavigationLinkDescriptor navigationLinkDescriptor = this.atomEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState);
            if (navigationLinkDescriptor == null)
            {
                this.EndEntry();
            }
            else
            {
                this.StartNavigationLink(navigationLinkDescriptor);
            }
            return true;
        }

        protected override bool ReadAtNavigationLinkStartImplementation()
        {
            ODataNavigationLink currentNavigationLink = base.CurrentNavigationLink;
            IODataAtomReaderEntryState linkParentEntityScope = (IODataAtomReaderEntryState) base.LinkParentEntityScope;
            AtomScope currentScope = (AtomScope) base.CurrentScope;
            IEdmNavigationProperty navigationProperty = currentScope.NavigationProperty;
            if (this.atomEntryAndFeedDeserializer.XmlReader.IsEmptyElement)
            {
                this.ReadAtNonExpandedNavigatLinkStart();
            }
            else
            {
                this.atomEntryAndFeedDeserializer.XmlReader.Read();
                ODataAtomDeserializerExpandedNavigationLinkContent content = this.atomEntryAndFeedDeserializer.ReadNavigationLinkContentBeforeExpansion();
                if (((content != ODataAtomDeserializerExpandedNavigationLinkContent.None) && (navigationProperty == null)) && (this.atomInputContext.Model.IsUserModel() && this.atomInputContext.MessageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.ReportUndeclaredLinkProperty)))
                {
                    if (!this.atomInputContext.MessageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(currentNavigationLink.Name, base.LinkParentEntityScope.EntityType.ODataFullName()));
                    }
                    this.atomEntryAndFeedDeserializer.SkipNavigationLinkContentOnExpansion();
                    this.ReadAtNonExpandedNavigatLinkStart();
                    return true;
                }
                switch (content)
                {
                    case ODataAtomDeserializerExpandedNavigationLinkContent.None:
                        this.ReadAtNonExpandedNavigatLinkStart();
                        break;

                    case ODataAtomDeserializerExpandedNavigationLinkContent.Empty:
                        if (currentNavigationLink.IsCollection != true)
                        {
                            currentNavigationLink.IsCollection = false;
                            linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, false);
                            this.EnterScope(ODataReaderState.EntryStart, null, base.CurrentEntityType);
                            break;
                        }
                        linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, true);
                        this.EnterScope(ODataReaderState.FeedStart, new ODataFeed(), base.CurrentEntityType);
                        this.CurrentFeedState.FeedElementEmpty = true;
                        break;

                    case ODataAtomDeserializerExpandedNavigationLinkContent.Entry:
                        if ((currentNavigationLink.IsCollection == true) || ((navigationProperty != null) && navigationProperty.Type.IsCollection()))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_ExpandedEntryInFeedNavigationLink);
                        }
                        currentNavigationLink.IsCollection = false;
                        linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, false);
                        this.ReadEntryStart();
                        break;

                    case ODataAtomDeserializerExpandedNavigationLinkContent.Feed:
                        if (currentNavigationLink.IsCollection == false)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_ExpandedFeedInEntryNavigationLink);
                        }
                        currentNavigationLink.IsCollection = true;
                        linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, true);
                        this.ReadFeedStart();
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataAtomReader_ReadAtNavigationLinkStartImplementation));
                }
            }
            return true;
        }

        private void ReadAtNonExpandedNavigatLinkStart()
        {
            ODataNavigationLink currentNavigationLink = base.CurrentNavigationLink;
            IODataAtomReaderEntryState linkParentEntityScope = (IODataAtomReaderEntryState) base.LinkParentEntityScope;
            linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, false, currentNavigationLink.IsCollection);
            if (this.atomInputContext.ReadingResponse)
            {
                AtomScope currentScope = (AtomScope) base.CurrentScope;
                IEdmNavigationProperty navigationProperty = currentScope.NavigationProperty;
                if (((currentNavigationLink.IsCollection == false) && (navigationProperty != null)) && navigationProperty.Type.IsCollection())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_DeferredEntryInFeedNavigationLink);
                }
                this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
            }
            else
            {
                ODataEntityReferenceLink item = new ODataEntityReferenceLink {
                    Url = currentNavigationLink.Url
                };
                this.EnterScope(ODataReaderState.EntityReferenceLink, item, null);
            }
        }

        protected override bool ReadAtStartImplementation()
        {
            this.atomEntryAndFeedDeserializer.ReadPayloadStart();
            if (base.ReadingFeed)
            {
                this.ReadFeedStart();
                return true;
            }
            this.ReadEntryStart();
            return true;
        }

        private void ReadEntryStart()
        {
            ODataEntry entry = ReaderUtils.CreateNewEntry();
            if (this.atomInputContext.MessageReaderSettings.ReaderBehavior.EntryXmlCustomizationCallback != null)
            {
                this.atomEntryAndFeedDeserializer.VerifyEntryStart();
                Uri xmlBaseUri = this.atomInputContext.XmlReader.XmlBaseUri;
                XmlReader objB = this.atomInputContext.MessageReaderSettings.ReaderBehavior.EntryXmlCustomizationCallback(entry, this.atomInputContext.XmlReader, this.atomInputContext.XmlReader.ParentXmlBaseUri);
                if (objB != null)
                {
                    if (object.ReferenceEquals(this.atomInputContext.XmlReader, objB))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_EntryXmlCustomizationCallbackReturnedSameInstance);
                    }
                    this.atomInputContext.PushCustomReader(objB, xmlBaseUri);
                    this.atomEntryAndFeedDeserializer = new ODataAtomEntryAndFeedDeserializer(this.atomInputContext);
                    this.atomEntryAndFeedDeserializersStack.Push(this.atomEntryAndFeedDeserializer);
                }
                else
                {
                    this.atomInputContext.PushCustomReader(this.atomInputContext.XmlReader, null);
                }
            }
            this.atomEntryAndFeedDeserializer.ReadEntryStart(entry);
            this.EnterScope(ODataReaderState.EntryStart, entry, base.CurrentEntityType);
            AtomScope currentScope = (AtomScope) base.CurrentScope;
            currentScope.DuplicatePropertyNamesChecker = this.atomInputContext.CreateDuplicatePropertyNamesChecker();
            string entityTypeNameFromPayload = this.atomEntryAndFeedDeserializer.FindTypeName();
            base.ApplyEntityTypeNameFromPayload(entityTypeNameFromPayload);
            if (base.CurrentFeedValidator != null)
            {
                base.CurrentFeedValidator.ValidateEntry(base.CurrentEntityType);
            }
            ODataEntityPropertyMappingCache cache = this.atomInputContext.Model.EnsureEpmCache(this.CurrentEntryState.EntityType, 0x7fffffff);
            if (cache != null)
            {
                currentScope.CachedEpm = cache;
            }
            if (this.atomEntryAndFeedDeserializer.XmlReader.IsEmptyElement)
            {
                this.CurrentEntryState.EntryElementEmpty = true;
            }
            else
            {
                this.atomEntryAndFeedDeserializer.XmlReader.Read();
                if (this.atomInputContext.UseServerApiBehavior)
                {
                    this.CurrentEntryState.FirstNavigationLinkDescriptor = null;
                }
                else
                {
                    this.CurrentEntryState.FirstNavigationLinkDescriptor = this.atomEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState);
                }
            }
        }

        private void ReadFeedStart()
        {
            ODataFeed item = new ODataFeed();
            this.atomEntryAndFeedDeserializer.ReadFeedStart();
            this.EnterScope(ODataReaderState.FeedStart, item, base.CurrentEntityType);
            if (this.atomEntryAndFeedDeserializer.XmlReader.IsEmptyElement)
            {
                this.CurrentFeedState.FeedElementEmpty = true;
            }
            else
            {
                this.atomEntryAndFeedDeserializer.XmlReader.Read();
                this.atomEntryAndFeedDeserializer.ReadFeedContent(this.CurrentFeedState, base.IsExpandedLinkContent);
            }
        }

        private void ReplaceScope(ODataReaderState state)
        {
            base.ReplaceScope(new AtomScope(state, this.Item, base.CurrentEntityType));
        }

        private void StartNavigationLink(ODataAtomReaderNavigationLinkDescriptor navigationLinkDescriptor)
        {
            IEdmEntityType expectedEntityType = null;
            if (navigationLinkDescriptor.NavigationProperty != null)
            {
                IEdmTypeReference type = navigationLinkDescriptor.NavigationProperty.Type;
                if (!type.IsCollection())
                {
                    if (navigationLinkDescriptor.NavigationLink.IsCollection == true)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_FeedNavigationLinkForResourceReferenceProperty(navigationLinkDescriptor.NavigationLink.Name));
                    }
                    navigationLinkDescriptor.NavigationLink.IsCollection = false;
                    expectedEntityType = type.AsEntity().EntityDefinition();
                }
                else
                {
                    if (!navigationLinkDescriptor.NavigationLink.IsCollection.HasValue)
                    {
                        navigationLinkDescriptor.NavigationLink.IsCollection = true;
                    }
                    expectedEntityType = type.AsCollection().ElementType().AsEntity().EntityDefinition();
                }
            }
            this.EnterScope(ODataReaderState.NavigationLinkStart, navigationLinkDescriptor.NavigationLink, expectedEntityType);
            ((AtomScope) base.CurrentScope).NavigationProperty = navigationLinkDescriptor.NavigationProperty;
        }

        private IODataAtomReaderEntryState CurrentEntryState
        {
            get
            {
                return (IODataAtomReaderEntryState) base.CurrentScope;
            }
        }

        private IODataAtomReaderFeedState CurrentFeedState
        {
            get
            {
                return (IODataAtomReaderFeedState) base.CurrentScope;
            }
        }

        private sealed class AtomScope : ODataReaderCore.Scope, IODataAtomReaderEntryState, IODataAtomReaderFeedState
        {
            private AtomEntryMetadata atomEntryMetadata;
            private AtomFeedMetadata atomFeedMetadata;
            private AtomScopeStateBitMask atomScopeState;
            private EpmCustomReaderValueCache epmCustomReaderValueCache;
            private bool? mediaLinkEntry;

            internal AtomScope(ODataReaderState state, ODataItem item, IEdmEntityType expectedEntityType) : base(state, item, expectedEntityType)
            {
            }

            private bool GetAtomScopeState(AtomScopeStateBitMask bitMask)
            {
                return ((this.atomScopeState & bitMask) == bitMask);
            }

            private void SetAtomScopeState(bool value, AtomScopeStateBitMask bitMask)
            {
                if (value)
                {
                    this.atomScopeState |= bitMask;
                }
                else
                {
                    this.atomScopeState &= ~bitMask;
                }
            }

            public ODataEntityPropertyMappingCache CachedEpm { get; set; }

            public Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker { get; set; }

            public bool ElementEmpty
            {
                get
                {
                    return ((this.atomScopeState & AtomScopeStateBitMask.EmptyElement) == AtomScopeStateBitMask.EmptyElement);
                }
                set
                {
                    if (value)
                    {
                        this.atomScopeState |= AtomScopeStateBitMask.EmptyElement;
                    }
                    else
                    {
                        this.atomScopeState &= ~AtomScopeStateBitMask.EmptyElement;
                    }
                }
            }

            public ODataAtomReaderNavigationLinkDescriptor FirstNavigationLinkDescriptor { get; set; }

            public bool HasAtomEntryMetadata
            {
                get
                {
                    return (this.atomEntryMetadata != null);
                }
            }

            public bool HasEpmCustomReaderValueCache
            {
                get
                {
                    return (this.epmCustomReaderValueCache != null);
                }
            }

            public bool? MediaLinkEntry
            {
                get
                {
                    return this.mediaLinkEntry;
                }
                set
                {
                    if (this.mediaLinkEntry.HasValue)
                    {
                        if (this.mediaLinkEntry.Value != value)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomReader_MediaLinkEntryMismatch);
                        }
                    }
                    this.mediaLinkEntry = value;
                }
            }

            AtomEntryMetadata IODataAtomReaderEntryState.AtomEntryMetadata
            {
                get
                {
                    if (this.atomEntryMetadata == null)
                    {
                        this.atomEntryMetadata = AtomMetadataReaderUtils.CreateNewAtomEntryMetadata();
                    }
                    return this.atomEntryMetadata;
                }
            }

            IEdmEntityType IODataAtomReaderEntryState.EntityType
            {
                get
                {
                    return base.EntityType;
                }
            }

            ODataEntry IODataAtomReaderEntryState.Entry
            {
                get
                {
                    return (ODataEntry) base.Item;
                }
            }

            bool IODataAtomReaderEntryState.EntryElementEmpty
            {
                get
                {
                    return this.ElementEmpty;
                }
                set
                {
                    this.ElementEmpty = value;
                }
            }

            EpmCustomReaderValueCache IODataAtomReaderEntryState.EpmCustomReaderValueCache
            {
                get
                {
                    return (this.epmCustomReaderValueCache ?? (this.epmCustomReaderValueCache = new EpmCustomReaderValueCache()));
                }
            }

            bool IODataAtomReaderEntryState.HasContent
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasContent);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasContent);
                }
            }

            bool IODataAtomReaderEntryState.HasEditLink
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasEditLink);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasEditLink);
                }
            }

            bool IODataAtomReaderEntryState.HasEditMediaLink
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasEditMediaLink);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasEditMediaLink);
                }
            }

            bool IODataAtomReaderEntryState.HasId
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasId);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasId);
                }
            }

            bool IODataAtomReaderEntryState.HasProperties
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasProperties);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasProperties);
                }
            }

            bool IODataAtomReaderEntryState.HasReadLink
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasReadLink);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasReadLink);
                }
            }

            bool IODataAtomReaderEntryState.HasTypeNameCategory
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasTypeNameCategory);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasTypeNameCategory);
                }
            }

            AtomFeedMetadata IODataAtomReaderFeedState.AtomFeedMetadata
            {
                get
                {
                    if (this.atomFeedMetadata == null)
                    {
                        this.atomFeedMetadata = AtomMetadataReaderUtils.CreateNewAtomFeedMetadata();
                    }
                    return this.atomFeedMetadata;
                }
            }

            ODataFeed IODataAtomReaderFeedState.Feed
            {
                get
                {
                    return (ODataFeed) base.Item;
                }
            }

            bool IODataAtomReaderFeedState.FeedElementEmpty
            {
                get
                {
                    return this.ElementEmpty;
                }
                set
                {
                    this.ElementEmpty = value;
                }
            }

            bool IODataAtomReaderFeedState.HasCount
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasCount);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasCount);
                }
            }

            bool IODataAtomReaderFeedState.HasNextPageLink
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasNextPageLinkInFeed);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasNextPageLinkInFeed);
                }
            }

            bool IODataAtomReaderFeedState.HasReadLink
            {
                get
                {
                    return this.GetAtomScopeState(AtomScopeStateBitMask.HasReadLinkInFeed);
                }
                set
                {
                    this.SetAtomScopeState(value, AtomScopeStateBitMask.HasReadLinkInFeed);
                }
            }

            public IEdmNavigationProperty NavigationProperty { get; set; }

            [Flags]
            private enum AtomScopeStateBitMask
            {
                EmptyElement = 1,
                HasContent = 0x10,
                HasCount = 0x80,
                HasEditLink = 4,
                HasEditMediaLink = 0x400,
                HasId = 8,
                HasNextPageLinkInFeed = 0x100,
                HasProperties = 0x40,
                HasReadLink = 2,
                HasReadLinkInFeed = 0x200,
                HasTypeNameCategory = 0x20,
                None = 0
            }
        }
    }
}

