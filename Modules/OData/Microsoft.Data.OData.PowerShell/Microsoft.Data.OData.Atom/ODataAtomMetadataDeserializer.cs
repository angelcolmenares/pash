namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal abstract class ODataAtomMetadataDeserializer : ODataAtomDeserializer
    {
        private readonly string AtomNamespace;
        private readonly string EmptyNamespace;

        internal ODataAtomMetadataDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.EmptyNamespace = nameTable.Add(string.Empty);
            this.AtomNamespace = nameTable.Add("http://www.w3.org/2005/Atom");
        }

        protected DateTimeOffset? ReadAtomDateConstruct()
        {
            string input = this.ReadElementStringValue().Trim();
            if (input.Length >= 20)
            {
                DateTimeOffset offset;
                if (input[0x13] == '.')
                {
                    int startIndex = 20;
                    while ((input.Length > startIndex) && char.IsDigit(input[startIndex]))
                    {
                        startIndex++;
                    }
                    input = input.Substring(0, 0x13) + input.Substring(startIndex);
                }
                if (DateTimeOffset.TryParseExact(input, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out offset))
                {
                    return new DateTimeOffset?(offset);
                }
                if (DateTimeOffset.TryParseExact(input, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out offset))
                {
                    return new DateTimeOffset?(offset);
                }
            }
            return new DateTimeOffset?(XmlConvert.ToDateTimeOffset(input));
        }

        protected string ReadAtomDateConstructAsString()
        {
            return this.ReadElementStringValue();
        }

        protected AtomPersonMetadata ReadAtomPersonConstruct(EpmTargetPathSegment epmTargetPathSegment)
        {
            AtomPersonMetadata metadata = new AtomPersonMetadata();
            if (base.XmlReader.IsEmptyElement)
            {
                goto Label_011B;
            }
            base.XmlReader.Read();
        Label_0022:
            switch (base.XmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    EpmTargetPathSegment segment;
                    string str2;
                    if ((base.XmlReader.NamespaceEquals(this.AtomNamespace) && this.ShouldReadElement(epmTargetPathSegment, base.XmlReader.LocalName, out segment)) && ((str2 = base.XmlReader.LocalName) != null))
                    {
                        if (!(str2 == "name"))
                        {
                            if (str2 == "uri")
                            {
                                Uri xmlBaseUri = base.XmlReader.XmlBaseUri;
                                string uriFromPayload = this.ReadElementStringValue();
                                if (segment != null)
                                {
                                    metadata.UriFromEpm = uriFromPayload;
                                }
                                if (this.ReadAtomMetadata)
                                {
                                    metadata.Uri = base.ProcessUriFromPayload(uriFromPayload, xmlBaseUri);
                                }
                                goto Label_0109;
                            }
                            if (str2 == "email")
                            {
                                metadata.Email = this.ReadElementStringValue();
                                goto Label_0109;
                            }
                        }
                        else
                        {
                            metadata.Name = this.ReadElementStringValue();
                            goto Label_0109;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    goto Label_0109;
            }
            base.XmlReader.Skip();
        Label_0109:
            if (base.XmlReader.NodeType != XmlNodeType.EndElement)
            {
                goto Label_0022;
            }
        Label_011B:
            base.XmlReader.Read();
            return metadata;
        }

        protected AtomTextConstruct ReadAtomTextConstruct()
        {
            AtomTextConstruct construct = new AtomTextConstruct();
            string str = null;
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace) && (string.CompareOrdinal(base.XmlReader.LocalName, "type") == 0))
                {
                    str = base.XmlReader.Value;
                }
            }
            base.XmlReader.MoveToElement();
            if (str == null)
            {
                construct.Kind = AtomTextConstructKind.Text;
            }
            else
            {
                string str2 = str;
                if (str2 == null)
                {
                    goto Label_00AE;
                }
                if (!(str2 == "text"))
                {
                    if (str2 == "html")
                    {
                        construct.Kind = AtomTextConstructKind.Html;
                        goto Label_00C5;
                    }
                    if (str2 == "xhtml")
                    {
                        construct.Kind = AtomTextConstructKind.Xhtml;
                        goto Label_00C5;
                    }
                    goto Label_00AE;
                }
                construct.Kind = AtomTextConstructKind.Text;
            }
            goto Label_00C5;
        Label_00AE:
            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryMetadataDeserializer_InvalidTextConstructKind(str, base.XmlReader.LocalName));
        Label_00C5:
            if (construct.Kind == AtomTextConstructKind.Xhtml)
            {
                construct.Text = base.XmlReader.ReadInnerXml();
                return construct;
            }
            construct.Text = this.ReadElementStringValue();
            return construct;
        }

        protected string ReadElementStringValue()
        {
            if (base.UseClientFormatBehavior)
            {
                return base.XmlReader.ReadFirstTextNodeValue();
            }
            return base.XmlReader.ReadElementValue();
        }

        protected AtomTextConstruct ReadTitleElement()
        {
            return this.ReadAtomTextConstruct();
        }

        protected bool ShouldReadElement(EpmTargetPathSegment parentSegment, string segmentName, out EpmTargetPathSegment subSegment)
        {
            Func<EpmTargetPathSegment, bool> predicate = null;
            subSegment = null;
            if (parentSegment != null)
            {
                if (predicate == null)
                {
                    predicate = segment => string.CompareOrdinal(segment.SegmentName, segmentName) == 0;
                }
                subSegment = parentSegment.SubSegments.FirstOrDefault<EpmTargetPathSegment>(predicate);
                if (((subSegment != null) && (subSegment.EpmInfo != null)) && subSegment.EpmInfo.Attribute.KeepInContent)
                {
                    return this.ReadAtomMetadata;
                }
            }
            if (subSegment == null)
            {
                return this.ReadAtomMetadata;
            }
            return true;
        }

        protected bool ReadAtomMetadata
        {
            get
            {
                return base.AtomInputContext.MessageReaderSettings.EnableAtomMetadataReading;
            }
        }
    }
}

