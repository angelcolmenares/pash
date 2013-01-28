namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ODataReaderEntityMaterializer : ODataEntityMaterializer
    {
        private ODataFeedOrEntryReader itemReader;
        private ODataMessageReader messageReader;

        public ODataReaderEntityMaterializer(ODataMessageReader odataMessageReader, ODataReader reader, ResponseInfo responseInfo, QueryComponents queryComponents, Type expectedType, ProjectionPlan materializeEntryPlan) : base(responseInfo, queryComponents, expectedType, materializeEntryPlan)
        {
            this.messageReader = odataMessageReader;
            this.itemReader = new ODataFeedOrEntryReader(reader, responseInfo);
        }

        protected override void OnDispose()
        {
            if (this.messageReader != null)
            {
                this.messageReader.Dispose();
                this.messageReader = null;
            }
            this.itemReader.Dispose();
        }

        internal static MaterializerEntry ParseSingleEntityPayload(IODataResponseMessage message, ResponseInfo responseInfo, Type expectedType)
        {
            ODataPayloadKind payloadKind = ODataPayloadKind.Entry;
            using (ODataMessageReader reader = ODataMaterializer.CreateODataMessageReader(message, responseInfo, false, ref payloadKind))
            {
                IEdmType orCreateEdmType = ClientEdmModel.GetModel(responseInfo.MaxProtocolVersion).GetOrCreateEdmType(expectedType);
                ODataReader reader2 = ODataMaterializer.CreateODataReader(reader, payloadKind, orCreateEdmType, responseInfo.MaxProtocolVersion);
                ODataFeedOrEntryReader reader3 = new ODataFeedOrEntryReader(reader2, responseInfo);
                ODataEntry currentEntry = null;
                bool flag = false;
                while (reader3.Read())
                {
                    flag |= reader3.CurrentFeed != null;
                    if (reader3.CurrentEntry != null)
                    {
                        if (currentEntry != null)
                        {
                            throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomParser_SingleEntry_MultipleFound);
                        }
                        currentEntry = reader3.CurrentEntry;
                    }
                }
                if (currentEntry == null)
                {
                    if (flag)
                    {
                        throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomParser_SingleEntry_NoneFound);
                    }
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomParser_SingleEntry_ExpectedFeedOrEntry);
                }
                return MaterializerEntry.GetEntry(currentEntry);
            }
        }

        protected override bool ReadNextFeedOrEntry()
        {
            return this.itemReader.Read();
        }

        internal override long CountValue
        {
            get
            {
                return this.itemReader.GetCountValue(!this.IsDisposed);
            }
        }

        internal override ODataEntry CurrentEntry
        {
            get
            {
                return this.itemReader.CurrentEntry;
            }
        }

        internal override ODataFeed CurrentFeed
        {
            get
            {
                return this.itemReader.CurrentFeed;
            }
        }

        internal override bool IsCountable
        {
            get
            {
                return true;
            }
        }

        protected override bool IsDisposed
        {
            get
            {
                return (this.messageReader == null);
            }
        }

        internal override bool IsEndOfStream
        {
            get
            {
                if (!this.IsDisposed)
                {
                    return this.itemReader.IsEndOfStream;
                }
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ODataFeedOrEntryReader
        {
            private readonly ODataReader reader;
            private readonly ResponseInfo responseInfo;
            private IEnumerator<ODataEntry> feedEntries;
            private ODataFeed currentFeed;
            private ODataEntry currentEntry;
            public ODataFeedOrEntryReader(ODataReader reader, ResponseInfo responseInfo)
            {
                this.reader = reader;
                this.responseInfo = responseInfo;
                this.currentEntry = null;
                this.currentFeed = null;
                this.feedEntries = null;
            }

            public ODataFeed CurrentFeed
            {
                get
                {
                    return this.currentFeed;
                }
            }
            public ODataEntry CurrentEntry
            {
                get
                {
                    return this.currentEntry;
                }
            }
            public bool IsEndOfStream
            {
                get
                {
                    return (this.reader.State == ODataReaderState.Completed);
                }
            }
            public long GetCountValue(bool readIfNoFeed)
            {
                if (((this.currentFeed == null) && (this.currentEntry == null)) && (readIfNoFeed && this.TryReadFeed(true, out this.currentFeed)))
                {
                    this.feedEntries = MaterializerFeed.GetFeed(this.currentFeed).Entries.GetEnumerator();
                }
                if ((this.currentFeed == null) || !this.currentFeed.Count.HasValue)
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_CountNotPresent);
                }
                return this.currentFeed.Count.Value;
            }

            public bool Read()
            {
                if (this.feedEntries != null)
                {
                    if (this.feedEntries.MoveNext())
                    {
                        this.currentEntry = this.feedEntries.Current;
                        return true;
                    }
                    this.feedEntries = null;
                    this.currentEntry = null;
                }
                ODataReaderState state = this.reader.State;
                switch (state)
                {
                    case ODataReaderState.Start:
                        ODataFeed feed;
                        MaterializerEntry entry;
                        if (!this.TryReadFeedOrEntry(true, out feed, out entry))
                        {
                            throw new NotImplementedException();
                        }
                        this.currentEntry = (entry != null) ? entry.Entry : null;
                        this.currentFeed = feed;
                        if (this.currentFeed != null)
                        {
                            this.feedEntries = MaterializerFeed.GetFeed(this.currentFeed).Entries.GetEnumerator();
                        }
                        return true;

                    case ODataReaderState.FeedStart:
                    case ODataReaderState.EntryStart:
                        break;

                    case ODataReaderState.FeedEnd:
                    case ODataReaderState.EntryEnd:
                        if (this.TryRead() || (this.reader.State != ODataReaderState.Completed))
                        {
                            throw System.Data.Services.Client.Error.InternalError(InternalError.UnexpectedReadState);
                        }
                        this.currentEntry = null;
                        return false;

                    default:
                        if (state != ODataReaderState.Completed)
                        {
                            break;
                        }
                        this.currentEntry = null;
                        return false;
                }
                throw System.Data.Services.Client.Error.InternalError(InternalError.UnexpectedReadState);
            }

            public void Dispose()
            {
                if (this.feedEntries != null)
                {
                    this.feedEntries.Dispose();
                    this.feedEntries = null;
                }
            }

            private bool TryReadFeedOrEntry(bool lazy, out ODataFeed feed, out MaterializerEntry entry)
            {
                if (this.TryStartReadFeedOrEntry())
                {
                    if (this.reader.State == ODataReaderState.EntryStart)
                    {
                        entry = this.ReadEntryCore();
                        feed = null;
                    }
                    else
                    {
                        entry = null;
                        feed = this.ReadFeedCore(lazy);
                    }
                }
                else
                {
                    feed = null;
                    entry = null;
                }
                if (feed == null)
                {
                    return (entry != null);
                }
                return true;
            }

            private bool TryStartReadFeedOrEntry()
            {
                if (!this.TryRead())
                {
                    return false;
                }
                if (this.reader.State != ODataReaderState.FeedStart)
                {
                    return (this.reader.State == ODataReaderState.EntryStart);
                }
                return true;
            }

            private bool TryReadFeed(bool lazy, out ODataFeed feed)
            {
                if (this.TryStartReadFeedOrEntry())
                {
                    this.ExpectState(ODataReaderState.FeedStart);
                    feed = this.ReadFeedCore(lazy);
                }
                else
                {
                    feed = null;
                }
                return (feed != null);
            }

            private ODataFeed ReadFeedCore(bool lazy)
            {
                this.ExpectState(ODataReaderState.FeedStart);
                ODataFeed item = (ODataFeed) this.reader.Item;
                IEnumerable<ODataEntry> entries = this.LazyReadEntries();
                if (lazy)
                {
                    MaterializerFeed.CreateFeed(item, entries);
                    return item;
                }
                MaterializerFeed.CreateFeed(item, new List<ODataEntry>(entries));
                return item;
            }

            private IEnumerable<ODataEntry> LazyReadEntries()
            {
                while (true)
                {
                    MaterializerEntry iteratorVariable0;
                    if (!this.TryReadEntry(out iteratorVariable0))
                    {
                        yield break;
                    }
                    yield return iteratorVariable0.Entry;
                }
            }

            private bool TryReadEntry(out MaterializerEntry entry)
            {
                if (this.TryStartReadFeedOrEntry())
                {
                    this.ExpectState(ODataReaderState.EntryStart);
                    entry = this.ReadEntryCore();
                    return true;
                }
                entry = null;
                return false;
            }

            private MaterializerEntry ReadEntryCore()
            {
                MaterializerEntry entry2;
                this.ExpectState(ODataReaderState.EntryStart);
                ODataEntry item = (ODataEntry) this.reader.Item;
                if (item == null)
                {
                    entry2 = MaterializerEntry.CreateEmpty();
                    this.ReadAndExpectState(ODataReaderState.EntryEnd);
                    return entry2;
                }
                entry2 = MaterializerEntry.CreateEntry(item, this.responseInfo.MaxProtocolVersion);
                do
                {
                    this.AssertRead();
                    switch (this.reader.State)
                    {
                        case ODataReaderState.EntryEnd:
                            break;

                        case ODataReaderState.NavigationLinkStart:
                            entry2.AddNavigationLink(this.ReadNavigationLink());
                            break;

                        default:
                            throw System.Data.Services.Client.Error.InternalError(InternalError.UnexpectedReadState);
                    }
                }
                while (this.reader.State != ODataReaderState.EntryEnd);
                entry2.UpdateEntityDescriptor();
                return entry2;
            }

            private ODataNavigationLink ReadNavigationLink()
            {
                MaterializerEntry entry;
                ODataFeed feed;
                ODataNavigationLink item = (ODataNavigationLink) this.reader.Item;
                if (this.TryReadFeedOrEntry(false, out feed, out entry))
                {
                    if (feed != null)
                    {
                        MaterializerNavigationLink.CreateLink(item, feed);
                    }
                    else
                    {
                        MaterializerNavigationLink.CreateLink(item, entry);
                    }
                    this.ReadAndExpectState(ODataReaderState.NavigationLinkEnd);
                }
                this.ExpectState(ODataReaderState.NavigationLinkEnd);
                return item;
            }

            private bool TryRead()
            {
                bool flag;
                try
                {
                    flag = this.reader.Read();
                }
                catch (ODataErrorException exception)
                {
                    throw new DataServiceClientException(System.Data.Services.Client.Strings.Deserialize_ServerException(exception.Error.Message), exception);
                }
                catch (ODataException exception2)
                {
                    throw new InvalidOperationException(exception2.Message, exception2);
                }
                return flag;
            }

            private void ReadAndExpectState(ODataReaderState expectedState)
            {
                this.AssertRead();
                this.ExpectState(expectedState);
            }

            private void AssertRead()
            {
                if (!this.TryRead())
                {
                    throw System.Data.Services.Client.Error.InternalError(InternalError.UnexpectedReadState);
                }
            }

            private void ExpectState(ODataReaderState expectedState)
            {
                if (this.reader.State != expectedState)
                {
                    throw System.Data.Services.Client.Error.InternalError(InternalError.UnexpectedReadState);
                }
            }
		}
    }
}

