namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Json;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Xml;

    internal sealed class PrimitiveConverter
    {
        private static readonly IPrimitiveTypeConverter geographyTypeConverter = new GeographyTypeConverter();
        private static readonly IPrimitiveTypeConverter geometryTypeConverter = new GeometryTypeConverter();
        private static readonly PrimitiveConverter primitiveConverter;
        private readonly Dictionary<Type, IPrimitiveTypeConverter> spatialPrimitiveTypeConverters = new Dictionary<Type, IPrimitiveTypeConverter>(EqualityComparer<Type>.Default);

        static PrimitiveConverter()
        {
            KeyValuePair<Type, IPrimitiveTypeConverter>[] spatialPrimitiveTypeConverters = new KeyValuePair<Type, IPrimitiveTypeConverter>[] { new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyPoint), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyLineString), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyPolygon), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyCollection), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyMultiPoint), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyMultiLineString), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeographyMultiPolygon), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(Geography), geographyTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryPoint), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryLineString), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryPolygon), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryCollection), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryMultiPoint), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryMultiLineString), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(GeometryMultiPolygon), geometryTypeConverter), new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(Geometry), geometryTypeConverter) };
            primitiveConverter = new PrimitiveConverter(spatialPrimitiveTypeConverters);
        }

        internal PrimitiveConverter(KeyValuePair<Type, IPrimitiveTypeConverter>[] spatialPrimitiveTypeConverters)
        {
            foreach (KeyValuePair<Type, IPrimitiveTypeConverter> pair in spatialPrimitiveTypeConverters)
            {
                this.spatialPrimitiveTypeConverters.Add(pair.Key, pair.Value);
            }
        }

        private bool TryGetConverter(Type type, out IPrimitiveTypeConverter primitiveTypeConverter)
        {
            if (typeof(ISpatial).IsAssignableFrom(type))
            {
                KeyValuePair<Type, IPrimitiveTypeConverter> pair = new KeyValuePair<Type, IPrimitiveTypeConverter>(typeof(object), null);
                foreach (KeyValuePair<Type, IPrimitiveTypeConverter> pair2 in this.spatialPrimitiveTypeConverters)
                {
                    if (pair2.Key.IsAssignableFrom(type) && pair.Key.IsAssignableFrom(pair2.Key))
                    {
                        pair = pair2;
                    }
                }
                primitiveTypeConverter = pair.Value;
                return (pair.Value != null);
            }
            primitiveTypeConverter = null;
            return false;
        }

        internal bool TryTokenizeFromXml(XmlReader reader, Type targetType, out object tokenizedPropertyValue)
        {
            IPrimitiveTypeConverter converter;
            tokenizedPropertyValue = null;
            if (this.TryGetConverter(targetType, out converter))
            {
                tokenizedPropertyValue = converter.TokenizeFromXml(reader);
                return true;
            }
            return false;
        }

        internal bool TryWriteAtom(object instance, XmlWriter writer)
        {
            return this.TryWriteValue(instance, delegate (IPrimitiveTypeConverter ptc) {
                ptc.WriteAtom(instance, writer);
            });
        }

        private bool TryWriteValue(object instance, Action<IPrimitiveTypeConverter> writeMethod)
        {
            IPrimitiveTypeConverter converter;
            Type type = instance.GetType();
            if (this.TryGetConverter(type, out converter))
            {
                writeMethod(converter);
                return true;
            }
            return false;
        }

        internal void WriteJson(object instance, JsonWriter jsonWriter, string typeName, ODataVersion odataVersion)
        {
            IPrimitiveTypeConverter converter;
            Type type = instance.GetType();
            this.TryGetConverter(type, out converter);
            converter.WriteJson(instance, jsonWriter, typeName, odataVersion);
        }

        internal static PrimitiveConverter Instance
        {
            get
            {
                return primitiveConverter;
            }
        }
    }
}

