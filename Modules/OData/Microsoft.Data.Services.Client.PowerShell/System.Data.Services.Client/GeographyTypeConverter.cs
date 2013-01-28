namespace System.Data.Services.Client
{
    using System.Spatial;
    using System.Xml;

    internal sealed class GeographyTypeConverter : PrimitiveTypeConverter
    {
        internal override PrimitiveParserToken TokenizeFromXml(XmlReader reader)
        {
            reader.ReadStartElement();
            return new InstancePrimitiveParserToken<Geography>(GmlFormatter.Create().Read<Geography>(reader));
        }
    }
}

