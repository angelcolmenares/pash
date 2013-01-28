using System.Data.Entity;
using System.Data.Entity.Core;

namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [DebuggerDisplay("ObjectContextServiceProvider: {Type}")]
    internal class ObjectContextServiceProvider : BaseServiceProvider, IDataServiceUpdateProvider2, IDataServiceUpdateProvider, IUpdatable
    {
        private readonly List<IDataServiceInvokable> actionsToInvoke;
        private System.Data.Entity.Core.Objects.ObjectContext objectContext;
        private readonly Dictionary<object, object> objectsToBeReplaced;
        private Func<int> saveChangesMethod;
        private static readonly string StoreGeneratedPatternAnnotationName = WebUtil.CreateFullNameForCustomAnnotation("http://schemas.microsoft.com/ado/2009/02/edm/annotation", "StoreGeneratedPattern");
        private List<StructuralType> typesWithoutOSpaceMetadata;

        internal ObjectContextServiceProvider(object dataServiceInstance, object dataSourceInstance) : base(dataServiceInstance, dataSourceInstance)
        {
            this.objectsToBeReplaced = new Dictionary<object, object>(ReferenceEqualityComparer<object>.Instance);
            this.actionsToInvoke = new List<IDataServiceInvokable>();
            this.typesWithoutOSpaceMetadata = new List<StructuralType>();
        }

        public void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            this.UpdateRelationship(targetResource, propertyName, resourceToBeAdded, true);
        }

        private static void ApplyChangesToEntity(System.Data.Entity.Core.Objects.ObjectContext objectContext, object originalTrackedEntity, object newEntity)
        {
            ObjectStateEntry objectStateEntry = objectContext.ObjectStateManager.GetObjectStateEntry(originalTrackedEntity);
            string entitySetName = GetEntitySetName(objectStateEntry, objectContext.DefaultContainerName);
            if (objectStateEntry.State == EntityState.Added)
            {
                objectContext.Detach(originalTrackedEntity);
                objectContext.AddObject(entitySetName, newEntity);
            }
            else
            {
                objectContext.ApplyPropertyChanges(entitySetName, newEntity);
            }
        }

        protected override void CheckConfigurationConsistency(DataServiceConfiguration configuration)
        {
            base.CheckConfigurationConsistency(configuration);
            HashSet<ResourceType> set = new HashSet<ResourceType>(EqualityComparer<ResourceType>.Default);
            foreach (ResourceType type in this.Types)
            {
                foreach (ResourceProperty property in type.PropertiesDeclaredOnThisType)
                {
                    if (property.TypeKind == ResourceTypeKind.EntityType)
                    {
                        set.Add(property.ResourceType);
                    }
                }
            }
            Dictionary<ResourceType, ResourceSet> dictionary = new Dictionary<ResourceType, ResourceSet>(ReferenceEqualityComparer<ResourceType>.Instance);
            foreach (KeyValuePair<string, ResourceSet> pair in base.EntitySets)
            {
                ResourceType resourceType = pair.Value.ResourceType;
                if (set.Contains(resourceType))
                {
                    ResourceSet set2;
                    if (dictionary.TryGetValue(resourceType, out set2))
                    {
                        EntitySetRights resourceSetRights = configuration.GetResourceSetRights(pair.Value);
                        EntitySetRights rights2 = configuration.GetResourceSetRights(set2);
                        if (resourceSetRights != rights2)
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_DifferentContainerRights(set2.Name, rights2, pair.Value.Name, resourceSetRights));
                        }
                    }
                    else
                    {
                        dictionary.Add(resourceType, pair.Value);
                    }
                }
            }
            CheckNavigationPropertiesBound(base.CurrentDataSource);
        }

        private static void CheckNavigationPropertiesBound(object dataSourceInstance)
        {
            MetadataWorkspace metadataWorkspace = DbContextHelper.GetObjectContext(dataSourceInstance).MetadataWorkspace;
            foreach (EntityType type in metadataWorkspace.GetItems<EntityType>(DataSpace.CSpace))
            {
                foreach (NavigationProperty property in type.NavigationProperties)
                {
                    foreach (EntitySet set in GetEntitySetsForType(metadataWorkspace, type))
                    {
                        if (!GetEntitySetsWithAssociationSets(metadataWorkspace, property.RelationshipType, property.FromEndMember).Contains<EntitySet>(set))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_NavigationPropertyUnbound(property.Name, type.FullName, set.Name));
                        }
                    }
                }
            }
        }

        public void ClearChanges()
        {
            foreach (ObjectStateEntry entry in this.ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified | EntityState.Deleted | EntityState.Added | EntityState.Unchanged))
            {
                if (((entry.State != EntityState.Detached) && !entry.IsRelationship) && (entry.Entity != null))
                {
                    this.ObjectContext.Detach(entry.Entity);
                }
            }
            this.objectsToBeReplaced.Clear();
            this.actionsToInvoke.Clear();
        }

        private static object CreateObject(System.Data.Entity.Core.Objects.ObjectContext context, Type clrType)
        {
            return context.GetType().GetMethod("CreateObject", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(new Type[] { clrType }).Invoke(context, null);
        }

        public object CreateResource(string containerName, string fullTypeName)
        {
            ResourceType type;
            WebUtil.CheckStringArgumentNullOrEmpty(fullTypeName, "fullTypeName");
            if (!this.TryResolveResourceType(fullTypeName, out type))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_ResourceTypeNameNotExist(fullTypeName));
            }
            if (type.InstanceType.IsAbstract)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.CannotCreateInstancesOfAbstractType(type.FullName));
            }
            if (containerName != null)
            {
                if (type.ResourceTypeKind != ResourceTypeKind.EntityType)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_EntityTypeExpected(type.FullName, type.ResourceTypeKind));
                }
                object entity = CreateObject(this.ObjectContext, type.InstanceType);
                this.ObjectContext.AddObject(containerName, entity);
                return entity;
            }
            if (type.ResourceTypeKind != ResourceTypeKind.ComplexType)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_ComplexTypeExpected(type.FullName, type.ResourceTypeKind));
            }
            return type.ConstructorDelegate();
        }

        private static ResourceType CreateResourceType(StructuralType cspaceType, Type clrType, ResourceType baseResourceType, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes)
        {
            ResourceTypeKind resourceTypeKind = (cspaceType.BuiltInTypeKind == BuiltInTypeKind.EntityType) ? ResourceTypeKind.EntityType : ResourceTypeKind.ComplexType;
            ResourceType type = new ResourceType(clrType, resourceTypeKind, baseResourceType, cspaceType.NamespaceName, cspaceType.Name, clrType.IsAbstract);
            if (GetEntityTypeDefaultStreamProperty(cspaceType))
            {
                type.IsMediaLinkEntry = true;
            }
            foreach (object obj2 in clrType.GetCustomAttributes(typeof(NamedStreamAttribute), baseResourceType == null))
            {
                type.AddProperty(new ResourceProperty(((NamedStreamAttribute) obj2).Name, ResourcePropertyKind.Stream, ResourceType.PrimitiveResourceTypeMap.GetPrimitive(typeof(Stream))));
            }
            knownTypes.Add(clrType, type);
            childTypes.Add(type, null);
            if (baseResourceType != null)
            {
                if (childTypes[baseResourceType] == null)
                {
                    childTypes[baseResourceType] = new List<ResourceType>();
                }
                childTypes[baseResourceType].Add(type);
            }
            PopulateAnnotations(cspaceType.MetadataProperties, new Action<string, string, object>(type.AddCustomAnnotation));
            return type;
        }

        public void DeleteResource(object resource)
        {
            WebUtil.CheckArgumentNull<object>(resource, "resource");
            this.ObjectContext.DeleteObject(resource);
        }

        internal static Type GetClrTypeForCSpaceType(MetadataWorkspace workspace, StructuralType edmType)
        {
            StructuralType type;
            if (workspace.TryGetObjectSpaceType(edmType, out type))
            {
                ObjectItemCollection itemCollection = (ObjectItemCollection) workspace.GetItemCollection(DataSpace.OSpace);
                return itemCollection.GetClrType(type);
            }
            return null;
        }

        internal IEnumerable<KeyValuePair<string, object>> GetEntityContainerAnnotations(string entityContainerName)
        {
            EntityContainer item = this.ObjectContext.MetadataWorkspace.GetItem<EntityContainer>(entityContainerName, DataSpace.CSpace);
            Dictionary<string, object> customAnnotations = null;
            PopulateAnnotations(item.MetadataProperties, delegate (string namespaceName, string name, object annotation) {
                if (customAnnotations == null)
                {
                    customAnnotations = new Dictionary<string, object>(StringComparer.Ordinal);
                }
                customAnnotations.Add(namespaceName + ":" + name, annotation);
            });
            return (customAnnotations ?? WebUtil.EmptyKeyValuePairStringObject);
        }

        private EntitySet GetEntitySet(string qualifiedEntitySetName)
        {
            string defaultContainerName;
            string str2;
            int length = qualifiedEntitySetName.LastIndexOf('.');
            if (length != -1)
            {
                defaultContainerName = qualifiedEntitySetName.Substring(0, length);
                str2 = qualifiedEntitySetName.Substring(length + 1);
            }
            else
            {
                defaultContainerName = this.ObjectContext.DefaultContainerName;
                str2 = qualifiedEntitySetName;
            }
            return this.ObjectContext.MetadataWorkspace.GetEntityContainer(defaultContainerName, DataSpace.CSpace).GetEntitySetByName(str2, false);
        }

        private static string GetEntitySetName(ObjectStateEntry entry, string defaultContainerName)
        {
            return GetEntitySetName(entry.EntitySet.Name, entry.EntitySet.EntityContainer.Name, entry.EntitySet.EntityContainer.Name == defaultContainerName);
        }

        private static string GetEntitySetName(string entitySetName, string entityContainerName, bool containedInDefaultEntityContainer)
        {
            if (containedInDefaultEntityContainer)
            {
                return entitySetName;
            }
            return (entityContainerName + "." + entitySetName);
        }

        private static IEnumerable<EntitySet> GetEntitySetsForType(MetadataWorkspace workspace, EntityType type)
        {
            foreach (EntityContainer iteratorVariable0 in workspace.GetItems<EntityContainer>(DataSpace.CSpace))
            {
                foreach (EntitySet iteratorVariable1 in iteratorVariable0.BaseEntitySets.OfType<EntitySet>())
                {
                    if (!IsAssignableFrom(iteratorVariable1.ElementType, type))
                    {
                        continue;
                    }
                    yield return iteratorVariable1;
                }
            }
        }

        private static IEnumerable<EntitySet> GetEntitySetsWithAssociationSets(MetadataWorkspace workspace, RelationshipType associationType, RelationshipEndMember member)
        {
            foreach (EntityContainer iteratorVariable0 in workspace.GetItems<EntityContainer>(DataSpace.CSpace))
            {
                foreach (AssociationSet iteratorVariable1 in iteratorVariable0.BaseEntitySets.OfType<AssociationSet>())
                {
                    if (iteratorVariable1.ElementType == associationType)
                    {
                        foreach (AssociationSetEnd iteratorVariable2 in iteratorVariable1.AssociationSetEnds)
                        {
                            if (iteratorVariable2.CorrespondingAssociationEndMember != member)
                            {
                                continue;
                            }
                            yield return iteratorVariable2.EntitySet;
                        }
                    }
                }
            }
        }

        private static bool GetEntityTypeDefaultStreamProperty(StructuralType type)
        {
            MetadataProperty property;
            bool flag = false;
            if (!type.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata:HasStream", false, out property))
            {
                return flag;
            }
            string str = (string) property.Value;
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_HasStreamAttributeEmpty(type.Name));
            }
            if (str != "true")
            {
                throw new NotSupportedException(System.Data.Services.Strings.ObjectContext_UnsupportedStreamProperty(str, type.Name));
            }
            return true;
        }

        private IEnumerable<EntityPropertyMappingAttribute> GetEpmAttrsFromComplexProperty(ResourceProperty complexProperty, string epmSourcePath, string epmTargetPath, string epmNsPrefix, string epmNsUri, bool epmKeepInContent)
        {
            foreach (ResourceProperty iteratorVariable0 in complexProperty.ResourceType.Properties)
            {
                string iteratorVariable1 = epmSourcePath + "/" + iteratorVariable0.Name;
                string iteratorVariable2 = epmTargetPath + "/" + iteratorVariable0.Name;
                if (iteratorVariable0.IsOfKind(ResourcePropertyKind.ComplexType))
                {
                    foreach (EntityPropertyMappingAttribute iteratorVariable3 in this.GetEpmAttrsFromComplexProperty(iteratorVariable0, iteratorVariable1, iteratorVariable2, epmNsPrefix, epmNsUri, epmKeepInContent))
                    {
                        yield return iteratorVariable3;
                    }
                }
                else
                {
                    yield return new EntityPropertyMappingAttribute(iteratorVariable1, iteratorVariable2, epmNsPrefix, epmNsUri, epmKeepInContent);
                }
            }
        }

        private void GetEpmInfoForResourceProperty(EpmHelper.EpmPropertyInformation propertyInformation, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            if (propertyInformation.IsAtom)
            {
                if (resourceProperty.IsOfKind(ResourcePropertyKind.ComplexType))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_SyndicationMappingForComplexPropertiesNotAllowed);
                }
                EntityPropertyMappingAttribute attribute = new EntityPropertyMappingAttribute(propertyInformation.SourcePath, propertyInformation.SyndicationItem, propertyInformation.ContentKind, propertyInformation.KeepInContent);
                resourceType.AddEntityPropertyMappingAttributeInternal(attribute, true);
            }
            else if (resourceProperty.IsOfKind(ResourcePropertyKind.ComplexType))
            {
                foreach (EntityPropertyMappingAttribute attribute2 in this.GetEpmAttrsFromComplexProperty(resourceProperty, propertyInformation.SourcePath, propertyInformation.TargetPath, propertyInformation.NsPrefix, propertyInformation.NsUri, propertyInformation.KeepInContent))
                {
                    resourceType.AddEntityPropertyMappingAttributeInternal(attribute2, true);
                }
            }
            else
            {
                EntityPropertyMappingAttribute attribute3 = new EntityPropertyMappingAttribute(propertyInformation.SourcePath, propertyInformation.TargetPath, propertyInformation.NsPrefix, propertyInformation.NsUri, propertyInformation.KeepInContent);
                resourceType.AddEntityPropertyMappingAttributeInternal(attribute3, true);
            }
        }

        private void GetEpmInfoForResourceType(MetadataWorkspace workspace, ResourceType resourceType)
        {
            if (resourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
            {
                StructuralType edmType = workspace.GetItem<StructuralType>(resourceType.FullName, DataSpace.CSpace);
                foreach (EpmHelper.EpmPropertyInformation information in EpmHelper.GetEpmInformationFromType(edmType))
                {
                    ResourceProperty resourcePropertyFromEpmPath = this.GetResourcePropertyFromEpmPath(resourceType, information.SourcePath);
                    if (resourcePropertyFromEpmPath == null)
                    {
                        throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownPropertyNameInEpmAttributes(information.SourcePath, resourceType.Name));
                    }
                    this.GetEpmInfoForResourceProperty(information, resourceType, resourcePropertyFromEpmPath);
                }
                foreach (EdmMember member in from m in edmType.Members
                    where m.DeclaringType == edmType
                    select m)
                {
                    ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                    ResourceProperty property2 = resourceType.TryResolvePropertiesDeclaredOnThisTypeByName(member.Name, stream);
                    foreach (EpmHelper.EpmPropertyInformation information2 in EpmHelper.GetEpmInformationFromProperty(member))
                    {
                        ResourceProperty resourceProperty = property2;
                        if (property2.IsOfKind(ResourcePropertyKind.ComplexType) && information2.PathGiven)
                        {
                            information2.SourcePath = property2.Name + "/" + information2.SourcePath;
                            resourceProperty = this.GetResourcePropertyFromEpmPath(resourceType, information2.SourcePath);
                        }
                        this.GetEpmInfoForResourceProperty(information2, resourceType, resourceProperty);
                    }
                }
            }
        }

        private static string GetEscapedEntitySetName(string qualifiedEntitySetName)
        {
            int length = qualifiedEntitySetName.LastIndexOf('.');
            if (-1 == length)
            {
                return ("[" + qualifiedEntitySetName + "]");
            }
            return ("[" + qualifiedEntitySetName.Substring(0, length) + "].[" + qualifiedEntitySetName.Substring(length + 1) + "]");
        }

        internal IList<ResourceProperty> GetETagProperties(string containerName, ResourceType resourceType)
        {
            EntitySetBase entitySet = this.GetEntitySet(containerName);
            EntityType item = this.ObjectContext.MetadataWorkspace.GetItem<EntityType>(resourceType.FullName, DataSpace.CSpace);
            List<ResourceProperty> list = new List<ResourceProperty>();
            foreach (EdmMember member in ((EntityConnection) this.ObjectContext.Connection).GetMetadataWorkspace().GetRequiredOriginalValueMembers(entitySet, item))
            {
                ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                ResourceProperty property = resourceType.TryResolvePropertyName(member.Name, stream);
                if (!property.IsOfKind(ResourcePropertyKind.Key))
                {
                    list.Add(property);
                }
            }
            return list;
        }

        private static string GetMultiplicity(RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.One:
                    return "1";

                case RelationshipMultiplicity.Many:
                    return "*";
            }
            return "0..1";
        }

        public override object GetOpenPropertyValue(object target, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
        {
            throw new NotImplementedException();
        }

        public object GetResource(IQueryable query, string fullTypeName)
        {
            WebUtil.CheckArgumentNull<IQueryable>(query, "query");
            ObjectQuery query2 = (ObjectQuery) query;
            query2.MergeOption = MergeOption.AppendOnly;
            object resource = null;
            foreach (object obj3 in (IEnumerable) query2)
            {
                if (resource != null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.SingleResourceExpected);
                }
                resource = obj3;
            }
            if (resource != null)
            {
                ResourceType singleResource = base.GetSingleResource(resource);
                if (singleResource == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownResourceTypeForClrType(resource.GetType().FullName));
                }
                if ((fullTypeName != null) && (singleResource.FullName != fullTypeName))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.TargetElementTypeOfTheUriSpecifiedDoesNotMatchWithTheExpectedType(singleResource.FullName, fullTypeName));
                }
            }
            return resource;
        }

        public override ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            NavigationProperty property;
            WebUtil.CheckArgumentNull<ResourceSet>(resourceSet, "resourceSet");
            WebUtil.CheckArgumentNull<ResourceType>(resourceType, "resourceType");
            WebUtil.CheckArgumentNull<ResourceProperty>(resourceProperty, "resourceProperty");
            EntitySet entitySet = this.GetEntitySet(resourceSet.Name);
            EntityType item = this.ObjectContext.MetadataWorkspace.GetItem<EntityType>(resourceType.FullName, DataSpace.CSpace);
            item.NavigationProperties.TryGetValue(resourceProperty.Name, false, out property);
            if (property == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_PropertyMustBeNavigationPropertyOnType(resourceProperty.Name, resourceType.FullName));
            }
            if ((item != ((EntityType) property.DeclaringType)) || (resourceType != DataServiceProviderWrapper.GetDeclaringTypeForProperty(resourceType, resourceProperty, null)))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_ResourceTypeMustBeDeclaringTypeForProperty(resourceType.FullName, resourceProperty.Name));
            }
            ResourceAssociationSet set2 = null;
            foreach (AssociationSet set3 in entitySet.EntityContainer.BaseEntitySets.OfType<AssociationSet>())
            {
                if (set3.ElementType != property.RelationshipType)
                {
                    continue;
                }
                AssociationSetEnd setEnd = set3.AssociationSetEnds[property.FromEndMember.Name];
                if (setEnd.EntitySet == entitySet)
                {
                    ResourceSet set5;
                    ResourceType type3;
                    ResourceAssociationSetEnd end2 = PopulateResourceAssociationSetEnd(setEnd, resourceSet, resourceType, resourceProperty);
                    ResourceAssociationTypeEnd end3 = PopulateResourceAssociationTypeEnd(setEnd.CorrespondingAssociationEndMember, resourceType, resourceProperty);
                    setEnd = set3.AssociationSetEnds[property.ToEndMember.Name];
                    EntitySet set4 = setEnd.EntitySet;
                    string name = GetEntitySetName(set4.Name, set4.EntityContainer.Name, this.ObjectContext.DefaultContainerName == set4.EntityContainer.Name);
                    this.TryResolveResourceSet(name, out set5);
                    EntityType elementType = (EntityType) ((RefType) property.ToEndMember.TypeUsage.EdmType).ElementType;
                    this.TryResolveResourceType(elementType.FullName, out type3);
                    ResourceProperty property2 = null;
                    foreach (NavigationProperty property3 in elementType.NavigationProperties)
                    {
                        if (property3.ToEndMember == property.FromEndMember)
                        {
                            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                            property2 = type3.TryResolvePropertyName(property3.Name, stream);
                            break;
                        }
                    }
                    ResourceAssociationSetEnd end4 = PopulateResourceAssociationSetEnd(setEnd, set5, type3, ((resourceType == type3) && (resourceProperty == property2)) ? null : property2);
                    ResourceAssociationTypeEnd end5 = PopulateResourceAssociationTypeEnd(setEnd.CorrespondingAssociationEndMember, type3, end4.ResourceProperty);
                    set2 = new ResourceAssociationSet(set3.Name, end2, end4);
                    PopulateAnnotations(set3.MetadataProperties, new Action<string, string, object>(set2.AddCustomAnnotation));
                    set2.ResourceAssociationType = PopulateResourceAssociationType(set3.ElementType, end3, end5);
                    return set2;
                }
            }
            return set2;
        }

        protected override IQueryable GetResourceContainerInstance(ResourceSet resourceContainer)
        {
            ObjectQuery resourceContainerInstance = this.InternalGetResourceContainerInstance(resourceContainer);
            resourceContainerInstance.MergeOption = MergeOption.NoTracking;
            return resourceContainerInstance;
        }

        private ResourceProperty GetResourcePropertyFromEpmPath(ResourceType baseResourceType, string sourcePath)
        {
            string[] strArray = sourcePath.Split(new char[] { '/' });
            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
            if (baseResourceType.TryResolvePropertiesDeclaredOnThisTypeByName(strArray[0], stream) == null)
            {
                if (baseResourceType.BaseType == null)
                {
                    return null;
                }
                return this.GetResourcePropertyFromEpmPath(baseResourceType.BaseType, sourcePath);
            }
            ResourceProperty property = null;
            foreach (string str in strArray)
            {
                ResourcePropertyKind exceptKind = ResourcePropertyKind.Stream;
                property = baseResourceType.TryResolvePropertiesDeclaredOnThisTypeByName(str, exceptKind);
                if (property == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_InaccessiblePropertyOnType(str, baseResourceType.Name));
                }
                baseResourceType = property.ResourceType;
            }
            return property;
        }

        public override object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServiceUpdateProvider))
            {
                return this;
            }
            return base.GetService(serviceType);
        }

        public object GetValue(object targetResource, string propertyName)
        {
            object obj2;
            WebUtil.CheckArgumentNull<object>(targetResource, "targetResource");
            WebUtil.CheckStringArgumentNullOrEmpty(propertyName, "propertyName");
            ResourceType singleResource = base.GetSingleResource(targetResource);
            if (singleResource == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownResourceTypeForClrType(targetResource.GetType().FullName));
            }
            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
            ResourceProperty resourceProperty = singleResource.TryResolvePropertyName(propertyName, stream);
            if (resourceProperty == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_PropertyNotDefinedOnType(singleResource.FullName, propertyName));
            }
            if (resourceProperty.IsOfKind(ResourcePropertyKind.Primitive))
            {
                return this.ObjectContext.ObjectStateManager.GetObjectStateEntry(targetResource).CurrentValues[propertyName];
            }
            if (this.objectsToBeReplaced.TryGetValue(targetResource, out obj2))
            {
                targetResource = obj2;
            }
            return this.GetPropertyValue(targetResource, resourceProperty);
        }

        internal override bool ImplementsIUpdatable()
        {
            return true;
        }

        private static void InitializeObjectItemCollection(System.Data.Entity.Core.Objects.ObjectContext objectContext, Assembly assembly)
        {
            objectContext.MetadataWorkspace.LoadFromAssembly(assembly);
        }

        private ObjectQuery InternalGetResourceContainerInstance(ResourceSet container)
        {
            if (container.ReadFromContextDelegate == null)
            {
                Type[] parameterTypes = new Type[] { typeof(object) };
                string escapedEntitySetName = GetEscapedEntitySetName(container.Name);
                MethodInfo meth = typeof(System.Data.Entity.Core.Objects.ObjectContext).GetMethod("CreateQuery", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(new Type[] { container.ResourceType.InstanceType });
                DynamicMethod method = new DynamicMethod("queryable_reader", typeof(IQueryable), parameterTypes, false);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, typeof(System.Data.Entity.Core.Objects.ObjectContext));
                iLGenerator.Emit(OpCodes.Ldstr, escapedEntitySetName);
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
                iLGenerator.Emit(OpCodes.Newarr, typeof(ObjectParameter));
                iLGenerator.Emit(OpCodes.Call, meth);
                iLGenerator.Emit(OpCodes.Ret);
                container.ReadFromContextDelegate = (Func<object, IQueryable>) method.CreateDelegate(typeof(Func<object, IQueryable>));
            }
            return (ObjectQuery) container.ReadFromContextDelegate(this.ObjectContext);
        }

        private static bool IsAssignableFrom(EntityType baseType, EntityType derivedType)
        {
            while (derivedType != null)
            {
                if (derivedType == baseType)
                {
                    return true;
                }
                derivedType = (EntityType) derivedType.BaseType;
            }
            return false;
        }

        private static bool IsOneToOneFKAssocation(AssociationType association)
        {
            if (!association.IsForeignKey)
            {
                return false;
            }
            return ((association.RelationshipEndMembers[0].RelationshipMultiplicity != RelationshipMultiplicity.Many) && (association.RelationshipEndMembers[1].RelationshipMultiplicity != RelationshipMultiplicity.Many));
        }

        private static void PopulateAnnotations(IEnumerable<MetadataProperty> metadataProperties, Action<string, string, object> addAnnotationMethod)
        {
            foreach (MetadataProperty property in metadataProperties)
            {
                if (property.PropertyKind != PropertyKind.System)
                {
                    int length = property.Name.LastIndexOf(':');
                    string str = property.Name.Substring(length + 1);
                    string str2 = property.Name.Substring(0, length);
                    if (str2 != "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
                    {
                        addAnnotationMethod(str2, str, property.Value);
                    }
                }
            }
        }

        private static void PopulateFacets(ResourceProperty property, IEnumerable<Facet> facets, bool ignoreNullableFacet)
        {
            foreach (Facet facet in facets)
            {
                if (facet.Name == "Nullable")
                {
                    if (!ignoreNullableFacet && !((bool) facet.Value))
                    {
                        property.AddCustomAnnotation(string.Empty, facet.Name, facet.Value);
                    }
                }
                else if (facet.Value != null)
                {
                    property.AddCustomAnnotation(string.Empty, facet.Name, facet.Value);
                }
            }
        }

        internal static void PopulateMemberMetadata(ResourceType resourceType, IProviderMetadata workspace, IDictionary<Type, ResourceType> knownTypes, PrimitiveResourceTypeMap primitiveResourceTypeMap)
        {
            IProviderType providerType = workspace.GetProviderType(resourceType.FullName);
            foreach (IProviderMember member in providerType.Members)
            {
				ResourcePropertyKind resourceSetReference = (ResourcePropertyKind)(-1);
                PropertyInfo info = resourceType.InstanceType.GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
                if (info == null)
                {
                    throw new DataServiceException(500, System.Data.Services.Strings.ObjectContext_PublicPropertyNotDefinedOnType(providerType.Name, member.Name));
                }
                ResourceType propertyResourceType = null;
                BuiltInTypeKind edmTypeKind = member.EdmTypeKind;
                switch (edmTypeKind)
                {
                    case BuiltInTypeKind.CollectionType:
                    {
                        resourceSetReference = ResourcePropertyKind.ResourceSetReference;
                        Type clrType = workspace.GetClrType(member.CollectionItemType);
                        propertyResourceType = knownTypes[clrType];
                        break;
                    }
                    case BuiltInTypeKind.CollectionKind:
                        break;

                    case BuiltInTypeKind.ComplexType:
                        resourceSetReference = ResourcePropertyKind.ComplexType;
                        propertyResourceType = knownTypes[info.PropertyType];
                        break;

                    case BuiltInTypeKind.EntityType:
                        resourceSetReference = ResourcePropertyKind.ResourceReference;
                        propertyResourceType = knownTypes[info.PropertyType];
                        break;

                    default:
                        if (edmTypeKind == BuiltInTypeKind.PrimitiveType)
                        {
                            Type propertyType = info.PropertyType;
                            propertyResourceType = primitiveResourceTypeMap.GetPrimitive(propertyType);
                            if (propertyResourceType == null)
                            {
                                throw new NotSupportedException(System.Data.Services.Strings.ObjectContext_PrimitiveTypeNotSupported(member.Name, providerType.Name, member.EdmTypeName));
                            }
                            if (member.IsKey)
                            {
                                resourceSetReference = ResourcePropertyKind.Key | ResourcePropertyKind.Primitive;
                            }
                            else
                            {
                                resourceSetReference = ResourcePropertyKind.Primitive;
                            }
                        }
                        break;
                }
                ResourceProperty resourceProperty = new ResourceProperty(info.Name, resourceSetReference, propertyResourceType);
                SetMimeTypeForMappedMember(resourceProperty, member);
                resourceType.AddProperty(resourceProperty);
                PopulateFacets(resourceProperty, member.Facets, resourceProperty.TypeKind == ResourceTypeKind.EntityType);
                PopulateAnnotations(member.MetadataProperties, new Action<string, string, object>(resourceProperty.AddCustomAnnotation));
            }
        }

        protected override void PopulateMetadata(IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IDictionary<string, ResourceSet> entitySets)
        {
            InitializeObjectItemCollection(this.ObjectContext, base.Type.Assembly);
            MetadataWorkspace metadataWorkspace = this.ObjectContext.MetadataWorkspace;
            foreach (StructuralType type in metadataWorkspace.GetItems<StructuralType>(DataSpace.CSpace))
            {
                if (((type.BuiltInTypeKind == BuiltInTypeKind.EntityType) || (type.BuiltInTypeKind == BuiltInTypeKind.ComplexType)) && (PopulateTypeMetadata(metadataWorkspace, type, knownTypes, childTypes) == null))
                {
                    this.typesWithoutOSpaceMetadata.Add(type);
                }
            }
            foreach (EntityContainer container in metadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace))
            {
                bool containedInDefaultEntityContainer = container.Name == this.ObjectContext.DefaultContainerName;
                foreach (EntitySetBase base2 in container.BaseEntitySets)
                {
                    if (base2.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
                    {
                        ResourceType type3;
                        EntitySet set = (EntitySet) base2;
                        Type clrTypeForCSpaceType = GetClrTypeForCSpaceType(metadataWorkspace, set.ElementType);
                        if ((clrTypeForCSpaceType == null) || !knownTypes.TryGetValue(clrTypeForCSpaceType, out type3))
                        {
                            throw new InvalidOperationException(System.Data.Services.Strings.ObjectContextServiceProvider_OSpaceTypeNotFound(set.ElementType.FullName));
                        }
                        string name = GetEntitySetName(set.Name, set.EntityContainer.Name, containedInDefaultEntityContainer);
                        ResourceSet set2 = new ResourceSet(name, type3) {
                            EntityContainerName = container.Name
                        };
                        entitySets.Add(name, set2);
                        PopulateAnnotations(base2.MetadataProperties, new Action<string, string, object>(set2.AddCustomAnnotation));
                    }
                }
            }
            foreach (ResourceType type4 in knownTypes.Values)
            {
                if (type4.ResourceTypeKind != ResourceTypeKind.Primitive)
                {
                    PopulateMemberMetadata(type4, new ObjectContextMetadata(metadataWorkspace), knownTypes, ResourceType.PrimitiveResourceTypeMap);
                    this.GetEpmInfoForResourceType(metadataWorkspace, type4);
                }
            }
        }

        protected override ResourceType PopulateMetadataForType(Type type, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets)
        {
            ResourceType type2;
            if (!knownTypes.TryGetValue(type, out type2))
            {
                StructuralType type3;
                StructuralType type4;
                InitializeObjectItemCollection(this.ObjectContext, type.Assembly);
                ObjectItemCollection itemCollection = (ObjectItemCollection) this.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
                if (!itemCollection.TryGetItem<StructuralType>(type.FullName, out type3) || !this.ObjectContext.MetadataWorkspace.TryGetEdmSpaceType(type3, out type4))
                {
                    return type2;
                }
                ResourceType baseResourceType = null;
                if (type4.BaseType != null)
                {
                    baseResourceType = this.PopulateMetadataForType(type.BaseType, knownTypes, childTypes, entitySets);
                }
                type2 = CreateResourceType(type4, type, baseResourceType, knownTypes, childTypes);
                this.typesWithoutOSpaceMetadata.Remove(type4);
            }
            return type2;
        }

        protected override void PopulateMetadataForUserSpecifiedTypes(IEnumerable<Type> userSpecifiedTypes, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes, IEnumerable<ResourceSet> entitySets)
        {
            foreach (Type type in userSpecifiedTypes)
            {
                if (this.PopulateMetadataForType(type, knownTypes, childTypes, entitySets) == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_InvalidTypeSpecified(type.FullName));
                }
            }
            if (this.typesWithoutOSpaceMetadata.Count != 0)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnableToLoadMetadataForType(this.typesWithoutOSpaceMetadata[0].FullName));
            }
            this.typesWithoutOSpaceMetadata = null;
        }

        private static void PopulateReferentialConstraint(ResourceAssociationType resourceAssociationType, ReferentialConstraint referentialConstraint)
        {
            ResourceAssociationTypeEnd principalEnd = resourceAssociationType.GetEnd(referentialConstraint.FromRole.Name);
            ResourceAssociationTypeEnd end = resourceAssociationType.GetEnd(referentialConstraint.ToRole.Name);
            List<ResourceProperty> dependentProperties = new List<ResourceProperty>();
            foreach (EdmProperty property in referentialConstraint.ToProperties)
            {
                dependentProperties.Add(end.ResourceType.TryResolvePropertyName(property.Name));
            }
            resourceAssociationType.ReferentialConstraint = new ResourceReferentialConstraint(principalEnd, dependentProperties);
            PopulateAnnotations(referentialConstraint.MetadataProperties, new Action<string, string, object>(resourceAssociationType.ReferentialConstraint.AddCustomAnnotation));
        }

        private static ResourceAssociationSetEnd PopulateResourceAssociationSetEnd(AssociationSetEnd setEnd, ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceAssociationSetEnd end = new ResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty) {
                Name = setEnd.Name
            };
            PopulateAnnotations(setEnd.MetadataProperties, new Action<string, string, object>(end.AddCustomAnnotation));
            return end;
        }

        private static ResourceAssociationType PopulateResourceAssociationType(AssociationType associationType, ResourceAssociationTypeEnd end1, ResourceAssociationTypeEnd end2)
        {
            ResourceAssociationType resourceAssociationType = new ResourceAssociationType(associationType.Name, associationType.NamespaceName, end1, end2);
            PopulateAnnotations(associationType.MetadataProperties, new Action<string, string, object>(resourceAssociationType.AddCustomAnnotation));
            if ((associationType.ReferentialConstraints != null) && (associationType.ReferentialConstraints.Count != 0))
            {
                PopulateReferentialConstraint(resourceAssociationType, associationType.ReferentialConstraints[0]);
            }
            return resourceAssociationType;
        }

        private static ResourceAssociationTypeEnd PopulateResourceAssociationTypeEnd(AssociationEndMember end, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceAssociationTypeEnd end2 = new ResourceAssociationTypeEnd(end.Name, resourceType, resourceProperty, GetMultiplicity(end.RelationshipMultiplicity), (EdmOnDeleteAction) end.DeleteBehavior);
            PopulateAnnotations(end.MetadataProperties, new Action<string, string, object>(end2.AddCustomAnnotation));
            return end2;
        }

        private static ResourceType PopulateTypeMetadata(MetadataWorkspace workspace, StructuralType edmType, IDictionary<Type, ResourceType> knownTypes, IDictionary<ResourceType, List<ResourceType>> childTypes)
        {
            ResourceType type = null;
            Type clrTypeForCSpaceType = GetClrTypeForCSpaceType(workspace, edmType);
            if ((clrTypeForCSpaceType == null) || knownTypes.TryGetValue(clrTypeForCSpaceType, out type))
            {
                return type;
            }
            ResourceType baseResourceType = null;
            if (edmType.BaseType != null)
            {
                baseResourceType = PopulateTypeMetadata(workspace, (StructuralType) edmType.BaseType, knownTypes, childTypes);
            }
            return CreateResourceType(edmType, clrTypeForCSpaceType, baseResourceType, knownTypes, childTypes);
        }

        private static bool PropertyIsNotNullable(ResourceProperty property)
        {
            bool flag;
            return (TryGetCustomAttributeValue<bool>(property, "Nullable", out flag) && !flag);
        }

        private static bool PropertyIsStoreGenerated(ResourceProperty property)
        {
            string str;
            if (!TryGetCustomAttributeValue<string>(property, StoreGeneratedPatternAnnotationName, out str))
            {
                return false;
            }
            if (!("Identity" == str))
            {
                return ("Computed" == str);
            }
            return true;
        }

        public void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            this.UpdateRelationship(targetResource, propertyName, resourceToBeRemoved, false);
        }

        public object ResetResource(object resource)
        {
            WebUtil.CheckArgumentNull<object>(resource, "resource");
            ResourceType singleResource = base.GetSingleResource(resource);
            if (singleResource == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownResourceTypeForClrType(resource.GetType().FullName));
            }
            if (singleResource.ResourceTypeKind == ResourceTypeKind.EntityType)
            {
                object instance = CreateObject(this.ObjectContext, singleResource.InstanceType);
                foreach (ResourceProperty property in singleResource.KeyProperties)
                {
                    object propertyValue = this.GetPropertyValue(resource, property);
                    singleResource.SetValue(instance, propertyValue, property);
                }
                this.objectsToBeReplaced.Add(resource, instance);
                ObjectStateEntry objectStateEntry = this.ObjectContext.ObjectStateManager.GetObjectStateEntry(resource);
                if (objectStateEntry.State != EntityState.Added)
                {
                    return resource;
                }
                this.ObjectContext.AddObject(GetEntitySetName(objectStateEntry, this.ObjectContext.DefaultContainerName), instance);
                return instance;
            }
            if (singleResource.ResourceTypeKind == ResourceTypeKind.ComplexType)
            {
                return singleResource.ConstructorDelegate();
            }
            return resource;
        }

        protected override ResourceType ResolveNonPrimitiveType(Type type)
        {
            Type objectType = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(type);
            return base.ResolveNonPrimitiveType(objectType);
        }

        public object ResolveResource(object resource)
        {
            object obj2;
            WebUtil.CheckArgumentNull<object>(resource, "resource");
            if (this.objectsToBeReplaced.TryGetValue(resource, out obj2))
            {
                ApplyChangesToEntity(this.ObjectContext, resource, obj2);
                this.objectsToBeReplaced.Remove(resource);
            }
            return resource;
        }

        public void SaveChanges()
        {
            foreach (IDataServiceInvokable invokable in this.actionsToInvoke)
            {
                invokable.Invoke();
            }
            foreach (KeyValuePair<object, object> pair in this.objectsToBeReplaced)
            {
                ApplyChangesToEntity(this.ObjectContext, pair.Key, pair.Value);
            }
            this.objectsToBeReplaced.Clear();
            this.actionsToInvoke.Clear();
            try
            {
                this.SaveContextChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch, exception);
            }
            catch (Exception exception2)
            {
                if (DbContextHelper.IsDbEntityValidationException(exception2))
                {
                    throw DbContextHelper.WrapDbEntityValidationException(exception2);
                }
                throw;
            }
        }

        private void SaveContextChanges()
        {
            if (this.saveChangesMethod == null)
            {
                DbContextHelper.GetObjectContext(base.CurrentDataSource, out this.objectContext, out this.saveChangesMethod);
            }
            this.saveChangesMethod();
        }

        public void ScheduleInvokable(IDataServiceInvokable invokable)
        {
            WebUtil.CheckArgumentNull<IDataServiceInvokable>(invokable, "invokable");
            this.actionsToInvoke.Add(invokable);
        }

        public void SetConcurrencyValues(object resource, bool? checkForEquality, IEnumerable<KeyValuePair<string, object>> concurrencyValues)
        {
            WebUtil.CheckArgumentNull<object>(resource, "resource");
            WebUtil.CheckArgumentNull<IEnumerable<KeyValuePair<string, object>>>(concurrencyValues, "concurrencyValues");
            if (!checkForEquality.HasValue)
            {
                ResourceType resourceType = base.GetResourceType(resource);
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_CannotPerformOperationWithoutETag((resourceType == null) ? resource.GetType().FullName : resourceType.FullName));
            }
            if (!checkForEquality.Value)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.ObjectContext_IfNoneMatchHeaderNotSupportedInUpdateAndDelete);
            }
            OriginalValueRecord updatableOriginalValues = this.ObjectContext.ObjectStateManager.GetObjectStateEntry(resource).GetUpdatableOriginalValues();
            foreach (KeyValuePair<string, object> pair in concurrencyValues)
            {
                int ordinal = updatableOriginalValues.GetOrdinal(pair.Key);
                updatableOriginalValues.SetValue(ordinal, pair.Value);
            }
        }

        private static void SetMimeTypeForMappedMember(ResourceProperty resourceProperty, IProviderMember csdlMember)
        {
            string mimeType = csdlMember.MimeType;
            if (mimeType != null)
            {
                resourceProperty.MimeType = mimeType;
            }
        }

        public void SetReference(object targetResource, string propertyName, object propertyValue)
        {
            this.UpdateRelationship(targetResource, propertyName, propertyValue, null);
        }

        public void SetValue(object targetResource, string propertyName, object propertyValue)
        {
            object obj2;
            WebUtil.CheckArgumentNull<object>(targetResource, "targetResource");
            WebUtil.CheckStringArgumentNullOrEmpty(propertyName, "propertyName");
            ResourceType singleResource = base.GetSingleResource(targetResource);
            if (singleResource == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownResourceTypeForClrType(targetResource.GetType().FullName));
            }
            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
            ResourceProperty resourceProperty = singleResource.TryResolvePropertyName(propertyName, stream);
            if (resourceProperty == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_PropertyNotDefinedOnType(singleResource.FullName, propertyName));
            }
            if (this.objectsToBeReplaced.TryGetValue(targetResource, out obj2))
            {
                singleResource.SetValue(obj2, propertyValue, resourceProperty);
            }
            else
            {
                if ((singleResource.ResourceTypeKind == ResourceTypeKind.EntityType) && resourceProperty.IsOfKind(ResourcePropertyKind.Primitive))
                {
                    if (((propertyValue == null) && PropertyIsNotNullable(resourceProperty)) && PropertyIsStoreGenerated(resourceProperty))
                    {
                        return;
                    }
                    try
                    {
                        ObjectStateEntry objectStateEntry = this.ObjectContext.ObjectStateManager.GetObjectStateEntry(targetResource);
                        int ordinal = objectStateEntry.CurrentValues.GetOrdinal(propertyName);
                        objectStateEntry.CurrentValues.SetValue(ordinal, propertyValue);
                        return;
                    }
                    catch (InvalidOperationException exception)
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ErrorInSettingPropertyValue(resourceProperty.Name), exception);
                    }
                }
                singleResource.SetValue(targetResource, propertyValue, resourceProperty);
            }
        }

        private static bool TryGetCustomAttributeValue<TValue>(ResourceProperty property, string customAttributeName, out TValue value)
        {
            foreach (KeyValuePair<string, object> pair in property.CustomAnnotations)
            {
                if ((pair.Key == customAttributeName) && (pair.Value is TValue))
                {
                    value = (TValue) pair.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        private void UpdateRelationship(object targetResource, string propertyName, object propertyValue, bool? addRelationship)
        {
            WebUtil.CheckArgumentNull<object>(targetResource, "targetResource");
            WebUtil.CheckStringArgumentNullOrEmpty(propertyName, "propertyName");
            ResourceType singleResource = base.GetSingleResource(targetResource);
            if (singleResource == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_UnknownResourceTypeForClrType(targetResource.GetType().FullName));
            }
            ResourcePropertyKind stream = ResourcePropertyKind.Stream;
            ResourceProperty property = singleResource.TryResolvePropertyName(propertyName, stream);
            if (property == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_PropertyNotDefinedOnType(singleResource.FullName, propertyName));
            }
            if (property.TypeKind != ResourceTypeKind.EntityType)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ObjectContext_PropertyMustBeNavigationProperty(propertyName, singleResource.FullName));
            }
            NavigationProperty property2 = this.ObjectContext.MetadataWorkspace.GetItem<EntityType>(singleResource.FullName, (DataSpace)1).NavigationProperties[propertyName];
            ObjectStateEntry objectStateEntry = this.ObjectContext.ObjectStateManager.GetObjectStateEntry(targetResource);
            IRelatedEnd relatedEnd = objectStateEntry.RelationshipManager.GetRelatedEnd(property2.RelationshipType.Name, property2.ToEndMember.Name);
            try
            {
                if (property.Kind == ResourcePropertyKind.ResourceReference)
                {
                    if ((IsOneToOneFKAssocation((AssociationType) property2.RelationshipType) && (objectStateEntry.State != EntityState.Added)) && !relatedEnd.IsLoaded)
                    {
                        relatedEnd.Load();
                    }
                    EntityReference reference = relatedEnd as EntityReference;
                    if (propertyValue == null)
                    {
                        reference.EntityKey = null;
                    }
                    else
                    {
                        EntityKey entityKey = reference.EntityKey;
                        if ((entityKey == null) || !entityKey.Equals(this.ObjectContext.ObjectStateManager.GetObjectStateEntry(propertyValue).EntityKey))
                        {
                            reference.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetSetMethod().Invoke(reference, new object[] { propertyValue });
                        }
                    }
                }
                else if (addRelationship == true)
                {
                    relatedEnd.Add(propertyValue);
                }
                else
                {
                    relatedEnd.Attach(propertyValue);
                    relatedEnd.Remove(propertyValue);
                }
            }
            catch (InvalidOperationException exception)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ErrorInSettingPropertyValue(propertyName), exception);
            }
            catch (ArgumentException exception2)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ErrorInSettingPropertyValue(propertyName), exception2);
            }
            catch (TargetInvocationException exception3)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_ErrorInSettingPropertyValue(propertyName), exception3.InnerException ?? exception3);
            }
        }

        public override string ContainerName
        {
            get
            {
                string defaultContainerName = this.ObjectContext.DefaultContainerName;
                if (string.IsNullOrEmpty(defaultContainerName))
                {
                    ResourceSet nset = base.ResourceSets.FirstOrDefault<ResourceSet>(set => !string.IsNullOrEmpty(set.EntityContainerName));
                    if (nset != null)
                    {
                        defaultContainerName = nset.EntityContainerName;
                    }
                }
                return defaultContainerName;
            }
        }

        public override string ContainerNamespace
        {
            get
            {
                return base.Type.Namespace;
            }
        }

        internal MetadataEdmSchemaVersion EdmSchemaVersion
        {
            get
            {
                switch (((EdmItemCollection) this.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSpace)).EdmVersion.ToString ())
                {
                    case "1.0":
                        return MetadataEdmSchemaVersion.Version1Dot0;

                    case "2.0":
                        return MetadataEdmSchemaVersion.Version2Dot0;
                }
                return MetadataEdmSchemaVersion.Version3Dot0;
            }
        }

        public override bool IsNullPropagationRequired
        {
            get
            {
                return false;
            }
        }

        private System.Data.Entity.Core.Objects.ObjectContext ObjectContext
        {
            get
            {
                if (this.objectContext == null)
                {
                    DbContextHelper.GetObjectContext(base.CurrentDataSource, out this.objectContext, out this.saveChangesMethod);
                }
                return this.objectContext;
            }
        }

        
    }
}

