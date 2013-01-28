using System.Data.Services.Providers;

namespace System.Data.Services.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;

    internal class DataServiceCacheItem
    {
        private readonly DataServiceConfiguration configuration;
        private readonly Dictionary<string, ResourceAssociationSet> resourceAssociationSetCache;
        private readonly Dictionary<string, ResourceSetWrapper> resourceSetWrapperCache;
        private readonly Dictionary<string, ResourceType> visibleTypeCache;

        internal DataServiceCacheItem(DataServiceConfiguration dataServiceConfiguration)
        {
            this.configuration = dataServiceConfiguration;
            this.resourceSetWrapperCache = new Dictionary<string, ResourceSetWrapper>(EqualityComparer<string>.Default);
            this.visibleTypeCache = new Dictionary<string, ResourceType>(EqualityComparer<string>.Default);
            this.resourceAssociationSetCache = new Dictionary<string, ResourceAssociationSet>(EqualityComparer<string>.Default);
        }

        internal DataServiceConfiguration Configuration
        {
            [DebuggerStepThrough]
            get
            {
                return this.configuration;
            }
        }

        internal Dictionary<string, ResourceAssociationSet> ResourceAssociationSetCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceAssociationSetCache;
            }
        }

        internal Dictionary<string, ResourceSetWrapper> ResourceSetWrapperCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceSetWrapperCache;
            }
        }

        internal Dictionary<string, ResourceType> VisibleTypeCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.visibleTypeCache;
            }
        }
    }
}

