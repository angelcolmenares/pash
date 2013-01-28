namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Threading.Tasks;

    internal sealed class ODataJsonWriter : ODataWriterCore
    {
        private readonly ODataJsonEntryAndFeedSerializer jsonEntryAndFeedSerializer;
        private readonly ODataJsonOutputContext jsonOutputContext;

        internal ODataJsonWriter(ODataJsonOutputContext jsonOutputContext, bool writingFeed) : base(jsonOutputContext, writingFeed)
        {
            this.jsonOutputContext = jsonOutputContext;
            this.jsonEntryAndFeedSerializer = new ODataJsonEntryAndFeedSerializer(this.jsonOutputContext);
        }

        protected override ODataWriterCore.EntryScope CreateEntryScope(ODataEntry entry, bool skipWriting)
        {
            return new ODataWriterCore.EntryScope(entry, skipWriting, this.jsonOutputContext.WritingResponse, this.jsonOutputContext.MessageWriterSettings.WriterBehavior);
        }

        protected override ODataWriterCore.FeedScope CreateFeedScope(ODataFeed feed, bool skipWriting)
        {
            return new JsonFeedScope(feed, skipWriting);
        }

        protected override void EndEntry(ODataEntry entry)
        {
            if (entry != null)
            {
                ProjectedPropertiesAnnotation projectedProperties = entry.GetAnnotation<ProjectedPropertiesAnnotation>();
                this.jsonEntryAndFeedSerializer.WriteProperties(base.EntryEntityType, entry.Properties, false, base.DuplicatePropertyNamesChecker, projectedProperties);
                this.jsonOutputContext.JsonWriter.EndObjectScope();
            }
        }

        protected override void EndFeed(ODataFeed feed)
        {
            if ((base.ParentNavigationLink == null) || this.jsonOutputContext.WritingResponse)
            {
                this.jsonOutputContext.JsonWriter.EndArrayScope();
                Uri nextPageLink = feed.NextPageLink;
                if ((this.jsonOutputContext.Version >= ODataVersion.V2) && this.jsonOutputContext.WritingResponse)
                {
                    this.WriteFeedCount(feed);
                    if (nextPageLink != null)
                    {
                        this.jsonOutputContext.JsonWriter.WriteName("__next");
                        this.jsonOutputContext.JsonWriter.WriteValue(this.jsonEntryAndFeedSerializer.UriToAbsoluteUriString(nextPageLink));
                    }
                    this.jsonOutputContext.JsonWriter.EndObjectScope();
                }
            }
        }

        protected override void EndNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
            if (!this.jsonOutputContext.WritingResponse && navigationLink.IsCollection.Value)
            {
                this.jsonOutputContext.JsonWriter.EndArrayScope();
            }
        }

        protected override void EndPayload()
        {
            this.jsonEntryAndFeedSerializer.WritePayloadEnd();
        }

        protected override Task FlushAsynchronously()
        {
            return this.jsonOutputContext.FlushAsync();
        }

        protected override void FlushSynchronously()
        {
            this.jsonOutputContext.Flush();
        }

        protected override void StartEntry(ODataEntry entry)
        {
            if (entry == null)
            {
                this.jsonOutputContext.JsonWriter.WriteValue((string) null);
            }
            else
            {
                this.jsonOutputContext.JsonWriter.StartObjectScope();
                ProjectedPropertiesAnnotation projectedProperties = entry.GetAnnotation<ProjectedPropertiesAnnotation>();
                this.jsonEntryAndFeedSerializer.WriteEntryMetadata(entry, projectedProperties, base.EntryEntityType, base.DuplicatePropertyNamesChecker);
            }
        }

        protected override void StartFeed(ODataFeed feed)
        {
            if ((base.ParentNavigationLink == null) || this.jsonOutputContext.WritingResponse)
            {
                if ((this.jsonOutputContext.Version >= ODataVersion.V2) && this.jsonOutputContext.WritingResponse)
                {
                    this.jsonOutputContext.JsonWriter.StartObjectScope();
                    this.WriteFeedCount(feed);
                    this.jsonOutputContext.JsonWriter.WriteDataArrayName();
                }
                this.jsonOutputContext.JsonWriter.StartArrayScope();
            }
        }

        protected override void StartNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
            this.jsonOutputContext.JsonWriter.WriteName(navigationLink.Name);
            if (!this.jsonOutputContext.WritingResponse)
            {
                if (!navigationLink.IsCollection.HasValue)
                {
                    throw new ODataException(Strings.ODataWriterCore_LinkMustSpecifyIsCollection);
                }
                if (navigationLink.IsCollection.Value)
                {
                    this.jsonOutputContext.JsonWriter.StartArrayScope();
                }
            }
        }

        protected override void StartPayload()
        {
            this.jsonEntryAndFeedSerializer.WritePayloadStart();
        }

        protected override void VerifyNotDisposed()
        {
            this.jsonOutputContext.VerifyNotDisposed();
        }

        protected override void WriteDeferredNavigationLink(ODataNavigationLink navigationLink)
        {
            if (navigationLink.Url == null)
            {
                throw new ODataException(Strings.ODataWriter_NavigationLinkMustSpecifyUrl);
            }
            this.jsonOutputContext.JsonWriter.WriteName(navigationLink.Name);
            this.jsonOutputContext.JsonWriter.StartObjectScope();
            this.jsonOutputContext.JsonWriter.WriteName("__deferred");
            this.jsonOutputContext.JsonWriter.StartObjectScope();
            this.jsonOutputContext.JsonWriter.WriteName("uri");
            this.jsonOutputContext.JsonWriter.WriteValue(this.jsonEntryAndFeedSerializer.UriToAbsoluteUriString(navigationLink.Url));
            this.jsonOutputContext.JsonWriter.EndObjectScope();
            this.jsonOutputContext.JsonWriter.EndObjectScope();
        }

        protected override void WriteEntityReferenceInNavigationLinkContent(ODataNavigationLink parentNavigationLink, ODataEntityReferenceLink entityReferenceLink)
        {
            this.jsonOutputContext.JsonWriter.StartObjectScope();
            this.jsonOutputContext.JsonWriter.WriteName("__metadata");
            this.jsonOutputContext.JsonWriter.StartObjectScope();
            this.jsonOutputContext.JsonWriter.WriteName("uri");
            this.jsonOutputContext.JsonWriter.WriteValue(this.jsonEntryAndFeedSerializer.UriToAbsoluteUriString(entityReferenceLink.Url));
            this.jsonOutputContext.JsonWriter.EndObjectScope();
            this.jsonOutputContext.JsonWriter.EndObjectScope();
        }

        private void WriteFeedCount(ODataFeed feed)
        {
            long? count = feed.Count;
            if (count.HasValue && !this.CurrentFeedScope.CountWritten)
            {
                this.jsonOutputContext.JsonWriter.WriteName("__count");
                this.jsonOutputContext.JsonWriter.WriteValue(count.Value);
                this.CurrentFeedScope.CountWritten = true;
            }
        }

        private JsonFeedScope CurrentFeedScope
        {
            get
            {
                return (base.CurrentScope as JsonFeedScope);
            }
        }

        private sealed class JsonFeedScope : ODataWriterCore.FeedScope
        {
            private bool countWritten;

            internal JsonFeedScope(ODataFeed feed, bool skipWriting) : base(feed, skipWriting)
            {
            }

            internal bool CountWritten
            {
                get
                {
                    return this.countWritten;
                }
                set
                {
                    this.countWritten = value;
                }
            }
        }
    }
}

