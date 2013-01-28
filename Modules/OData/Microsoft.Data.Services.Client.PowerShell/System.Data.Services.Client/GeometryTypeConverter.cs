namespace System.Data.Services.Client
{
    using System.Spatial;
    using System.Xml;

    internal sealed class GeometryTypeConverter : PrimitiveTypeConverter
    {
        internal override PrimitiveParserToken TokenizeFromXml(XmlReader reader)
        {
            reader.ReadStartElement();
            return new InstancePrimitiveParserToken<Geometry>(GmlFormatter.Create().Read<Geometry>(reader));
        }
    }
}

