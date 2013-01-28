namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MaterializerFeed
    {
        private readonly ODataFeed feed;
        private readonly IEnumerable<ODataEntry> entries;
        private MaterializerFeed(ODataFeed feed, IEnumerable<ODataEntry> entries)
        {
            this.feed = feed;
            this.entries = entries;
        }

        public ODataFeed Feed
        {
            get
            {
                return this.feed;
            }
        }
        public IEnumerable<ODataEntry> Entries
        {
            get
            {
                return this.entries;
            }
        }
        public Uri NextPageLink
        {
            get
            {
                return this.feed.NextPageLink;
            }
        }
        public static MaterializerFeed CreateFeed(ODataFeed feed, IEnumerable<ODataEntry> entries)
        {
            if (entries == null)
            {
                entries = Enumerable.Empty<ODataEntry>();
            }
            else
            {
                feed.SetAnnotation<IEnumerable<ODataEntry>>(entries);
            }
            return new MaterializerFeed(feed, entries);
        }

        public static MaterializerFeed GetFeed(ODataFeed feed)
        {
            return new MaterializerFeed(feed, feed.GetAnnotation<IEnumerable<ODataEntry>>());
        }
    }
}

