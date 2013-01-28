namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class EpmSourceTree
    {
        private readonly EpmTargetTree epmTargetTree;
        private readonly EpmSourcePathSegment root = new EpmSourcePathSegment();

        internal EpmSourceTree(EpmTargetTree epmTargetTree)
        {
            this.epmTargetTree = epmTargetTree;
        }

        internal void Add(EntityPropertyMappingInfo epmInfo, IEnumerable<ResourceProperty> declaredProperties)
        {
            Dictionary<ResourceType, IEnumerable<ResourceProperty>> declaredPropertiesLookup = new Dictionary<ResourceType, IEnumerable<ResourceProperty>>(EqualityComparer<ResourceType>.Default);
            declaredPropertiesLookup.Add(epmInfo.ActualPropertyType, declaredProperties);
            EpmSourcePathSegment root = this.Root;
            EpmSourcePathSegment segment2 = null;
            ResourceType actualPropertyType = epmInfo.ActualPropertyType;
            for (int i = 0; i < epmInfo.PropertyValuePath.Length; i++)
            {
                string propertyName = epmInfo.PropertyValuePath[i];
                if (propertyName.Length == 0)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_InvalidSourcePath(epmInfo.DefiningType.Name, epmInfo.Attribute.SourcePath));
                }
                actualPropertyType = GetPropertyType(actualPropertyType, propertyName, declaredPropertiesLookup);
                segment2 = root.SubProperties.SingleOrDefault<EpmSourcePathSegment>(e => e.PropertyName == propertyName);
                if (segment2 != null)
                {
                    root = segment2;
                }
                else
                {
                    EpmSourcePathSegment item = new EpmSourcePathSegment(propertyName);
                    root.SubProperties.Add(item);
                    root = item;
                }
            }
            if ((actualPropertyType != null) && (actualPropertyType.ResourceTypeKind != ResourceTypeKind.Primitive))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_EndsWithNonPrimitiveType(root.PropertyName));
            }
            if (segment2 != null)
            {
                if (segment2.EpmInfo.DefiningTypesAreEqual(epmInfo))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_DuplicateEpmAttrsWithSameSourceName(epmInfo.Attribute.SourcePath, epmInfo.DefiningType.Name));
                }
                this.epmTargetTree.Remove(segment2.EpmInfo);
            }
            root.EpmInfo = epmInfo;
            this.epmTargetTree.Add(epmInfo);
        }

        private static IEnumerable<ResourceProperty> GetDeclaredProperties(ResourceType resourceType, Dictionary<ResourceType, IEnumerable<ResourceProperty>> declaredPropertiesLookup)
        {
            IEnumerable<ResourceProperty> propertiesDeclaredOnThisType;
            if (!declaredPropertiesLookup.TryGetValue(resourceType, out propertiesDeclaredOnThisType))
            {
                propertiesDeclaredOnThisType = resourceType.PropertiesDeclaredOnThisType;
                declaredPropertiesLookup.Add(resourceType, propertiesDeclaredOnThisType);
            }
            return propertiesDeclaredOnThisType;
        }

        private static ResourceType GetPropertyType(ResourceType resourceType, string propertyName, Dictionary<ResourceType, IEnumerable<ResourceProperty>> declaredPropertiesLookup)
        {
            Func<ResourceProperty, bool> predicate = null;
            ResourceProperty property = null;
            if (resourceType != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.Name == propertyName;
                }
                property = GetDeclaredProperties(resourceType, declaredPropertiesLookup).FirstOrDefault<ResourceProperty>(predicate);
                if ((property == null) && (resourceType.BaseType != null))
                {
                    property = resourceType.BaseType.TryResolvePropertyName(propertyName);
                }
            }
            if (property != null)
            {
                if (property.IsOfKind(ResourcePropertyKind.Collection))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_CollectionPropertyCannotBeMapped(propertyName, resourceType.FullName));
                }
                if (property.IsOfKind(ResourcePropertyKind.Stream))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_NamedStreamCannotBeMapped(propertyName, resourceType.FullName));
                }
                if (property.IsOfKind(ResourcePropertyKind.Primitive) && property.ResourceType.InstanceType.IsSpatial())
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_SpatialTypeCannotBeMapped(propertyName, resourceType.FullName));
                }
                return property.ResourceType;
            }
            if ((resourceType != null) && !resourceType.IsOpenType)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.EpmSourceTree_InaccessiblePropertyOnType(propertyName, resourceType.FullName));
            }
            return null;
        }

        internal void Validate(ResourceType resourceType, IEnumerable<ResourceProperty> declaredProperties)
        {
            Dictionary<ResourceType, IEnumerable<ResourceProperty>> declaredPropertiesLookup = new Dictionary<ResourceType, IEnumerable<ResourceProperty>>(EqualityComparer<ResourceType>.Default);
            declaredPropertiesLookup.Add(resourceType, declaredProperties);
            Validate(this.Root, resourceType, declaredPropertiesLookup);
        }

        private static void Validate(EpmSourcePathSegment pathSegment, ResourceType resourceType, Dictionary<ResourceType, IEnumerable<ResourceProperty>> declaredPropertiesLookup)
        {
            foreach (EpmSourcePathSegment segment in pathSegment.SubProperties)
            {
                ResourceType type = GetPropertyType(resourceType, segment.PropertyName, declaredPropertiesLookup);
                Validate(segment, type, declaredPropertiesLookup);
            }
        }

        internal EpmSourcePathSegment Root
        {
            get
            {
                return this.root;
            }
        }
    }
}

