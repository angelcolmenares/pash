namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal interface IODataAtomReaderEntryState
    {
        Microsoft.Data.OData.Atom.AtomEntryMetadata AtomEntryMetadata { get; }

        ODataEntityPropertyMappingCache CachedEpm { get; }

        Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker { get; }

        IEdmEntityType EntityType { get; }

        ODataEntry Entry { get; }

        bool EntryElementEmpty { get; set; }

        Microsoft.Data.OData.Atom.EpmCustomReaderValueCache EpmCustomReaderValueCache { get; }

        ODataAtomReaderNavigationLinkDescriptor FirstNavigationLinkDescriptor { get; set; }

        bool HasContent { get; set; }

        bool HasEditLink { get; set; }

        bool HasEditMediaLink { get; set; }

        bool HasId { get; set; }

        bool HasProperties { get; set; }

        bool HasReadLink { get; set; }

        bool HasTypeNameCategory { get; set; }

        bool? MediaLinkEntry { get; set; }
    }
}

