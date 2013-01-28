namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;

    internal static class XmlReaderExtensions
    {
        [Conditional("DEBUG")]
        internal static void AssertBuffering(this BufferingXmlReader bufferedXmlReader)
        {
        }

        [Conditional("DEBUG")]
        internal static void AssertNotBuffering(this BufferingXmlReader bufferedXmlReader)
        {
        }

        private static bool IsNullOrWhitespace(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        internal static bool LocalNameEquals(this XmlReader reader, string localName)
        {
            return object.ReferenceEquals(reader.LocalName, localName);
        }

        internal static bool NamespaceEquals(this XmlReader reader, string namespaceUri)
        {
            return object.ReferenceEquals(reader.NamespaceURI, namespaceUri);
        }

        internal static string ReadElementContentValue(this XmlReader reader)
        {
            reader.MoveToElement();
            string str = null;
            if (reader.IsEmptyElement)
            {
                return string.Empty;
            }
            StringBuilder builder = null;
            bool flag = false;
            while (!flag && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.SignificantWhitespace:
                    {
                        if (str != null)
                        {
                            break;
                        }
                        str = reader.Value;
                        continue;
                    }
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    {
                        continue;
                    }
                    case XmlNodeType.EndElement:
                    {
                        flag = true;
                        continue;
                    }
                    default:
                        throw new ODataException(Strings.XmlReaderExtension_InvalidNodeInStringValue(reader.NodeType));
                }
                if (builder == null)
                {
                    builder = new StringBuilder();
                    builder.Append(str);
                    builder.Append(reader.Value);
                }
                else
                {
                    builder.Append(reader.Value);
                }
            }
            if (builder != null)
            {
                return builder.ToString();
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        internal static string ReadElementValue(this XmlReader reader)
        {
            string str = reader.ReadElementContentValue();
            reader.Read();
            return str;
        }

        internal static string ReadFirstTextNodeValue(this XmlReader reader)
        {
            reader.MoveToElement();
            string str = null;
            if (!reader.IsEmptyElement)
            {
                bool flag = false;
                while (!flag && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            reader.SkipElementContent();
                            continue;
                        }
                        case XmlNodeType.Attribute:
                        {
                            continue;
                        }
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.SignificantWhitespace:
                            break;

                        case XmlNodeType.EndElement:
                        {
                            flag = true;
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    if (str == null)
                    {
                        str = reader.Value;
                    }
                }
            }
            reader.Read();
            return (str ?? string.Empty);
        }

        internal static void ReadPayloadEnd(this XmlReader reader)
        {
            reader.SkipInsignificantNodes();
            if ((reader.NodeType != XmlNodeType.None) && !reader.EOF)
            {
                throw new ODataException(Strings.XmlReaderExtension_InvalidRootNode(reader.NodeType));
            }
        }

        internal static void ReadPayloadStart(this XmlReader reader)
        {
            reader.SkipInsignificantNodes();
            if (reader.NodeType != XmlNodeType.Element)
            {
                throw new ODataException(Strings.XmlReaderExtension_InvalidRootNode(reader.NodeType));
            }
        }

        internal static void SkipElementContent(this XmlReader reader)
        {
            reader.MoveToElement();
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Skip();
                }
            }
        }

        internal static void SkipInsignificantNodes(this XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.None:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                        break;

                    case XmlNodeType.Attribute:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                        return;

                    case XmlNodeType.Text:
                        if (IsNullOrWhitespace(reader.Value))
                        {
                            break;
                        }
                        return;

                    default:
                        return;
                }
            }
            while (reader.Read());
        }

        internal static bool TryReadEmptyElement(this XmlReader reader)
        {
            reader.MoveToElement();
            return (reader.IsEmptyElement || (reader.Read() && (reader.NodeType == XmlNodeType.EndElement)));
        }

        internal static bool TryReadToNextElement(this XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

