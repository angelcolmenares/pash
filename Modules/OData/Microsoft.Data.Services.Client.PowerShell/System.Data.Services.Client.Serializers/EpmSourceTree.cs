namespace System.Data.Services.Client.Serializers
{
    using Microsoft.Data.Edm;
    using System;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Linq;

    internal sealed class EpmSourceTree
    {
        private readonly EpmTargetTree epmTargetTree;
        private readonly EpmSourcePathSegment root = new EpmSourcePathSegment();

        internal EpmSourceTree(EpmTargetTree epmTargetTree)
        {
            this.epmTargetTree = epmTargetTree;
        }

        internal void Add(EntityPropertyMappingInfo epmInfo)
        {
            EpmSourcePathSegment root = this.Root;
            EpmSourcePathSegment segment2 = null;
            ClientTypeAnnotation actualPropertyType = epmInfo.ActualPropertyType;
            for (int i = 0; i < epmInfo.PropertyValuePath.Length; i++)
            {
                string propertyName = epmInfo.PropertyValuePath[i];
                if (propertyName.Length == 0)
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.EpmSourceTree_InvalidSourcePath(epmInfo.DefiningType.Name, epmInfo.Attribute.SourcePath));
                }
                actualPropertyType = GetPropertyType(actualPropertyType, propertyName);
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
            if ((actualPropertyType != null) && !PrimitiveType.IsKnownNullableType(actualPropertyType.ElementType))
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.EpmSourceTree_EndsWithNonPrimitiveType(root.PropertyName));
            }
            if (segment2 != null)
            {
                if (segment2.EpmInfo.DefiningTypesAreEqual(epmInfo))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.EpmSourceTree_DuplicateEpmAttrsWithSameSourceName(epmInfo.Attribute.SourcePath, epmInfo.DefiningType.Name));
                }
                this.epmTargetTree.Remove(segment2.EpmInfo);
            }
            root.EpmInfo = epmInfo;
            this.epmTargetTree.Add(epmInfo);
        }

        private static ClientTypeAnnotation GetPropertyType(ClientTypeAnnotation clientType, string propertyName)
        {
            ClientPropertyAnnotation property = clientType.GetProperty(propertyName, true);
            if (property == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.EpmSourceTree_InaccessiblePropertyOnType(propertyName, clientType.ElementTypeName));
            }
            if (property.IsStreamLinkProperty)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.EpmSourceTree_NamedStreamCannotBeMapped(propertyName, clientType.ElementTypeName));
            }
            if (property.IsSpatialType)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.EpmSourceTree_SpatialTypeCannotBeMapped(propertyName, clientType.ElementTypeName));
            }
            if (property.IsPrimitiveOrComplexCollection)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.EpmSourceTree_CollectionPropertyCannotBeMapped(propertyName, clientType.ElementTypeName));
            }
            ClientEdmModel model = ClientEdmModel.GetModel(clientType.MaxProtocolVersion);
            IEdmType orCreateEdmType = model.GetOrCreateEdmType(property.PropertyType);
            return model.GetClientTypeAnnotation(orCreateEdmType);
        }

        internal void Validate(ClientTypeAnnotation resourceType)
        {
            Validate(this.Root, resourceType);
        }

        private static void Validate(EpmSourcePathSegment pathSegment, ClientTypeAnnotation resourceType)
        {
            foreach (EpmSourcePathSegment segment in pathSegment.SubProperties)
            {
                ClientTypeAnnotation propertyType = GetPropertyType(resourceType, segment.PropertyName);
                Validate(segment, propertyType);
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

