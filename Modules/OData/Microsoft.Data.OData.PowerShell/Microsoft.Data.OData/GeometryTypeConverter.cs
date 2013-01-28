namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Json;
    using System;
    using System.Collections.Generic;
    using System.Spatial;
    using System.Xml;

    internal sealed class GeometryTypeConverter : IPrimitiveTypeConverter
    {
        public object TokenizeFromXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Geometry geometry = GmlFormatter.Create().Read<Geometry>(reader);
            reader.SkipInsignificantNodes();
            return geometry;
        }

        public void WriteAtom(object instance, XmlWriter writer)
        {
            ((Geometry) instance).SendTo((GeometryPipeline) GmlFormatter.Create().CreateWriter(writer));
        }

        public void WriteJson(object instance, JsonWriter jsonWriter, string typeName, ODataVersion odataVersion)
        {
            IDictionary<string, object> jsonObjectValue = GeoJsonObjectFormatter.Create().Write((ISpatial) instance);
            jsonWriter.WriteJsonObjectValue(jsonObjectValue, typeName, odataVersion);
        }
    }
}

