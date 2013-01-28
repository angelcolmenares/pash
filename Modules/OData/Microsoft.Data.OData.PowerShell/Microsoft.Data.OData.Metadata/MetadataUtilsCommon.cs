namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal static class MetadataUtilsCommon
    {
        internal static IEdmEntityTypeReference AsEntityOrNull(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return null;
            }
            if (typeReference.TypeKind() != EdmTypeKind.Entity)
            {
                return null;
            }
            return typeReference.AsEntity();
        }

        internal static IEdmPrimitiveTypeReference AsPrimitiveOrNull(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return null;
            }
            if (typeReference.TypeKind() != EdmTypeKind.Primitive)
            {
                return null;
            }
            return typeReference.AsPrimitive();
        }

        internal static IEdmStructuredTypeReference AsStructuredOrNull(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return null;
            }
            if (!typeReference.IsStructured())
            {
                return null;
            }
            return typeReference.AsStructured();
        }

        internal static bool CanConvertPrimitiveTypeTo(IEdmPrimitiveType sourcePrimitiveType, IEdmPrimitiveType targetPrimitiveType)
        {
            EdmPrimitiveTypeKind primitiveKind = sourcePrimitiveType.PrimitiveKind;
            EdmPrimitiveTypeKind kind2 = targetPrimitiveType.PrimitiveKind;
            switch (primitiveKind)
            {
                case EdmPrimitiveTypeKind.Byte:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Byte:
                        case EdmPrimitiveTypeKind.Decimal:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                            return true;
                    }
                    break;

                case EdmPrimitiveTypeKind.Int16:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                            return true;
                    }
                    break;

                case EdmPrimitiveTypeKind.Int32:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                            return true;
                    }
                    break;

                case EdmPrimitiveTypeKind.Int64:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                            return true;
                    }
                    break;

                case EdmPrimitiveTypeKind.SByte:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.SByte:
                        case EdmPrimitiveTypeKind.Single:
                            return true;
                    }
                    break;

                case EdmPrimitiveTypeKind.Single:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Double:
					    case EdmPrimitiveTypeKind.Single:
                            break;
                    }
                    return true;

                default:
                    if (primitiveKind == kind2)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        internal static bool IsNonEntityODataCollectionTypeKind(this IEdmType type)
        {
            IEdmCollectionType type2 = type as IEdmCollectionType;
            return ((type2 != null) && ((type2.ElementType == null) || (type2.ElementType.TypeKind() != EdmTypeKind.Entity)));
        }

        internal static bool IsNonEntityODataCollectionTypeKind(this IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(typeReference.Definition, "typeReference.Definition");
            return typeReference.Definition.IsNonEntityODataCollectionTypeKind();
        }

        internal static bool IsODataComplexTypeKind(this IEdmType type)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(type, "type");
            return (type.TypeKind == EdmTypeKind.Complex);
        }

        internal static bool IsODataComplexTypeKind(this IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(typeReference.Definition, "typeReference.Definition");
            return typeReference.Definition.IsODataComplexTypeKind();
        }

        internal static bool IsODataEntityTypeKind(this IEdmType type)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(type, "type");
            return (type.TypeKind == EdmTypeKind.Entity);
        }

        internal static bool IsODataEntityTypeKind(this IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(typeReference.Definition, "typeReference.Definition");
            return typeReference.Definition.IsODataEntityTypeKind();
        }

        internal static bool IsODataPrimitiveTypeKind(this IEdmType type)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(type, "type");
            if (type.TypeKind != EdmTypeKind.Primitive)
            {
                return false;
            }
            return !type.IsStream();
        }

        internal static bool IsODataPrimitiveTypeKind(this IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(typeReference.Definition, "typeReference.Definition");
            return typeReference.Definition.IsODataPrimitiveTypeKind();
        }

        internal static bool IsODataValueType(this IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference type = typeReference.AsPrimitiveOrNull();
            if (type != null)
            {
                switch (type.PrimitiveKind())
                {
                    case EdmPrimitiveTypeKind.Boolean:
                    case EdmPrimitiveTypeKind.Byte:
                    case EdmPrimitiveTypeKind.DateTime:
                    case EdmPrimitiveTypeKind.DateTimeOffset:
                    case EdmPrimitiveTypeKind.Decimal:
                    case EdmPrimitiveTypeKind.Double:
                    case EdmPrimitiveTypeKind.Guid:
                    case EdmPrimitiveTypeKind.Int16:
                    case EdmPrimitiveTypeKind.Int32:
                    case EdmPrimitiveTypeKind.Int64:
                    case EdmPrimitiveTypeKind.SByte:
                    case EdmPrimitiveTypeKind.Single:
                    case EdmPrimitiveTypeKind.Time:
                        return true;
                }
            }
            return false;
        }

        internal static string ODataFullName(this IEdmType type)
        {
            IEdmCollectionType type2 = type as IEdmCollectionType;
            if (type2 != null)
            {
                string str = type2.ElementType.ODataFullName();
                if (str == null)
                {
                    return null;
                }
                return ("Collection(" + str + ")");
            }
            IEdmSchemaElement element = type as IEdmSchemaElement;
            if (element == null)
            {
                return null;
            }
            return element.FullName();
        }

        internal static string ODataFullName(this IEdmTypeReference typeReference)
        {
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull<IEdmType>(typeReference.Definition, "typeReference.Definition");
            return typeReference.Definition.ODataFullName();
        }
    }
}

