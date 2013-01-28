using Microsoft.Data.Edm.Annotations;

namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Caching;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class DataServiceProviderWrapper
    {
        private string containerNameCache;
        private string containerNamePrefix;
        private string containerNamespaceCache;
        private IDataService dataService;
        private MetadataEdmSchemaVersion edmSchemaVersion;
        private string fullyQualifiedContainerNamePrefix;
        private bool IsResourceSetsCacheInitialized;
        private readonly DataServiceCacheItem metadata;
        private IDataServiceMetadataProvider metadataProvider;
        private readonly Dictionary<DataServiceOperationContext, MetadataProviderEdmModel> metadataProviderEdmModels;
        private readonly Dictionary<DataServiceOperationContext, IEdmModel> models;
        private readonly Dictionary<string, OperationWrapper> operationWrapperCache;
        private IDataServiceQueryProvider queryProvider;
        private Version responseMetadataVersion;

        internal DataServiceProviderWrapper(DataServiceCacheItem cacheItem, IDataServiceMetadataProvider metadataProvider, IDataServiceQueryProvider queryProvider, IDataService dataService)
        {
            this.metadata = cacheItem;
            this.metadataProvider = metadataProvider;
            this.queryProvider = queryProvider;
            this.dataService = dataService;
            this.operationWrapperCache = new Dictionary<string, OperationWrapper>(EqualityComparer<string>.Default);
            this.metadataProviderEdmModels = new Dictionary<DataServiceOperationContext, MetadataProviderEdmModel>(EqualityComparer<DataServiceOperationContext>.Default);
            this.models = new Dictionary<DataServiceOperationContext, IEdmModel>(EqualityComparer<DataServiceOperationContext>.Default);
            this.edmSchemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
            this.containerNameCache = null;
            this.containerNamespaceCache = null;
        }

        private static void AddUniqueNameToSet(string name, HashSet<string> names, string exceptionString)
        {
            if (name != null)
            {
                if (names.Contains(name))
                {
                    throw new DataServiceException(500, exceptionString);
                }
                names.Add(name);
            }
        }

        [Conditional("DEBUG")]
        private static void AssertCacheNotPreloaded(DataServiceProviderWrapper wrapper)
        {
        }

        internal void CheckIfOrderedType(Type clrType)
        {
            BaseServiceProvider metadataProvider = this.metadataProvider as BaseServiceProvider;
            if ((metadataProvider != null) && !metadataProvider.GetTypeIsOrdered(clrType))
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestQueryParser_OrderByDoesNotSupportType(WebUtil.GetTypeName(clrType)));
            }
        }

        internal void DisposeDataSource()
        {
            WebUtil.Dispose(this.metadataProvider);
            if (this.metadataProvider != this.queryProvider)
            {
                WebUtil.Dispose(this.queryProvider);
            }
            this.metadataProvider = null;
            this.queryProvider = null;
            this.dataService = null;
        }

        internal ResourceSetWrapper GetContainer(ResourceSetWrapper sourceContainer, ResourceType sourceResourceType, ResourceProperty navigationProperty)
        {
            ResourceAssociationSet set = this.GetResourceAssociationSet(sourceContainer, sourceResourceType, navigationProperty);
            if (set != null)
            {
                ResourceAssociationSetEnd end = set.GetRelatedResourceAssociationSetEnd(sourceContainer, sourceResourceType, navigationProperty);
                return this.ValidateResourceSet(end.ResourceSet);
            }
            return null;
        }

        internal static ResourceType GetDeclaringTypeForProperty(ResourceType resourceType, ResourceProperty resourceProperty, ResourceType rootType = null)
        {
            while (resourceType != rootType)
            {
                if (resourceType.TryResolvePropertiesDeclaredOnThisTypeByName(resourceProperty.Name) == resourceProperty)
                {
                    return resourceType;
                }
                resourceType = resourceType.BaseType;
            }
            return resourceType;
        }

        public IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType)
        {
            IEnumerable<ResourceType> derivedTypes = this.metadataProvider.GetDerivedTypes(resourceType);
            if (derivedTypes != null)
            {
                foreach (ResourceType iteratorVariable1 in derivedTypes)
                {
                    ResourceType iteratorVariable2 = this.ValidateResourceType(iteratorVariable1);
                    if (iteratorVariable2 != null)
                    {
                        yield return iteratorVariable2;
                    }
                }
            }
        }

        internal IEnumerable<KeyValuePair<string, object>> GetEntityContainerAnnotations(string entityContainerName)
        {
            ObjectContextServiceProvider metadataProvider = this.metadataProvider as ObjectContextServiceProvider;
            if (metadataProvider != null)
            {
                return metadataProvider.GetEntityContainerAnnotations(entityContainerName);
            }
            return WebUtil.EmptyKeyValuePairStringObject;
        }

        internal IList<ResourceProperty> GetETagProperties(string containerName, ResourceType resourceType)
        {
            ObjectContextServiceProvider metadataProvider = this.metadataProvider as ObjectContextServiceProvider;
            if (metadataProvider != null)
            {
                return metadataProvider.GetETagProperties(containerName, resourceType);
            }
            return resourceType.ETagProperties;
        }

        internal MetadataEdmSchemaVersion GetMetadataEdmSchemaVersion(DataServiceOperationContext operationContext)
        {
            this.GetMetadataVersion(operationContext);
            ObjectContextServiceProvider metadataProvider = this.metadataProvider as ObjectContextServiceProvider;
            if (metadataProvider != null)
            {
                this.edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(this.edmSchemaVersion, metadataProvider.EdmSchemaVersion);
            }
            return this.edmSchemaVersion;
        }

        internal IEdmModel GetMetadataModel(DataServiceOperationContext operationContext)
        {
            IEdmModel metadataProviderEdmModel;
            if (!this.models.TryGetValue(operationContext, out metadataProviderEdmModel))
            {
                metadataProviderEdmModel = this.GetMetadataProviderEdmModel(operationContext);
                Func<IEdmModel, IEnumerable<IEdmModel>> annotationsBuilder = this.Configuration.AnnotationsBuilder;
                if (annotationsBuilder != null)
                {
                    IEnumerable<IEdmModel> source = annotationsBuilder(metadataProviderEdmModel);
                    if ((source != null) && (source.Count<IEdmModel>() > 0))
                    {
                        if (source.Any<IEdmModel>(a => a == null))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_AnnotationsBuilderCannotReturnNullModels);
                        }
                        metadataProviderEdmModel = new AnnotatedModel(metadataProviderEdmModel, source);
                    }
                }
                this.models[operationContext] = metadataProviderEdmModel;
            }
            return metadataProviderEdmModel;
        }

        internal MetadataProviderEdmModel GetMetadataProviderEdmModel(DataServiceOperationContext operationContext)
        {
            MetadataProviderEdmModel model;
            if (!this.metadataProviderEdmModels.TryGetValue(operationContext, out model))
            {
                model = new MetadataProviderEdmModel(this, operationContext, this.dataService.StreamProvider);
                this.metadataProviderEdmModels[operationContext] = model;
            }
            return model;
        }

        internal Version GetMetadataVersion(DataServiceOperationContext operationContext)
        {
            if (this.responseMetadataVersion == null)
            {
                RequestDescription.UpdateMetadataVersion(this, operationContext, out this.responseMetadataVersion, out this.edmSchemaVersion);
            }
            this.responseMetadataVersion = WebUtil.RaiseVersion(this.responseMetadataVersion, operationContext.Host.RequestMinVersion);
            if (this.responseMetadataVersion == RequestDescription.Version3Dot0)
            {
                this.edmSchemaVersion = WebUtil.RaiseMetadataEdmSchemaVersion(this.edmSchemaVersion, MetadataEdmSchemaVersion.Version3Dot0);
            }
            return this.responseMetadataVersion;
        }

        internal string GetNameFromContainerQualifiedName(string containerQualifiedName, out bool nameIsContainerQualified)
        {
            nameIsContainerQualified = false;
            string str = containerQualifiedName;
            this.containerNamePrefix = this.containerNamePrefix ?? (this.ContainerName + ".");
            this.fullyQualifiedContainerNamePrefix = this.fullyQualifiedContainerNamePrefix ?? (string.IsNullOrEmpty(this.ContainerNamespace) ? this.containerNamePrefix : (this.ContainerNamespace + "." + this.containerNamePrefix));
            if (str.StartsWith(this.fullyQualifiedContainerNamePrefix, StringComparison.Ordinal))
            {
                str = str.Substring(this.fullyQualifiedContainerNamePrefix.Length);
                nameIsContainerQualified = true;
            }
            else if (str.StartsWith(this.containerNamePrefix, StringComparison.Ordinal))
            {
                str = str.Substring(this.containerNamePrefix.Length);
                nameIsContainerQualified = true;
            }
            if (string.IsNullOrEmpty(str))
            {
                nameIsContainerQualified = false;
                return containerQualifiedName;
            }
            return str;
        }

        public object GetOpenPropertyValue(object target, string propertyName)
        {
            return this.queryProvider.GetOpenPropertyValue(target, propertyName);
        }

        public IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
        {
            IEnumerable<KeyValuePair<string, object>> openPropertyValues = this.queryProvider.GetOpenPropertyValues(target);
            if (openPropertyValues == null)
            {
                return WebUtil.EmptyKeyValuePairStringObject;
            }
            return openPropertyValues;
        }

        public object GetPropertyValue(object target, ResourceProperty resourceProperty, ResourceType resourceType)
        {
            if (!resourceProperty.CanReflectOnInstanceTypeProperty)
            {
                return this.queryProvider.GetPropertyValue(target, resourceProperty);
            }
            if (resourceType == null)
            {
                resourceType = this.GetResourceType(target);
            }
            return resourceType.GetPropertyValue(resourceProperty, target);
        }

        public ConstantExpression GetQueryRootForResourceSet(ResourceSetWrapper resourceSet, DataServiceOperationContext operationContext)
        {
            IQueryable queryRootForResourceSet = this.queryProvider.GetQueryRootForResourceSet(resourceSet.ResourceSet);
            WebUtil.CheckResourceExists(queryRootForResourceSet != null, resourceSet.Name);
            if (!resourceSet.QueryRootType.IsAssignableFrom(queryRootForResourceSet.GetType()))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_InvalidQueryRootType(resourceSet.Name, resourceSet.QueryRootType.FullName));
            }
            return Expression.Constant(queryRootForResourceSet);
        }

        private IEnumerable<ResourceType> GetReachableComplexTypesFromOperation(OperationWrapper operation, HashSet<ResourceType> visitedTypes)
        {
            List<ResourceType> iteratorVariable0 = new List<ResourceType>();
            if ((operation.ResultType != null) && (operation.ResultType.ResourceTypeKind == ResourceTypeKind.ComplexType))
            {
                iteratorVariable0.Add(operation.ResultType);
            }
            iteratorVariable0.AddRange(from p in operation.Parameters
                where p.ParameterType.ResourceTypeKind == ResourceTypeKind.ComplexType
                select p.ParameterType);
            foreach (ResourceType iteratorVariable1 in iteratorVariable0)
            {
                foreach (ResourceType iteratorVariable2 in this.GetResourceTypeAndReachableComplexTypes(iteratorVariable1, visitedTypes))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        private IEnumerable<ResourceType> GetReachableTypesFromSet(ResourceSetWrapper resourceSet, HashSet<ResourceType> visitedTypes)
        {
            if (this.HasDerivedTypes(resourceSet.ResourceType))
            {
                foreach (ResourceType iteratorVariable0 in this.GetDerivedTypes(resourceSet.ResourceType))
                {
                    foreach (ResourceType iteratorVariable1 in this.GetResourceTypeAndReachableComplexTypes(iteratorVariable0, visitedTypes))
                    {
                        yield return iteratorVariable1;
                    }
                }
            }
            for (ResourceType iteratorVariable2 = resourceSet.ResourceType; iteratorVariable2 != null; iteratorVariable2 = iteratorVariable2.BaseType)
            {
                foreach (ResourceType iteratorVariable3 in this.GetResourceTypeAndReachableComplexTypes(iteratorVariable2, visitedTypes))
                {
                    yield return iteratorVariable3;
                }
            }
        }

        public ResourceAssociationSet GetResourceAssociationSet(ResourceSetWrapper resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceAssociationSet set;
            resourceType = GetDeclaringTypeForProperty(resourceType, resourceProperty, null);
            string key = string.Concat(new object[] { resourceSet.Name, '_', resourceType.FullName, '_', resourceProperty.Name });
            if (!this.ResourceAssociationSetCache.TryGetValue(key, out set))
            {
                set = this.metadataProvider.GetResourceAssociationSet(resourceSet.ResourceSet, resourceType, resourceProperty);
                if (set != null)
                {
                    ResourceAssociationSetEnd end = set.GetResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty);
                    ResourceAssociationSetEnd end2 = set.GetRelatedResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty);
                    ResourceSetWrapper wrapper = this.ValidateResourceSet(end2.ResourceSet);
                    if (wrapper == null)
                    {
                        set = null;
                    }
                    else
                    {
                        ResourceType type = this.ValidateResourceType(end2.ResourceType);
                        ResourceProperty property = null;
                        if (end2.ResourceProperty != null)
                        {
                            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                            property = type.TryResolvePropertyName(end2.ResourceProperty.Name, stream);
                        }
                        resourceType = this.ValidateResourceType(end.ResourceType);
                        if ((((end.ResourceSet != resourceSet.ResourceSet) || (end.ResourceType != resourceType)) || ((end.ResourceProperty != resourceProperty) || (end2.ResourceSet != wrapper.ResourceSet))) || ((end2.ResourceType != type) || (end2.ResourceProperty != property)))
                        {
                            set = new ResourceAssociationSet(set.Name, new ResourceAssociationSetEnd(resourceSet.ResourceSet, resourceType, resourceProperty), new ResourceAssociationSetEnd(wrapper.ResourceSet, type, property));
                        }
                    }
                }
                this.ResourceAssociationSetCache.Add(key, set);
            }
            return set;
        }

        internal IEnumerable<ResourceProperty> GetResourceSerializableProperties(ResourceSetWrapper resourceSet, ResourceType resourceType)
        {
            if (resourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
            {
                return resourceSet.GetEntitySerializableProperties(this, resourceType);
            }
            return resourceType.Properties;
        }

        public IEnumerable<ResourceSetWrapper> GetResourceSets()
        {
            if (!this.IsResourceSetsCacheInitialized)
            {
                IEnumerable<ResourceSet> resourceSets = this.metadataProvider.ResourceSets;
                if (resourceSets != null)
                {
                    HashSet<string> names = new HashSet<string>(EqualityComparer<string>.Default);
                    foreach (ResourceSet set2 in resourceSets)
                    {
                        AddUniqueNameToSet((set2 != null) ? set2.Name : null, names, System.Data.Services.Strings.DataServiceProviderWrapper_MultipleEntitySetsWithSameName(set2.Name));
                        this.ValidateResourceSet(set2);
                    }
                    this.IsResourceSetsCacheInitialized = true;
                }
            }
            return (from resourceSetWrapper in this.ResourceSetWrapperCache.Values
                where resourceSetWrapper != null
                select resourceSetWrapper);
        }

        public ResourceType GetResourceType(object instance)
        {
            return this.ValidateResourceType(this.queryProvider.GetResourceType(instance));
        }

        private IEnumerable<ResourceType> GetResourceTypeAndReachableComplexTypes(ResourceType resourceType, HashSet<ResourceType> visitedTypes)
        {
            resourceType = this.ValidateResourceType(resourceType);
            if (!visitedTypes.Contains(resourceType))
            {
                visitedTypes.Add(resourceType);
                yield return resourceType;
                foreach (ResourceProperty iteratorVariable0 in resourceType.PropertiesDeclaredOnThisType)
                {
                    ResourceType itemType = iteratorVariable0.ResourceType;
                    if (itemType.ResourceTypeKind == ResourceTypeKind.Collection)
                    {
                        CollectionResourceType collectionResourceType = (CollectionResourceType) itemType;
                        this.ValidateCollectionResourceType(collectionResourceType);
                        itemType = collectionResourceType.ItemType;
                    }
                    if (itemType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                    {
                        foreach (ResourceType iteratorVariable2 in this.GetResourceTypeAndReachableComplexTypes(itemType, visitedTypes))
                        {
                            yield return iteratorVariable2;
                        }
                    }
                }
            }
        }

        internal T GetService<T>() where T: class
        {
            if ((this.metadataProvider is ObjectContextServiceProvider) && IsUnsupportedProviderForEntityFramework(typeof(T)))
            {
                return WebUtil.GetService<T>(this.metadataProvider);
            }
            T service = WebUtil.GetService<T>(this.dataService.Instance);
            if (service != null)
            {
                return service;
            }
            service = this.CurrentDataSource as T;
            if ((service != null) && ((typeof(T) != typeof(IUpdatable)) || !(service is IDataServiceUpdateProvider)))
            {
                return service;
            }
            if (this.IsV1Provider)
            {
                return WebUtil.GetService<T>(this.metadataProvider);
            }
            return default(T);
        }

        public IEnumerable<OperationWrapper> GetVisibleOperations(DataServiceOperationContext operationContext)
        {
            HashSet<string> names = new HashSet<string>(EqualityComparer<string>.Default);
            IEnumerable<ServiceOperation> serviceOperations = this.metadataProvider.ServiceOperations;
            if (serviceOperations != null)
            {
                foreach (ServiceOperation iteratorVariable2 in serviceOperations)
                {
                    AddUniqueNameToSet((iteratorVariable2 != null) ? iteratorVariable2.Name : null, names, System.Data.Services.Strings.DataServiceProviderWrapper_MultipleServiceOperationsWithSameName(iteratorVariable2.Name));
                    OperationWrapper iteratorVariable3 = this.ValidateOperation(iteratorVariable2);
                    if (iteratorVariable3 != null)
                    {
                        yield return iteratorVariable3;
                    }
                }
            }
            IEnumerator<OperationWrapper> enumerator = this.dataService.ActionProvider.GetServiceActions(operationContext).GetEnumerator();
            Func<ResourceSetWrapper, bool> predicate = null;
            OperationWrapper serviceAction;
            while (enumerator.MoveNext())
            {
                serviceAction = enumerator.Current;
                AddUniqueNameToSet(serviceAction.Name, names, System.Data.Services.Strings.DataServiceProviderWrapper_MultipleServiceOperationsWithSameName(serviceAction.Name));
                List<ResourceSetWrapper> source = new List<ResourceSetWrapper>();
                if (serviceAction.BindingParameter != null)
                {
                    ResourceType subType = serviceAction.BindingParameter.ParameterType;
                    if (subType.ResourceTypeKind == ResourceTypeKind.EntityCollection)
                    {
                        subType = ((EntityCollectionResourceType) subType).ItemType;
                    }
                    foreach (ResourceSetWrapper wrapper in this.GetResourceSets())
                    {
                        if (wrapper.ResourceType.IsAssignableFrom(subType))
                        {
                            source.Add(wrapper);
                        }
                    }
                    if (source.Count<ResourceSetWrapper>() == 0)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_ActionHasNoBindableSet(serviceAction.Name, serviceAction.BindingParameter.ParameterType.FullName));
                    }
                }
                if (serviceAction.ResultSetPathExpression != null)
                {
                    if (predicate == null)
                    {
                        predicate = set => serviceAction.ResultSetPathExpression.GetTargetSet(this, set) != null;
                    }
                    if (!source.Any<ResourceSetWrapper>(predicate))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_ActionHasNoVisibleSetReachableFromPathExpression(serviceAction.Name, serviceAction.ResultSetPathExpression.PathExpression));
                    }
                }
                yield return serviceAction;
            }
        }

        public IEnumerable<ResourceType> GetVisibleTypes(DataServiceOperationContext operationContext)
        {
            if (this.Configuration.AccessEnabledForAllResourceTypes)
            {
                foreach (ResourceType iteratorVariable0 in this.Types)
                {
                    yield return iteratorVariable0;
                }
            }
            else
            {
                HashSet<ResourceType> visitedTypes = new HashSet<ResourceType>(EqualityComparer<ResourceType>.Default);
                IEnumerable<ResourceType> first = new ResourceType[0];
                foreach (ResourceSetWrapper wrapper in this.GetResourceSets())
                {
                    first = first.Concat<ResourceType>(this.GetReachableTypesFromSet(wrapper, visitedTypes));
                }
                foreach (OperationWrapper wrapper2 in this.GetVisibleOperations(operationContext))
                {
                    first = first.Concat<ResourceType>(this.GetReachableComplexTypesFromOperation(wrapper2, visitedTypes));
                }
                foreach (string str in this.Configuration.GetAccessEnabledResourceTypes())
                {
                    ResourceType resourceType = this.TryResolveResourceType(str);
                    if (resourceType == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.MetadataSerializer_AccessEnabledTypeNoLongerExists(str));
                    }
                    first = first.Concat<ResourceType>(this.GetResourceTypeAndReachableComplexTypes(resourceType, visitedTypes));
                }
                foreach (ResourceType iteratorVariable3 in first)
                {
                    yield return iteratorVariable3;
                }
            }
        }

        internal bool HasAnnotations(DataServiceOperationContext operationContext)
        {
            return (this.GetMetadataModel(operationContext).VocabularyAnnotations.Count<IEdmVocabularyAnnotation>() > 0);
        }

        public bool HasDerivedTypes(ResourceType resourceType)
        {
            return this.metadataProvider.HasDerivedTypes(resourceType);
        }

        public ConstantExpression InvokeServiceOperation(OperationWrapper serviceOperation, object[] parameters)
        {
            ConstantExpression expression;
            try
            {
                expression = Expression.Constant(this.queryProvider.InvokeServiceOperation(serviceOperation.ServiceOperation, parameters));
            }
            catch (TargetInvocationException exception)
            {
                ErrorHandler.HandleTargetInvocationException(exception);
                throw;
            }
            return expression;
        }

        internal static bool IsUnsupportedProviderForEntityFramework(Type providerType)
        {
            return ((((providerType != typeof(IDataServiceStreamProvider)) && (providerType != typeof(IDataServiceStreamProvider2))) && (providerType != typeof(IDataServiceActionProvider))) && (providerType != typeof(IExpandProvider)));
        }

        internal bool IsV1ProviderAndImplementsUpdatable()
        {
            BaseServiceProvider metadataProvider = this.metadataProvider as BaseServiceProvider;
            return ((metadataProvider != null) && metadataProvider.ImplementsIUpdatable());
        }

        internal void PopulateMetadataCacheItemForBuiltInProvider()
        {
            IEnumerable<ServiceOperation> serviceOperations = this.metadataProvider.ServiceOperations;
            if (serviceOperations != null)
            {
                serviceOperations.Count<ServiceOperation>();
            }
            this.GetVisibleTypes(this.dataService.OperationContext).Count<ResourceType>();
            foreach (ResourceSetWrapper wrapper in this.GetResourceSets())
            {
                foreach (ResourceType type in this.GetDerivedTypes(wrapper.ResourceType))
                {
                    wrapper.GetEntitySerializableProperties(this, type);
                    foreach (ResourceProperty property in type.PropertiesDeclaredOnThisType.Where<ResourceProperty>(delegate (ResourceProperty p) {
                        if (p.Kind != ResourcePropertyKind.ResourceReference)
                        {
                            return p.Kind == ResourcePropertyKind.ResourceSetReference;
                        }
                        return true;
                    }))
                    {
                        this.GetResourceAssociationSet(wrapper, type, property);
                    }
                }
                ResourceType resourceType = wrapper.ResourceType;
                wrapper.GetEntitySerializableProperties(this, resourceType);
                foreach (ResourceProperty property2 in resourceType.Properties.Where<ResourceProperty>(delegate (ResourceProperty p) {
                    if (p.Kind != ResourcePropertyKind.ResourceReference)
                    {
                        return p.Kind == ResourcePropertyKind.ResourceSetReference;
                    }
                    return true;
                }))
                {
                    this.GetResourceAssociationSet(wrapper, resourceType, property2);
                }
            }
        }

        public ResourceSetWrapper TryResolveResourceSet(string name)
        {
            ResourceSetWrapper wrapper;
            ResourceSet set;
            if (this.ResourceSetWrapperCache.TryGetValue(name, out wrapper))
            {
                return wrapper;
            }
            if (this.metadataProvider.TryResolveResourceSet(name, out set))
            {
                return this.ValidateResourceSet(set);
            }
            return null;
        }

        public ResourceType TryResolveResourceType(string name)
        {
            ResourceType type;
            if (this.VisibleTypeCache.TryGetValue(name, out type))
            {
                return type;
            }
            if (this.metadataProvider.TryResolveResourceType(name, out type))
            {
                return this.ValidateResourceType(type);
            }
            return null;
        }

        public OperationWrapper TryResolveServiceOperation(string name)
        {
            OperationWrapper wrapper;
            ServiceOperation operation;
            if (this.OperationWrapperCache.TryGetValue(name, out wrapper))
            {
                if ((wrapper != null) && (wrapper.Kind == OperationKind.ServiceOperation))
                {
                    return wrapper;
                }
                return null;
            }
            if (this.metadataProvider.TryResolveServiceOperation(name, out operation))
            {
                return this.ValidateOperation(operation);
            }
            return null;
        }

        private void ValidateCollectionResourceType(CollectionResourceType collectionResourceType)
        {
            if ((collectionResourceType.ItemType.ResourceTypeKind == ResourceTypeKind.ComplexType) && this.HasDerivedTypes(collectionResourceType.ItemType))
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceProviderWrapper_CollectionOfComplexTypeWithDerivedTypes(collectionResourceType.ItemType.FullName));
            }
        }

        internal OperationWrapper ValidateOperation(Operation operation)
        {
            OperationWrapper wrapper = null;
            if (operation != null)
            {
                if (this.OperationWrapperCache.TryGetValue(operation.Name, out wrapper))
                {
                    return wrapper;
                }
                wrapper = new OperationWrapper(operation);
                wrapper.ApplyConfiguration(this.Configuration, this);
                if (!wrapper.IsVisible)
                {
                    wrapper = null;
                }
                this.OperationWrapperCache[operation.Name] = wrapper;
            }
            return wrapper;
        }

        internal ResourceSetWrapper ValidateResourceSet(ResourceSet resourceSet)
        {
            ResourceSetWrapper wrapper = null;
            if (resourceSet != null)
            {
                if (this.ResourceSetWrapperCache.TryGetValue(resourceSet.Name, out wrapper))
                {
                    return wrapper;
                }
                wrapper = ResourceSetWrapper.CreateResourceSetWrapper(resourceSet, this, new Func<ResourceType, ResourceType>(this.ValidateResourceType));
                this.ResourceSetWrapperCache[resourceSet.Name] = wrapper;
            }
            return wrapper;
        }

        private ResourceType ValidateResourceType(ResourceType resourceType)
        {
            ResourceType type;
            if (resourceType == null)
            {
                return null;
            }
            if (this.VisibleTypeCache.TryGetValue(resourceType.FullName, out type))
            {
                return type;
            }
            ValidateResourceTypeReadOnly(resourceType);
            this.VisibleTypeCache[resourceType.FullName] = resourceType;
            Version minimumProtocolVersion = resourceType.GetMinimumProtocolVersion(true);
            Version maxProtocolVersion = this.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion();
            WebUtil.CheckMaxProtocolVersion(minimumProtocolVersion, maxProtocolVersion);
            return resourceType;
        }

        private static void ValidateResourceTypeReadOnly(ResourceType resourceType)
        {
            if (!resourceType.IsReadOnly)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.DataServiceProviderWrapper_ResourceTypeNotReadonly(resourceType.FullName));
            }
        }

        internal DataServiceConfiguration Configuration
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.Configuration;
            }
        }

        public string ContainerName
        {
            get
            {
                if (this.containerNameCache == null)
                {
                    this.containerNameCache = this.metadataProvider.ContainerName;
                    if (string.IsNullOrEmpty(this.containerNameCache))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_ContainerNameMustNotBeNullOrEmpty);
                    }
                }
                return this.containerNameCache;
            }
        }

        public string ContainerNamespace
        {
            get
            {
                if (this.containerNamespaceCache == null)
                {
                    this.containerNamespaceCache = this.metadataProvider.ContainerNamespace;
                    if (string.IsNullOrEmpty(this.containerNamespaceCache) && !this.IsV1Provider)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.DataServiceProviderWrapper_ContainerNamespaceMustNotBeNullOrEmpty);
                    }
                }
                return this.containerNamespaceCache;
            }
        }

        public object CurrentDataSource
        {
            get
            {
                return this.queryProvider.CurrentDataSource;
            }
        }

        internal bool IsV1Provider
        {
            get
            {
                return (this.metadataProvider is BaseServiceProvider);
            }
        }

        internal IDataServiceMetadataProvider MetadataProvider
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadataProvider;
            }
        }

        public bool NullPropagationRequired
        {
            get
            {
                return this.queryProvider.IsNullPropagationRequired;
            }
        }

        internal Dictionary<string, OperationWrapper> OperationWrapperCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.operationWrapperCache;
            }
        }

        internal IProjectionProvider ProjectionProvider
        {
            get
            {
                if (!this.IsV1Provider)
                {
                    return null;
                }
                if (this.metadataProvider is ObjectContextServiceProvider)
                {
                    return new BasicExpandProvider(this, false, false);
                }
                return new BasicExpandProvider(this, true, true);
            }
        }

        internal IDataServiceQueryProvider QueryProvider
        {
            [DebuggerStepThrough]
            get
            {
                return this.queryProvider;
            }
        }

        private Dictionary<string, ResourceAssociationSet> ResourceAssociationSetCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.ResourceAssociationSetCache;
            }
        }

        private Dictionary<string, ResourceSetWrapper> ResourceSetWrapperCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.ResourceSetWrapperCache;
            }
        }

        private IEnumerable<ResourceType> Types
        {
            get
            {
                IEnumerable<ResourceType> types = this.metadataProvider.Types;
                if (types != null)
                {
                    HashSet<string> names = new HashSet<string>(EqualityComparer<string>.Default);
                    foreach (ResourceType iteratorVariable2 in types)
                    {
                        if ((iteratorVariable2.ResourceTypeKind == ResourceTypeKind.EntityType) || (iteratorVariable2.ResourceTypeKind == ResourceTypeKind.ComplexType))
                        {
                            AddUniqueNameToSet((iteratorVariable2 != null) ? iteratorVariable2.FullName : null, names, System.Data.Services.Strings.DataServiceProviderWrapper_MultipleResourceTypesWithSameName(iteratorVariable2.FullName));
                            ResourceType iteratorVariable3 = this.ValidateResourceType(iteratorVariable2);
                            if (iteratorVariable3 != null)
                            {
                                yield return iteratorVariable3;
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<string, ResourceType> VisibleTypeCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.VisibleTypeCache;
            }
        }

        
    }
}

