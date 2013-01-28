namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Spatial;

    internal static class EdmLibraryExtensions
    {
        private static readonly Dictionary<Type, IEdmPrimitiveTypeReference> PrimitiveTypeReferenceMap = new Dictionary<Type, IEdmPrimitiveTypeReference>(EqualityComparer<Type>.Default);

        static EdmLibraryExtensions()
        {
            PrimitiveTypeReferenceMap.Add(typeof(bool), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean), false));
            PrimitiveTypeReferenceMap.Add(typeof(bool?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Boolean), true));
            PrimitiveTypeReferenceMap.Add(typeof(byte), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Byte), false));
            PrimitiveTypeReferenceMap.Add(typeof(byte?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Byte), true));
            PrimitiveTypeReferenceMap.Add(typeof(DateTime), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTime), false));
            PrimitiveTypeReferenceMap.Add(typeof(DateTime?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTime), true));
            PrimitiveTypeReferenceMap.Add(typeof(DateTimeOffset), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), false));
            PrimitiveTypeReferenceMap.Add(typeof(DateTimeOffset?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), true));
            PrimitiveTypeReferenceMap.Add(typeof(decimal), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Decimal), false));
            PrimitiveTypeReferenceMap.Add(typeof(decimal?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Decimal), true));
            PrimitiveTypeReferenceMap.Add(typeof(double), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double), false));
            PrimitiveTypeReferenceMap.Add(typeof(double?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Double), true));
            PrimitiveTypeReferenceMap.Add(typeof(short), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int16), false));
            PrimitiveTypeReferenceMap.Add(typeof(short?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int16), true));
            PrimitiveTypeReferenceMap.Add(typeof(int), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false));
            PrimitiveTypeReferenceMap.Add(typeof(int?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), true));
            PrimitiveTypeReferenceMap.Add(typeof(long), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64), false));
            PrimitiveTypeReferenceMap.Add(typeof(long?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int64), true));
            PrimitiveTypeReferenceMap.Add(typeof(sbyte), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.SByte), false));
            PrimitiveTypeReferenceMap.Add(typeof(sbyte?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.SByte), true));
            PrimitiveTypeReferenceMap.Add(typeof(string), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String), true));
            PrimitiveTypeReferenceMap.Add(typeof(float), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single), false));
            PrimitiveTypeReferenceMap.Add(typeof(float?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Single), true));
            PrimitiveTypeReferenceMap.Add(typeof(byte[]), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Binary), true));
            PrimitiveTypeReferenceMap.Add(typeof(Stream), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Stream), false));
            PrimitiveTypeReferenceMap.Add(typeof(Guid), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid), false));
            PrimitiveTypeReferenceMap.Add(typeof(Guid?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Guid), true));
            PrimitiveTypeReferenceMap.Add(typeof(TimeSpan), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Time), false));
            PrimitiveTypeReferenceMap.Add(typeof(TimeSpan?), ToTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Time), true));
        }

        internal static IEdmCollectionTypeReference AsCollectionOrNull(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return null;
            }
            if (typeReference.TypeKind() != EdmTypeKind.Collection)
            {
                return null;
            }
            IEdmCollectionTypeReference reference = typeReference.AsCollection();
            if (!reference.IsNonEntityODataCollectionTypeKind())
            {
                return null;
            }
            return reference;
        }

        internal static IEdmComplexTypeReference AsComplexOrNull(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return null;
            }
            if (typeReference.TypeKind() != EdmTypeKind.Complex)
            {
                return null;
            }
            return typeReference.AsComplex();
        }

        internal static IEdmPrimitiveType BaseType(this IEdmPrimitiveType type)
        {
            switch (type.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.None:
                case EdmPrimitiveTypeKind.Binary:
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
                case EdmPrimitiveTypeKind.String:
                case EdmPrimitiveTypeKind.Stream:
                case EdmPrimitiveTypeKind.Time:
                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.Geometry:
                    return null;

                case EdmPrimitiveTypeKind.GeographyPoint:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);

                case EdmPrimitiveTypeKind.GeographyLineString:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);

                case EdmPrimitiveTypeKind.GeographyPolygon:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);

                case EdmPrimitiveTypeKind.GeographyCollection:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);

                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);

                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);

                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);

                case EdmPrimitiveTypeKind.GeometryPoint:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);

                case EdmPrimitiveTypeKind.GeometryLineString:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);

                case EdmPrimitiveTypeKind.GeometryPolygon:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);

                case EdmPrimitiveTypeKind.GeometryCollection:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);

                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);

                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);

                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_BaseType));
        }

        internal static IEdmTypeReference Clone(this IEdmTypeReference typeReference, bool nullable)
        {
            if (typeReference == null)
            {
                return null;
            }
            switch (typeReference.TypeKind())
            {
                case EdmTypeKind.Primitive:
                {
                    EdmPrimitiveTypeKind kind2 = typeReference.PrimitiveKind();
                    IEdmPrimitiveType definition = (IEdmPrimitiveType) typeReference.Definition;
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Binary:
                        {
                            IEdmBinaryTypeReference reference = (IEdmBinaryTypeReference) typeReference;
                            return new EdmBinaryTypeReference(definition, nullable, reference.IsUnbounded, reference.MaxLength, reference.IsFixedLength);
                        }
                        case EdmPrimitiveTypeKind.Boolean:
                        case EdmPrimitiveTypeKind.Byte:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Guid:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.SByte:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Stream:
                            return new EdmPrimitiveTypeReference(definition, nullable);

                        case EdmPrimitiveTypeKind.DateTime:
                        case EdmPrimitiveTypeKind.DateTimeOffset:
                        case EdmPrimitiveTypeKind.Time:
                        {
                            IEdmTemporalTypeReference reference4 = (IEdmTemporalTypeReference) typeReference;
                            return new EdmTemporalTypeReference(definition, nullable, reference4.Precision);
                        }
                        case EdmPrimitiveTypeKind.Decimal:
                        {
                            IEdmDecimalTypeReference reference3 = (IEdmDecimalTypeReference) typeReference;
                            return new EdmDecimalTypeReference(definition, nullable, reference3.Precision, reference3.Scale);
                        }
                        case EdmPrimitiveTypeKind.String:
                        {
                            IEdmStringTypeReference reference2 = (IEdmStringTypeReference) typeReference;
                            return new EdmStringTypeReference(definition, nullable, reference2.IsUnbounded, reference2.MaxLength, reference2.IsFixedLength, reference2.IsUnicode, reference2.Collation);
                        }
                        case EdmPrimitiveTypeKind.Geography:
                        case EdmPrimitiveTypeKind.GeographyPoint:
                        case EdmPrimitiveTypeKind.GeographyLineString:
                        case EdmPrimitiveTypeKind.GeographyPolygon:
                        case EdmPrimitiveTypeKind.GeographyCollection:
                        case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                        case EdmPrimitiveTypeKind.GeographyMultiLineString:
                        case EdmPrimitiveTypeKind.GeographyMultiPoint:
                        case EdmPrimitiveTypeKind.Geometry:
                        case EdmPrimitiveTypeKind.GeometryPoint:
                        case EdmPrimitiveTypeKind.GeometryLineString:
                        case EdmPrimitiveTypeKind.GeometryPolygon:
                        case EdmPrimitiveTypeKind.GeometryCollection:
                        case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                        case EdmPrimitiveTypeKind.GeometryMultiLineString:
                        case EdmPrimitiveTypeKind.GeometryMultiPoint:
                        {
                            IEdmSpatialTypeReference reference5 = (IEdmSpatialTypeReference) typeReference;
                            return new EdmSpatialTypeReference(definition, nullable, reference5.SpatialReferenceIdentifier);
                        }
                    }
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_Clone_PrimitiveTypeKind));
                }
                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference((IEdmEntityType) typeReference.Definition, nullable);

                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference((IEdmComplexType) typeReference.Definition, nullable);

                case EdmTypeKind.Row:
                    return new EdmRowTypeReference((IEdmRowType) typeReference.Definition, nullable);

                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference((IEdmCollectionType) typeReference.Definition, nullable);

                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference((IEdmEntityReferenceType) typeReference.Definition, nullable);

                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference((IEdmEnumType) typeReference.Definition, nullable);
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_Clone_TypeKind));
        }

        internal static bool ContainsProperty(this IEdmType type, IEdmProperty property)
        {
            Func<IEdmProperty, bool> predicate = null;
            Func<IEdmProperty, bool> func2 = null;
            Func<IEdmNavigationProperty, bool> func3 = null;
            IEdmComplexType type2 = type as IEdmComplexType;
            if (type2 != null)
            {
                if (predicate == null)
                {
                    predicate = p => p == property;
                }
                return type2.Properties().Any<IEdmProperty>(predicate);
            }
            IEdmEntityType type3 = type as IEdmEntityType;
            if (type3 == null)
            {
                return false;
            }
            if (func2 == null)
            {
                func2 = p => p == property;
            }
            if (type3.Properties().Any<IEdmProperty>(func2))
            {
                return true;
            }
            if (func3 == null)
            {
                func3 = p => p == property;
            }
            return type3.NavigationProperties().Any<IEdmNavigationProperty>(func3);
        }

        internal static bool ContainsProperty(this IEdmTypeReference typeReference, IEdmProperty property)
        {
            IEdmStructuredTypeReference reference = typeReference.AsStructuredOrNull();
            if (reference == null)
            {
                return false;
            }
            return reference.Definition.ContainsProperty(property);
        }

        internal static IEnumerable<IEdmEntityType> EntityTypes(this IEdmModel model)
        {
            IEnumerable<IEdmSchemaElement> schemaElements = model.SchemaElements;
            if (schemaElements != null)
            {
                return schemaElements.OfType<IEdmEntityType>();
            }
            return null;
        }

        internal static IEdmTypeReference GetCollectionItemType(this IEdmTypeReference typeReference)
        {
            IEdmCollectionTypeReference type = typeReference.AsCollectionOrNull();
            if (type != null)
            {
                return type.ElementType();
            }
            return null;
        }

        internal static string GetCollectionItemTypeName(string typeName)
        {
            return GetCollectionItemTypeName(typeName, false);
        }

        private static string GetCollectionItemTypeName(string typeName, bool isNested)
        {
            int length = "Collection".Length;
            if (((typeName == null) || !typeName.StartsWith("Collection(", StringComparison.Ordinal)) || ((typeName[typeName.Length - 1] != ')') || (typeName.Length == (length + 2))))
            {
                return null;
            }
            if (isNested)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NestedCollectionsAreNotSupported);
            }
            string str = typeName.Substring(length + 1, typeName.Length - (length + 2));
            GetCollectionItemTypeName(str, true);
            return str;
        }

        internal static IEdmCollectionType GetCollectionType(IEdmType itemType)
        {
            if (!itemType.IsODataPrimitiveTypeKind() && !itemType.IsODataComplexTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EdmLibraryExtensions_CollectionItemCanBeOnlyPrimitiveOrComplex);
            }
            return new EdmCollectionType(itemType.ToTypeReference());
        }

        internal static IEdmPrimitiveType GetCommonBaseType(this IEdmPrimitiveType firstType, IEdmPrimitiveType secondType)
        {
            IEdmPrimitiveType type;
            if (((IEdmType) firstType).IsEquivalentTo((IEdmType) secondType))
            {
                return firstType;
            }
            for (type = firstType; type != null; type = type.BaseType())
            {
                if (type.IsAssignableFrom(secondType))
                {
                    return type;
                }
            }
            for (type = secondType; type != null; type = type.BaseType())
            {
                if (type.IsAssignableFrom(firstType))
                {
                    return type;
                }
            }
            return null;
        }

        internal static IEdmStructuredType GetCommonBaseType(this IEdmStructuredType firstType, IEdmStructuredType secondType)
        {
            IEdmStructuredType type;
            if (firstType.IsEquivalentTo(secondType))
            {
                return firstType;
            }
            for (type = firstType; type != null; type = type.BaseType)
            {
                if (type.IsAssignableFrom(secondType))
                {
                    return type;
                }
            }
            for (type = secondType; type != null; type = type.BaseType)
            {
                if (type.IsAssignableFrom(firstType))
                {
                    return type;
                }
            }
            return null;
        }

        internal static Type GetPrimitiveClrType(IEdmPrimitiveTypeReference primitiveTypeReference)
        {
            return GetPrimitiveClrType(primitiveTypeReference.PrimitiveDefinition(), primitiveTypeReference.IsNullable);
        }

        internal static Type GetPrimitiveClrType(IEdmPrimitiveType primitiveType, bool isNullable)
        {
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Binary:
                    return typeof(byte[]);

                case EdmPrimitiveTypeKind.Boolean:
                    if (isNullable)
                    {
                        return typeof(bool?);
                    }
                    return typeof(bool);

                case EdmPrimitiveTypeKind.Byte:
                    if (isNullable)
                    {
                        return typeof(byte?);
                    }
                    return typeof(byte);

                case EdmPrimitiveTypeKind.DateTime:
                    if (isNullable)
                    {
                        return typeof(DateTime?);
                    }
                    return typeof(DateTime);

                case EdmPrimitiveTypeKind.DateTimeOffset:
                    if (isNullable)
                    {
                        return typeof(DateTimeOffset?);
                    }
                    return typeof(DateTimeOffset);

                case EdmPrimitiveTypeKind.Decimal:
                    if (isNullable)
                    {
                        return typeof(decimal?);
                    }
                    return typeof(decimal);

                case EdmPrimitiveTypeKind.Double:
                    if (isNullable)
                    {
                        return typeof(double?);
                    }
                    return typeof(double);

                case EdmPrimitiveTypeKind.Guid:
                    if (isNullable)
                    {
                        return typeof(Guid?);
                    }
                    return typeof(Guid);

                case EdmPrimitiveTypeKind.Int16:
                    if (isNullable)
                    {
                        return typeof(short?);
                    }
                    return typeof(short);

                case EdmPrimitiveTypeKind.Int32:
                    if (isNullable)
                    {
                        return typeof(int?);
                    }
                    return typeof(int);

                case EdmPrimitiveTypeKind.Int64:
                    if (isNullable)
                    {
                        return typeof(long?);
                    }
                    return typeof(long);

                case EdmPrimitiveTypeKind.SByte:
                    if (isNullable)
                    {
                        return typeof(sbyte?);
                    }
                    return typeof(sbyte);

                case EdmPrimitiveTypeKind.Single:
                    if (isNullable)
                    {
                        return typeof(float?);
                    }
                    return typeof(float);

                case EdmPrimitiveTypeKind.String:
                    return typeof(string);

                case EdmPrimitiveTypeKind.Stream:
                    return typeof(Stream);

                case EdmPrimitiveTypeKind.Time:
                    if (isNullable)
                    {
                        return typeof(TimeSpan?);
                    }
                    return typeof(TimeSpan);

                case EdmPrimitiveTypeKind.Geography:
                    return typeof(Geography);

                case EdmPrimitiveTypeKind.GeographyPoint:
                    return typeof(GeographyPoint);

                case EdmPrimitiveTypeKind.GeographyLineString:
                    return typeof(GeographyLineString);

                case EdmPrimitiveTypeKind.GeographyPolygon:
                    return typeof(GeographyPolygon);

                case EdmPrimitiveTypeKind.GeographyCollection:
                    return typeof(GeographyCollection);

                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    return typeof(GeographyMultiPolygon);

                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    return typeof(GeographyMultiLineString);

                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    return typeof(GeographyMultiPoint);

                case EdmPrimitiveTypeKind.Geometry:
                    return typeof(Geometry);

                case EdmPrimitiveTypeKind.GeometryPoint:
                    return typeof(GeometryPoint);

                case EdmPrimitiveTypeKind.GeometryLineString:
                    return typeof(GeometryLineString);

                case EdmPrimitiveTypeKind.GeometryPolygon:
                    return typeof(GeometryPolygon);

                case EdmPrimitiveTypeKind.GeometryCollection:
                    return typeof(GeometryCollection);

                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    return typeof(GeometryMultiPolygon);

                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    return typeof(GeometryMultiLineString);

                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return typeof(GeometryMultiPoint);
            }
            return null;
        }

        internal static IEdmPrimitiveTypeReference GetPrimitiveTypeReference(Type clrType)
        {
            IEdmPrimitiveTypeReference reference;
            if (PrimitiveTypeReferenceMap.TryGetValue(clrType, out reference))
            {
                return reference;
            }
            IEdmPrimitiveType primitiveType = null;
            if (typeof(GeographyPoint).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint);
            }
            else if (typeof(GeographyLineString).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString);
            }
            else if (typeof(GeographyPolygon).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon);
            }
            else if (typeof(GeographyMultiPoint).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint);
            }
            else if (typeof(GeographyMultiLineString).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString);
            }
            else if (typeof(GeographyMultiPolygon).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon);
            }
            else if (typeof(GeographyCollection).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection);
            }
            else if (typeof(Geography).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geography);
            }
            else if (typeof(GeometryPoint).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint);
            }
            else if (typeof(GeometryLineString).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString);
            }
            else if (typeof(GeometryPolygon).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon);
            }
            else if (typeof(GeometryMultiPoint).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint);
            }
            else if (typeof(GeometryMultiLineString).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString);
            }
            else if (typeof(GeometryMultiPolygon).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon);
            }
            else if (typeof(GeometryCollection).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection);
            }
            else if (typeof(Geometry).IsAssignableFrom(clrType))
            {
                primitiveType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Geometry);
            }
            if (primitiveType == null)
            {
                return null;
            }
            return ToTypeReference(primitiveType, true);
        }

        internal static bool IsAssignableFrom(this IEdmPrimitiveType baseType, IEdmPrimitiveType subtype)
        {
            if (((IEdmType) baseType).IsEquivalentTo((IEdmType) subtype))
            {
                return true;
            }
            if (!baseType.IsSpatialType() || !subtype.IsSpatialType())
            {
                return false;
            }
            EdmPrimitiveTypeKind primitiveKind = baseType.PrimitiveKind;
            EdmPrimitiveTypeKind kind2 = subtype.PrimitiveKind;
            switch (primitiveKind)
            {
                case EdmPrimitiveTypeKind.Geography:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Geography:
                        case EdmPrimitiveTypeKind.GeographyCollection:
                        case EdmPrimitiveTypeKind.GeographyLineString:
                        case EdmPrimitiveTypeKind.GeographyMultiLineString:
                        case EdmPrimitiveTypeKind.GeographyMultiPoint:
                        case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                        case EdmPrimitiveTypeKind.GeographyPoint:
                            return true;
                    }
                    return (kind2 == EdmPrimitiveTypeKind.GeographyPolygon);

                case EdmPrimitiveTypeKind.GeographyPoint:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyPoint);

                case EdmPrimitiveTypeKind.GeographyLineString:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyLineString);

                case EdmPrimitiveTypeKind.GeographyPolygon:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyPolygon);

                case EdmPrimitiveTypeKind.GeographyCollection:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.GeographyCollection:
                        case EdmPrimitiveTypeKind.GeographyMultiLineString:
                        case EdmPrimitiveTypeKind.GeographyMultiPoint:
                            return true;
                    }
                    return (kind2 == EdmPrimitiveTypeKind.GeographyMultiPolygon);

                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyMultiPolygon);

                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyMultiLineString);

                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    return (kind2 == EdmPrimitiveTypeKind.GeographyMultiPoint);

                case EdmPrimitiveTypeKind.Geometry:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.Geometry:
                        case EdmPrimitiveTypeKind.GeometryCollection:
                        case EdmPrimitiveTypeKind.GeometryLineString:
                        case EdmPrimitiveTypeKind.GeometryMultiLineString:
                        case EdmPrimitiveTypeKind.GeometryMultiPoint:
                        case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                        case EdmPrimitiveTypeKind.GeometryPoint:
                            return true;
                    }
                    return (kind2 == EdmPrimitiveTypeKind.GeometryPolygon);

                case EdmPrimitiveTypeKind.GeometryPoint:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryPoint);

                case EdmPrimitiveTypeKind.GeometryLineString:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryLineString);

                case EdmPrimitiveTypeKind.GeometryPolygon:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryPolygon);

                case EdmPrimitiveTypeKind.GeometryCollection:
                    switch (kind2)
                    {
                        case EdmPrimitiveTypeKind.GeometryCollection:
                        case EdmPrimitiveTypeKind.GeometryMultiLineString:
                        case EdmPrimitiveTypeKind.GeometryMultiPoint:
                            return true;
                    }
                    return (kind2 == EdmPrimitiveTypeKind.GeometryMultiPolygon);

                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryMultiPolygon);

                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryMultiLineString);

                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return (kind2 == EdmPrimitiveTypeKind.GeometryMultiPoint);
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_IsAssignableFrom));
        }

        internal static bool IsAssignableFrom(this IEdmStructuredType baseType, IEdmStructuredType subtype)
        {
            if (baseType.TypeKind == subtype.TypeKind)
            {
                if (!baseType.IsODataEntityTypeKind() && !baseType.IsODataComplexTypeKind())
                {
                    return false;
                }
                for (IEdmStructuredType type = subtype; type != null; type = type.BaseType)
                {
                    if (type.IsEquivalentTo(baseType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsGeographyType(this IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference type = typeReference.AsPrimitiveOrNull();
            if (type != null)
            {
                switch (type.PrimitiveKind())
                {
                    case EdmPrimitiveTypeKind.Geography:
                    case EdmPrimitiveTypeKind.GeographyPoint:
                    case EdmPrimitiveTypeKind.GeographyLineString:
                    case EdmPrimitiveTypeKind.GeographyPolygon:
                    case EdmPrimitiveTypeKind.GeographyCollection:
                    case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                    case EdmPrimitiveTypeKind.GeographyMultiLineString:
                    case EdmPrimitiveTypeKind.GeographyMultiPoint:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsGeometryType(this IEdmTypeReference typeReference)
        {
            IEdmPrimitiveTypeReference type = typeReference.AsPrimitiveOrNull();
            if (type != null)
            {
                switch (type.PrimitiveKind())
                {
                    case EdmPrimitiveTypeKind.Geometry:
                    case EdmPrimitiveTypeKind.GeometryPoint:
                    case EdmPrimitiveTypeKind.GeometryLineString:
                    case EdmPrimitiveTypeKind.GeometryPolygon:
                    case EdmPrimitiveTypeKind.GeometryCollection:
                    case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                    case EdmPrimitiveTypeKind.GeometryMultiLineString:
                    case EdmPrimitiveTypeKind.GeometryMultiPoint:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsOpenType(this IEdmType type)
        {
            IEdmStructuredType type2 = type as IEdmStructuredType;
            return ((type2 != null) && type2.IsOpen);
        }

        internal static bool IsPrimitiveType(Type clrType)
        {
            if (!PrimitiveTypeReferenceMap.ContainsKey(clrType))
            {
                return typeof(ISpatial).IsAssignableFrom(clrType);
            }
            return true;
        }

        internal static bool IsSpatialType(this IEdmPrimitiveType primitiveType)
        {
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return true;
            }
            return false;
        }

        internal static bool IsStream(this IEdmType type)
        {
            IEdmPrimitiveType type2 = type as IEdmPrimitiveType;
            if (type2 == null)
            {
                return false;
            }
            return (type2.PrimitiveKind == EdmPrimitiveTypeKind.Stream);
        }

        internal static bool IsUserModel(this IEdmModel model)
        {
            return !(model is EdmCoreModel);
        }

        internal static IEdmSchemaType ResolvePrimitiveTypeName(string typeName)
        {
            return EdmCoreModel.Instance.FindDeclaredType(typeName);
        }

        internal static IEdmCollectionTypeReference ToCollectionTypeReference(this IEdmComplexTypeReference itemTypeReference)
        {
            IEdmCollectionType type = new EdmCollectionType(itemTypeReference);
            return (IEdmCollectionTypeReference) type.ToTypeReference();
        }

        internal static IEdmCollectionTypeReference ToCollectionTypeReference(this IEdmPrimitiveTypeReference itemTypeReference)
        {
            IEdmCollectionType type = new EdmCollectionType(itemTypeReference);
            return (IEdmCollectionTypeReference) type.ToTypeReference();
        }

        internal static IEdmTypeReference ToTypeReference(this IEdmType type)
        {
            return type.ToTypeReference(false);
        }

        private static EdmPrimitiveTypeReference ToTypeReference(IEdmPrimitiveType primitiveType, bool nullable)
        {
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.Binary:
                    return new EdmBinaryTypeReference(primitiveType, nullable);

                case EdmPrimitiveTypeKind.Boolean:
                case EdmPrimitiveTypeKind.Byte:
                case EdmPrimitiveTypeKind.Double:
                case EdmPrimitiveTypeKind.Guid:
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                case EdmPrimitiveTypeKind.Int64:
                case EdmPrimitiveTypeKind.SByte:
                case EdmPrimitiveTypeKind.Single:
                case EdmPrimitiveTypeKind.Stream:
                    return new EdmPrimitiveTypeReference(primitiveType, nullable);

                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    return new EdmTemporalTypeReference(primitiveType, nullable);

                case EdmPrimitiveTypeKind.Decimal:
                    return new EdmDecimalTypeReference(primitiveType, nullable);

                case EdmPrimitiveTypeKind.String:
                    return new EdmStringTypeReference(primitiveType, nullable);

                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    return new EdmSpatialTypeReference(primitiveType, nullable);
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_PrimitiveTypeReference));
        }

        internal static IEdmTypeReference ToTypeReference(this IEdmType type, bool nullable)
        {
            if (type == null)
            {
                return null;
            }
            switch (type.TypeKind)
            {
                case EdmTypeKind.Primitive:
                    return ToTypeReference((IEdmPrimitiveType) type, nullable);

                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference((IEdmEntityType) type, nullable);

                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference((IEdmComplexType) type, nullable);

                case EdmTypeKind.Row:
                    return new EdmRowTypeReference((IEdmRowType) type, nullable);

                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference((IEdmCollectionType) type, nullable);

                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference((IEdmEntityReferenceType) type, nullable);
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodesCommon.EdmLibraryExtensions_ToTypeReference));
        }

        internal static bool TryGetPrimitiveTypeName(object value, out string typeName)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = GetPrimitiveTypeReference(value.GetType());
            if (primitiveTypeReference == null)
            {
                typeName = null;
                return false;
            }
            typeName = primitiveTypeReference.FullName();
            return true;
        }
    }
}

