namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;

    internal interface IODataJsonReaderEntryState
    {
        Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker { get; }

        IEdmEntityType EntityType { get; }

        ODataEntry Entry { get; }

        ODataNavigationLink FirstNavigationLink { get; set; }

        IEdmNavigationProperty FirstNavigationProperty { get; set; }

        bool MetadataPropertyFound { get; set; }
    }
}

