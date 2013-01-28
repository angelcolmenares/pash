namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataJsonReader : ODataReaderCore
    {
        private readonly ODataJsonEntryAndFeedDeserializer jsonEntryAndFeedDeserializer;
        private readonly ODataJsonInputContext jsonInputContext;

        internal ODataJsonReader(ODataJsonInputContext jsonInputContext, IEdmEntityType expectedEntityType, bool readingFeed, IODataReaderWriterListener listener) : base(jsonInputContext, expectedEntityType, readingFeed, listener)
        {
            this.jsonInputContext = jsonInputContext;
            this.jsonEntryAndFeedDeserializer = new ODataJsonEntryAndFeedDeserializer(jsonInputContext);
            if (!this.jsonInputContext.Model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_ParsingWithoutMetadata);
            }
        }

        private void EnterScope(ODataReaderState state, ODataItem item, IEdmEntityType expectedEntityType)
        {
            base.EnterScope(new JsonScope(state, item, expectedEntityType));
        }

        protected override bool ReadAtEntityReferenceLink()
        {
            base.PopScope(ODataReaderState.EntityReferenceLink);
            if (base.CurrentNavigationLink.IsCollection == true)
            {
                this.ReadExpandedCollectionNavigationLinkContentInRequest();
            }
            else
            {
                this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
            }
            return true;
        }

        protected override bool ReadAtEntryEndImplementation()
        {
            bool isTopLevel = base.IsTopLevel;
            bool isExpandedLinkContent = base.IsExpandedLinkContent;
            base.PopScope(ODataReaderState.EntryEnd);
            this.jsonEntryAndFeedDeserializer.JsonReader.Read();
            JsonNodeType nodeType = this.jsonEntryAndFeedDeserializer.JsonReader.NodeType;
            bool flag3 = true;
            if (isTopLevel)
            {
                this.jsonEntryAndFeedDeserializer.ReadPayloadEnd(base.IsReadingNestedPayload);
                this.ReplaceScope(ODataReaderState.Completed);
                return false;
            }
            if (isExpandedLinkContent)
            {
                this.ReadExpandedNavigationLinkEnd(false);
                return flag3;
            }
            if (this.CurrentJsonScope.FeedInExpandedNavigationLinkInRequest)
            {
                this.ReadExpandedCollectionNavigationLinkContentInRequest();
                return flag3;
            }
            JsonNodeType type2 = nodeType;
            if (type2 != JsonNodeType.StartObject)
            {
                if (type2 != JsonNodeType.EndArray)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadEntriesOfFeed(this.jsonEntryAndFeedDeserializer.JsonReader.NodeType));
                }
            }
            else
            {
                this.ReadEntryStart();
                return flag3;
            }
            this.jsonEntryAndFeedDeserializer.ReadFeedEnd(base.CurrentFeed, this.CurrentJsonScope.FeedHasResultsWrapper, base.IsExpandedLinkContent);
            this.ReplaceScope(ODataReaderState.FeedEnd);
            return flag3;
        }

        protected override bool ReadAtEntryStartImplementation()
        {
            if (base.CurrentEntry == null)
            {
                this.ReplaceScope(ODataReaderState.EntryEnd);
            }
            else if (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.EndObject)
            {
                this.ReplaceScope(ODataReaderState.EntryEnd);
            }
            else if (this.jsonInputContext.UseServerApiBehavior)
            {
                IEdmNavigationProperty property;
                ODataNavigationLink navigationLink = this.jsonEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState, out property);
                if (navigationLink != null)
                {
                    this.StartNavigationLink(navigationLink, property);
                }
                else
                {
                    this.ReplaceScope(ODataReaderState.EntryEnd);
                }
            }
            else
            {
                this.StartNavigationLink(this.CurrentEntryState.FirstNavigationLink, this.CurrentEntryState.FirstNavigationProperty);
            }
            return true;
        }

        protected override bool ReadAtFeedEndImplementation()
        {
            bool isTopLevel = base.IsTopLevel;
            base.PopScope(ODataReaderState.FeedEnd);
            if (isTopLevel)
            {
                this.jsonEntryAndFeedDeserializer.JsonReader.Read();
                this.jsonEntryAndFeedDeserializer.ReadPayloadEnd(base.IsReadingNestedPayload);
                this.ReplaceScope(ODataReaderState.Completed);
                return false;
            }
            if (this.jsonInputContext.ReadingResponse)
            {
                this.jsonEntryAndFeedDeserializer.JsonReader.Read();
                this.ReadExpandedNavigationLinkEnd(true);
            }
            else
            {
                this.ReadExpandedCollectionNavigationLinkContentInRequest();
            }
            return true;
        }

        protected override bool ReadAtFeedStartImplementation()
        {
            JsonNodeType nodeType = this.jsonEntryAndFeedDeserializer.JsonReader.NodeType;
            if (nodeType != JsonNodeType.StartObject)
            {
                if (nodeType != JsonNodeType.EndArray)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadEntriesOfFeed(this.jsonEntryAndFeedDeserializer.JsonReader.NodeType));
                }
            }
            else
            {
                this.ReadEntryStart();
                goto Label_006E;
            }
            this.jsonEntryAndFeedDeserializer.ReadFeedEnd(base.CurrentFeed, this.CurrentJsonScope.FeedHasResultsWrapper, base.IsExpandedLinkContent);
            this.ReplaceScope(ODataReaderState.FeedEnd);
        Label_006E:
            return true;
        }

        protected override bool ReadAtNavigationLinkEndImplementation()
        {
            IEdmNavigationProperty property;
            base.PopScope(ODataReaderState.NavigationLinkEnd);
            ODataNavigationLink navigationLink = this.jsonEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState, out property);
            if (navigationLink == null)
            {
                this.ReplaceScope(ODataReaderState.EntryEnd);
            }
            else
            {
                this.StartNavigationLink(navigationLink, property);
            }
            return true;
        }

        protected override bool ReadAtNavigationLinkStartImplementation()
        {
            ODataNavigationLink currentNavigationLink = base.CurrentNavigationLink;
            IODataJsonReaderEntryState linkParentEntityScope = (IODataJsonReaderEntryState) base.LinkParentEntityScope;
            if (this.jsonInputContext.ReadingResponse && this.jsonEntryAndFeedDeserializer.IsDeferredLink(true))
            {
                linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, false, currentNavigationLink.IsCollection);
                this.jsonEntryAndFeedDeserializer.ReadDeferredNavigationLink(currentNavigationLink);
                this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
            }
            else if (!currentNavigationLink.IsCollection.Value)
            {
                if (!this.jsonInputContext.ReadingResponse && this.jsonEntryAndFeedDeserializer.IsEntityReferenceLink())
                {
                    linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, false, false);
                    ODataEntityReferenceLink item = this.jsonEntryAndFeedDeserializer.ReadEntityReferenceLink();
                    this.EnterScope(ODataReaderState.EntityReferenceLink, item, null);
                }
                else
                {
                    linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, false);
                    if (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.PrimitiveValue)
                    {
                        this.EnterScope(ODataReaderState.EntryStart, null, base.CurrentEntityType);
                    }
                    else
                    {
                        this.ReadEntryStart();
                    }
                }
            }
            else
            {
                linkParentEntityScope.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(currentNavigationLink, true, true);
                if (this.jsonInputContext.ReadingResponse)
                {
                    this.ReadFeedStart(true);
                }
                else
                {
                    if ((this.jsonEntryAndFeedDeserializer.JsonReader.NodeType != JsonNodeType.StartObject) && (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType != JsonNodeType.StartArray))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadFeedStart(this.jsonEntryAndFeedDeserializer.JsonReader.NodeType));
                    }
                    bool isResultsWrapperExpected = this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.StartObject;
                    this.jsonEntryAndFeedDeserializer.ReadFeedStart(new ODataFeed(), isResultsWrapperExpected, true);
                    this.CurrentJsonScope.FeedHasResultsWrapper = isResultsWrapperExpected;
                    this.ReadExpandedCollectionNavigationLinkContentInRequest();
                }
            }
            return true;
        }

        protected override bool ReadAtStartImplementation()
        {
            this.jsonEntryAndFeedDeserializer.ReadPayloadStart(base.IsReadingNestedPayload);
            if (base.ReadingFeed)
            {
                this.ReadFeedStart(false);
                return true;
            }
            this.ReadEntryStart();
            return true;
        }

        private void ReadEntryMetadata()
        {
            this.jsonEntryAndFeedDeserializer.JsonReader.StartBuffering();
            bool flag = false;
            while (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.Property)
            {
                if (string.CompareOrdinal(this.jsonEntryAndFeedDeserializer.JsonReader.ReadPropertyName(), "__metadata") == 0)
                {
                    flag = true;
                    break;
                }
                this.jsonEntryAndFeedDeserializer.JsonReader.SkipValue();
            }
            string entityTypeNameFromPayload = null;
            object bookmark = null;
            if (flag)
            {
                bookmark = this.jsonEntryAndFeedDeserializer.JsonReader.BookmarkCurrentPosition();
                entityTypeNameFromPayload = this.jsonEntryAndFeedDeserializer.ReadTypeNameFromMetadataPropertyValue();
            }
            base.ApplyEntityTypeNameFromPayload(entityTypeNameFromPayload);
            if (base.CurrentFeedValidator != null)
            {
                base.CurrentFeedValidator.ValidateEntry(base.CurrentEntityType);
            }
            if (flag)
            {
                this.jsonEntryAndFeedDeserializer.JsonReader.MoveToBookmark(bookmark);
                this.jsonEntryAndFeedDeserializer.ReadEntryMetadataPropertyValue(this.CurrentEntryState);
            }
            this.jsonEntryAndFeedDeserializer.JsonReader.StopBuffering();
            this.jsonEntryAndFeedDeserializer.ValidateEntryMetadata(this.CurrentEntryState);
        }

        private void ReadEntryStart()
        {
            this.jsonEntryAndFeedDeserializer.ReadEntryStart();
            this.StartEntry();
            this.ReadEntryMetadata();
            if (this.jsonInputContext.UseServerApiBehavior)
            {
                this.CurrentEntryState.FirstNavigationLink = null;
                this.CurrentEntryState.FirstNavigationProperty = null;
            }
            else
            {
                IEdmNavigationProperty property;
                this.CurrentEntryState.FirstNavigationLink = this.jsonEntryAndFeedDeserializer.ReadEntryContent(this.CurrentEntryState, out property);
                this.CurrentEntryState.FirstNavigationProperty = property;
            }
        }

        private void ReadExpandedCollectionNavigationLinkContentInRequest()
        {
            if (this.jsonEntryAndFeedDeserializer.IsEntityReferenceLink())
            {
                if (this.State == ODataReaderState.FeedStart)
                {
                    this.ReplaceScope(ODataReaderState.FeedEnd);
                }
                else
                {
                    this.CurrentJsonScope.ExpandedNavigationLinkInRequestHasContent = true;
                    ODataEntityReferenceLink item = this.jsonEntryAndFeedDeserializer.ReadEntityReferenceLink();
                    this.EnterScope(ODataReaderState.EntityReferenceLink, item, null);
                }
            }
            else if ((this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.EndArray) || (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.EndObject))
            {
                if (this.State == ODataReaderState.FeedStart)
                {
                    this.jsonEntryAndFeedDeserializer.ReadFeedEnd(base.CurrentFeed, this.CurrentJsonScope.FeedHasResultsWrapper, true);
                    this.ReplaceScope(ODataReaderState.FeedEnd);
                }
                else if (!this.CurrentJsonScope.ExpandedNavigationLinkInRequestHasContent)
                {
                    this.CurrentJsonScope.ExpandedNavigationLinkInRequestHasContent = true;
                    this.EnterScope(ODataReaderState.FeedStart, new ODataFeed(), base.CurrentEntityType);
                    this.CurrentJsonScope.FeedInExpandedNavigationLinkInRequest = true;
                }
                else
                {
                    if (this.CurrentJsonScope.FeedHasResultsWrapper)
                    {
                        ODataFeed feed = new ODataFeed();
                        this.jsonEntryAndFeedDeserializer.ReadFeedEnd(feed, true, true);
                    }
                    this.jsonEntryAndFeedDeserializer.JsonReader.Read();
                    this.ReadExpandedNavigationLinkEnd(true);
                }
            }
            else if (this.State == ODataReaderState.FeedStart)
            {
                if (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType != JsonNodeType.StartObject)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadEntriesOfFeed(this.jsonEntryAndFeedDeserializer.JsonReader.NodeType));
                }
                this.ReadEntryStart();
            }
            else
            {
                this.CurrentJsonScope.ExpandedNavigationLinkInRequestHasContent = true;
                this.EnterScope(ODataReaderState.FeedStart, new ODataFeed(), base.CurrentEntityType);
                this.CurrentJsonScope.FeedInExpandedNavigationLinkInRequest = true;
            }
        }

        private void ReadExpandedNavigationLinkEnd(bool isCollection)
        {
            base.CurrentNavigationLink.IsCollection = new bool?(isCollection);
            this.ReplaceScope(ODataReaderState.NavigationLinkEnd);
        }

        private void ReadFeedStart(bool isExpandedLinkContent)
        {
            ODataFeed feed = new ODataFeed();
            if ((this.jsonEntryAndFeedDeserializer.JsonReader.NodeType != JsonNodeType.StartObject) && (this.jsonEntryAndFeedDeserializer.JsonReader.NodeType != JsonNodeType.StartArray))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonReader_CannotReadFeedStart(this.jsonEntryAndFeedDeserializer.JsonReader.NodeType));
            }
            bool isResultsWrapperExpected = this.jsonEntryAndFeedDeserializer.JsonReader.NodeType == JsonNodeType.StartObject;
            this.jsonEntryAndFeedDeserializer.ReadFeedStart(feed, isResultsWrapperExpected, isExpandedLinkContent);
            this.EnterScope(ODataReaderState.FeedStart, feed, base.CurrentEntityType);
            this.CurrentJsonScope.FeedHasResultsWrapper = isResultsWrapperExpected;
        }

        private void ReplaceScope(ODataReaderState state)
        {
            base.ReplaceScope(new JsonScope(state, this.Item, base.CurrentEntityType));
        }

        private void StartEntry()
        {
            this.EnterScope(ODataReaderState.EntryStart, ReaderUtils.CreateNewEntry(), base.CurrentEntityType);
            this.CurrentJsonScope.DuplicatePropertyNamesChecker = this.jsonInputContext.CreateDuplicatePropertyNamesChecker();
        }

        private void StartNavigationLink(ODataNavigationLink navigationLink, IEdmNavigationProperty navigationProperty)
        {
            IEdmEntityType expectedEntityType = null;
            if (navigationProperty != null)
            {
                IEdmTypeReference type = navigationProperty.Type;
                expectedEntityType = type.IsCollection() ? type.AsCollection().ElementType().AsEntity().EntityDefinition() : type.AsEntity().EntityDefinition();
            }
            this.EnterScope(ODataReaderState.NavigationLinkStart, navigationLink, expectedEntityType);
        }

        private IODataJsonReaderEntryState CurrentEntryState
        {
            get
            {
                return (IODataJsonReaderEntryState) base.CurrentScope;
            }
        }

        private JsonScope CurrentJsonScope
        {
            get
            {
                return (JsonScope) base.CurrentScope;
            }
        }

        private sealed class JsonScope : ODataReaderCore.Scope, IODataJsonReaderEntryState
        {
            internal JsonScope(ODataReaderState state, ODataItem item, IEdmEntityType expectedEntityType) : base(state, item, expectedEntityType)
            {
            }

            public Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker { get; set; }

            public bool ExpandedNavigationLinkInRequestHasContent { get; set; }

            public bool FeedHasResultsWrapper { get; set; }

            public bool FeedInExpandedNavigationLinkInRequest { get; set; }

            public ODataNavigationLink FirstNavigationLink { get; set; }

            public IEdmNavigationProperty FirstNavigationProperty { get; set; }

            public bool MetadataPropertyFound { get; set; }

            IEdmEntityType IODataJsonReaderEntryState.EntityType
            {
                get
                {
                    return base.EntityType;
                }
            }

            ODataEntry IODataJsonReaderEntryState.Entry
            {
                get
                {
                    return (ODataEntry) base.Item;
                }
            }
        }
    }
}

