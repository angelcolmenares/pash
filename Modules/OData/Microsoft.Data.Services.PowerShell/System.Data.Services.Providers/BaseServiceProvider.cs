namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Caching;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Web;
    using System.Threading;

    internal abstract class BaseServiceProvider : IDataServiceMetadataProvider, IDataServiceQueryProvider, IDisposable, IServiceProvider
    {
        private readonly object dataServiceInstance;
        private object dataSourceInstance;
        private ProviderMetadataCacheItem metadata;
        private bool metadataRequiresInitialization;
        protected const BindingFlags ResourceContainerBindingFlags = (BindingFlags.Public | BindingFlags.Instance);

        protected BaseServiceProvider(object dataServiceInstance, object dataSourceInstance)
        {
            WebUtil.CheckArgumentNull<object>(dataServiceInstance, "dataServiceInstance");
            WebUtil.CheckArgumentNull<object>(dataSourceInstance, "dataSourceInstance");
            this.dataServiceInstance = dataServiceInstance;
            this.dataSourceInstance = dataSourceInstance;
        }

        internal void AddOperationsFromType(System.Type type)
        {
            foreach (MethodInfo info in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
            {
                if (info.GetCustomAttributes(typeof(WebGetAttribute), true).Length != 0)
                {
                    this.AddServiceOperation(info, "GET");
                }
                else if (info.GetCustomAttributes(typeof(WebInvokeAttribute), true).Length != 0)
                {
                    this.AddServiceOperation(info, "POST");
                }
            }
        }

        private void AddServiceOperation(MethodInfo method, string protocolMethod)
        {
            ServiceOperationResultKind @void;
            if (this.metadata.ServiceOperations.ContainsKey(method.Name))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_OverloadingNotSupported(this.Type, method));
            }
            bool flag = SingleResultAttribute.MethodHasSingleResult(method);
            ResourceType primitive = null;
            if (method.ReturnType == typeof(void))
            {
                @void = ServiceOperationResultKind.Void;
            }
            else
            {
                System.Type returnType;
                if (WebUtil.IsPrimitiveType(method.ReturnType))
                {
                    @void = ServiceOperationResultKind.DirectValue;
                    returnType = method.ReturnType;
                    primitive = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(returnType);
                }
                else
                {
                    System.Type genericInterfaceElementType = GetGenericInterfaceElementType(method.ReturnType, new TypeFilter(BaseServiceProvider.IQueryableTypeFilter));
                    if (genericInterfaceElementType != null)
                    {
                        @void = flag ? ServiceOperationResultKind.QueryWithSingleResult : ServiceOperationResultKind.QueryWithMultipleResults;
                        returnType = genericInterfaceElementType;
                    }
                    else
                    {
                        System.Type iEnumerableElement = GetIEnumerableElement(method.ReturnType);
                        if (iEnumerableElement != null)
                        {
                            @void = ServiceOperationResultKind.Enumeration;
                            returnType = iEnumerableElement;
                        }
                        else
                        {
                            returnType = method.ReturnType;
                            @void = ServiceOperationResultKind.DirectValue;
                        }
                    }
                    primitive = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(returnType);
                    if (primitive == null)
                    {
                        primitive = this.PopulateMetadataForType(returnType, this.TypeCache, this.ChildTypesCache, this.EntitySets.Values);
                    }
                }
                if (primitive == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_UnsupportedReturnType(method, method.ReturnType));
                }
                if ((@void == ServiceOperationResultKind.Enumeration) && flag)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_IEnumerableAlwaysMultiple(this.Type, method));
                }
            }
            ParameterInfo[] parameters = method.GetParameters();
            ServiceOperationParameter[] parameterArray = new ServiceOperationParameter[parameters.Length];
            for (int i = 0; i < parameterArray.Length; i++)
            {
                ParameterInfo info = parameters[i];
                if (info.IsOut || info.IsRetval)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_ParameterNotIn(method, info));
                }
                ResourceType parameterType = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(info.ParameterType);
                if (parameterType == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_ParameterTypeNotSupported(method, info, info.ParameterType));
                }
                string name = info.Name ?? ("p" + i.ToString(CultureInfo.InvariantCulture));
                parameterArray[i] = new ServiceOperationParameter(name, parameterType);
            }
            ResourceSet container = null;
            if (((primitive != null) && (primitive.ResourceTypeKind == ResourceTypeKind.EntityType)) && !this.TryFindAnyContainerForType(primitive, out container))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_ServiceOperationMissingSingleEntitySet(method, primitive.FullName));
            }
            ServiceOperation operation = new ServiceOperation(method.Name, @void, primitive, container, protocolMethod, parameterArray) {
                CustomState = method
            };
            MimeTypeAttribute mimeTypeAttribute = MimeTypeAttribute.GetMimeTypeAttribute(method);
            if (mimeTypeAttribute != null)
            {
                operation.MimeType = mimeTypeAttribute.MimeType;
            }
            this.metadata.ServiceOperations.Add(method.Name, operation);
        }

        internal void ApplyConfiguration(DataServiceConfiguration configuration)
        {
            if (this.metadataRequiresInitialization)
            {
                this.PopulateMetadataForUserSpecifiedTypes(configuration.GetKnownTypes(), this.TypeCache, this.ChildTypesCache, this.EntitySets.Values);
                if (configuration.DataServiceBehavior.UseMetadataKeyOrderForBuiltInProviders)
                {
                    foreach (ResourceSet set in this.EntitySets.Values)
                    {
                        set.UseMetadataKeyOrder = true;
                    }
                }
                this.CheckConfigurationConsistency(configuration);
            }
        }

        protected virtual void CheckConfigurationConsistency(DataServiceConfiguration configuration)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            WebUtil.Dispose(this.dataSourceInstance);
            this.dataSourceInstance = null;
        }

        public IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType)
        {
            WebUtil.CheckArgumentNull<ResourceType>(resourceType, "resourceType");
            if (!this.ChildTypesCache.ContainsKey(resourceType))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_UnknownResourceTypeInstance(resourceType.FullName));
            }
            List<ResourceType> iteratorVariable0 = this.ChildTypesCache[resourceType];
            if (iteratorVariable0 != null)
            {
                foreach (ResourceType iteratorVariable1 in iteratorVariable0)
                {
                    yield return iteratorVariable1;
                    foreach (ResourceType iteratorVariable2 in this.GetDerivedTypes(iteratorVariable1))
                    {
                        yield return iteratorVariable2;
                    }
                }
            }
        }

        internal static System.Type GetGenericInterfaceElementType(System.Type type, TypeFilter typeFilter)
        {
            if (typeFilter(type, null))
            {
                return type.GetGenericArguments()[0];
            }
            System.Type[] typeArray = type.FindInterfaces(typeFilter, null);
            if ((typeArray != null) && (typeArray.Length == 1))
            {
                return typeArray[0].GetGenericArguments()[0];
            }
            return null;
        }

        internal static System.Type GetIEnumerableElement(System.Type type)
        {
            return GetGenericInterfaceElementType(type, new TypeFilter(BaseServiceProvider.IEnumerableTypeFilter));
        }

        protected static System.Type GetIQueryableElement(System.Type type)
        {
            return GetGenericInterfaceElementType(type, new TypeFilter(BaseServiceProvider.IQueryableTypeFilter));
        }

        private ResourceType GetNonPrimitiveType(System.Type type)
        {
            ResourceType primitive = this.ResolveNonPrimitiveType(type);
            if (primitive == null)
            {
                System.Type iEnumerableElement = GetIEnumerableElement(type);
                if (iEnumerableElement != null)
                {
                    primitive = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(iEnumerableElement);
                    if (primitive == null)
                    {
                        primitive = this.ResolveNonPrimitiveType(iEnumerableElement);
                    }
                }
            }
            return primitive;
        }

        public abstract object GetOpenPropertyValue(object target, string propertyName);
        public abstract IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target);
        public object GetPropertyValue(object target, ResourceProperty resourceProperty)
        {
            object obj2;
            WebUtil.CheckArgumentNull<object>(target, "target");
            WebUtil.CheckArgumentNull<ResourceProperty>(resourceProperty, "resourceProperty");
            try
            {
                obj2 = this.GetResourceType(target).GetPropertyInfo(resourceProperty).GetGetMethod().Invoke(target, null);
            }
            catch (TargetInvocationException exception)
            {
                ErrorHandler.HandleTargetInvocationException(exception);
                throw;
            }
            return obj2;
        }

        public IQueryable GetQueryRootForResourceSet(ResourceSet container)
        {
            WebUtil.CheckArgumentNull<ResourceSet>(container, "container");
            return this.GetResourceContainerInstance(container);
        }

        public abstract ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty);
        protected abstract IQueryable GetResourceContainerInstance(ResourceSet resourceContainer);
        public ResourceType GetResourceType(object resource)
        {
            WebUtil.CheckArgumentNull<object>(resource, "resource");
            return this.GetNonPrimitiveType(resource.GetType());
        }

        public virtual object GetService(System.Type serviceType)
        {
            if ((!(typeof(IDataServiceMetadataProvider) == serviceType) && !(typeof(IDataServiceQueryProvider) == serviceType)) && !(typeof(IProjectionProvider) == serviceType))
            {
                return null;
            }
            return this;
        }

        protected ResourceType GetSingleResource(object resource)
        {
            return this.GetResourceType(resource);
        }

        public virtual bool GetTypeIsOrdered(System.Type type)
        {
            if (!(type == typeof(object)))
            {
                return WebUtil.IsPrimitiveType(type);
            }
            return true;
        }

        public bool HasDerivedTypes(ResourceType resourceType)
        {
            WebUtil.CheckArgumentNull<ResourceType>(resourceType, "resourceType");
            if (!this.ChildTypesCache.ContainsKey(resourceType))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BaseServiceProvider_UnknownResourceTypeInstance(resourceType.FullName));
            }
            return (this.ChildTypesCache[resourceType] != null);
        }

        private static bool IEnumerableTypeFilter(System.Type m, object filterCriteria)
        {
            return (m.IsGenericType && (m.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        internal abstract bool ImplementsIUpdatable();
        public object InvokeServiceOperation(ServiceOperation serviceOperation, object[] parameters)
        {
            object obj2;
            WebUtil.CheckArgumentNull<ServiceOperation>(serviceOperation, "serviceOperation");
            try
            {
                obj2 = ((MethodInfo) serviceOperation.CustomState).Invoke(this.dataServiceInstance, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance, null, parameters, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException exception)
            {
                ErrorHandler.HandleTargetInvocationException(exception);
                throw;
            }
            return obj2;
        }

        private static bool IQueryableTypeFilter(System.Type m, object filterCriteria)
        {
            return (m.IsGenericType && (m.GetGenericTypeDefinition() == typeof(IQueryable<>)));
        }

        internal void LoadMetadata()
        {
            System.Type serviceType = this.dataServiceInstance.GetType();
            System.Type type = this.dataSourceInstance.GetType();
            this.metadata = MetadataCache<ProviderMetadataCacheItem>.TryLookup(serviceType, this.dataSourceInstance);
            if (this.metadata == null)
            {
                this.metadata = new ProviderMetadataCacheItem(type);
                this.PopulateMetadata();
                this.AddOperationsFromType(serviceType);
                this.metadataRequiresInitialization = true;
            }
        }

        internal void MakeMetadataReadonly()
        {
            if (this.metadataRequiresInitialization)
            {
                foreach (ResourceSet set in this.ResourceSets)
                {
                    set.SetReadOnly();
                }
                foreach (ResourceType type in this.Types)
                {
                    type.SetReadOnly();
                    type.PropertiesDeclaredOnThisType.Count<ResourceProperty>();
                }
                foreach (ServiceOperation operation in this.ServiceOperations)
                {
                    operation.SetReadOnly();
                }
                this.metadata = MetadataCache<ProviderMetadataCacheItem>.AddCacheItem(this.dataServiceInstance.GetType(), this.dataSourceInstance, this.metadata);
            }
        }

        internal void PopulateMetadata()
        {
            this.PopulateMetadata(this.TypeCache, this.ChildTypesCache, this.EntitySets);
        }

        protected abstract void PopulateMetadata(IDictionary<System.Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IDictionary<string, ResourceSet> entitySets);
        protected abstract ResourceType PopulateMetadataForType(System.Type type, IDictionary<System.Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets);
        protected abstract void PopulateMetadataForUserSpecifiedTypes(IEnumerable<System.Type> userSpecifiedTypes, IDictionary<System.Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets);
        protected virtual ResourceType ResolveNonPrimitiveType(System.Type type)
        {
            ResourceType type2;
            this.TypeCache.TryGetValue(type, out type2);
            return type2;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool TryFindAnyContainerForType(ResourceType type, out ResourceSet container)
        {
            foreach (ResourceSet set in this.EntitySets.Values)
            {
                if (set.ResourceType.IsAssignableFrom(type))
                {
                    container = set;
                    return true;
                }
            }
            container = null;
            return false;
        }

        protected static bool TryGetType(IDictionary<System.Type, ResourceType> knownTypes, System.Type type, out ResourceType resourceType)
        {
            resourceType = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(type);
            if (resourceType == null)
            {
                knownTypes.TryGetValue(type, out resourceType);
            }
            return (resourceType != null);
        }

        public bool TryResolveResourceSet(string name, out ResourceSet resourceSet)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            return this.EntitySets.TryGetValue(name, out resourceSet);
        }

        public bool TryResolveResourceType(string name, out ResourceType resourceType)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            foreach (ResourceType type in this.TypeCache.Values)
            {
                if (type.FullName == name)
                {
                    resourceType = type;
                    return true;
                }
            }
            resourceType = null;
            return false;
        }

        public bool TryResolveServiceOperation(string name, out ServiceOperation serviceOperation)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            return this.metadata.ServiceOperations.TryGetValue(name, out serviceOperation);
        }

        private Dictionary<ResourceType, List<ResourceType>> ChildTypesCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.ChildTypesCache;
            }
        }

        public abstract string ContainerName { get; }

        public abstract string ContainerNamespace { get; }

        public object CurrentDataSource
        {
            [DebuggerStepThrough]
            get
            {
                return this.dataSourceInstance;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        protected IDictionary<string, ResourceSet> EntitySets
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.EntitySets;
            }
        }

        public abstract bool IsNullPropagationRequired { get; }

        public IEnumerable<ResourceSet> ResourceSets
        {
            get
            {
                return this.EntitySets.Values;
            }
        }

        public IEnumerable<ServiceOperation> ServiceOperations
        {
            get
            {
                foreach (ServiceOperation iteratorVariable0 in this.metadata.ServiceOperations.Values)
                {
                    yield return iteratorVariable0;
                }
            }
        }

        protected System.Type Type
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.Type;
            }
        }

        private Dictionary<System.Type, ResourceType> TypeCache
        {
            [DebuggerStepThrough]
            get
            {
                return this.metadata.TypeCache;
            }
        }

        public IEnumerable<ResourceType> Types
        {
            get
            {
                return this.metadata.TypeCache.Values;
            }
        }

        
    }
}

