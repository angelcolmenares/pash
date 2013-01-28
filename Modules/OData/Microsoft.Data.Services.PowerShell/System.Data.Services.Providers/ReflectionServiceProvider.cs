namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Xml;

    [DebuggerDisplay("ReflectionServiceProvider: {Type}")]
    internal class ReflectionServiceProvider : BaseServiceProvider
    {
        internal ReflectionServiceProvider(object dataServiceInstance, object dataSourceInstance) : base(dataServiceInstance, dataSourceInstance)
        {
        }

        private static ResourceType BuildHierarchyForEntityType(Type type, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, Queue<ResourceType> unvisitedTypes, bool entityTypeCandidate)
        {
            List<Type> list = new List<Type>();
            if (!type.IsVisible)
            {
                return null;
            }
            if (CommonUtil.IsUnsupportedType(type))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_UnsupportedType(type.FullName));
            }
            Type baseType = type;
            ResourceType resourceType = null;
            while (baseType != null)
            {
                if (BaseServiceProvider.TryGetType(knownTypes, baseType, out resourceType))
                {
                    break;
                }
                list.Add(baseType);
                baseType = baseType.BaseType;
            }
            if (resourceType == null)
            {
                if (!entityTypeCandidate)
                {
                    return null;
                }
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (CommonUtil.IsUnsupportedType(list[j]))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_UnsupportedAncestorType(type.FullName, list[j].FullName));
                    }
                    if (DoesTypeHaveKeyProperties(list[j], entityTypeCandidate))
                    {
                        break;
                    }
                    list.RemoveAt(j);
                }
            }
            else
            {
                if (resourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
                {
                    return null;
                }
                if (list.Count == 0)
                {
                    return resourceType;
                }
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ResourceType item = CreateResourceType(list[i], ResourceTypeKind.EntityType, resourceType, knownTypes, childTypes);
                unvisitedTypes.Enqueue(item);
                resourceType = item;
            }
            return resourceType;
        }

        private static void BuildReflectionEpmInfo(ResourceType currentResourceType)
        {
            if (currentResourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
            {
                foreach (EntityPropertyMappingAttribute attribute in currentResourceType.InstanceType.GetCustomAttributes(typeof(EntityPropertyMappingAttribute), currentResourceType.BaseType == null))
                {
                    currentResourceType.AddEntityPropertyMappingAttribute(attribute);
                }
            }
        }

        private static void BuildTypeProperties(ResourceType parentResourceType, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, Queue<ResourceType> unvisitedTypes, IEnumerable<ResourceSet> entitySets)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (parentResourceType.BaseType != null)
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
            }
            HashSet<string> set = new HashSet<string>(IgnorePropertiesAttribute.GetProperties(parentResourceType.InstanceType, false, bindingFlags), StringComparer.Ordinal);
            HashSet<string> source = new HashSet<string>(LoadETagProperties(parentResourceType), StringComparer.Ordinal);
            ResourceKeyKind kind = (ResourceKeyKind) 0x7fffffff;
            PropertyInfo[] properties = parentResourceType.InstanceType.GetProperties(bindingFlags);
            if (!properties.Any<PropertyInfo>() && (parentResourceType.BaseType == null))
            {
                throw new NotSupportedException(System.Data.Services.Strings.ReflectionProvider_ResourceTypeHasNoPublicallyVisibleProperties(parentResourceType.FullName));
            }
            foreach (PropertyInfo info in properties)
            {
                if (!set.Contains(info.Name))
                {
                    ResourceType collectionResourceType;
                    if (!info.CanRead || (info.GetIndexParameters().Length != 0))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_InvalidProperty(info.Name, parentResourceType.FullName));
                    }
					ResourcePropertyKind collection = (ResourcePropertyKind)(-1);
                    Type propertyType = info.PropertyType;
                    bool flag = false;
                    if (!BaseServiceProvider.TryGetType(knownTypes, propertyType, out collectionResourceType))
                    {
                        Type iEnumerableElement = BaseServiceProvider.GetIEnumerableElement(info.PropertyType);
                        if (iEnumerableElement != null)
                        {
                            BaseServiceProvider.TryGetType(knownTypes, iEnumerableElement, out collectionResourceType);
                            flag = true;
                            propertyType = iEnumerableElement;
                        }
                    }
                    if (collectionResourceType != null)
                    {
                        if (collectionResourceType.ResourceTypeKind == ResourceTypeKind.Primitive)
                        {
                            if (flag)
                            {
                                collection = ResourcePropertyKind.Collection;
                            }
                            else
                            {
                                ResourceKeyKind kind3;
                                if (((parentResourceType.BaseType == null) && (parentResourceType.ResourceTypeKind == ResourceTypeKind.EntityType)) && IsPropertyKeyProperty(info, out kind3))
                                {
                                    if (kind3 < kind)
                                    {
                                        if (parentResourceType.KeyProperties.Count != 0)
                                        {
                                            parentResourceType.RemoveKeyProperties();
                                        }
                                        kind = kind3;
                                        collection = ResourcePropertyKind.Key | ResourcePropertyKind.Primitive;
                                    }
                                    else if (kind3 == kind)
                                    {
                                        collection = ResourcePropertyKind.Key | ResourcePropertyKind.Primitive;
                                    }
                                    else
                                    {
                                        collection = ResourcePropertyKind.Primitive;
                                    }
                                }
                                else
                                {
                                    collection = ResourcePropertyKind.Primitive;
                                }
                            }
                        }
                        else if (collectionResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                        {
                            collection = flag ? ResourcePropertyKind.Collection : ResourcePropertyKind.ComplexType;
                        }
                        else if (collectionResourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
                        {
                            collection = flag ? ResourcePropertyKind.ResourceSetReference : ResourcePropertyKind.ResourceReference;
                        }
                    }
                    else
                    {
                        collectionResourceType = IsEntityOrComplexType(propertyType, knownTypes, childTypes, unvisitedTypes);
                        if (collectionResourceType != null)
                        {
                            if (collectionResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                            {
                                if (flag)
                                {
                                    if (BaseServiceProvider.GetIEnumerableElement(propertyType) != null)
                                    {
                                        throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_CollectionOfCollectionProperty(info.Name, parentResourceType.FullName));
                                    }
                                    collection = ResourcePropertyKind.Collection;
                                }
                                else
                                {
                                    collection = ResourcePropertyKind.ComplexType;
                                }
                            }
                            else
                            {
                                collection = flag ? ResourcePropertyKind.ResourceSetReference : ResourcePropertyKind.ResourceReference;
                            }
                        }
                    }
                    if ((collectionResourceType == null) || ((collectionResourceType.ResourceTypeKind == ResourceTypeKind.EntityType) && (parentResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)))
                    {
                        if (collectionResourceType != null)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_ComplexTypeWithNavigationProperty(info.Name, parentResourceType.FullName));
                        }
                        if (flag && (BaseServiceProvider.GetIEnumerableElement(propertyType) != null))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_CollectionOfCollectionProperty(info.Name, parentResourceType.FullName));
                        }
                        if (flag)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_CollectionOfUnsupportedTypeProperty(info.Name, parentResourceType.FullName, propertyType));
                        }
                        if (CommonUtil.IsUnsupportedType(propertyType))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_UnsupportedPropertyType(info.Name, parentResourceType.FullName));
                        }
                        throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_InvalidProperty(info.Name, parentResourceType.FullName));
                    }
                    if ((collectionResourceType.ResourceTypeKind == ResourceTypeKind.EntityType) && (InternalGetContainerForResourceType(propertyType, entitySets) == null))
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_EntityPropertyWithNoEntitySet(parentResourceType.FullName, info.Name));
                    }
                    if (collection == ResourcePropertyKind.Collection)
                    {
                        collectionResourceType = ResourceType.GetCollectionResourceType(collectionResourceType);
                    }
                    if (source.Remove(info.Name))
                    {
                        collection |= ResourcePropertyKind.ETag;
                    }
                    ResourceProperty property = new ResourceProperty(info.Name, collection, collectionResourceType);
                    MimeTypeAttribute mimeTypeAttribute = MimeTypeAttribute.GetMimeTypeAttribute(info);
                    if (mimeTypeAttribute != null)
                    {
                        property.MimeType = mimeTypeAttribute.MimeType;
                    }
                    parentResourceType.AddProperty(property);
                }
            }
            if ((parentResourceType.ResourceTypeKind == ResourceTypeKind.EntityType) && ((parentResourceType.KeyProperties == null) || (parentResourceType.KeyProperties.Count == 0)))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_KeyPropertiesCannotBeIgnored(parentResourceType.FullName));
            }
            if (source.Count != 0)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_ETagPropertyNameNotValid(source.ElementAt<string>(0), parentResourceType.FullName));
            }
        }

        private static ResourceType CreateResourceType(Type type, ResourceTypeKind kind, ResourceType baseType, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes)
        {
            ResourceType type2 = new ResourceType(type, kind, baseType, type.Namespace, CommonUtil.GetModelTypeName(type), type.IsAbstract) {
                IsOpenType = false
            };
            if (type.GetCustomAttributes(typeof(HasStreamAttribute), true).Length == 1)
            {
                type2.IsMediaLinkEntry = true;
            }
            foreach (object obj2 in type.GetCustomAttributes(typeof(NamedStreamAttribute), baseType == null))
            {
                type2.AddProperty(new ResourceProperty(((NamedStreamAttribute) obj2).Name, ResourcePropertyKind.Stream, ResourceType.PrimitiveResourceTypeMap.GetPrimitive(typeof(Stream))));
            }
            knownTypes.Add(type, type2);
            childTypes.Add(type2, null);
            if (baseType != null)
            {
                if (childTypes[baseType] == null)
                {
                    childTypes[baseType] = new List<ResourceType>();
                }
                childTypes[baseType].Add(type2);
            }
            return type2;
        }

        private static bool DoesTypeHaveKeyProperties(Type type, bool entityTypeCandidate)
        {
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                ResourceKeyKind kind;
                if (IsPropertyKeyProperty(info, out kind))
                {
                    if ((kind == ResourceKeyKind.AttributedKey) && !entityTypeCandidate)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_EntityTypeHasKeyButNoEntitySet(type.FullName));
                    }
                    if (!entityTypeCandidate)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public override object GetOpenPropertyValue(object target, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
        {
            throw new NotImplementedException();
        }

        public override ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceSet set;
            ResourceType type;
            string str;
            WebUtil.CheckArgumentNull<ResourceSet>(resourceSet, "resourceSet");
            WebUtil.CheckArgumentNull<ResourceType>(resourceType, "resourceType");
            WebUtil.CheckArgumentNull<ResourceProperty>(resourceProperty, "resourceProperty");
            if (!base.TryResolveResourceSet(resourceSet.Name, out set) || (set != resourceSet))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_UnknownResourceSet(resourceSet.Name));
            }
            if (!base.TryResolveResourceType(resourceType.FullName, out type) || (type != resourceType))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_UnknownResourceType(resourceType.FullName));
            }
            if (resourceType != DataServiceProviderWrapper.GetDeclaringTypeForProperty(resourceType, resourceProperty, null))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_ResourceTypeMustBeDeclaringTypeForProperty(resourceType.FullName, resourceProperty.Name));
            }
            ResourceType type2 = resourceProperty.ResourceType;
            if (type2.ResourceTypeKind != ResourceTypeKind.EntityType)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_PropertyMustBeNavigationPropertyOnType(resourceProperty.Name, resourceType.FullName));
            }
            ResourceSet containerForResourceType = InternalGetContainerForResourceType(type2.InstanceType, base.EntitySets.Values);
            if (base.Types.Count<ResourceType>(rt => (rt.Name == resourceType.Name)) > 1)
            {
                str = resourceType.FullName.Replace('.', '_') + '_' + resourceProperty.Name;
            }
            else
            {
                str = resourceType.Name + '_' + resourceProperty.Name;
            }
            ResourceAssociationSetEnd end = new ResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty);
            return new ResourceAssociationSet(str, end, new ResourceAssociationSetEnd(containerForResourceType, type2, null));
        }

        protected override IQueryable GetResourceContainerInstance(ResourceSet resourceContainer)
        {
            if (resourceContainer.ReadFromContextDelegate == null)
            {
                MethodInfo getMethod = base.Type.GetProperty(resourceContainer.Name, BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                Type[] parameterTypes = new Type[] { typeof(object) };
                DynamicMethod method = new DynamicMethod("queryable_reader", typeof(IQueryable), parameterTypes, false);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, base.Type);
                iLGenerator.Emit(OpCodes.Call, getMethod);
                iLGenerator.Emit(OpCodes.Ret);
                resourceContainer.ReadFromContextDelegate = (Func<object, IQueryable>) method.CreateDelegate(typeof(Func<object, IQueryable>));
            }
            return resourceContainer.ReadFromContextDelegate(base.CurrentDataSource);
        }

        public override bool GetTypeIsOrdered(Type type)
        {
            if (!typeof(IComparable).IsAssignableFrom(type))
            {
                return base.GetTypeIsOrdered(type);
            }
            return true;
        }

        private static bool HasGenericParameters(Type type)
        {
            if (type.IsGenericType)
            {
                foreach (Type type2 in type.GetGenericArguments())
                {
                    if (type2.IsGenericParameter)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override bool ImplementsIUpdatable()
        {
            return typeof(IUpdatable).IsAssignableFrom(base.Type);
        }

        private static ResourceSet InternalGetContainerForResourceType(Type type, IEnumerable<ResourceSet> entitySets)
        {
            foreach (ResourceSet set in entitySets)
            {
                if (set.ResourceType.InstanceType.IsAssignableFrom(type))
                {
                    return set;
                }
            }
            return null;
        }

        private static bool IsComplexType(Type type)
        {
            return ((((type.IsVisible && !type.IsArray) && (!type.IsPointer && !type.IsCOMObject)) && ((!type.IsInterface && !(type == typeof(IntPtr))) && (!(type == typeof(UIntPtr)) && !(type == typeof(char))))) && ((!(type == typeof(TimeSpan)) && !(type == typeof(DateTimeOffset))) && (!(type == typeof(Uri)) && !type.IsEnum)));
        }

        private static ResourceType IsEntityOrComplexType(Type type, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, Queue<ResourceType> unvisitedTypes)
        {
            if (type.IsValueType || CommonUtil.IsUnsupportedType(type))
            {
                return null;
            }
            ResourceType item = BuildHierarchyForEntityType(type, knownTypes, childTypes, unvisitedTypes, false);
            if ((item == null) && IsComplexType(type))
            {
                item = CreateResourceType(type, ResourceTypeKind.ComplexType, null, knownTypes, childTypes);
                unvisitedTypes.Enqueue(item);
            }
            return item;
        }

        internal static bool IsPropertyKeyProperty(PropertyInfo property, out ResourceKeyKind keyKind)
        {
            keyKind = ~ResourceKeyKind.AttributedKey;
            if (WebUtil.IsPrimitiveType(property.PropertyType) && !property.PropertyType.IsGenericType)
            {
                DataServiceKeyAttribute attribute = property.ReflectedType.GetCustomAttributes(true).OfType<DataServiceKeyAttribute>().FirstOrDefault<DataServiceKeyAttribute>();
                if ((attribute != null) && attribute.KeyNames.Contains(property.Name))
                {
                    keyKind = ResourceKeyKind.AttributedKey;
                    return true;
                }
                if (property.Name == (property.DeclaringType.Name + "ID"))
                {
                    keyKind = ResourceKeyKind.TypeNameId;
                    return true;
                }
                if (property.Name == "ID")
                {
                    keyKind = ResourceKeyKind.Id;
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<string> LoadETagProperties(ResourceType resourceType)
        {
            bool inherit = resourceType.BaseType == null;
            ETagAttribute[] customAttributes = (ETagAttribute[]) resourceType.InstanceType.GetCustomAttributes(typeof(ETagAttribute), inherit);
            if (customAttributes.Length == 1)
            {
                return customAttributes[0].PropertyNames;
            }
            return WebUtil.EmptyStringArray;
        }

        protected override void PopulateMetadata(IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IDictionary<string, ResourceSet> entitySets)
        {
            Queue<ResourceType> unvisitedTypes = new Queue<ResourceType>();
            List<string> list = new List<string>(IgnorePropertiesAttribute.GetProperties(base.Type, true, BindingFlags.Public | BindingFlags.Instance));
            foreach (PropertyInfo info in base.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((!list.Contains(info.Name) && info.CanRead) && (info.GetIndexParameters().Length == 0))
                {
                    Type iQueryableElement = BaseServiceProvider.GetIQueryableElement(info.PropertyType);
                    if (iQueryableElement != null)
                    {
                        ResourceType elementType = BuildHierarchyForEntityType(iQueryableElement, knownTypes, childTypes, unvisitedTypes, true);
                        if (elementType == null)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_InvalidEntitySetProperty(info.Name, XmlConvert.EncodeName(this.ContainerName)));
                        }
                        foreach (KeyValuePair<string, ResourceSet> pair in entitySets)
                        {
                            Type instanceType = pair.Value.ResourceType.InstanceType;
                            if (instanceType.IsAssignableFrom(iQueryableElement))
                            {
                                throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_MultipleEntitySetsForSameType(pair.Value.Name, info.Name, instanceType.FullName, elementType.FullName));
                            }
                            if (iQueryableElement.IsAssignableFrom(instanceType))
                            {
                                throw new InvalidOperationException(System.Data.Services.Strings.ReflectionProvider_MultipleEntitySetsForSameType(info.Name, pair.Value.Name, elementType.FullName, instanceType.FullName));
                            }
                        }
                        ResourceSet set = new ResourceSet(info.Name, elementType);
                        entitySets.Add(info.Name, set);
                    }
                }
            }
            PopulateMetadataForTypes(knownTypes, childTypes, unvisitedTypes, entitySets.Values);
            PopulateMetadataForDerivedTypes(knownTypes, childTypes, unvisitedTypes, entitySets.Values);
        }

        private static void PopulateMetadataForDerivedTypes(IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, Queue<ResourceType> unvisitedTypes, IEnumerable<ResourceSet> entitySets)
        {
            List<ResourceType> list = new List<ResourceType>();
            foreach (ResourceSet set in entitySets)
            {
                list.Add(set.ResourceType);
            }
            HashSet<Assembly> set2 = new HashSet<Assembly>(EqualityComparer<Assembly>.Default);
            List<Type> list2 = new List<Type>();
            foreach (ResourceType type in knownTypes.Values)
            {
                if (type.ResourceTypeKind != ResourceTypeKind.Primitive)
                {
                    Assembly item = type.InstanceType.Assembly;
                    if (!set2.Contains(item))
                    {
                        foreach (Type type2 in item.GetTypes())
                        {
                            if ((type2.IsVisible && !HasGenericParameters(type2)) && !knownTypes.ContainsKey(type2))
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i].InstanceType.IsAssignableFrom(type2))
                                    {
                                        list2.Add(type2);
                                    }
                                }
                            }
                        }
                        set2.Add(item);
                    }
                }
            }
            foreach (Type type3 in list2)
            {
                BuildHierarchyForEntityType(type3, knownTypes, childTypes, unvisitedTypes, false);
                PopulateMetadataForTypes(knownTypes, childTypes, unvisitedTypes, entitySets);
            }
        }

        protected override ResourceType PopulateMetadataForType(Type type, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets)
        {
            ResourceType type2;
            Queue<ResourceType> unvisitedTypes = new Queue<ResourceType>();
            if (!BaseServiceProvider.TryGetType(knownTypes, type, out type2))
            {
                type2 = IsEntityOrComplexType(type, knownTypes, childTypes, unvisitedTypes);
                if (type2 != null)
                {
                    PopulateMetadataForTypes(knownTypes, childTypes, unvisitedTypes, entitySets);
                }
            }
            return type2;
        }

        private static void PopulateMetadataForTypes(IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, Queue<ResourceType> unvisitedTypes, IEnumerable<ResourceSet> entitySets)
        {
            while (unvisitedTypes.Count != 0)
            {
                ResourceType parentResourceType = unvisitedTypes.Dequeue();
                BuildTypeProperties(parentResourceType, knownTypes, childTypes, unvisitedTypes, entitySets);
                BuildReflectionEpmInfo(parentResourceType);
            }
        }

        protected override void PopulateMetadataForUserSpecifiedTypes(IEnumerable<Type> userSpecifiedTypes, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets)
        {
            Queue<ResourceType> unvisitedTypes = new Queue<ResourceType>();
            foreach (Type type in userSpecifiedTypes)
            {
                ResourceType type2;
                if (!BaseServiceProvider.TryGetType(knownTypes, type, out type2) && (IsEntityOrComplexType(type, knownTypes, childTypes, unvisitedTypes) == null))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_InvalidTypeSpecified(type.FullName));
                }
            }
            PopulateMetadataForTypes(knownTypes, childTypes, unvisitedTypes, entitySets);
        }

        public override string ContainerName
        {
            get
            {
                return base.Type.Name;
            }
        }

        public override string ContainerNamespace
        {
            get
            {
                return base.Type.Namespace;
            }
        }

        public override bool IsNullPropagationRequired
        {
            get
            {
                return true;
            }
        }
    }
}

