namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}: {ResourceType}")]
    internal class ResourceSetWrapper
    {
        private DataServiceProtocolVersion? epmMinDSPV;
        private bool hasAccessibleNavigationProperty;
        private bool hasEntityPropertyMappings;
        private bool hasNavigationPropertyOrNamedStreamOnDerivedTypes;
        private bool hasOpenTypes;
        private Version metadataVersion;
        private int pageSize;
        private MethodInfo[] readAuthorizationMethods;
        private readonly Dictionary<System.Data.Services.Providers.ResourceType, ResourcePropertyCache> resourcePropertyCache;
        private readonly System.Data.Services.Providers.ResourceSet resourceSet;
        private System.Data.Services.Providers.ResourceType resourceType;
        private EntitySetRights rights;
        private const string UseMetadataKeyOrderDictionaryKey = "UseMetadataKeyOrder";
        private MethodInfo[] writeAuthorizationMethods;

        private ResourceSetWrapper(System.Data.Services.Providers.ResourceSet resourceSet)
        {
            this.resourceSet = resourceSet;
            this.resourcePropertyCache = new Dictionary<System.Data.Services.Providers.ResourceType, ResourcePropertyCache>(ReferenceEqualityComparer<System.Data.Services.Providers.ResourceType>.Instance);
        }

        private void ApplyConfiguration(DataServiceConfiguration configuration)
        {
            this.rights = configuration.GetResourceSetRights(this.resourceSet);
            this.pageSize = configuration.GetResourceSetPageSize(this.resourceSet);
            if (this.pageSize < 0)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataService_SDP_PageSizeMustbeNonNegative(this.pageSize, this.Name));
            }
            this.readAuthorizationMethods = configuration.GetReadAuthorizationMethods(this.resourceSet);
            this.writeAuthorizationMethods = configuration.GetWriteAuthorizationMethods(this.resourceSet);
        }

        private void CheckHierarchy(DataServiceProviderWrapper provider)
        {
            if (!this.epmMinDSPV.HasValue)
            {
                System.Data.Services.Providers.ResourceType resourceType = this.resourceSet.ResourceType;
                bool hasEntityPropertyMappings = resourceType.HasEntityPropertyMappings;
                DataServiceProtocolVersion epmMinimumDataServiceProtocolVersion = resourceType.EpmMinimumDataServiceProtocolVersion;
                bool flag2 = false;
                bool flag3 = this.GetEntitySerializableProperties(provider, resourceType).Any<ResourceProperty>(p => p.TypeKind == ResourceTypeKind.EntityType);
                bool isOpenType = resourceType.IsOpenType;
                Version metadataVersion = resourceType.MetadataVersion;
                if (provider.HasDerivedTypes(resourceType))
                {
                    foreach (System.Data.Services.Providers.ResourceType type2 in provider.GetDerivedTypes(resourceType))
                    {
                        if (type2.EpmMinimumDataServiceProtocolVersion > epmMinimumDataServiceProtocolVersion)
                        {
                            epmMinimumDataServiceProtocolVersion = type2.EpmMinimumDataServiceProtocolVersion;
                        }
                        hasEntityPropertyMappings |= type2.HasEntityPropertyMappings;
                        bool flag5 = this.GetEntitySerializablePropertiesDeclaredOnTheResourceType(provider, type2).Any<ResourceProperty>(p => p.TypeKind == ResourceTypeKind.EntityType);
                        flag3 |= flag5;
                        flag2 |= type2.HasNamedStreamsDeclaredOnThisType | flag5;
                        metadataVersion = WebUtil.RaiseVersion(metadataVersion, type2.MetadataVersion);
                        isOpenType |= type2.IsOpenType;
                        if (((epmMinimumDataServiceProtocolVersion == DataServiceProtocolVersion.V3) && (metadataVersion == RequestDescription.Version3Dot0)) && (flag2 && isOpenType))
                        {
                            break;
                        }
                    }
                }
                this.hasEntityPropertyMappings = hasEntityPropertyMappings;
                this.epmMinDSPV = new DataServiceProtocolVersion?(epmMinimumDataServiceProtocolVersion);
                this.hasNavigationPropertyOrNamedStreamOnDerivedTypes = flag2;
                this.hasAccessibleNavigationProperty = flag3;
                this.hasOpenTypes = isOpenType;
                this.metadataVersion = metadataVersion;
            }
        }

        internal static ResourceSetWrapper CreateResourceSetWrapper(System.Data.Services.Providers.ResourceSet resourceSet, DataServiceProviderWrapper provider, Func<System.Data.Services.Providers.ResourceType, System.Data.Services.Providers.ResourceType> resourceTypeValidator)
        {
            if (!resourceSet.IsReadOnly)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceProviderWrapper_ResourceContainerNotReadonly(resourceSet.Name));
            }
            ResourceSetWrapper wrapper = new ResourceSetWrapper(resourceSet);
            wrapper.ApplyConfiguration(provider.Configuration);
            if (!wrapper.IsVisible)
            {
                return null;
            }
            wrapper.resourceType = resourceTypeValidator(resourceSet.ResourceType);
            return wrapper;
        }

        internal IEnumerable<ResourceProperty> GetEntitySerializableProperties(DataServiceProviderWrapper provider, System.Data.Services.Providers.ResourceType entityType)
        {
            return this.InitializeResourcePropertyCache(provider, entityType).Properties;
        }

        private IEnumerable<ResourceProperty> GetEntitySerializablePropertiesDeclaredOnTheResourceType(DataServiceProviderWrapper provider, System.Data.Services.Providers.ResourceType entityType)
        {
            return this.InitializeResourcePropertyCache(provider, entityType).PropertiesDeclaredOnTheType;
        }

        internal IEnumerable<ResourceProperty> GetKeyPropertiesForOrderBy()
        {
            if (!this.UseMetadataKeyOrder)
            {
                return this.ResourceType.KeyProperties;
            }
            return (from resourceProperty in this.ResourceType.Properties
                where resourceProperty.IsOfKind(ResourcePropertyKind.Key)
                select resourceProperty);
        }

        internal bool HasAccessibleNavigationProperty(DataServiceProviderWrapper provider)
        {
            this.CheckHierarchy(provider);
            return this.hasAccessibleNavigationProperty;
        }

        internal bool HasEntityPropertyMappings(DataServiceProviderWrapper provider)
        {
            this.CheckHierarchy(provider);
            return this.hasEntityPropertyMappings;
        }

        internal bool HasNavigationPropertyOrNamedStreamsOnDerivedTypes(DataServiceProviderWrapper provider)
        {
            this.CheckHierarchy(provider);
            return this.hasNavigationPropertyOrNamedStreamOnDerivedTypes;
        }

        private ResourcePropertyCache InitializeResourcePropertyCache(DataServiceProviderWrapper provider, System.Data.Services.Providers.ResourceType type)
        {
            ResourcePropertyCache cache;
            if (!this.resourcePropertyCache.TryGetValue(type, out cache))
            {
                cache = new ResourcePropertyCache {
                    Properties = new List<ResourceProperty>()
                };
                foreach (ResourceProperty property in type.Properties)
                {
                    if ((property.TypeKind != ResourceTypeKind.EntityType) || (provider.GetContainer(this, type, property) != null))
                    {
                        cache.Properties.Add(property);
                    }
                }
                cache.PropertiesDeclaredOnTheType = new List<ResourceProperty>();
                foreach (ResourceProperty property2 in type.PropertiesDeclaredOnThisType)
                {
                    if ((property2.TypeKind != ResourceTypeKind.EntityType) || (provider.GetContainer(this, type, property2) != null))
                    {
                        cache.PropertiesDeclaredOnTheType.Add(property2);
                    }
                }
                this.resourcePropertyCache.Add(type, cache);
            }
            return cache;
        }

        internal Version MinimumResponsePayloadVersion(IDataService service, bool considerEpmInVersion)
        {
            Version versionToRaise = RequestDescription.Version1Dot0;
            if (service.Configuration.DataServiceBehavior.ShouldIncludeAssociationLinksInResponse && this.HasAccessibleNavigationProperty(service.Provider))
            {
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, RequestDescription.Version3Dot0);
            }
            if (considerEpmInVersion)
            {
                Version targetVersion = this.VerifyEpmProtocolVersion(service.Provider).ToVersion();
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, targetVersion);
            }
            this.CheckHierarchy(service.Provider);
            versionToRaise = WebUtil.RaiseVersion(versionToRaise, this.metadataVersion);
            if (this.hasOpenTypes)
            {
                Version version3 = service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion();
                Version requestMaxVersion = service.OperationContext.Host.RequestMaxVersion;
                Version version5 = (requestMaxVersion < version3) ? requestMaxVersion : version3;
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, version5);
            }
            return versionToRaise;
        }

        internal DataServiceProtocolVersion VerifyEpmProtocolVersion(DataServiceProviderWrapper provider)
        {
            this.CheckHierarchy(provider);
            return this.epmMinDSPV.Value;
        }

        public MethodInfo[] ChangeInterceptors
        {
            [DebuggerStepThrough]
            get
            {
                return this.writeAuthorizationMethods;
            }
        }

        internal IEnumerable<KeyValuePair<string, object>> CustomAnnotations
        {
            get
            {
                return this.resourceSet.CustomAnnotations;
            }
        }

        internal string EntityContainerName
        {
            get
            {
                return this.resourceSet.EntityContainerName;
            }
        }

        public bool IsVisible
        {
            get
            {
                return (this.rights != EntitySetRights.None);
            }
        }

        public string Name
        {
            get
            {
                return this.resourceSet.Name;
            }
        }

        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
        }

        public MethodInfo[] QueryInterceptors
        {
            [DebuggerStepThrough]
            get
            {
                return this.readAuthorizationMethods;
            }
        }

        internal Type QueryRootType
        {
            get
            {
                return this.resourceSet.QueryRootType;
            }
        }

        internal System.Data.Services.Providers.ResourceSet ResourceSet
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceSet;
            }
        }

        public System.Data.Services.Providers.ResourceType ResourceType
        {
            get
            {
                return this.resourceType;
            }
        }

        public EntitySetRights Rights
        {
            get
            {
                return this.rights;
            }
        }

        internal bool UseMetadataKeyOrder
        {
            get
            {
                object obj2;
                Dictionary<string, object> customState = this.resourceSet.CustomState as Dictionary<string, object>;
                return ((((customState != null) && customState.TryGetValue("UseMetadataKeyOrder", out obj2)) && ((obj2 is bool) && ((bool) obj2))) || this.resourceSet.UseMetadataKeyOrder);
            }
        }

        private class ResourcePropertyCache
        {
            public List<ResourceProperty> Properties { get; set; }

            public List<ResourceProperty> PropertiesDeclaredOnTheType { get; set; }
        }
    }
}

