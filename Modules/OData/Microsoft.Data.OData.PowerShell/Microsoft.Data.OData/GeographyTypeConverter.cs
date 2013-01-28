namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Json;
    using System;
    using System.Collections.Generic;
    using System.Spatial;
    using System.Xml;

    internal sealed class GeographyTypeConverter : IPrimitiveTypeConverter
    {
        public object TokenizeFromXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Geography geography = GmlFormatter.Create().Read<Geography>(reader);
            reader.SkipInsignificantNodes();
            return geography;
        }

        public void WriteAtom(object instance, XmlWriter writer)
        {
            ((Geography) instance).SendTo((GeographyPipeline) GmlFormatter.Create().CreateWriter(writer));
        }

        public void WriteJson(object instance, JsonWriter jsonWriter, string typeName, ODataVersion odataVersion)
        {
            IDictionary<string, object> jsonObjectValue = GeoJsonObjectFormatter.Create().Write((ISpatial) instance);
            jsonWriter.WriteJsonObjectValue(jsonObjectValue, typeName, odataVersion);
        }
    }
}

