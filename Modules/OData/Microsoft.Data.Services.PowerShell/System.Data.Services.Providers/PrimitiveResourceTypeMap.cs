using System.Xml.Linq;
using System.Data.Linq;
using System.IO;

namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Linq;
	using System.Spatial;

    internal class PrimitiveResourceTypeMap
    {
        private static readonly KeyValuePair<Type, string>[] builtInTypesMapping = new KeyValuePair<Type, string>[] { 
            new KeyValuePair<Type, string>(typeof(string), "Edm.String"), new KeyValuePair<Type, string>(typeof(bool), "Edm.Boolean"), new KeyValuePair<Type, string>(typeof(bool?), "Edm.Boolean"), new KeyValuePair<Type, string>(typeof(byte), "Edm.Byte"), new KeyValuePair<Type, string>(typeof(byte?), "Edm.Byte"), new KeyValuePair<Type, string>(typeof(DateTime), "Edm.DateTime"), new KeyValuePair<Type, string>(typeof(DateTime?), "Edm.DateTime"), new KeyValuePair<Type, string>(typeof(decimal), "Edm.Decimal"), new KeyValuePair<Type, string>(typeof(decimal?), "Edm.Decimal"), new KeyValuePair<Type, string>(typeof(double), "Edm.Double"), new KeyValuePair<Type, string>(typeof(double?), "Edm.Double"), new KeyValuePair<Type, string>(typeof(Guid), "Edm.Guid"), new KeyValuePair<Type, string>(typeof(Guid?), "Edm.Guid"), new KeyValuePair<Type, string>(typeof(short), "Edm.Int16"), new KeyValuePair<Type, string>(typeof(short?), "Edm.Int16"), new KeyValuePair<Type, string>(typeof(int), "Edm.Int32"), 
            new KeyValuePair<Type, string>(typeof(int?), "Edm.Int32"), new KeyValuePair<Type, string>(typeof(long), "Edm.Int64"), new KeyValuePair<Type, string>(typeof(long?), "Edm.Int64"), new KeyValuePair<Type, string>(typeof(sbyte), "Edm.SByte"), new KeyValuePair<Type, string>(typeof(sbyte?), "Edm.SByte"), new KeyValuePair<Type, string>(typeof(float), "Edm.Single"), new KeyValuePair<Type, string>(typeof(float?), "Edm.Single"), new KeyValuePair<Type, string>(typeof(byte[]), "Edm.Binary"), new KeyValuePair<Type, string>(typeof(Stream), "Edm.Stream"), new KeyValuePair<Type, string>(typeof(Geography), "Edm.Geography"), new KeyValuePair<Type, string>(typeof(GeographyPoint), "Edm.GeographyPoint"), new KeyValuePair<Type, string>(typeof(GeographyLineString), "Edm.GeographyLineString"), new KeyValuePair<Type, string>(typeof(GeographyPolygon), "Edm.GeographyPolygon"), new KeyValuePair<Type, string>(typeof(GeographyCollection), "Edm.GeographyCollection"), new KeyValuePair<Type, string>(typeof(GeographyMultiLineString), "Edm.GeographyMultiLineString"), new KeyValuePair<Type, string>(typeof(GeographyMultiPoint), "Edm.GeographyMultiPoint"), 
            new KeyValuePair<Type, string>(typeof(GeographyMultiPolygon), "Edm.GeographyMultiPolygon"), new KeyValuePair<Type, string>(typeof(Geometry), "Edm.Geometry"), new KeyValuePair<Type, string>(typeof(GeometryPoint), "Edm.GeometryPoint"), new KeyValuePair<Type, string>(typeof(GeometryLineString), "Edm.GeometryLineString"), new KeyValuePair<Type, string>(typeof(GeometryPolygon), "Edm.GeometryPolygon"), new KeyValuePair<Type, string>(typeof(GeometryCollection), "Edm.GeometryCollection"), new KeyValuePair<Type, string>(typeof(GeometryMultiLineString), "Edm.GeometryMultiLineString"), new KeyValuePair<Type, string>(typeof(GeometryMultiPoint), "Edm.GeometryMultiPoint"), new KeyValuePair<Type, string>(typeof(GeometryMultiPolygon), "Edm.GeometryMultiPolygon"), new KeyValuePair<Type, string>(typeof(TimeSpan), "Edm.Time"), new KeyValuePair<Type, string>(typeof(TimeSpan?), "Edm.Time"), new KeyValuePair<Type, string>(typeof(DateTimeOffset), "Edm.DateTimeOffset"), new KeyValuePair<Type, string>(typeof(DateTimeOffset?), "Edm.DateTimeOffset"), new KeyValuePair<Type, string>(typeof(XElement), "Edm.String"), new KeyValuePair<Type, string>(typeof(Binary), "Edm.Binary")
         };
        private static readonly Type[] inheritablePrimitiveClrTypes = new Type[] { typeof(Geography), typeof(GeographyPoint), typeof(GeographyLineString), typeof(GeographyPolygon), typeof(GeographyCollection), typeof(GeographyMultiPoint), typeof(GeographyMultiLineString), typeof(GeographyMultiPolygon), typeof(Geometry), typeof(GeometryPoint), typeof(GeometryLineString), typeof(GeometryPolygon), typeof(GeometryCollection), typeof(GeometryMultiPoint), typeof(GeometryMultiLineString), typeof(GeometryMultiPolygon) };
        private readonly ResourceType[] inheritablePrimitiveResourceTypes;
        private readonly ResourceType[] primitiveResourceTypes;

        internal PrimitiveResourceTypeMap() : this(builtInTypesMapping)
        {
        }

        internal PrimitiveResourceTypeMap(KeyValuePair<Type, string>[] primitiveTypesEdmNameMapping)
        {
            int length = primitiveTypesEdmNameMapping.Length;
            this.primitiveResourceTypes = new ResourceType[length];
            List<ResourceType> list = new List<ResourceType>(inheritablePrimitiveClrTypes.Length);
            for (int i = 0; i < length; i++)
            {
                string name = primitiveTypesEdmNameMapping[i].Value.Substring("Edm".Length + 1);
                this.primitiveResourceTypes[i] = new ResourceType(primitiveTypesEdmNameMapping[i].Key, ResourceTypeKind.Primitive, "Edm", name);
                if (inheritablePrimitiveClrTypes.Contains<Type>(primitiveTypesEdmNameMapping[i].Key))
                {
                    list.Add(this.primitiveResourceTypes[i]);
                }
            }
            this.inheritablePrimitiveResourceTypes = list.ToArray();
        }

        internal ResourceType GetPrimitive(string fullEdmTypeName)
        {
            return this.primitiveResourceTypes.FirstOrDefault<ResourceType>(rt => (rt.FullName == fullEdmTypeName));
        }

        internal ResourceType GetPrimitive(Type type)
        {
            WebUtil.CheckArgumentNull<Type>(type, "type");
            ResourceType type2 = this.primitiveResourceTypes.FirstOrDefault<ResourceType>(rt => rt.InstanceType == type);
            if (type2 == null)
            {
                foreach (ResourceType type3 in this.inheritablePrimitiveResourceTypes)
                {
                    if (type3.InstanceType.IsAssignableFrom(type) && ((type2 == null) || type2.InstanceType.IsAssignableFrom(type3.InstanceType)))
                    {
                        type2 = type3;
                    }
                }
            }
            return type2;
        }

        internal bool IsPrimitive(Type type)
        {
            return (this.GetPrimitive(type) != null);
        }

        internal ResourceType[] AllPrimitives
        {
            get
            {
                return this.primitiveResourceTypes;
            }
        }
    }
}

