namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal interface IProviderMember
    {
        EntityType CollectionItemType { get; }

        BuiltInTypeKind EdmTypeKind { get; }

        string EdmTypeName { get; }

        IEnumerable<Facet> Facets { get; }

        bool IsKey { get; }

        IEnumerable<MetadataProperty> MetadataProperties { get; }

        string MimeType { get; }

        string Name { get; }
    }
}

