namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static class ODataAtomReaderUtils
    {
        internal static XmlReader CreateXmlReader(Stream stream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings)
        {
            XmlReaderSettings settings = CreateXmlReaderSettings(messageReaderSettings);
            if (encoding != null)
            {
                return XmlReader.Create(new StreamReader(stream, encoding), settings);
            }
            return XmlReader.Create(stream, settings);
        }

        private static XmlReaderSettings CreateXmlReaderSettings(ODataMessageReaderSettings messageReaderSettings)
        {
            return new XmlReaderSettings { CheckCharacters = messageReaderSettings.CheckCharacters, ConformanceLevel = ConformanceLevel.Document, CloseInput = true, DtdProcessing = DtdProcessing.Prohibit };
        }

        internal static bool ReadMetadataNullAttributeValue(string attributeValue)
        {
            return XmlConvert.ToBoolean(attributeValue);
        }
    }
}

