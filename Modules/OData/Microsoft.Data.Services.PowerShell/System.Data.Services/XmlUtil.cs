namespace System.Data.Services
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static class XmlUtil
    {
        internal static XmlWriter CreateXmlWriterAndWriteProcessingInstruction(Stream stream, Encoding encoding)
        {
            XmlWriterSettings settings = CreateXmlWriterSettings(encoding);
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + encoding.WebName + "\" standalone=\"yes\"");
            return writer;
        }

        internal static XmlWriterSettings CreateXmlWriterSettings(Encoding encoding)
        {
            return new XmlWriterSettings { CheckCharacters = false, ConformanceLevel = ConformanceLevel.Fragment, Encoding = encoding, NewLineHandling = NewLineHandling.Entitize, NamespaceHandling = NamespaceHandling.OmitDuplicates };
        }

        internal static XmlWriter GetErrorXmlWriter(Stream stream, Encoding encoding, XmlWriter writer)
        {
            XmlWriter writer2 = writer;
            if ((writer2.WriteState != WriteState.Error) && (writer2.WriteState != WriteState.Closed))
            {
                return writer2;
            }
            return XmlWriter.Create(stream, CreateXmlWriterSettings(encoding));
        }
    }
}

