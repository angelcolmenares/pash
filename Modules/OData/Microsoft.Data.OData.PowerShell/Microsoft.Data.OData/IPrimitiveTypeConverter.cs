namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Json;
    using System;
    using System.Xml;

    internal interface IPrimitiveTypeConverter
    {
        object TokenizeFromXml(XmlReader reader);
        void WriteAtom(object instance, XmlWriter writer);
        void WriteJson(object instance, JsonWriter jsonWriter, string typeName, ODataVersion odataVersion);
    }
}

