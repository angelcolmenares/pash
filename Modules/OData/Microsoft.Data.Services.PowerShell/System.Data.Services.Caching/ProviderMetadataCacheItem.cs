using System.Data.Services.Providers;

namespace System.Data.Services.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class ProviderMetadataCacheItem
    {
        private readonly Dictionary<ResourceType, List<ResourceType>> childTypesCache = new Dictionary<ResourceType, List<ResourceType>>(ReferenceEqualityComparer<ResourceType>.Instance);
        private readonly Dictionary<string, ResourceSet> entitySets = new Dictionary<string, ResourceSet>(EqualityComparer<string>.Default);
        private readonly Dictionary<string, ServiceOperation> serviceOperations = new Dictionary<string, ServiceOperation>(EqualityComparer<string>.Default);
        private readonly System.Type type;
        private readonly Dictionary<System.Type, ResourceType> typeCache = new Dictionary<System.Type, ResourceType>(EqualityComparer<System.Type>.Default);

        internal ProviderMetadataCacheItem(System.Type type)
        {
            this.type = type;
        }

        internal Dictionary<ResourceType, List<ResourceType>> ChildTypesCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.childTypesCache;
            }
        }

        internal Dictionary<string, ResourceSet> EntitySets
        {
            [DebuggerStepThrough]
            get
            {
                return this.entitySets;
            }
        }

        internal Dictionary<string, ServiceOperation> ServiceOperations
        {
            [DebuggerStepThrough]
            get
            {
                return this.serviceOperations;
            }
        }

        internal System.Type Type
        {
            [DebuggerStepThrough]
            get
            {
                return this.type;
            }
        }

        internal Dictionary<System.Type, ResourceType> TypeCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.typeCache;
            }
        }
    }
}

