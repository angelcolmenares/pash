namespace Microsoft.Data.OData
{
    using System;

    internal enum ODataPayloadKind
    {
        Batch = 11,
        BinaryValue = 6,
        Collection = 7,
        EntityReferenceLink = 3,
        EntityReferenceLinks = 4,
        Entry = 1,
        Error = 10,
        Feed = 0,
        MetadataDocument = 9,
        Parameter = 12,
        Property = 2,
        ServiceDocument = 8,
        Unsupported = 0x7fffffff,
        Value = 5
    }
}

