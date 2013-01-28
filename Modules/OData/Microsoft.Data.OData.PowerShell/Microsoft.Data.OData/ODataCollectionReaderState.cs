namespace Microsoft.Data.OData
{
    using System;

    internal enum ODataCollectionReaderState
    {
        Start,
        CollectionStart,
        Value,
        CollectionEnd,
        Exception,
        Completed
    }
}

