namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface IDataServiceMetadataProvider
    {
        IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType);
        ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty);
        bool HasDerivedTypes(ResourceType resourceType);
        bool TryResolveResourceSet(string name, out ResourceSet resourceSet);
        bool TryResolveResourceType(string name, out ResourceType resourceType);
        bool TryResolveServiceOperation(string name, out ServiceOperation serviceOperation);

        string ContainerName { get; }

        string ContainerNamespace { get; }

        IEnumerable<ResourceSet> ResourceSets { get; }

        IEnumerable<ServiceOperation> ServiceOperations { get; }

        IEnumerable<ResourceType> Types { get; }
    }
}

