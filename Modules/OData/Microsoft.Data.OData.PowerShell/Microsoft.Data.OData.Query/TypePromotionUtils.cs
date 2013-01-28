namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal static class TypePromotionUtils
    {
        internal static bool CanConvertTo(IEdmTypeReference sourceReference, IEdmTypeReference targetReference)
        {
            if (sourceReference.IsEquivalentTo(targetReference))
            {
                return true;
            }
            if (targetReference.IsODataComplexTypeKind() || targetReference.IsODataEntityTypeKind())
            {
                return ((IEdmStructuredType) targetReference.Definition).IsAssignableFrom(((IEdmStructuredType) sourceReference.Definition));
            }
            if (IsOpenPropertyType(targetReference))
            {
                return true;
            }
            IEdmPrimitiveTypeReference type = sourceReference.AsPrimitiveOrNull();
            IEdmPrimitiveTypeReference reference2 = targetReference.AsPrimitiveOrNull();
            return (((type != null) && (reference2 != null)) && MetadataUtilsCommon.CanConvertPrimitiveTypeTo(type.PrimitiveDefinition(), reference2.PrimitiveDefinition()));
        }

        private static bool IsOpenPropertyType(IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference type = typeReference.AsPrimitiveOrNull();
            return ((type != null) && (type.PrimitiveKind() == EdmPrimitiveTypeKind.None));
        }
    }
}

