namespace Microsoft.Data.OData
{
    using System;

    internal enum ODataReaderState
    {
        Start,
        FeedStart,
        FeedEnd,
        EntryStart,
        EntryEnd,
        NavigationLinkStart,
        NavigationLinkEnd,
        EntityReferenceLink,
        Exception,
        Completed
    }
}

