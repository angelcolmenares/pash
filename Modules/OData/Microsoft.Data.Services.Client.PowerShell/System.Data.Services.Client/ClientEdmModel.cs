namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Client.Providers;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ClientEdmModel : EdmElement, IEdmModel, IEdmElement
    {
        private readonly Dictionary<Type, EdmTypeCacheValue> clrToEdmTypeCache = new Dictionary<Type, EdmTypeCacheValue>(EqualityComparer<Type>.Default);
        private readonly IEnumerable<IEdmModel> coreModel = new IEdmModel[] { EdmCoreModel.Instance };
        private readonly EdmDirectValueAnnotationsManager directValueAnnotationsManager = new EdmDirectValueAnnotationsManager();
        private static readonly Dictionary<DataServiceProtocolVersion, ClientEdmModel> modelCache = new Dictionary<DataServiceProtocolVersion, ClientEdmModel>(EqualityComparer<DataServiceProtocolVersion>.Default);
        private readonly DataServiceProtocolVersion protocolVersion;
        private readonly Dictionary<string, ClientTypeAnnotation> typeNameToClientTypeAnnotationCache = new Dictionary<string, ClientTypeAnnotation>(StringComparer.Ordinal);

        static ClientEdmModel()
        {
            foreach (DataServiceProtocolVersion version in Enum.GetValues(typeof(DataServiceProtocolVersion)).Cast<DataServiceProtocolVersion>())
            {
                ClientEdmModel model = new ClientEdmModel(version);
                model.SetEdmVersion(version.ToVersion());
                modelCache.Add(version, model);
            }
        }

        private ClientEdmModel(DataServiceProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
        }

        private IEdmProperty CreateEdmProperty(IEdmStructuredType declaringType, PropertyInfo propertyInfo)
        {
            IEdmProperty property;
            IEdmType edmType = this.GetOrCreateEdmTypeInternal(propertyInfo.PropertyType).EdmType;
            bool isNullable = ClientTypeUtil.CanAssignNull(propertyInfo.PropertyType);
            if ((edmType.TypeKind == EdmTypeKind.Entity) || ((edmType.TypeKind == EdmTypeKind.Collection) && (((IEdmCollectionType) edmType).ElementType.TypeKind() == EdmTypeKind.Entity)))
            {
                IEdmEntityType type2 = declaringType as IEdmEntityType;
                if (type2 == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientTypeCache_NonEntityTypeCannotContainEntityProperties(propertyInfo.Name, propertyInfo.DeclaringType.ToString()));
                }
                property = EdmNavigationProperty.CreateNavigationPropertyWithPartner(propertyInfo.Name, edmType.ToEdmTypeReference(isNullable), null, false, EdmOnDeleteAction.None, "Partner", type2.ToEdmTypeReference(true), null, false, EdmOnDeleteAction.None);
            }
            else
            {
                property = new EdmStructuralProperty(declaringType, propertyInfo.Name, edmType.ToEdmTypeReference(isNullable));
            }
            property.SetClientPropertyAnnotation(new ClientPropertyAnnotation(property, propertyInfo, this.protocolVersion));
            return property;
        }

        public IEdmEntityContainer FindDeclaredEntityContainer(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
        {
            throw new NotImplementedException();
        }

        public IEdmSchemaType FindDeclaredType(string qualifiedName)
        {
            return (IEdmSchemaType) this.typeNameToClientTypeAnnotationCache[qualifiedName].EdmType;
        }

        public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
        {
            return null;
        }

        public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
        {
            return Enumerable.Empty<IEdmVocabularyAnnotation>();
        }

        public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType type)
        {
            throw new NotImplementedException();
        }

        internal ClientTypeAnnotation GetClientTypeAnnotation(string edmTypeName)
        {
            IEdmType edmType = this.clrToEdmTypeCache.Values.First<EdmTypeCacheValue>(e => (e.EdmType.FullName() == edmTypeName)).EdmType;
            return this.GetClientTypeAnnotation(edmType);
        }

        internal static ClientEdmModel GetModel(DataServiceProtocolVersion dataServiceProtocolVersion)
        {
            return modelCache[dataServiceProtocolVersion];
        }

        private ClientTypeAnnotation GetOrCreateClientTypeAnnotation(IEdmType edmType, Type type)
        {
            string key = type.ToString();
            if ((edmType.TypeKind == EdmTypeKind.Complex) || (edmType.TypeKind == EdmTypeKind.Entity))
            {
                lock (this.typeNameToClientTypeAnnotationCache)
                {
                    ClientTypeAnnotation annotation;
                    if (this.typeNameToClientTypeAnnotationCache.TryGetValue(key, out annotation) && (annotation.ElementType != type))
                    {
                        key = type.AssemblyQualifiedName;
                        if (this.typeNameToClientTypeAnnotationCache.TryGetValue(key, out annotation) && (annotation.ElementType != type))
                        {
                            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MultipleTypesWithSameName(key));
                        }
                    }
                    if (annotation == null)
                    {
                        annotation = new ClientTypeAnnotation(edmType, type, key, this.protocolVersion);
                        this.typeNameToClientTypeAnnotationCache.Add(key, annotation);
                    }
                    return annotation;
                }
            }
            return new ClientTypeAnnotation(edmType, type, key, this.protocolVersion);
        }

        internal IEdmType GetOrCreateEdmType(Type type)
        {
            EdmTypeCacheValue orCreateEdmTypeInternal = this.GetOrCreateEdmTypeInternal(type);
            IEdmType edmType = orCreateEdmTypeInternal.EdmType;
            if ((edmType.TypeKind == EdmTypeKind.Complex) || (edmType.TypeKind == EdmTypeKind.Entity))
            {
                bool? hasProperties = orCreateEdmTypeInternal.HasProperties;
                if (!hasProperties.HasValue)
                {
                    hasProperties = new bool?(ClientTypeUtil.GetPropertiesOnType(type, false).Any<PropertyInfo>());
                    lock (this.clrToEdmTypeCache)
                    {
                        EdmTypeCacheValue value3 = this.clrToEdmTypeCache[type];
                        value3.HasProperties = hasProperties;
                    }
                }
                if (hasProperties == false)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_NoSettableFields(type.ToString()));
                }
            }
            return edmType;
        }

        private EdmTypeCacheValue GetOrCreateEdmTypeInternal(Type type)
        {
            EdmTypeCacheValue value2;
            lock (this.clrToEdmTypeCache)
            {
                this.clrToEdmTypeCache.TryGetValue(type, out value2);
            }
            if (value2 == null)
            {
                PropertyInfo[] emptyPropertyInfoArray;
                bool flag;
                if (PrimitiveType.IsKnownNullableType(type))
                {
                    return this.GetOrCreateEdmTypeInternal(null, type, ClientTypeUtil.EmptyPropertyInfoArray, false, false);
                }
                Type[] typeArray = GetTypeHierarchy(type, out emptyPropertyInfoArray, out flag);
                bool isEntity = emptyPropertyInfoArray != null;
                emptyPropertyInfoArray = emptyPropertyInfoArray ?? ClientTypeUtil.EmptyPropertyInfoArray;
                foreach (Type type2 in typeArray)
                {
                    IEdmStructuredType edmBaseType = (value2 == null) ? null : (value2.EdmType as IEdmStructuredType);
                    value2 = this.GetOrCreateEdmTypeInternal(edmBaseType, type2, emptyPropertyInfoArray, isEntity, (type2 == type) ? new bool?(flag) : null);
                    emptyPropertyInfoArray = ClientTypeUtil.EmptyPropertyInfoArray;
                }
            }
            return value2;
        }

        private EdmTypeCacheValue GetOrCreateEdmTypeInternal(IEdmStructuredType edmBaseType, Type type, PropertyInfo[] keyProperties, bool isEntity, bool? hasProperties)
        {
            EdmTypeCacheValue value2;
            Action<MetadataProviderEdmEntityType> action3 = null;
            Action<MetadataProviderEdmComplexType> action4 = null;
            lock (this.clrToEdmTypeCache)
            {
                this.clrToEdmTypeCache.TryGetValue(type, out value2);
            }
            if (value2 == null)
            {
                if (PrimitiveType.IsKnownNullableType(type))
                {
                    PrimitiveType type3;
                    PrimitiveType.TryGetPrimitiveType(type, out type3);
                    value2 = new EdmTypeCacheValue(type3.CreateEdmPrimitiveType(), hasProperties);
                }
                else
                {
                    Type type2;
                    if (((type2 = ClientTypeUtil.GetImplementationType(type, typeof(ICollection<>))) != null) && (ClientTypeUtil.GetImplementationType(type, typeof(IDictionary<,>)) == null))
                    {
                        Type type4 = type2.GetGenericArguments()[0];
                        value2 = new EdmTypeCacheValue(new EdmCollectionType(this.GetOrCreateEdmTypeInternal(type4).EdmType.ToEdmTypeReference(ClientTypeUtil.CanAssignNull(type4))), hasProperties);
                    }
                    else if (isEntity)
                    {
                        if (action3 == null)
                        {
                            action3 = delegate (MetadataProviderEdmEntityType entityType) {
                                List<IEdmProperty> list = new List<IEdmProperty>();
                                List<IEdmStructuralProperty> list1 = new List<IEdmStructuralProperty>();
                                using (IEnumerator<PropertyInfo> enumerator = (from p in ClientTypeUtil.GetPropertiesOnType(type, edmBaseType != null)
                                    orderby p.Name
                                    select p).GetEnumerator())
                                {
                                    Func<PropertyInfo, bool> predicate = null;
                                    while (enumerator.MoveNext())
                                    {
                                        PropertyInfo property = enumerator.Current;
                                        IEdmProperty item = this.CreateEdmProperty(entityType, property);
                                        list.Add(item);
                                        if (edmBaseType == null)
                                        {
                                            if (predicate == null)
                                            {
                                                predicate = k => (k.DeclaringType == type) && (k.Name == property.Name);
                                            }
                                            if (keyProperties.Any<PropertyInfo>(predicate))
                                            {
                                                list1.Add((IEdmStructuralProperty) item);
                                            }
                                        }
                                    }
                                }
                                foreach (IEdmProperty property2 in list)
                                {
                                    entityType.AddProperty(property2);
                                }
                                entityType.AddKeys(list1);
                            };
                        }
                        Action<MetadataProviderEdmEntityType> propertyLoadAction = action3;
                        value2 = new EdmTypeCacheValue(new MetadataProviderEdmEntityType(CommonUtil.GetModelTypeNamespace(type), CommonUtil.GetModelTypeName(type), (IEdmEntityType) edmBaseType, type.IsAbstract(), false, propertyLoadAction), hasProperties);
                    }
                    else
                    {
                        if (action4 == null)
                        {
                            action4 = delegate (MetadataProviderEdmComplexType complexType) {
                                List<IEdmProperty> list = new List<IEdmProperty>();
                                foreach (PropertyInfo info in from p in ClientTypeUtil.GetPropertiesOnType(type, edmBaseType != null)
                                    orderby p.Name
                                    select p)
                                {
                                    IEdmProperty item = this.CreateEdmProperty(complexType, info);
                                    list.Add(item);
                                }
                                foreach (IEdmProperty property2 in list)
                                {
                                    complexType.AddProperty(property2);
                                }
                            };
                        }
                        Action<MetadataProviderEdmComplexType> action2 = action4;
                        value2 = new EdmTypeCacheValue(new MetadataProviderEdmComplexType(CommonUtil.GetModelTypeNamespace(type), CommonUtil.GetModelTypeName(type), (IEdmComplexType) edmBaseType, type.IsAbstract(), action2), hasProperties);
                    }
                }
                IEdmType edmType = value2.EdmType;
                ClientTypeAnnotation orCreateClientTypeAnnotation = this.GetOrCreateClientTypeAnnotation(edmType, type);
                edmType.SetClientTypeAnnotation(orCreateClientTypeAnnotation);
                if ((edmType.TypeKind == EdmTypeKind.Entity) || (edmType.TypeKind == EdmTypeKind.Complex))
                {
                    IEdmStructuredType edmStructuredType = edmType as IEdmStructuredType;
                    this.SetMimeTypeForProperties(edmStructuredType);
                }
                lock (this.clrToEdmTypeCache)
                {
                    EdmTypeCacheValue value3;
                    if (this.clrToEdmTypeCache.TryGetValue(type, out value3))
                    {
                        return value3;
                    }
                    this.clrToEdmTypeCache.Add(type, value2);
                }
            }
            return value2;
        }

        private static Type[] GetTypeHierarchy(Type type, out PropertyInfo[] keyProperties, out bool hasProperties)
        {
            keyProperties = ClientTypeUtil.GetKeyPropertiesOnType(type, out hasProperties);
            List<Type> list = new List<Type>();
            if (keyProperties != null)
            {
                Type declaringType;
                if (keyProperties.Length > 0)
                {
                    declaringType = keyProperties[0].DeclaringType;
                }
                else
                {
                    declaringType = type;
                    while (!declaringType.GetCustomAttributes(false).OfType<DataServiceEntityAttribute>().Any<DataServiceEntityAttribute>() && (declaringType.GetBaseType() != null))
                    {
                        declaringType = declaringType.GetBaseType();
                    }
                }
                do
                {
                    list.Insert(0, type);
                }
                while ((type != declaringType) && ((type = type.GetBaseType()) != null));
            }
            else
            {
                do
                {
                    list.Insert(0, type);
                }
                while (((type = type.GetBaseType()) != null) && ClientTypeUtil.GetPropertiesOnType(type, false).Any<PropertyInfo>());
            }
            return list.ToArray();
        }

        private void SetMimeTypeForProperties(IEdmStructuredType edmStructuredType)
        {
            Func<IEdmProperty, bool> predicate = null;
            Func<IEdmProperty, bool> func2 = null;
            MimeTypePropertyAttribute attribute = (MimeTypePropertyAttribute) this.GetClientTypeAnnotation(edmStructuredType).ElementType.GetCustomAttributes(typeof(MimeTypePropertyAttribute), true).SingleOrDefault<object>();
            if (attribute != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.Name == attribute.DataPropertyName;
                }
                IEdmProperty edmProperty = edmStructuredType.Properties().SingleOrDefault<IEdmProperty>(predicate);
                if (edmProperty == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingMimeTypeDataProperty(this.GetClientTypeAnnotation(edmStructuredType).ElementTypeName, attribute.DataPropertyName));
                }
                if (func2 == null)
                {
                    func2 = p => p.Name == attribute.MimeTypePropertyName;
                }
                IEdmProperty property2 = edmStructuredType.Properties().SingleOrDefault<IEdmProperty>(func2);
                if (property2 == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingMimeTypeProperty(this.GetClientTypeAnnotation(edmStructuredType).ElementTypeName, attribute.MimeTypePropertyName));
                }
                this.GetClientPropertyAnnotation(edmProperty).MimeTypeProperty = this.GetClientPropertyAnnotation(property2);
            }
        }

        public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
        {
            get
            {
                return this.directValueAnnotationsManager;
            }
        }

        public IEnumerable<IEdmModel> ReferencedModels
        {
            get
            {
                return this.coreModel;
            }
        }

        public IEnumerable<IEdmSchemaElement> SchemaElements
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
        {
            get
            {
                return Enumerable.Empty<IEdmVocabularyAnnotation>();
            }
        }

        private sealed class EdmTypeCacheValue
        {
            private readonly IEdmType edmType;
            private bool? hasProperties;

            public EdmTypeCacheValue(IEdmType edmType, bool? hasProperties)
            {
                this.edmType = edmType;
                this.hasProperties = hasProperties;
            }

            public IEdmType EdmType
            {
                get
                {
                    return this.edmType;
                }
            }

            public bool? HasProperties
            {
                get
                {
                    return this.hasProperties;
                }
                set
                {
                    this.hasProperties = value;
                }
            }
        }
    }
}

