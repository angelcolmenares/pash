namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Xml.Linq;

    internal sealed class PrimitiveType
    {
        private static readonly Dictionary<Type, PrimitiveType> clrMapping = new Dictionary<Type, PrimitiveType>(EqualityComparer<Type>.Default);
        private static readonly Dictionary<Type, PrimitiveType> derivedPrimitiveTypeMapping = new Dictionary<Type, PrimitiveType>(EqualityComparer<Type>.Default);
        private static readonly Dictionary<string, PrimitiveType> edmMapping = new Dictionary<string, PrimitiveType>(EqualityComparer<string>.Default);
        private static readonly HashSet<Type> knownNonPrimitiveTypes = new HashSet<Type>(EqualityComparer<Type>.Default);
        private EdmPrimitiveTypeKind primitiveKind;

        static PrimitiveType()
        {
            InitializeTypes();
        }

        private PrimitiveType(Type clrType, string edmTypeName, EdmPrimitiveTypeKind primitiveKind, PrimitiveTypeConverter typeConverter, bool hasReverseMapping)
        {
            this.ClrType = clrType;
            this.EdmTypeName = edmTypeName;
            this.primitiveKind = primitiveKind;
            this.TypeConverter = typeConverter;
            this.HasReverseMapping = hasReverseMapping;
        }

        internal IEdmPrimitiveType CreateEdmPrimitiveType()
        {
            return ClientEdmPrimitiveType.CreateType(this.primitiveKind);
        }

        internal static void DeleteKnownType(Type clrType, string edmTypeName)
        {
            clrMapping.Remove(clrType);
            if (edmTypeName != null)
            {
                edmMapping.Remove(edmTypeName);
            }
        }

        private static void InitializeTypes()
        {
            RegisterKnownType(typeof(bool), "Edm.Boolean", EdmPrimitiveTypeKind.Boolean, new BooleanTypeConverter(), true);
            RegisterKnownType(typeof(byte), "Edm.Byte", EdmPrimitiveTypeKind.Byte, new ByteTypeConverter(), true);
            RegisterKnownType(typeof(byte[]), "Edm.Binary", EdmPrimitiveTypeKind.Binary, new ByteArrayTypeConverter(), true);
            RegisterKnownType(typeof(DateTime), "Edm.DateTime", EdmPrimitiveTypeKind.DateTime, new DateTimeTypeConverter(), true);
            RegisterKnownType(typeof(DateTimeOffset), "Edm.DateTimeOffset", EdmPrimitiveTypeKind.DateTimeOffset, new DateTimeOffsetTypeConverter(), true);
            RegisterKnownType(typeof(decimal), "Edm.Decimal", EdmPrimitiveTypeKind.Decimal, new DecimalTypeConverter(), true);
            RegisterKnownType(typeof(double), "Edm.Double", EdmPrimitiveTypeKind.Double, new DoubleTypeConverter(), true);
            RegisterKnownType(typeof(Guid), "Edm.Guid", EdmPrimitiveTypeKind.Guid, new GuidTypeConverter(), true);
            RegisterKnownType(typeof(short), "Edm.Int16", EdmPrimitiveTypeKind.Int16, new Int16TypeConverter(), true);
            RegisterKnownType(typeof(int), "Edm.Int32", EdmPrimitiveTypeKind.Int32, new Int32TypeConverter(), true);
            RegisterKnownType(typeof(long), "Edm.Int64", EdmPrimitiveTypeKind.Int64, new Int64TypeConverter(), true);
            RegisterKnownType(typeof(float), "Edm.Single", EdmPrimitiveTypeKind.Single, new SingleTypeConverter(), true);
            RegisterKnownType(typeof(string), "Edm.String", EdmPrimitiveTypeKind.String, new StringTypeConverter(), true);
            RegisterKnownType(typeof(sbyte), "Edm.SByte", EdmPrimitiveTypeKind.SByte, new SByteTypeConverter(), true);
            RegisterKnownType(typeof(TimeSpan), "Edm.Time", EdmPrimitiveTypeKind.Time, new TimeSpanTypeConverter(), true);
            RegisterKnownType(typeof(Geography), "Edm.Geography", EdmPrimitiveTypeKind.Geography, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyPoint), "Edm.GeographyPoint", EdmPrimitiveTypeKind.GeographyPoint, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyLineString), "Edm.GeographyLineString", EdmPrimitiveTypeKind.GeographyLineString, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyPolygon), "Edm.GeographyPolygon", EdmPrimitiveTypeKind.GeographyPolygon, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyCollection), "Edm.GeographyCollection", EdmPrimitiveTypeKind.GeographyCollection, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyMultiPoint), "Edm.GeographyMultiPoint", EdmPrimitiveTypeKind.GeographyMultiPoint, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyMultiLineString), "Edm.GeographyMultiLineString", EdmPrimitiveTypeKind.GeographyMultiLineString, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(GeographyMultiPolygon), "Edm.GeographyMultiPolygon", EdmPrimitiveTypeKind.GeographyMultiPolygon, new GeographyTypeConverter(), true);
            RegisterKnownType(typeof(Geometry), "Edm.Geometry", EdmPrimitiveTypeKind.Geometry, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryPoint), "Edm.GeometryPoint", EdmPrimitiveTypeKind.GeometryPoint, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryLineString), "Edm.GeometryLineString", EdmPrimitiveTypeKind.GeometryLineString, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryPolygon), "Edm.GeometryPolygon", EdmPrimitiveTypeKind.GeometryPolygon, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryCollection), "Edm.GeometryCollection", EdmPrimitiveTypeKind.GeometryCollection, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryMultiPoint), "Edm.GeometryMultiPoint", EdmPrimitiveTypeKind.GeometryMultiPoint, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryMultiLineString), "Edm.GeometryMultiLineString", EdmPrimitiveTypeKind.GeometryMultiLineString, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(GeometryMultiPolygon), "Edm.GeometryMultiPolygon", EdmPrimitiveTypeKind.GeometryMultiPolygon, new GeometryTypeConverter(), true);
            RegisterKnownType(typeof(DataServiceStreamLink), "Edm.Stream", EdmPrimitiveTypeKind.Stream, new NamedStreamTypeConverter(), false);
            RegisterKnownType(typeof(char), "Edm.String", EdmPrimitiveTypeKind.String, new CharTypeConverter(), false);
            RegisterKnownType(typeof(char[]), "Edm.String", EdmPrimitiveTypeKind.String, new CharArrayTypeConverter(), false);
            RegisterKnownType(typeof(Type), "Edm.String", EdmPrimitiveTypeKind.String, new ClrTypeConverter(), false);
            RegisterKnownType(typeof(Uri), "Edm.String", EdmPrimitiveTypeKind.String, new System.Data.Services.Client.UriTypeConverter(), false);
            RegisterKnownType(typeof(XDocument), "Edm.String", EdmPrimitiveTypeKind.String, new XDocumentTypeConverter(), false);
            RegisterKnownType(typeof(XElement), "Edm.String", EdmPrimitiveTypeKind.String, new XElementTypeConverter(), false);
            RegisterKnownType(typeof(ushort), null, EdmPrimitiveTypeKind.String, new UInt16TypeConverter(), false);
            RegisterKnownType(typeof(uint), null, EdmPrimitiveTypeKind.String, new UInt32TypeConverter(), false);
            RegisterKnownType(typeof(ulong), null, EdmPrimitiveTypeKind.String, new UInt64TypeConverter(), false);
            RegisterKnownType(typeof(BinaryTypeSub), "Edm.Binary", EdmPrimitiveTypeKind.Binary, new BinaryTypeConverter(), false);
        }

        private static bool IsBinaryType(Type type)
        {
            if (((BinaryTypeConverter.BinaryType == null) && (type.Name == "Binary")) && ((type.Namespace == "System.Data.Linq") && AssemblyName.ReferenceMatchesDefinition(type.Assembly.GetName(), new AssemblyName("System.Data.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))))
            {
                BinaryTypeConverter.BinaryType = type;
            }
            return (type == BinaryTypeConverter.BinaryType);
        }

        internal static bool IsKnownNullableType(Type type)
        {
            return IsKnownType(Nullable.GetUnderlyingType(type) ?? type);
        }

        internal static bool IsKnownType(Type type)
        {
            PrimitiveType type2;
            return TryGetPrimitiveType(type, out type2);
        }

        internal static void RegisterKnownType(Type clrType, string edmTypeName, EdmPrimitiveTypeKind primitiveKind, PrimitiveTypeConverter converter, bool twoWay)
        {
            PrimitiveType type = new PrimitiveType(clrType, edmTypeName, primitiveKind, converter, twoWay);
            clrMapping.Add(clrType, type);
            if (twoWay)
            {
                edmMapping.Add(edmTypeName, type);
            }
        }

        internal static bool TryGetPrimitiveType(string edmTypeName, out PrimitiveType ptype)
        {
            return edmMapping.TryGetValue(edmTypeName, out ptype);
        }

        internal static bool TryGetPrimitiveType(Type clrType, out PrimitiveType ptype)
        {
            Type type = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (!TryGetWellKnownPrimitiveType(type, out ptype))
            {
                KeyValuePair<Type, PrimitiveType>[] pairArray;
                lock (knownNonPrimitiveTypes)
                {
                    if (knownNonPrimitiveTypes.Contains(clrType))
                    {
                        ptype = null;
                        return false;
                    }
                }
                lock (derivedPrimitiveTypeMapping)
                {
                    if (derivedPrimitiveTypeMapping.TryGetValue(clrType, out ptype))
                    {
                        return true;
                    }
                    pairArray = (from m in clrMapping
                        where !m.Key.IsPrimitive() && !m.Key.IsSealed()
                        select m).Concat<KeyValuePair<Type, PrimitiveType>>(derivedPrimitiveTypeMapping).ToArray<KeyValuePair<Type, PrimitiveType>>();
                }
                KeyValuePair<Type, PrimitiveType> pair = new KeyValuePair<Type, PrimitiveType>(typeof(object), null);
                foreach (KeyValuePair<Type, PrimitiveType> pair2 in pairArray)
                {
                    if (type.IsSubclassOf(pair2.Key) && pair2.Key.IsSubclassOf(pair.Key))
                    {
                        pair = pair2;
                    }
                }
                if (pair.Value == null)
                {
                    lock (knownNonPrimitiveTypes)
                    {
                        knownNonPrimitiveTypes.Add(clrType);
                    }
                    return false;
                }
                ptype = pair.Value;
                lock (derivedPrimitiveTypeMapping)
                {
                    derivedPrimitiveTypeMapping[type] = ptype;
                }
            }
            return true;
        }

        private static bool TryGetWellKnownPrimitiveType(Type clrType, out PrimitiveType ptype)
        {
            ptype = null;
            if (!clrMapping.TryGetValue(clrType, out ptype) && IsBinaryType(clrType))
            {
                ptype = clrMapping[typeof(BinaryTypeSub)];
            }
            return (ptype != null);
        }

        internal Type ClrType { get; private set; }

        internal string EdmTypeName { get; private set; }

        internal bool HasReverseMapping { get; private set; }

        internal PrimitiveTypeConverter TypeConverter { get; private set; }

        private sealed class BinaryTypeSub
        {
        }

        private class ClientEdmPrimitiveType : EdmType, IEdmPrimitiveType, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
        {
            private readonly string name;
            private readonly string namespaceName;
            private readonly EdmPrimitiveTypeKind primitiveKind;

            private ClientEdmPrimitiveType(string namespaceName, string name, EdmPrimitiveTypeKind primitiveKind)
            {
                this.namespaceName = namespaceName;
                this.name = name;
                this.primitiveKind = primitiveKind;
            }

            public static IEdmPrimitiveType CreateType(EdmPrimitiveTypeKind primitiveKind)
            {
                return new PrimitiveType.ClientEdmPrimitiveType("Edm", primitiveKind.ToString(), primitiveKind);
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public string Namespace
            {
                get
                {
                    return this.namespaceName;
                }
            }

            public EdmPrimitiveTypeKind PrimitiveKind
            {
                get
                {
                    return this.primitiveKind;
                }
            }

            public EdmSchemaElementKind SchemaElementKind
            {
                get
                {
                    return EdmSchemaElementKind.TypeDefinition;
                }
            }

            public override EdmTypeKind TypeKind
            {
                get
                {
                    return EdmTypeKind.Primitive;
                }
            }
        }
    }
}

