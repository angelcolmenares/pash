namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
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
            List<EpmSourcePathSegment> list = new List<EpmSourcePathSegment>();
            EpmSourcePathSegment root = this.Root;
            EpmSourcePathSegment segment2 = null;
            IEdmType actualPropertyType = epmInfo.ActualPropertyType;
            string[] strArray = epmInfo.Attribute.SourcePath.Split(new char[] { '/' });
            int length = strArray.Length;
            for (int i = 0; i < length; i++)
            {
                string propertyName = strArray[i];
                if (propertyName.Length == 0)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_InvalidSourcePath(epmInfo.DefiningType.ODataFullName(), epmInfo.Attribute.SourcePath));
                }
                IEdmTypeReference propertyType = GetPropertyType(actualPropertyType, propertyName);
                IEdmType type2 = (propertyType == null) ? null : propertyType.Definition;
                if ((type2 == null) && (i < (length - 1)))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_OpenComplexPropertyCannotBeMapped(propertyName, actualPropertyType.ODataFullName()));
                }
                actualPropertyType = type2;
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
                list.Add(root);
            }
            if ((actualPropertyType != null) && !actualPropertyType.IsODataPrimitiveTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_EndsWithNonPrimitiveType(root.PropertyName));
            }
            if (segment2 != null)
            {
                if (segment2.EpmInfo.DefiningTypesAreEqual(epmInfo))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_DuplicateEpmAttributesWithSameSourceName(epmInfo.DefiningType.ODataFullName(), epmInfo.Attribute.SourcePath));
                }
                this.epmTargetTree.Remove(segment2.EpmInfo);
            }
            epmInfo.SetPropertyValuePath(list.ToArray());
            root.EpmInfo = epmInfo;
            this.epmTargetTree.Add(epmInfo);
        }

        private static IEdmTypeReference GetPropertyType(IEdmType type, string propertyName)
        {
            IEdmStructuredType type2 = type as IEdmStructuredType;
            IEdmProperty property = (type2 == null) ? null : type2.FindProperty(propertyName);
            if (property != null)
            {
                IEdmTypeReference typeReference = property.Type;
                if (typeReference.IsNonEntityODataCollectionTypeKind())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_CollectionPropertyCannotBeMapped(propertyName, type.ODataFullName()));
                }
                if (typeReference.IsStream())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_StreamPropertyCannotBeMapped(propertyName, type.ODataFullName()));
                }
                if (typeReference.IsSpatial())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_SpatialTypeCannotBeMapped(propertyName, type.ODataFullName()));
                }
                return property.Type;
            }
            if ((type != null) && !type.IsOpenType())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_MissingPropertyOnType(propertyName, type.ODataFullName()));
            }
            return null;
        }

        internal void Validate(IEdmEntityType entityType)
        {
            Validate(this.Root, entityType);
        }

        private static void Validate(EpmSourcePathSegment pathSegment, IEdmType type)
        {
            foreach (EpmSourcePathSegment segment in pathSegment.SubProperties)
            {
                IEdmTypeReference propertyType = GetPropertyType(type, segment.PropertyName);
                IEdmType type2 = (propertyType == null) ? null : propertyType.Definition;
                Validate(segment, type2);
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

