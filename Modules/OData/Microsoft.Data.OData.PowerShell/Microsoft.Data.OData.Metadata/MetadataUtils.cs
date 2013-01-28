namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Values;
    using Microsoft.Data.Edm.Values;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
	using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class MetadataUtils
    {
        internal static IEnumerable<IEdmDirectValueAnnotation> GetODataAnnotations(this IEdmModel model, IEdmElement annotatable)
        {
            IEnumerable<IEdmDirectValueAnnotation> enumerable = model.DirectValueAnnotations(annotatable);
            if (enumerable == null)
            {
                return null;
            }
            return (from a in enumerable
                where a.NamespaceUri == "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"
                select a);
        }

        private static IEdmType ResolveTypeName(IEdmModel model, IEdmType expectedType, string typeName, Func<IEdmType, string, IEdmType> customTypeResolver, ODataVersion version, out EdmTypeKind typeKind)
        {
            IEdmType collectionType = null;
            EdmTypeKind kind;
            string str = (version >= ODataVersion.V3) ? EdmLibraryExtensions.GetCollectionItemTypeName(typeName) : null;
            if (str == null)
            {
                if ((customTypeResolver != null) && model.IsUserModel())
                {
                    collectionType = customTypeResolver(expectedType, typeName);
                    if (collectionType == null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.MetadataUtils_ResolveTypeName(typeName));
                    }
                }
                else
                {
                    collectionType = model.FindType(typeName);
                }
                if (((version < ODataVersion.V3) && (collectionType != null)) && collectionType.IsSpatial())
                {
                    collectionType = null;
                }
                typeKind = (collectionType == null) ? EdmTypeKind.None : collectionType.TypeKind;
                return collectionType;
            }
            typeKind = EdmTypeKind.Collection;
            IEdmType definition = null;
            if (((customTypeResolver != null) && (expectedType != null)) && (expectedType.TypeKind == EdmTypeKind.Collection))
            {
                definition = ((IEdmCollectionType) expectedType).ElementType.Definition;
            }
            IEdmType itemType = ResolveTypeName(model, definition, str, customTypeResolver, version, out kind);
            if (itemType != null)
            {
                collectionType = EdmLibraryExtensions.GetCollectionType(itemType);
            }
            return collectionType;
        }

        internal static IEdmType ResolveTypeNameForRead(IEdmModel model, IEdmType expectedType, string typeName, ODataReaderBehavior readerBehavior, ODataVersion version, out EdmTypeKind typeKind)
        {
            Func<IEdmType, string, IEdmType> customTypeResolver = (readerBehavior == null) ? null : readerBehavior.TypeResolver;
            return ResolveTypeName(model, expectedType, typeName, customTypeResolver, version, out typeKind);
        }

        internal static IEdmType ResolveTypeNameForWrite(IEdmModel model, string typeName)
        {
            EdmTypeKind kind;
            return ResolveTypeName(model, null, typeName, null, ODataVersion.V3, out kind);
        }

        internal static void SetODataAnnotation(this IEdmModel model, IEdmElement annotatable, string localName, string value)
        {
            IEdmStringValue value2 = null;
            if (value != null)
            {
                value2 = new EdmStringConstant(EdmCoreModel.Instance.GetString(true), value);
            }
            model.SetAnnotationValue(annotatable, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", localName, value2);
        }

        internal static bool TryGetODataAnnotation(this IEdmModel model, IEdmElement annotatable, string localName, out string value)
        {
            object obj2 = model.GetAnnotationValue(annotatable, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", localName);
            if (obj2 == null)
            {
                value = null;
                return false;
            }
            IEdmStringValue value2 = obj2 as IEdmStringValue;
            if (value2 == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomWriterMetadataUtils_InvalidAnnotationValue(localName, obj2.GetType().FullName));
            }
            value = value2.Value;
            return true;
        }
    }
}

