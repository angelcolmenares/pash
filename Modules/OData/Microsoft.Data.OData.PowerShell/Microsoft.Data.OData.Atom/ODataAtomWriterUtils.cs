namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static class ODataAtomWriterUtils
    {
        internal static XmlWriter CreateXmlWriter(Stream stream, ODataMessageWriterSettings messageWriterSettings, Encoding encoding)
        {
            XmlWriterSettings settings = CreateXmlWriterSettings(messageWriterSettings, encoding);
            return XmlWriter.Create(stream, settings);
        }

        private static XmlWriterSettings CreateXmlWriterSettings(ODataMessageWriterSettings messageWriterSettings, Encoding encoding)
        {
            return new XmlWriterSettings { CheckCharacters = messageWriterSettings.CheckCharacters, ConformanceLevel = ConformanceLevel.Document, OmitXmlDeclaration = false, Encoding = encoding ?? MediaTypeUtils.EncodingUtf8NoPreamble, NewLineHandling = NewLineHandling.Entitize, Indent = messageWriterSettings.Indent, CloseOutput = false };
        }

        internal static void WriteError(XmlWriter writer, ODataError error, bool includeDebugInformation, int maxInnerErrorDepth)
        {
            ErrorUtils.WriteXmlError(writer, error, includeDebugInformation, maxInnerErrorDepth);
        }

        internal static void WriteETag(XmlWriter writer, string etag)
        {
            writer.WriteAttributeString("m", "etag", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", etag);
        }

        internal static void WriteNullAttribute(XmlWriter writer)
        {
            writer.WriteAttributeString("m", "null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
        }

        private static void WritePreserveSpaceAttributeIfNeeded(XmlWriter writer, string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if ((length > 0) && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[length - 1])))
                {
                    writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                }
            }
        }

        internal static void WriteRaw(XmlWriter writer, string value)
        {
            WritePreserveSpaceAttributeIfNeeded(writer, value);
            writer.WriteRaw(value);
        }

        internal static void WriteString(XmlWriter writer, string value)
        {
            WritePreserveSpaceAttributeIfNeeded(writer, value);
            writer.WriteString(value);
        }
    }
}

