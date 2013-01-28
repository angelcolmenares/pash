namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;

    internal interface IODataAtomReaderFeedState
    {
        Microsoft.Data.OData.Atom.AtomFeedMetadata AtomFeedMetadata { get; }

        ODataFeed Feed { get; }

        bool FeedElementEmpty { get; set; }

        bool HasCount { get; set; }

        bool HasNextPageLink { get; set; }

        bool HasReadLink { get; set; }
    }
}

