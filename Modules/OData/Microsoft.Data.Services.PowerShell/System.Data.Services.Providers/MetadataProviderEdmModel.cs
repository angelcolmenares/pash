namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Annotations;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class MetadataProviderEdmModel : EdmElement, IEdmModel, IEdmElement
    {
        private readonly Dictionary<string, string> associationSetByKeyCache;
        private MetadataProviderState cacheState;
        private readonly IEdmModel coreModel = EdmCoreModel.Instance;
        private readonly Dictionary<IEdmStructuredType, List<IEdmStructuredType>> derivedTypeMappings;
        private readonly EdmDirectValueAnnotationsManager directValueAnnotationsManager = new EdmDirectValueAnnotationsManager();
        private readonly Dictionary<string, MetadataProviderEdmEntityContainer> entityContainerCache;
        private readonly Dictionary<ResourceType, IEdmCollectionType> entityPrimitiveOrComplexCollectionTypeCache;
        private const bool EntityPrimitiveOrComplexCollectionTypeDefaultNullability = true;
        private const bool EntityTypeDefaultNullability = true;
        private MetadataProviderState materializationState;
        private readonly DataServiceProviderWrapper metadataProvider;
        private readonly DataServiceOperationContext operationContext;
        private const bool PrimitiveOrComplexCollectionItemTypeDefaultNullability = false;
        private readonly Dictionary<ResourceType, IEdmCollectionType> primitiveOrComplexCollectionTypeCache;
        private const bool PrimitiveOrComplexCollectionTypeDefaultNullability = true;
        private readonly IEnumerable<IEdmModel> referencedModels = new IEdmModel[] { EdmCoreModel.Instance };
        private readonly Dictionary<string, HashSet<ResourceType>> resourceTypesPerNamespaceCache;
        private readonly Dictionary<string, IEdmSchemaType> schemaTypeCache;
        private readonly DataServiceStreamProviderWrapper streamProviderWrapper;

        internal MetadataProviderEdmModel(DataServiceProviderWrapper provider, DataServiceOperationContext operationContext, DataServiceStreamProviderWrapper streamProviderWrapper)
        {
            this.metadataProvider = provider;
            this.operationContext = operationContext;
            this.streamProviderWrapper = streamProviderWrapper;
            this.schemaTypeCache = new Dictionary<string, IEdmSchemaType>(StringComparer.Ordinal);
            this.resourceTypesPerNamespaceCache = new Dictionary<string, HashSet<ResourceType>>(StringComparer.Ordinal);
            this.entityContainerCache = new Dictionary<string, MetadataProviderEdmEntityContainer>(StringComparer.Ordinal);
            this.primitiveOrComplexCollectionTypeCache = new Dictionary<ResourceType, IEdmCollectionType>(EqualityComparer<ResourceType>.Default);
            this.entityPrimitiveOrComplexCollectionTypeCache = new Dictionary<ResourceType, IEdmCollectionType>(EqualityComparer<ResourceType>.Default);
            this.derivedTypeMappings = new Dictionary<IEdmStructuredType, List<IEdmStructuredType>>(EqualityComparer<IEdmStructuredType>.Default);
            this.associationSetByKeyCache = new Dictionary<string, string>(StringComparer.Ordinal);
            Version version = this.metadataProvider.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion();
            this.SetDataServiceVersion(version);
            Version version2 = null;
            if (!MetadataProviderUtils.DataServiceEdmVersionMap.TryGetValue(version, out version2))
            {
                this.SetEdmVersion(Microsoft.Data.Edm.Library.EdmConstants.EdmVersionLatest);
            }
            else
            {
                this.SetEdmVersion(version2);
            }
        }

        private IEdmComplexType AddComplexType(ResourceType resourceType, string resourceTypeNamespace)
        {
            Action<MetadataProviderEdmComplexType> propertyLoadAction = delegate (MetadataProviderEdmComplexType type) {
                IEnumerable<ResourceProperty> allVisiblePropertiesDeclaredInThisType = this.GetAllVisiblePropertiesDeclaredInThisType(resourceType);
                if (allVisiblePropertiesDeclaredInThisType != null)
                {
                    foreach (ResourceProperty property in allVisiblePropertiesDeclaredInThisType)
                    {
                        this.CreateProperty(type, property);
                    }
                }
            };
            MetadataProviderEdmComplexType schemaType = new MetadataProviderEdmComplexType(resourceTypeNamespace, resourceType.Name, (resourceType.BaseType != null) ? ((IEdmComplexType) this.EnsureSchemaType(resourceType.BaseType)) : null, false, propertyLoadAction);
            this.CacheSchemaType(schemaType);
            MetadataProviderUtils.ConvertCustomAnnotations(this, resourceType.CustomAnnotations, schemaType);
            return schemaType;
        }

        private IEdmEntityType AddEntityType(ResourceType resourceType, string resourceTypeNamespace)
        {
            Action<MetadataProviderEdmEntityType> propertyLoadAction = delegate (MetadataProviderEdmEntityType type) {
                IEnumerable<ResourceProperty> allVisiblePropertiesDeclaredInThisType = this.GetAllVisiblePropertiesDeclaredInThisType(resourceType);
                if (allVisiblePropertiesDeclaredInThisType != null)
                {
                    foreach (ResourceProperty property in allVisiblePropertiesDeclaredInThisType)
                    {
                        IEdmProperty property2 = this.CreateProperty(type, property);
                        if (property.IsOfKind(ResourcePropertyKind.Key))
                        {
                            type.AddKeys(new IEdmStructuralProperty[] { (IEdmStructuralProperty) property2 });
                        }
                    }
                }
            };
            MetadataProviderEdmEntityType schemaType = new MetadataProviderEdmEntityType(resourceTypeNamespace, resourceType.Name, (resourceType.BaseType != null) ? ((IEdmEntityType) this.EnsureSchemaType(resourceType.BaseType)) : null, resourceType.IsAbstract, resourceType.IsOpenType, propertyLoadAction);
            this.CacheSchemaType(schemaType);
            if (resourceType.IsMediaLinkEntry && ((resourceType.BaseType == null) || !resourceType.BaseType.IsMediaLinkEntry))
            {
                this.SetHasDefaultStream(schemaType, true);
            }
            if (resourceType.HasEntityPropertyMappings)
            {
                MetadataProviderUtils.ConvertEntityPropertyMappings(this, resourceType, schemaType);
            }
            MetadataProviderUtils.ConvertCustomAnnotations(this, resourceType.CustomAnnotations, schemaType);
            return schemaType;
        }

        private bool AddVisibleResourceTypeToTypesInNamespaceCache(ResourceType resourceType, ref bool hasVisibleMediaLinkEntry, ref bool hasVisibleNamedStreams)
        {
            string typeNamespace = this.GetTypeNamespace(resourceType);
            HashSet<ResourceType> resourceTypesForNamespace = this.GetResourceTypesForNamespace(typeNamespace);
            if (resourceType.IsMediaLinkEntry)
            {
                hasVisibleMediaLinkEntry = true;
            }
            if (resourceType.HasNamedStreams)
            {
                hasVisibleNamedStreams = true;
            }
            return resourceTypesForNamespace.Add(resourceType);
        }

        [Conditional("DEBUG")]
        internal void AssertCacheState(MetadataProviderState state)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertMaterializationState(MetadataProviderState state)
        {
        }

        private void CacheSchemaType(IEdmSchemaType schemaType)
        {
            string key = schemaType.FullName();
            this.schemaTypeCache.Add(key, schemaType);
            IEdmStructuredType item = schemaType as IEdmStructuredType;
            if ((item != null) && (item.BaseType != null))
            {
                List<IEdmStructuredType> list;
                if (!this.derivedTypeMappings.TryGetValue(item.BaseType, out list))
                {
                    list = new List<IEdmStructuredType>();
                    this.derivedTypeMappings[item.BaseType] = list;
                }
                list.Add(item);
            }
        }

        private static string ComputeSchemaTypeCacheKey(string namespaceName, ResourceType resourceType)
        {
            return (namespaceName + "." + resourceType.Name);
        }

        private IEdmProperty CreateProperty(EdmStructuredType declaringType, ResourceProperty resourceProperty)
        {
            IEdmProperty property;
            List<KeyValuePair<string, object>> annotations = (resourceProperty.CustomAnnotations == null) ? null : resourceProperty.CustomAnnotations.ToList<KeyValuePair<string, object>>();
            ODataNullValueBehaviorKind nullValueReadBehaviorKind = ODataNullValueBehaviorKind.Default;
            if (resourceProperty.IsOfKind(ResourcePropertyKind.Primitive) || resourceProperty.IsOfKind(ResourcePropertyKind.Stream))
            {
                IEdmPrimitiveTypeReference typeReference = MetadataProviderUtils.CreatePrimitiveTypeReference(resourceProperty.ResourceType, annotations);
                if (resourceProperty.IsOfKind(ResourcePropertyKind.Key))
                {
                    if (typeReference.IsNullable)
                    {
                        typeReference = (IEdmPrimitiveTypeReference) typeReference.Clone(false);
                    }
                    nullValueReadBehaviorKind = ODataNullValueBehaviorKind.IgnoreValue;
                }
                else if (MetadataProviderUtils.ShouldDisablePrimitivePropertyNullValidation(resourceProperty, typeReference))
                {
                    nullValueReadBehaviorKind = ODataNullValueBehaviorKind.DisableValidation;
                }
                string andRemoveDefaultValue = MetadataProviderUtils.GetAndRemoveDefaultValue(annotations);
                EdmConcurrencyMode concurrencyMode = resourceProperty.IsOfKind(ResourcePropertyKind.ETag) ? EdmConcurrencyMode.Fixed : EdmConcurrencyMode.None;
                property = declaringType.AddStructuralProperty(resourceProperty.Name, typeReference, andRemoveDefaultValue, concurrencyMode);
                string mimeType = resourceProperty.MimeType;
                if (!string.IsNullOrEmpty(mimeType))
                {
                    this.SetMimeType(property, mimeType);
                }
            }
            else if (resourceProperty.IsOfKind(ResourcePropertyKind.ComplexType))
            {
                IEdmTypeReference reference2 = this.EnsureTypeReference(resourceProperty.ResourceType, annotations);
                string defaultValue = MetadataProviderUtils.GetAndRemoveDefaultValue(annotations);
                property = declaringType.AddStructuralProperty(resourceProperty.Name, reference2, defaultValue, EdmConcurrencyMode.None);
                if (this.metadataProvider.IsV1Provider && !reference2.IsNullable)
                {
                    nullValueReadBehaviorKind = ODataNullValueBehaviorKind.DisableValidation;
                }
            }
            else if (resourceProperty.IsOfKind(ResourcePropertyKind.Collection))
            {
                string str4 = MetadataProviderUtils.GetAndRemoveDefaultValue(annotations);
                IEdmTypeReference reference3 = this.EnsureTypeReference(resourceProperty.ResourceType, annotations);
                property = declaringType.AddStructuralProperty(resourceProperty.Name, reference3, str4, EdmConcurrencyMode.None);
            }
            else
            {
                if (!resourceProperty.IsOfKind(ResourcePropertyKind.ResourceSetReference) && !resourceProperty.IsOfKind(ResourcePropertyKind.ResourceReference))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderEdmModel_UnsupportedResourcePropertyKind(resourceProperty.Kind.ToString()));
                }
                EdmEntityType type = (EdmEntityType) declaringType;
                IEdmTypeReference reference4 = resourceProperty.IsOfKind(ResourcePropertyKind.ResourceSetReference) ? this.EnsureEntityPrimitiveOrComplexCollectionTypeReference(resourceProperty.ResourceType, annotations) : this.EnsureTypeReference(resourceProperty.ResourceType, annotations);
                property = new MetadataProviderEdmNavigationProperty(type, resourceProperty.Name, reference4);
                type.AddProperty(property);
            }
            this.SetNullValueReaderBehavior(property, nullValueReadBehaviorKind);
            MetadataProviderUtils.ConvertCustomAnnotations(this, annotations, property);
            return property;
        }

        private IEdmCollectionType EnsureCollectionItemTypeIsEntityPrimitiveOrComplex(ResourceType itemResourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            IEdmCollectionType type;
            if (!this.entityPrimitiveOrComplexCollectionTypeCache.TryGetValue(itemResourceType, out type))
            {
                IEdmTypeReference reference;
                switch (itemResourceType.ResourceTypeKind)
                {
                    case ResourceTypeKind.EntityType:
                        reference = this.EnsureTypeReference(itemResourceType, customAnnotations);
                        break;

                    case ResourceTypeKind.ComplexType:
                        reference = this.EnsureTypeReference(itemResourceType, customAnnotations);
                        break;

                    case ResourceTypeKind.Primitive:
                        reference = MetadataProviderUtils.CreatePrimitiveTypeReference(itemResourceType, customAnnotations);
                        break;

                    default:
                        throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderEdmModel_UnsupportedCollectionItemType_EntityPrimitiveOrComplex(itemResourceType.ResourceTypeKind.ToString()));
                }
                type = new EdmCollectionType(reference);
                this.entityPrimitiveOrComplexCollectionTypeCache.Add(itemResourceType, type);
            }
            return type;
        }

        private IEdmCollectionType EnsureCollectionItemTypeIsPrimitiveOrComplex(ResourceType itemResourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            IEdmCollectionType type;
            if (!this.primitiveOrComplexCollectionTypeCache.TryGetValue(itemResourceType, out type))
            {
                IEdmTypeReference reference;
                switch (itemResourceType.ResourceTypeKind)
                {
                    case ResourceTypeKind.ComplexType:
                        reference = this.EnsureTypeReference(itemResourceType, customAnnotations);
                        reference = reference.IsNullable ? reference.Clone(false) : reference;
                        break;

                    case ResourceTypeKind.Primitive:
                    {
                        MetadataProviderUtils.GetAndRemoveNullableFacet(customAnnotations);
                        IEdmPrimitiveTypeReference typeReference = MetadataProviderUtils.CreatePrimitiveTypeReference(itemResourceType, customAnnotations);
                        reference = typeReference.IsNullable ? typeReference.Clone(false) : typeReference;
                        break;
                    }
                    default:
                        throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderEdmModel_UnsupportedCollectionItemType_PrimitiveOrComplex(itemResourceType.ResourceTypeKind.ToString()));
                }
                type = new EdmCollectionType(reference);
                this.primitiveOrComplexCollectionTypeCache.Add(itemResourceType, type);
            }
            return type;
        }

        internal MetadataProviderEdmEntityContainer EnsureDefaultEntityContainer()
        {
            MetadataProviderEdmEntityContainer container;
            string containerName = this.metadataProvider.ContainerName;
            if (!this.entityContainerCache.TryGetValue(containerName, out container))
            {
                container = new MetadataProviderEdmEntityContainer(this, containerName, this.GetContainerNamespace());
                this.SetIsDefaultEntityContainer(container, true);
                MetadataProviderUtils.ConvertCustomAnnotations(this, this.metadataProvider.GetEntityContainerAnnotations(containerName), container);
                this.entityContainerCache.Add(containerName, container);
                this.entityContainerCache.Add(container.FullName(), container);
            }
            return container;
        }

        private IEdmCollectionType EnsureEntityCollectionType(ResourceType resourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            ResourceType itemResourceType = resourceType;
            EntityCollectionResourceType type2 = resourceType as EntityCollectionResourceType;
            if (type2 != null)
            {
                itemResourceType = type2.ItemType;
            }
            return this.EnsureCollectionItemTypeIsEntityPrimitiveOrComplex(itemResourceType, customAnnotations);
        }

        private void EnsureEntityContainers()
        {
            if (!this.HasCacheState(MetadataProviderState.EntityContainers))
            {
                string containerName = this.metadataProvider.ContainerName;
                string containerNamespace = this.GetContainerNamespace();
                MetadataProviderEdmEntityContainer container = this.EnsureDefaultEntityContainer();
                IEnumerable<ResourceSetWrapper> resourceSets = this.metadataProvider.GetResourceSets();
                if (resourceSets != null)
                {
                    foreach (ResourceSetWrapper wrapper in resourceSets)
                    {
                        MetadataProviderEdmEntityContainer container2;
                        string key = wrapper.EntityContainerName ?? containerName;
                        if (!this.entityContainerCache.TryGetValue(key, out container2))
                        {
                            container2 = new MetadataProviderEdmEntityContainer(this, key, containerNamespace);
                            MetadataProviderUtils.ConvertCustomAnnotations(this, this.metadataProvider.GetEntityContainerAnnotations(key), container2);
                            this.entityContainerCache.Add(containerNamespace + '.' + key, container2);
                            this.entityContainerCache.Add(key, container2);
                        }
                        string entitySetName = MetadataProviderUtils.GetEntitySetName(wrapper.ResourceSet);
                        container2.AddEntitySet(entitySetName, wrapper);
                    }
                }
                IEnumerable<OperationWrapper> visibleOperations = this.metadataProvider.GetVisibleOperations(this.operationContext);
                if (visibleOperations != null)
                {
                    foreach (OperationWrapper wrapper2 in visibleOperations)
                    {
                        container.EnsureFunctionImport(wrapper2);
                    }
                }
                this.SetCacheState(MetadataProviderState.EntityContainers);
            }
        }

        internal IEdmTypeReference EnsureEntityPrimitiveOrComplexCollectionTypeReference(ResourceType itemResourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            return this.EnsureCollectionItemTypeIsEntityPrimitiveOrComplex(itemResourceType, customAnnotations).ToTypeReference(true);
        }

        private void EnsureFullMetadata()
        {
            if (!this.HasCacheState(MetadataProviderState.Full))
            {
                bool hasVisibleMediaLinkEntry = false;
                bool hasVisibleNamedStreams = false;
                IEnumerable<ResourceType> visibleTypes = this.metadataProvider.GetVisibleTypes(this.operationContext).ToList<ResourceType>();
                this.GroupResourceTypesByNamespace(visibleTypes, ref hasVisibleMediaLinkEntry, ref hasVisibleNamedStreams);
                if (this.streamProviderWrapper != null)
                {
                    if (hasVisibleNamedStreams)
                    {
                        this.streamProviderWrapper.LoadAndValidateStreamProvider2();
                    }
                    else if (hasVisibleMediaLinkEntry)
                    {
                        this.streamProviderWrapper.LoadAndValidateStreamProvider();
                    }
                }
                this.EnsureStructuredTypes(visibleTypes);
                this.EnsureEntityContainers();
                this.PairUpNavigationProperties();
                this.SetCacheState(MetadataProviderState.Full);
            }
        }

        private IEdmCollectionType EnsurePrimitiveOrComplexCollectionType(ResourceType resourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            CollectionResourceType type = (CollectionResourceType) resourceType;
            ResourceType itemType = type.ItemType;
            return this.EnsureCollectionItemTypeIsPrimitiveOrComplex(itemType, customAnnotations);
        }

        internal IEdmSchemaType EnsureSchemaType(ResourceType resourceType)
        {
            IEdmSchemaType type;
            ResourceTypeKind resourceTypeKind = resourceType.ResourceTypeKind;
            if (resourceTypeKind == ResourceTypeKind.Primitive)
            {
                return MetadataProviderUtils.CreatePrimitiveTypeReference(resourceType).PrimitiveDefinition();
            }
            string typeNamespace = this.GetTypeNamespace(resourceType);
            string key = ComputeSchemaTypeCacheKey(typeNamespace, resourceType);
            if (this.schemaTypeCache.TryGetValue(key, out type))
            {
                return type;
            }
            switch (resourceTypeKind)
            {
                case ResourceTypeKind.EntityType:
                    return this.AddEntityType(resourceType, typeNamespace);

                case ResourceTypeKind.ComplexType:
                    return this.AddComplexType(resourceType, typeNamespace);
            }
            throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderEdmModel_UnsupportedSchemaTypeKind(resourceTypeKind.ToString()));
        }

        private void EnsureStructuredTypes(IEnumerable<ResourceType> visibleTypes)
        {
            if (visibleTypes != null)
            {
                foreach (ResourceType type in visibleTypes)
                {
                    this.EnsureSchemaType(type);
                }
            }
        }

        internal IEdmTypeReference EnsureTypeReference(ResourceType resourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            switch (resourceType.ResourceTypeKind)
            {
                case ResourceTypeKind.EntityType:
                {
                    bool? andRemoveNullableFacet = MetadataProviderUtils.GetAndRemoveNullableFacet(customAnnotations);
                    bool nullable = andRemoveNullableFacet.HasValue ? andRemoveNullableFacet.Value : true;
                    return this.EnsureSchemaType(resourceType).ToTypeReference(nullable);
                }
                case ResourceTypeKind.ComplexType:
                {
                    bool? nullable2 = MetadataProviderUtils.GetAndRemoveNullableFacet(customAnnotations);
                    bool flag2 = this.GetDataServiceVersion() >= RequestDescription.Version3Dot0;
                    Version edmVersion = this.GetEdmVersion();
                    if ((edmVersion != null) && (edmVersion < MetadataEdmSchemaVersion.Version3Dot0.ToVersion()))
                    {
                        flag2 = false;
                    }
                    bool flag3 = nullable2.HasValue ? nullable2.Value : flag2;
                    return this.EnsureSchemaType(resourceType).ToTypeReference(flag3);
                }
                case ResourceTypeKind.Primitive:
                    return MetadataProviderUtils.CreatePrimitiveTypeReference(resourceType, customAnnotations);

                case ResourceTypeKind.Collection:
                    return this.EnsurePrimitiveOrComplexCollectionType(resourceType, customAnnotations).ToTypeReference(true);

                case ResourceTypeKind.EntityCollection:
                    return this.EnsureEntityCollectionType(resourceType, customAnnotations).ToTypeReference(true);
            }
            throw new InvalidOperationException(System.Data.Services.Strings.MetadataProviderEdmModel_UnsupportedResourceTypeKind(resourceType.ResourceTypeKind.ToString()));
        }

        public IEdmEntityContainer FindDeclaredEntityContainer(string name)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            this.RunInState(new Action(this.EnsureEntityContainers), MetadataProviderState.EntityContainers);
            return this.FindExistingEntityContainer(name);
        }

        public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
        {
            return Enumerable.Empty<IEdmFunction>();
        }

        public IEdmSchemaType FindDeclaredType(string qualifiedName)
        {
            IEdmSchemaType type;
            WebUtil.CheckStringArgumentNullOrEmpty(qualifiedName, "qualifiedName");
            if (this.schemaTypeCache.TryGetValue(qualifiedName, out type))
            {
                return type;
            }
            if (this.coreModel.FindDeclaredType(qualifiedName) == null)
            {
                if (this.cacheState == MetadataProviderState.Full)
                {
                    return null;
                }
                ResourceType resourceType = this.metadataProvider.TryResolveResourceType(qualifiedName);
                if (resourceType != null)
                {
                    return this.EnsureSchemaType(resourceType);
                }
            }
            return null;
        }

        public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
        {
            return null;
        }

        public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
        {
            return Enumerable.Empty<IEdmVocabularyAnnotation>();
        }

        public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
        {
            List<IEdmStructuredType> list;
            this.RunInState(new Action(this.EnsureFullMetadata), MetadataProviderState.Full);
            if (this.derivedTypeMappings.TryGetValue(baseType, out list))
            {
                return list;
            }
            return Enumerable.Empty<IEdmStructuredType>();
        }

        internal IEdmEntitySet FindEntitySet(ResourceSet resourceSet)
        {
            string name = resourceSet.EntityContainerName ?? this.metadataProvider.ContainerName;
            return this.FindExistingEntityContainer(name).FindEntitySet(MetadataProviderUtils.GetEntitySetName(resourceSet));
        }

        private MetadataProviderEdmEntityContainer FindExistingEntityContainer(string name)
        {
            MetadataProviderEdmEntityContainer container;
            if (this.entityContainerCache.TryGetValue(name, out container))
            {
                return container;
            }
            return null;
        }

        private IEnumerable<ResourceProperty> GetAllVisiblePropertiesDeclaredInThisType(ResourceType resourceType)
        {
            foreach (ResourceProperty iteratorVariable0 in resourceType.PropertiesDeclaredOnThisType)
            {
                if ((iteratorVariable0.TypeKind == ResourceTypeKind.EntityType) && (this.HasCacheState(MetadataProviderState.Full) || this.HasMaterializationState(MetadataProviderState.Full)))
                {
                    ResourceType resourceType2 = iteratorVariable0.ResourceType;
                    string schemaElementNamespace = this.GetTypeNamespace(resourceType2);
                    if (!this.GetResourceTypesForNamespace(schemaElementNamespace).Contains(resourceType2))
                    {
                        continue;
                    }
                }
                yield return iteratorVariable0;
            }
        }

        private string GetContainerNamespace()
        {
            string containerNamespace = this.metadataProvider.ContainerNamespace;
            if (string.IsNullOrEmpty(containerNamespace))
            {
                containerNamespace = this.metadataProvider.ContainerName;
            }
            return containerNamespace;
        }

        private HashSet<ResourceType> GetResourceTypesForNamespace(string schemaElementNamespace)
        {
            HashSet<ResourceType> set;
            if (!this.resourceTypesPerNamespaceCache.TryGetValue(schemaElementNamespace, out set))
            {
                set = new HashSet<ResourceType>(EqualityComparer<ResourceType>.Default);
                this.resourceTypesPerNamespaceCache.Add(schemaElementNamespace, set);
            }
            return set;
        }

        private string GetTypeNamespace(ResourceType resourceType)
        {
            string containerNamespace = resourceType.Namespace;
            if (string.IsNullOrEmpty(containerNamespace))
            {
                containerNamespace = this.GetContainerNamespace();
            }
            return containerNamespace;
        }

        private void GroupResourceTypesByNamespace(IEnumerable<ResourceType> visibleTypes, ref bool hasVisibleMediaLinkEntry, ref bool hasVisibleNamedStreams)
        {
            foreach (ResourceType type in visibleTypes)
            {
                this.AddVisibleResourceTypeToTypesInNamespaceCache(type, ref hasVisibleMediaLinkEntry, ref hasVisibleNamedStreams);
            }
        }

        private bool HasCacheState(MetadataProviderState state)
        {
            return (this.cacheState >= state);
        }

        private bool HasMaterializationState(MetadataProviderState state)
        {
            return (this.materializationState >= state);
        }

        private void PairUpNavigationProperties()
        {
            string containerName = this.metadataProvider.ContainerName;
            IEnumerable<ResourceSetWrapper> resourceSets = this.metadataProvider.GetResourceSets();
            if (resourceSets != null)
            {
                foreach (ResourceSetWrapper wrapper in resourceSets)
                {
                    string str2 = wrapper.EntityContainerName ?? containerName;
                    MetadataProviderEdmEntityContainer entityContainer = this.entityContainerCache[str2];
                    this.PairUpNavigationPropertiesForEntitySet(entityContainer, wrapper);
                }
            }
        }

        private void PairUpNavigationPropertiesForEntitySet(MetadataProviderEdmEntityContainer entityContainer, ResourceSetWrapper resourceSet)
        {
            foreach (ResourceType type in this.MetadataProvider.GetDerivedTypes(resourceSet.ResourceType))
            {
                this.PairUpNavigationPropertiesForEntitySetAndType(entityContainer, resourceSet, type);
            }
            for (ResourceType type2 = resourceSet.ResourceType; type2 != null; type2 = type2.BaseType)
            {
                this.PairUpNavigationPropertiesForEntitySetAndType(entityContainer, resourceSet, type2);
            }
        }

        private void PairUpNavigationPropertiesForEntitySetAndType(MetadataProviderEdmEntityContainer entityContainer, ResourceSetWrapper resourceSet, ResourceType resourceType)
        {
            IEnumerable<ResourceProperty> allVisiblePropertiesDeclaredInThisType = this.GetAllVisiblePropertiesDeclaredInThisType(resourceType);
            if (allVisiblePropertiesDeclaredInThisType != null)
            {
                foreach (ResourceProperty property in from p in allVisiblePropertiesDeclaredInThisType
                    where p.TypeKind == ResourceTypeKind.EntityType
                    select p)
                {
                    this.PairUpNavigationProperty(entityContainer, resourceSet, resourceType, property);
                }
            }
        }

        private void PairUpNavigationProperty(MetadataProviderEdmEntityContainer entityContainer, ResourceSetWrapper resourceSet, ResourceType resourceType, ResourceProperty navigationProperty)
        {
            string key = string.Concat(new object[] { resourceSet.Name, '_', resourceType.FullName, '_', navigationProperty.Name });
            if (!this.associationSetByKeyCache.ContainsKey(key))
            {
                ResourceAssociationSet resourceAssociationSet = this.MetadataProvider.GetResourceAssociationSet(resourceSet, resourceType, navigationProperty);
                if (resourceAssociationSet != null)
                {
                    string str2;
                    string str3;
                    ResourceAssociationSetEnd end = resourceAssociationSet.GetRelatedResourceAssociationSetEnd(resourceSet, resourceType, navigationProperty);
                    if (end.ResourceProperty != null)
                    {
                        ResourceAssociationSet set2 = this.MetadataProvider.GetResourceAssociationSet(this.MetadataProvider.ValidateResourceSet(end.ResourceSet), end.ResourceType, end.ResourceProperty);
                        if ((set2 == null) || (resourceAssociationSet.Name != set2.Name))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ResourceAssociationSet_BidirectionalAssociationMustReturnSameResourceAssociationSetFromBothEnd);
                        }
                    }
                    if (end.ResourceProperty != null)
                    {
                        str2 = string.Concat(new object[] { end.ResourceSet.Name, '_', end.ResourceType.FullName, '_', end.ResourceProperty.Name });
                    }
                    else
                    {
                        str2 = string.Concat(new object[] { end.ResourceSet.Name, "_Null_", resourceType.FullName, '_', navigationProperty.Name });
                    }
                    if (this.associationSetByKeyCache.TryGetValue(str2, out str3))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceAssociationSet_MultipleAssociationSetsForTheSameAssociationTypeMustNotReferToSameEndSets(str3, resourceAssociationSet.Name, end.ResourceSet.Name));
                    }
                    ResourceAssociationType resourceAssociationType = resourceAssociationSet.ResourceAssociationType;
                    this.PairUpNavigationPropertyWithResourceAssociationSet(entityContainer, resourceAssociationSet, resourceAssociationType, resourceType, navigationProperty);
                    this.associationSetByKeyCache.Add(str2, resourceAssociationSet.Name);
                    this.associationSetByKeyCache.Add(key, resourceAssociationSet.Name);
                }
            }
        }

        private void PairUpNavigationPropertyWithResourceAssociationSet(MetadataProviderEdmEntityContainer entityContainer, ResourceAssociationSet resourceAssociationSet, ResourceAssociationType resourceAssociationType, ResourceType resourceType, ResourceProperty navigationProperty)
        {
            string associationEndName;
            EdmMultiplicity multiplicity;
            EdmOnDeleteAction deleteBehavior;
            string str3;
            EdmMultiplicity multiplicity2;
            EdmOnDeleteAction none;
            string typeNamespace;
            bool flag = (resourceAssociationSet.End1.ResourceProperty != null) && (resourceAssociationSet.End2.ResourceProperty != null);
            string entitySetName = MetadataProviderUtils.GetEntitySetName(resourceAssociationSet.End1.ResourceSet);
            string name = MetadataProviderUtils.GetEntitySetName(resourceAssociationSet.End2.ResourceSet);
            bool isPrinciple = false;
            List<IEdmStructuralProperty> dependentProperties = null;
            if (resourceAssociationType != null)
            {
                associationEndName = resourceAssociationType.End1.Name;
                deleteBehavior = resourceAssociationType.End1.DeleteBehavior;
                multiplicity2 = MetadataProviderUtils.ConvertMultiplicity(resourceAssociationType.End1.Multiplicity);
                str3 = resourceAssociationType.End2.Name;
                none = resourceAssociationType.End2.DeleteBehavior;
                multiplicity = MetadataProviderUtils.ConvertMultiplicity(resourceAssociationType.End2.Multiplicity);
                ResourceReferentialConstraint referentialConstraint = resourceAssociationType.ReferentialConstraint;
                if (referentialConstraint != null)
                {
                    isPrinciple = object.ReferenceEquals(resourceAssociationType.End1, referentialConstraint.PrincipalEnd);
                    IEdmEntityType type = isPrinciple ? ((IEdmEntityType) this.EnsureSchemaType(resourceAssociationSet.End2.ResourceType)) : ((IEdmEntityType) this.EnsureSchemaType(resourceAssociationSet.End1.ResourceType));
                    dependentProperties = new List<IEdmStructuralProperty>();
                    foreach (ResourceProperty property in referentialConstraint.DependentProperties)
                    {
                        IEdmProperty property2 = type.FindProperty(property.Name);
                        dependentProperties.Add((IEdmStructuralProperty) property2);
                    }
                }
            }
            else
            {
                if (!flag)
                {
                    if (resourceAssociationSet.End1.ResourceProperty != null)
                    {
                        associationEndName = resourceType.Name;
                        str3 = navigationProperty.Name;
                    }
                    else
                    {
                        associationEndName = navigationProperty.Name;
                        str3 = resourceType.Name;
                    }
                }
                else
                {
                    associationEndName = MetadataProviderUtils.GetAssociationEndName(resourceAssociationSet.End1.ResourceType, resourceAssociationSet.End1.ResourceProperty);
                    str3 = MetadataProviderUtils.GetAssociationEndName(resourceAssociationSet.End2.ResourceType, resourceAssociationSet.End2.ResourceProperty);
                }
                multiplicity = MetadataProviderUtils.GetMultiplicity(resourceAssociationSet.End1.ResourceProperty);
                deleteBehavior = EdmOnDeleteAction.None;
                multiplicity2 = MetadataProviderUtils.GetMultiplicity(resourceAssociationSet.End2.ResourceProperty);
                none = EdmOnDeleteAction.None;
            }
            string associationName = (resourceAssociationType == null) ? MetadataProviderUtils.GetAssociationName(resourceAssociationSet) : resourceAssociationType.Name;
            if ((resourceAssociationType == null) || (resourceAssociationType.NamespaceName == null))
            {
                ResourceAssociationSetEnd end = (resourceAssociationSet.End1.ResourceProperty != null) ? resourceAssociationSet.End1 : resourceAssociationSet.End2;
                typeNamespace = this.GetTypeNamespace(end.ResourceType);
            }
            else
            {
                typeNamespace = resourceAssociationType.NamespaceName;
            }
            ResourceProperty resourceProperty = resourceAssociationSet.End1.ResourceProperty;
            ResourceProperty property4 = resourceAssociationSet.End2.ResourceProperty;
            MetadataProviderEdmNavigationProperty partnerProperty = null;
            MetadataProviderEdmNavigationProperty property6 = null;
            if (resourceProperty != null)
            {
                IEdmEntityType type2 = (IEdmEntityType) this.EnsureSchemaType(resourceAssociationSet.End1.ResourceType);
                partnerProperty = (MetadataProviderEdmNavigationProperty) type2.FindProperty(resourceProperty.Name);
            }
            if (property4 != null)
            {
                IEdmEntityType type3 = (IEdmEntityType) this.EnsureSchemaType(resourceAssociationSet.End2.ResourceType);
                property6 = (MetadataProviderEdmNavigationProperty) type3.FindProperty(property4.Name);
            }
            IEdmNavigationProperty property7 = (partnerProperty != null) ? ((IEdmNavigationProperty) partnerProperty) : ((IEdmNavigationProperty) new MetadataProviderEdmSilentNavigationProperty(property6, deleteBehavior, multiplicity, associationEndName));
            IEdmNavigationProperty partner = (property6 != null) ? ((IEdmNavigationProperty) property6) : ((IEdmNavigationProperty) new MetadataProviderEdmSilentNavigationProperty(partnerProperty, none, multiplicity2, str3));
            MetadataProviderUtils.FixUpNavigationPropertyWithAssociationSetData(property7, partner, isPrinciple, dependentProperties, deleteBehavior, multiplicity);
            MetadataProviderUtils.FixUpNavigationPropertyWithAssociationSetData(partner, property7, !isPrinciple, dependentProperties, none, multiplicity2);
            EdmEntitySet entitySet = (EdmEntitySet) entityContainer.FindEntitySet(entitySetName);
            EdmEntitySet target = (EdmEntitySet) entityContainer.FindEntitySet(name);
            if (partnerProperty != null)
            {
                entitySet.AddNavigationTarget(partnerProperty, target);
                this.SetAssociationSetName(entitySet, partnerProperty, resourceAssociationSet.Name);
                this.SetAssociationSetAnnotations(entitySet, partnerProperty, MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.End1.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.End2.CustomAnnotations));
            }
            if (property6 != null)
            {
                target.AddNavigationTarget(property6, entitySet);
                this.SetAssociationSetName(target, property6, resourceAssociationSet.Name);
                if (partnerProperty == null)
                {
                    this.SetAssociationSetAnnotations(entitySet, property6, MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.End1.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationSet.End2.CustomAnnotations));
                }
            }
            this.SetAssociationNamespace(property7, typeNamespace);
            this.SetAssociationName(property7, associationName);
            this.SetAssociationEndName(property7, associationEndName);
            this.SetAssociationNamespace(partner, typeNamespace);
            this.SetAssociationName(partner, associationName);
            this.SetAssociationEndName(partner, str3);
            if (resourceAssociationType != null)
            {
                this.SetAssociationAnnotations(property7, MetadataProviderUtils.ConvertCustomAnnotations(this, resourceAssociationType.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, (property7.GetPrimary() == property7) ? resourceAssociationType.End1.CustomAnnotations : resourceAssociationType.End2.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, (property7.GetPrimary() == property7) ? resourceAssociationType.End2.CustomAnnotations : resourceAssociationType.End1.CustomAnnotations), MetadataProviderUtils.ConvertCustomAnnotations(this, (resourceAssociationType.ReferentialConstraint != null) ? resourceAssociationType.ReferentialConstraint.CustomAnnotations : null));
            }
        }

        private void RunInState(Action action, MetadataProviderState state)
        {
            this.SetMaterializationState(state);
            action();
            this.SetMaterializationState(MetadataProviderState.Incremental);
        }

        private void SetCacheState(MetadataProviderState newState)
        {
            if (this.cacheState < newState)
            {
                this.cacheState = newState;
            }
        }

        private void SetMaterializationState(MetadataProviderState newState)
        {
            if (newState == MetadataProviderState.Incremental)
            {
                this.materializationState = MetadataProviderState.Incremental;
            }
            else if (this.materializationState < newState)
            {
                this.materializationState = newState;
            }
        }

        public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
        {
            get
            {
                return this.directValueAnnotationsManager;
            }
        }

        internal DataServiceProviderWrapper MetadataProvider
        {
            get
            {
                return this.metadataProvider;
            }
        }

        public IEnumerable<IEdmModel> ReferencedModels
        {
            get
            {
                return this.referencedModels;
            }
        }

        public IEnumerable<IEdmSchemaElement> SchemaElements
        {
            get
            {
                this.RunInState(new Action(this.EnsureFullMetadata), MetadataProviderState.Full);
                foreach (IEdmSchemaType iteratorVariable0 in this.schemaTypeCache.Values)
                {
                    yield return iteratorVariable0;
                }
                foreach (IEdmEntityContainer iteratorVariable1 in this.entityContainerCache.Values.Distinct<MetadataProviderEdmEntityContainer>())
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
        {
            get
            {
                return Enumerable.Empty<IEdmVocabularyAnnotation>();
            }
        }

        
    }
}

