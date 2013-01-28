namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Data.Services.Serializers;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Threading;

    [DebuggerDisplay("{Name}: {InstanceType}, {ResourceTypeKind}")]
    internal class ResourceType
    {
        private readonly bool abstractType;
        private ReadOnlyCollection<ResourceProperty> allProperties;
        private bool? basesHaveEpmInfo;
        private readonly ResourceType baseType;
        private bool canReflectOnInstanceType;
        private Func<object> constructorDelegate;
        private Dictionary<string, object> customAnnotations;
        internal static readonly ReadOnlyCollection<ResourceProperty> EmptyProperties = new ReadOnlyCollection<ResourceProperty>(new ResourceProperty[0]);
        private EpmInfoPerResourceType epmInfo;
        private bool epmInfoInitialized;
        private ReadOnlyCollection<ResourceProperty> etagProperties;
        private readonly string fullName;
        private bool isLoadPropertiesMethodCalled;
        private bool isMediaLinkEntry;
        private bool isOpenType;
        private bool isReadOnly;
        private ReadOnlyCollection<ResourceProperty> keyProperties;
        private readonly object lockPropertiesLoad;
        private Version metadataVersion;
        private readonly string name;
        private readonly string namespaceName;
        private static System.Data.Services.Providers.PrimitiveResourceTypeMap primitiveResourceTypeMapping;
        internal static readonly ResourceType PrimitiveStringResourceType = PrimitiveResourceTypeMap.GetPrimitive(typeof(string));
        private IList<ResourceProperty> propertiesDeclaredOnThisType;
        private Dictionary<ResourceProperty, ResourcePropertyInfo> propertyInfosDeclaredOnThisType;
        private readonly System.Data.Services.Providers.ResourceTypeKind resourceTypeKind;
        private MetadataEdmSchemaVersion schemaVersion;
        private readonly Type type;

        internal ResourceType(Type type, System.Data.Services.Providers.ResourceTypeKind resourceTypeKind, string namespaceName, string name) : this(type, null, namespaceName, name, false)
        {
            this.resourceTypeKind = resourceTypeKind;
            this.isReadOnly = true;
            switch (resourceTypeKind)
            {
                case System.Data.Services.Providers.ResourceTypeKind.Primitive:
                    this.InitializeMetadataAndSchemaVersionForPrimitiveType();
                    return;

                case System.Data.Services.Providers.ResourceTypeKind.Collection:
                    this.InitializeMetadataAndSchemaVersionForCollectionType();
                    return;

                case System.Data.Services.Providers.ResourceTypeKind.EntityCollection:
                    this.InitializeMetadataAndSchemaVersionForEntityCollectionType();
                    return;
            }
        }

        private ResourceType(Type type, ResourceType baseType, string namespaceName, string name, bool isAbstract)
        {
            this.lockPropertiesLoad = new object();
            this.propertyInfosDeclaredOnThisType = new Dictionary<ResourceProperty, ResourcePropertyInfo>(ReferenceEqualityComparer<ResourceProperty>.Instance);
            this.schemaVersion = ~MetadataEdmSchemaVersion.Version1Dot0;
            WebUtil.CheckArgumentNull<Type>(type, "type");
            WebUtil.CheckArgumentNull<string>(name, "name");
            this.name = name;
            this.namespaceName = namespaceName ?? string.Empty;
            if ((name == "String") && object.ReferenceEquals(namespaceName, "Edm"))
            {
                this.fullName = "Edm.String";
            }
            else
            {
                this.fullName = string.IsNullOrEmpty(namespaceName) ? name : (namespaceName + "." + name);
            }
            this.type = type;
            this.abstractType = isAbstract;
            this.canReflectOnInstanceType = true;
            if (baseType != null)
            {
                this.baseType = baseType;
            }
        }

        public ResourceType(Type instanceType, System.Data.Services.Providers.ResourceTypeKind resourceTypeKind, ResourceType baseType, string namespaceName, string name, bool isAbstract) : this(instanceType, baseType, namespaceName, name, isAbstract)
        {
            WebUtil.CheckArgumentNull<Type>(instanceType, "instanceType");
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckResourceTypeKind(resourceTypeKind, "resourceTypeKind");
            if (((resourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.Primitive) || (resourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.Collection)) || (resourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.EntityCollection))
            {
                throw new ArgumentException(System.Data.Services.Strings.ResourceType_InvalidValueForResourceTypeKind("resourceTypeKind"), "resourceTypeKind");
            }
            if ((baseType != null) && (baseType.ResourceTypeKind != resourceTypeKind))
            {
                throw new ArgumentException(System.Data.Services.Strings.ResourceType_InvalidResourceTypeKindInheritance(resourceTypeKind.ToString(), baseType.ResourceTypeKind.ToString()), "resourceTypeKind");
            }
            if (instanceType.IsValueType)
            {
                throw new ArgumentException(System.Data.Services.Strings.ResourceType_TypeCannotBeValueType, "instanceType");
            }
            this.resourceTypeKind = resourceTypeKind;
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        public void AddEntityPropertyMappingAttribute(EntityPropertyMappingAttribute attribute)
        {
            this.AddEntityPropertyMappingAttributeInternal(attribute, false);
        }

        internal void AddEntityPropertyMappingAttributeInternal(EntityPropertyMappingAttribute attribute, bool isEFProvider)
        {
            WebUtil.CheckArgumentNull<EntityPropertyMappingAttribute>(attribute, "attribute");
            this.ThrowIfSealed();
            if (this.ResourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.EntityType)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.EpmOnlyAllowedOnEntityTypes(this.Name));
            }
            if (this.epmInfo == null)
            {
                this.epmInfo = new EpmInfoPerResourceType(isEFProvider);
            }
            this.OwnEpmInfo.Add(attribute);
        }

        public void AddProperty(ResourceProperty property)
        {
            WebUtil.CheckArgumentNull<ResourceProperty>(property, "property");
            this.ThrowIfSealed();
            this.AddPropertyInternal(property);
        }

        private void AddPropertyInternal(ResourceProperty property)
        {
            if (this.propertiesDeclaredOnThisType == null)
            {
                this.propertiesDeclaredOnThisType = new List<ResourceProperty>();
            }
            foreach (ResourceProperty property2 in this.propertiesDeclaredOnThisType)
            {
                if (property2.Name == property.Name)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_PropertyWithSameNameAlreadyExists(property2.Name, this.FullName));
                }
            }
            if (property.IsOfKind(ResourcePropertyKind.Stream))
            {
                if (this.ResourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.EntityType)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_NamedStreamsOnlyApplyToEntityType(this.FullName));
                }
            }
            else
            {
                if (property.IsOfKind(ResourcePropertyKind.Key))
                {
                    if (this.baseType != null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_NoKeysInDerivedTypes);
                    }
                    if (this.ResourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.EntityType)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_KeyPropertiesOnlyOnEntityTypes);
                    }
                    if (typeof(ISpatial).IsAssignableFrom(property.ResourceType.type))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_SpatialKeyOrETag(property.Name, this.name));
                    }
                }
                if (property.IsOfKind(ResourcePropertyKind.ETag))
                {
                    if (this.ResourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.EntityType)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_ETagPropertiesOnlyOnEntityTypes);
                    }
                    if (typeof(ISpatial).IsAssignableFrom(property.ResourceType.type))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_SpatialKeyOrETag(property.Name, this.name));
                    }
                }
            }
            this.propertiesDeclaredOnThisType.Add(property);
        }

        private void BuildDynamicEpmInfo(ResourceType currentResourceType, ReadOnlyCollection<ResourceProperty> actualTypeDeclaredProperties)
        {
            if (currentResourceType.BaseType != null)
            {
                this.BuildDynamicEpmInfo(currentResourceType.BaseType, actualTypeDeclaredProperties);
            }
            if (currentResourceType.HasEntityPropertyMappings)
            {
                foreach (EntityPropertyMappingAttribute attribute in currentResourceType.AllEpmInfo.ToList<EntityPropertyMappingAttribute>())
                {
                    this.EpmSourceTree.Add(new EntityPropertyMappingInfo(attribute, currentResourceType, this, this.epmInfo.EFProvider), actualTypeDeclaredProperties);
                    if ((this == currentResourceType) && !EpmPropertyExistsInDeclaredProperties(attribute, actualTypeDeclaredProperties))
                    {
                        this.InheritedEpmInfo.Add(attribute);
                        this.OwnEpmInfo.Remove(attribute);
                    }
                }
            }
        }

        internal static bool CompareReferences(ResourceType resourceType1, ResourceType resourceType2)
        {
            return (resourceType1 == resourceType2);
        }

        private Version ComputeMetadataAndSchemaVersionForPropertyCollection(IEnumerable<ResourceProperty> propertyCollection, HashSet<ResourceType> visitedComplexTypes, out MetadataEdmSchemaVersion propertySchemaVersion)
        {
            Version versionToRaise = RequestDescription.Version1Dot0;
            propertySchemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
            foreach (ResourceProperty property in propertyCollection)
            {
                if (property.IsOfKind(ResourcePropertyKind.ComplexType))
                {
                    MetadataEdmSchemaVersion version2;
                    if (visitedComplexTypes == null)
                    {
                        visitedComplexTypes = new HashSet<ResourceType>(ReferenceEqualityComparer<ResourceType>.Instance);
                    }
                    else if (visitedComplexTypes.Contains(property.ResourceType))
                    {
                        continue;
                    }
                    visitedComplexTypes.Add(property.ResourceType);
                    versionToRaise = WebUtil.RaiseVersion(versionToRaise, this.ComputeMetadataAndSchemaVersionForPropertyCollection(property.ResourceType.PropertiesDeclaredOnThisType, visitedComplexTypes, out version2));
                    propertySchemaVersion = WebUtil.RaiseVersion(propertySchemaVersion, version2);
                }
                else if ((property.IsOfKind(ResourcePropertyKind.Primitive) || property.IsOfKind(ResourcePropertyKind.Collection)) || property.IsOfKind(ResourcePropertyKind.Stream))
                {
                    versionToRaise = WebUtil.RaiseVersion(versionToRaise, property.ResourceType.MetadataVersion);
                    propertySchemaVersion = WebUtil.RaiseVersion(propertySchemaVersion, property.ResourceType.SchemaVersion);
                }
            }
            return versionToRaise;
        }

        private ReadOnlyCollection<ResourceProperty> CreateReadOnlyDeclaredPropertiesCollection()
        {
            this.GetPropertiesDeclaredOnThisType();
            return new ReadOnlyCollection<ResourceProperty>(this.propertiesDeclaredOnThisType ?? EmptyProperties);
        }

        private ReadOnlyCollection<ResourceProperty> CreateReadOnlyPropertiesCollection()
        {
            List<ResourceProperty> list = new List<ResourceProperty>();
            if (this.BaseType != null)
            {
                list.AddRange(this.BaseType.Properties);
            }
            list.AddRange(this.PropertiesDeclaredOnThisType);
            return new ReadOnlyCollection<ResourceProperty>(list);
        }

        internal void EnsureEpmInfoAvailability()
        {
            this.InitializeProperties();
        }

        private static bool EpmPropertyExistsInDeclaredProperties(EntityPropertyMappingAttribute epmAttr, ReadOnlyCollection<ResourceProperty> declaredProperties)
        {
            int index = epmAttr.SourcePath.IndexOf('/');
            string propertyToLookFor = (index == -1) ? epmAttr.SourcePath : epmAttr.SourcePath.Substring(0, index);
            return declaredProperties.Any<ResourceProperty>(p => (p.Name == propertyToLookFor));
        }

        public static CollectionResourceType GetCollectionResourceType(ResourceType itemType)
        {
            WebUtil.CheckArgumentNull<ResourceType>(itemType, "itemType");
            return new CollectionResourceType(itemType);
        }

        public static EntityCollectionResourceType GetEntityCollectionResourceType(ResourceType itemType)
        {
            WebUtil.CheckArgumentNull<ResourceType>(itemType, "itemType");
            return new EntityCollectionResourceType(itemType);
        }

        internal Version GetMinimumProtocolVersion(bool considerEpmInVersion)
        {
            Version versionToRaise = WebUtil.RaiseVersion(RequestDescription.Version1Dot0, this.MetadataVersion);
            if (considerEpmInVersion)
            {
                Version targetVersion = this.EpmMinimumDataServiceProtocolVersion.ToVersion();
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, targetVersion);
            }
            return versionToRaise;
        }

        internal Version GetMinimumResponseVersion(IDataService service, ResourceSetWrapper resourceSet, bool considerEpmInVersion)
        {
            Version versionToRaise = WebUtil.RaiseVersion(RequestDescription.Version1Dot0, this.GetMinimumProtocolVersion(considerEpmInVersion));
            if (service.Configuration.DataServiceBehavior.ShouldIncludeAssociationLinksInResponse && resourceSet.GetEntitySerializableProperties(service.Provider, this).Any<ResourceProperty>(p => (p.TypeKind == System.Data.Services.Providers.ResourceTypeKind.EntityType)))
            {
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, RequestDescription.Version3Dot0);
            }
            if (this.IsOpenType)
            {
                Version version2 = service.Configuration.DataServiceBehavior.MaxProtocolVersion.ToVersion();
                Version requestMaxVersion = service.OperationContext.Host.RequestMaxVersion;
                Version targetVersion = (requestMaxVersion < version2) ? requestMaxVersion : version2;
                versionToRaise = WebUtil.RaiseVersion(versionToRaise, targetVersion);
            }
            return versionToRaise;
        }

        public static ResourceType GetPrimitiveResourceType(Type type)
        {
            return PrimitiveResourceTypeMap.GetPrimitive(type);
        }

        private void GetPropertiesDeclaredOnThisType()
        {
            if (!this.isLoadPropertiesMethodCalled)
            {
                foreach (ResourceProperty property in this.LoadPropertiesDeclaredOnThisType())
                {
                    this.AddPropertyInternal(property);
                    if (this.IsReadOnly)
                    {
                        property.SetReadOnly();
                    }
                }
                this.isLoadPropertiesMethodCalled = true;
            }
        }

        internal PropertyInfo GetPropertyInfo(ResourceProperty resourceProperty)
        {
            return this.GetResourcePropertyInfo(resourceProperty).PropertyInfo;
        }

        private ResourcePropertyInfo GetPropertyInfoDecaredOnThisType(ResourceProperty resourceProperty)
        {
            ResourcePropertyInfo info;
            if (this.propertyInfosDeclaredOnThisType == null)
            {
                this.propertyInfosDeclaredOnThisType = new Dictionary<ResourceProperty, ResourcePropertyInfo>(ReferenceEqualityComparer<ResourceProperty>.Instance);
            }
            if (!this.propertyInfosDeclaredOnThisType.TryGetValue(resourceProperty, out info))
            {
                PropertyInfo property = this.InstanceType.GetProperty(resourceProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    throw new DataServiceException(500, System.Data.Services.Strings.BadProvider_UnableToGetPropertyInfo(this.FullName, resourceProperty.Name));
                }
                info = new ResourcePropertyInfo(property);
                this.propertyInfosDeclaredOnThisType.Add(resourceProperty, info);
            }
            return info;
        }

        internal object GetPropertyValue(ResourceProperty resourceProperty, object target)
        {
            return this.GetResourcePropertyInfo(resourceProperty).PropertyGetter(target);
        }

        private ResourcePropertyInfo GetResourcePropertyInfo(ResourceProperty resourceProperty)
        {
            ResourcePropertyInfo propertyInfoDecaredOnThisType = null;
            for (ResourceType type = this; (propertyInfoDecaredOnThisType == null) && (type != null); type = type.BaseType)
            {
                propertyInfoDecaredOnThisType = type.GetPropertyInfoDecaredOnThisType(resourceProperty);
            }
            return propertyInfoDecaredOnThisType;
        }

        private void InitializeMetadataAndSchemaVersionForCollectionType()
        {
            this.metadataVersion = RequestDescription.Version3Dot0;
            this.schemaVersion = MetadataEdmSchemaVersion.Version3Dot0;
        }

        private void InitializeMetadataAndSchemaVersionForComplexOrEntityType()
        {
            MetadataEdmSchemaVersion version3;
            Version versionToRaise = RequestDescription.Version1Dot0;
            MetadataEdmSchemaVersion schemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
            if (this.baseType != null)
            {
                versionToRaise = this.baseType.MetadataVersion;
                schemaVersion = this.baseType.SchemaVersion;
            }
            versionToRaise = WebUtil.RaiseVersion(versionToRaise, this.ComputeMetadataAndSchemaVersionForPropertyCollection(this.PropertiesDeclaredOnThisType, null, out version3));
            schemaVersion = WebUtil.RaiseVersion(schemaVersion, version3);
            this.metadataVersion = versionToRaise;
            this.schemaVersion = schemaVersion;
        }

        private void InitializeMetadataAndSchemaVersionForEntityCollectionType()
        {
            this.metadataVersion = RequestDescription.Version1Dot0;
            this.schemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
        }

        private void InitializeMetadataAndSchemaVersionForPrimitiveType()
        {
            if (this.type == typeof(Stream))
            {
                this.metadataVersion = RequestDescription.Version3Dot0;
                this.schemaVersion = MetadataEdmSchemaVersion.Version3Dot0;
            }
            else if (typeof(ISpatial).IsAssignableFrom(this.type))
            {
                this.metadataVersion = RequestDescription.Version3Dot0;
                this.schemaVersion = MetadataEdmSchemaVersion.Version3Dot0;
            }
            else
            {
                this.metadataVersion = RequestDescription.Version1Dot0;
                this.schemaVersion = MetadataEdmSchemaVersion.Version1Dot0;
            }
        }

        private ReadOnlyCollection<ResourceProperty> InitializeProperties()
        {
            if (this.allProperties == null)
            {
                if (!this.isReadOnly)
                {
                    return this.CreateReadOnlyPropertiesCollection();
                }
                lock (this.lockPropertiesLoad)
                {
                    if (this.allProperties == null)
                    {
                        this.allProperties = this.CreateReadOnlyPropertiesCollection();
                    }
                }
            }
            return this.allProperties;
        }

        internal bool IsAssignableFrom(ResourceType subType)
        {
            while (subType != null)
            {
                if (subType == this)
                {
                    return true;
                }
                subType = subType.BaseType;
            }
            return false;
        }

        protected virtual IEnumerable<ResourceProperty> LoadPropertiesDeclaredOnThisType()
        {
            return new ResourceProperty[0];
        }

        private void MarkEpmInfoInitialized(ReadOnlyCollection<ResourceProperty> declaredProperties)
        {
            this.epmInfoInitialized = true;
            if (this.epmInfo != null)
            {
                this.EpmSourceTree.Validate(this, declaredProperties);
            }
        }

        internal void RemoveKeyProperties()
        {
            ResourceProperty property = this.KeyProperties[0];
            property.Kind ^= ResourcePropertyKind.Key;
        }

        private static int ResourcePropertyComparison(ResourceProperty a, ResourceProperty b)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
        }

        public void SetReadOnly()
        {
            if (this.ResourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.Collection)
            {
                CollectionResourceType type = this as CollectionResourceType;
                type.ItemType.SetReadOnly();
            }
            if (!this.isReadOnly)
            {
                this.isReadOnly = true;
                if (this.BaseType != null)
                {
                    this.BaseType.SetReadOnly();
                    if (this.BaseType.IsOpenType && (this.ResourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.ComplexType))
                    {
                        this.isOpenType = true;
                    }
                    if (this.BaseType.IsMediaLinkEntry)
                    {
                        this.isMediaLinkEntry = true;
                    }
                    if (!this.BaseType.CanReflectOnInstanceType)
                    {
                        this.canReflectOnInstanceType = false;
                    }
                }
                if (this.propertiesDeclaredOnThisType != null)
                {
                    foreach (ResourceProperty property in this.propertiesDeclaredOnThisType)
                    {
                        property.SetReadOnly();
                    }
                }
            }
        }

        internal void SetValue(object instance, object propertyValue, ResourceProperty resourceProperty)
        {
            MethodInfo setMethod = this.GetPropertyInfo(resourceProperty).GetSetMethod();
            if (setMethod == null)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_PropertyValueCannotBeSet(resourceProperty.Name));
            }
            try
            {
                setMethod.Invoke(instance, new object[] { propertyValue });
            }
            catch (TargetInvocationException exception)
            {
                ErrorHandler.HandleTargetInvocationException(exception);
                throw;
            }
            catch (ArgumentException exception2)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ErrorInSettingPropertyValue(resourceProperty.Name), exception2);
            }
        }

        private void ThrowIfSealed()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_Sealed(this.FullName));
            }
        }

        internal ResourceProperty TryResolvePropertiesDeclaredOnThisTypeByName(string propertyName)
        {
            return this.TryResolvePropertiesDeclaredOnThisTypeByName(propertyName, 0);
        }

        internal ResourceProperty TryResolvePropertiesDeclaredOnThisTypeByName(string propertyName, ResourcePropertyKind exceptKind)
        {
            return this.PropertiesDeclaredOnThisType.FirstOrDefault<ResourceProperty>(p => ((p.Name == propertyName) && ((p.Kind & exceptKind) == 0)));
        }

        internal ResourceProperty TryResolvePropertyName(string propertyName)
        {
            return this.TryResolvePropertyName(propertyName, 0);
        }

        internal ResourceProperty TryResolvePropertyName(string propertyName, ResourcePropertyKind exceptKind)
        {
            return this.Properties.FirstOrDefault<ResourceProperty>(p => ((p.Name == propertyName) && ((p.Kind & exceptKind) == 0)));
        }

        private void ValidateType(ReadOnlyCollection<ResourceProperty> declaredProperties)
        {
            if (this.BaseType != null)
            {
                using (IEnumerator<ResourceProperty> enumerator = this.BaseType.Properties.GetEnumerator())
                {
                    Func<ResourceProperty, bool> predicate = null;
                    ResourceProperty rp;
                    while (enumerator.MoveNext())
                    {
                        rp = enumerator.Current;
                        if (predicate == null)
                        {
                            predicate = p => p.Name == rp.Name;
                        }
                        if (declaredProperties.Where<ResourceProperty>(predicate).FirstOrDefault<ResourceProperty>() != null)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_PropertyWithSameNameAlreadyExists(rp.Name, this.FullName));
                        }
                    }
                    goto Label_00C0;
                }
            }
            if ((this.ResourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.EntityType) && ((from p in declaredProperties
                where p.IsOfKind(ResourcePropertyKind.Key)
                select p).FirstOrDefault<ResourceProperty>() == null))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_MissingKeyPropertiesForEntity(this.FullName));
            }
        Label_00C0:
            foreach (ResourceProperty property in declaredProperties)
            {
                property.SetReadOnly();
                if (property.CanReflectOnInstanceTypeProperty)
                {
                    this.GetPropertyInfoDecaredOnThisType(property);
                }
            }
            try
            {
                if (!this.EpmInfoInitialized)
                {
                    this.BuildDynamicEpmInfo(this, declaredProperties);
                    this.MarkEpmInfoInitialized(declaredProperties);
                }
            }
            catch
            {
                if (this.HasEntityPropertyMappings && !this.EpmInfoInitialized)
                {
                    this.epmInfo.Reset();
                }
                throw;
            }
        }

        internal IEnumerable<EntityPropertyMappingAttribute> AllEpmInfo
        {
            get
            {
                return this.epmInfo.OwnEpmInfo.Concat<EntityPropertyMappingAttribute>(this.epmInfo.InheritedEpmInfo);
            }
        }

        public ResourceType BaseType
        {
            [DebuggerStepThrough]
            get
            {
                return this.baseType;
            }
        }

        public bool CanReflectOnInstanceType
        {
            [DebuggerStepThrough]
            get
            {
                return this.canReflectOnInstanceType;
            }
            set
            {
                this.ThrowIfSealed();
                this.canReflectOnInstanceType = value;
            }
        }

        internal Func<object> ConstructorDelegate
        {
            get
            {
                if (this.constructorDelegate == null)
                {
                    this.constructorDelegate = (Func<object>) WebUtil.CreateNewInstanceConstructor(this.InstanceType, this.FullName, typeof(object));
                }
                return this.constructorDelegate;
            }
        }

        internal IEnumerable<KeyValuePair<string, object>> CustomAnnotations
        {
            get
            {
                if (this.customAnnotations == null)
                {
                    return WebUtil.EmptyKeyValuePairStringObject;
                }
                return this.customAnnotations;
            }
        }

        public object CustomState { get; set; }

        internal bool EpmInfoInitialized
        {
            get
            {
                return this.epmInfoInitialized;
            }
        }

        internal DataServiceProtocolVersion EpmMinimumDataServiceProtocolVersion
        {
            get
            {
                this.InitializeProperties();
                if (this.HasEntityPropertyMappings)
                {
                    return this.EpmTargetTree.MinimumDataServiceProtocolVersion;
                }
                return DataServiceProtocolVersion.V1;
            }
        }

        internal System.Data.Services.Serializers.EpmSourceTree EpmSourceTree
        {
            get
            {
                if (this.epmInfo == null)
                {
                    this.epmInfo = new EpmInfoPerResourceType(false);
                }
                return this.epmInfo.EpmSourceTree;
            }
        }

        internal System.Data.Services.Serializers.EpmTargetTree EpmTargetTree
        {
            get
            {
                return this.epmInfo.EpmTargetTree;
            }
        }

        public ReadOnlyCollection<ResourceProperty> ETagProperties
        {
            get
            {
                if (this.etagProperties == null)
                {
                    ReadOnlyCollection<ResourceProperty> onlys = new ReadOnlyCollection<ResourceProperty>((from p in this.Properties
                        where p.IsOfKind(ResourcePropertyKind.ETag)
                        select p).ToList<ResourceProperty>());
                    if (!this.isReadOnly)
                    {
                        return onlys;
                    }
                    this.etagProperties = onlys;
                }
                return this.etagProperties;
            }
        }

        public string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        internal bool HasEntityPropertyMappings
        {
            get
            {
                if (this.epmInfo != null)
                {
                    return true;
                }
                if (!this.basesHaveEpmInfo.HasValue)
                {
                    this.basesHaveEpmInfo = new bool?((this.BaseType != null) ? this.BaseType.HasEntityPropertyMappings : false);
                }
                return this.basesHaveEpmInfo.Value;
            }
        }

        internal bool HasNamedStreams
        {
            get
            {
                return this.Properties.Any<ResourceProperty>(p => p.IsOfKind(ResourcePropertyKind.Stream));
            }
        }

        internal bool HasNamedStreamsDeclaredOnThisType
        {
            get
            {
                return this.PropertiesDeclaredOnThisType.Any<ResourceProperty>(p => p.IsOfKind(ResourcePropertyKind.Stream));
            }
        }

        internal IList<EntityPropertyMappingAttribute> InheritedEpmInfo
        {
            get
            {
                return this.epmInfo.InheritedEpmInfo;
            }
        }

        public Type InstanceType
        {
            [DebuggerStepThrough]
            get
            {
                return this.type;
            }
        }

        public bool IsAbstract
        {
            get
            {
                return this.abstractType;
            }
        }

        public bool IsMediaLinkEntry
        {
            [DebuggerStepThrough]
            get
            {
                return this.isMediaLinkEntry;
            }
            set
            {
                this.ThrowIfSealed();
                if ((this.resourceTypeKind != System.Data.Services.Providers.ResourceTypeKind.EntityType) && value)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_HasStreamAttributeOnlyAppliesToEntityType(this.name));
                }
                this.isMediaLinkEntry = value;
            }
        }

        public bool IsOpenType
        {
            [DebuggerStepThrough]
            get
            {
                return this.isOpenType;
            }
            set
            {
                this.ThrowIfSealed();
                if ((this.resourceTypeKind == System.Data.Services.Providers.ResourceTypeKind.ComplexType) && value)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ResourceType_ComplexTypeCannotBeOpen(this.FullName));
                }
                this.isOpenType = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public ReadOnlyCollection<ResourceProperty> KeyProperties
        {
            get
            {
                if (this.keyProperties == null)
                {
                    ReadOnlyCollection<ResourceProperty> emptyProperties;
                    ResourceType baseType = this;
                    while (baseType.BaseType != null)
                    {
                        baseType = baseType.BaseType;
                    }
                    if (baseType.Properties == null)
                    {
                        emptyProperties = EmptyProperties;
                    }
                    else
                    {
                        List<ResourceProperty> list = (from p in baseType.Properties
                            where p.IsOfKind(ResourcePropertyKind.Key)
                            select p).ToList<ResourceProperty>();
                        list.Sort(new Comparison<ResourceProperty>(ResourceType.ResourcePropertyComparison));
                        emptyProperties = new ReadOnlyCollection<ResourceProperty>(list);
                    }
                    if (!this.isReadOnly)
                    {
                        return emptyProperties;
                    }
                    this.keyProperties = emptyProperties;
                }
                return this.keyProperties;
            }
        }

        internal Version MetadataVersion
        {
            get
            {
                if (this.metadataVersion == null)
                {
                    this.InitializeMetadataAndSchemaVersionForComplexOrEntityType();
                }
                return this.metadataVersion;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal IEnumerable<ResourceProperty> NamedStreams
        {
            get
            {
                return (from p in this.Properties
                    where p.IsOfKind(ResourcePropertyKind.Stream)
                    select p);
            }
        }

        internal IEnumerable<ResourceProperty> NamedStreamsDeclaredOnThisType
        {
            get
            {
                return (from p in this.PropertiesDeclaredOnThisType
                    where p.IsOfKind(ResourcePropertyKind.Stream)
                    select p);
            }
        }

        public string Namespace
        {
            get
            {
                return this.namespaceName;
            }
        }

        internal IList<EntityPropertyMappingAttribute> OwnEpmInfo
        {
            get
            {
                return this.epmInfo.OwnEpmInfo;
            }
        }

        internal static System.Data.Services.Providers.PrimitiveResourceTypeMap PrimitiveResourceTypeMap
        {
            get
            {
                if (primitiveResourceTypeMapping == null)
                {
                    System.Data.Services.Providers.PrimitiveResourceTypeMap map = new System.Data.Services.Providers.PrimitiveResourceTypeMap();
                    Interlocked.CompareExchange<System.Data.Services.Providers.PrimitiveResourceTypeMap>(ref primitiveResourceTypeMapping, map, null);
                }
                return primitiveResourceTypeMapping;
            }
        }

        public ReadOnlyCollection<ResourceProperty> Properties
        {
            get
            {
                return this.InitializeProperties();
            }
        }

        public ReadOnlyCollection<ResourceProperty> PropertiesDeclaredOnThisType
        {
            get
            {
                ReadOnlyCollection<ResourceProperty> propertiesDeclaredOnThisType = this.propertiesDeclaredOnThisType as ReadOnlyCollection<ResourceProperty>;
                if (propertiesDeclaredOnThisType == null)
                {
                    if (!this.isReadOnly)
                    {
                        return this.CreateReadOnlyDeclaredPropertiesCollection();
                    }
                    lock (this.lockPropertiesLoad)
                    {
                        propertiesDeclaredOnThisType = this.propertiesDeclaredOnThisType as ReadOnlyCollection<ResourceProperty>;
                        if (propertiesDeclaredOnThisType == null)
                        {
                            propertiesDeclaredOnThisType = this.CreateReadOnlyDeclaredPropertiesCollection();
                            this.ValidateType(propertiesDeclaredOnThisType);
                            this.propertiesDeclaredOnThisType = propertiesDeclaredOnThisType;
                        }
                    }
                }
                return propertiesDeclaredOnThisType;
            }
        }

        public System.Data.Services.Providers.ResourceTypeKind ResourceTypeKind
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceTypeKind;
            }
        }

        internal MetadataEdmSchemaVersion SchemaVersion
        {
            get
            {
                if (this.schemaVersion == ~MetadataEdmSchemaVersion.Version1Dot0)
                {
                    this.InitializeMetadataAndSchemaVersionForComplexOrEntityType();
                }
                return this.schemaVersion;
            }
        }

        private sealed class EpmInfoPerResourceType
        {
            private System.Data.Services.Serializers.EpmSourceTree epmSourceTree;
            private System.Data.Services.Serializers.EpmTargetTree epmTargetTree;
            private List<EntityPropertyMappingAttribute> inheritedEpmInfo;
            private List<EntityPropertyMappingAttribute> ownEpmInfo;

            internal EpmInfoPerResourceType(bool isEFProvider)
            {
                this.EFProvider = isEFProvider;
            }

            internal void Reset()
            {
                this.epmTargetTree = null;
                this.epmSourceTree = null;
                this.inheritedEpmInfo = null;
            }

            internal bool EFProvider { get; private set; }

            internal System.Data.Services.Serializers.EpmSourceTree EpmSourceTree
            {
                get
                {
                    if (this.epmSourceTree == null)
                    {
                        this.epmSourceTree = new System.Data.Services.Serializers.EpmSourceTree(this.EpmTargetTree);
                    }
                    return this.epmSourceTree;
                }
            }

            internal System.Data.Services.Serializers.EpmTargetTree EpmTargetTree
            {
                get
                {
                    if (this.epmTargetTree == null)
                    {
                        this.epmTargetTree = new System.Data.Services.Serializers.EpmTargetTree();
                    }
                    return this.epmTargetTree;
                }
            }

            internal List<EntityPropertyMappingAttribute> InheritedEpmInfo
            {
                get
                {
                    if (this.inheritedEpmInfo == null)
                    {
                        this.inheritedEpmInfo = new List<EntityPropertyMappingAttribute>();
                    }
                    return this.inheritedEpmInfo;
                }
            }

            internal List<EntityPropertyMappingAttribute> OwnEpmInfo
            {
                get
                {
                    if (this.ownEpmInfo == null)
                    {
                        this.ownEpmInfo = new List<EntityPropertyMappingAttribute>();
                    }
                    return this.ownEpmInfo;
                }
            }
        }

        private class ResourcePropertyInfo
        {
            internal ResourcePropertyInfo(System.Reflection.PropertyInfo propertyInfo)
            {
                ParameterExpression expression;
                this.PropertyInfo = propertyInfo;
                this.PropertyGetter = (Func<object, object>) Expression.Lambda(Expression.Convert(Expression.Call(Expression.Convert(expression = Expression.Parameter(typeof(object), "instance"), propertyInfo.DeclaringType), propertyInfo.GetGetMethod()), typeof(object)), new ParameterExpression[] { expression }).Compile();
            }

            internal Func<object, object> PropertyGetter { get; private set; }

            internal System.Reflection.PropertyInfo PropertyInfo { get; private set; }
        }
    }
}

