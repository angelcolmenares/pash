namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Xml;

    internal class ODataAtomSerializer : ODataSerializer
    {
        private ODataAtomOutputContext atomOutputContext;

        internal ODataAtomSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
            this.atomOutputContext = atomOutputContext;
        }

        internal string UriToUrlAttributeValue(Uri uri)
        {
            return this.UriToUrlAttributeValue(uri, true);
        }

        internal string UriToUrlAttributeValue(Uri uri, bool failOnRelativeUriWithoutBaseUri)
        {
            if (base.UrlResolver != null)
            {
                Uri uri2 = base.UrlResolver.ResolveUrl(base.MessageWriterSettings.BaseUri, uri);
                if (uri2 != null)
                {
                    return UriUtilsCommon.UriToString(uri2);
                }
            }
            if (!uri.IsAbsoluteUri)
            {
                if ((base.MessageWriterSettings.BaseUri == null) && failOnRelativeUriWithoutBaseUri)
                {
                    throw new ODataException(Strings.ODataWriter_RelativeUriUsedWithoutBaseUriSpecified(UriUtilsCommon.UriToString(uri)));
                }
                uri = UriUtils.EnsureEscapedRelativeUri(uri);
            }
            return UriUtilsCommon.UriToString(uri);
        }

        internal void WriteBaseUriAndDefaultNamespaceAttributes()
        {
            Uri baseUri = base.MessageWriterSettings.BaseUri;
            if (baseUri != null)
            {
                this.XmlWriter.WriteAttributeString("base", "http://www.w3.org/XML/1998/namespace", baseUri.AbsoluteUri);
            }
            this.WriteDefaultNamespaceAttributes(DefaultNamespaceFlags.All);
        }

        internal void WriteCount(long count, bool includeNamespaceDeclaration)
        {
            this.XmlWriter.WriteStartElement("m", "count", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            if (includeNamespaceDeclaration)
            {
                this.WriteDefaultNamespaceAttributes(DefaultNamespaceFlags.ODataMetadata);
            }
            this.XmlWriter.WriteValue(count);
            this.XmlWriter.WriteEndElement();
        }

        internal void WriteDefaultNamespaceAttributes(DefaultNamespaceFlags flags)
        {
            if ((flags & DefaultNamespaceFlags.Atom) == DefaultNamespaceFlags.Atom)
            {
                this.XmlWriter.WriteAttributeString("xmlns", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2005/Atom");
            }
            if ((flags & DefaultNamespaceFlags.OData) == DefaultNamespaceFlags.OData)
            {
                this.XmlWriter.WriteAttributeString("d", "http://www.w3.org/2000/xmlns/", base.MessageWriterSettings.WriterBehavior.ODataNamespace);
            }
            if ((flags & DefaultNamespaceFlags.ODataMetadata) == DefaultNamespaceFlags.ODataMetadata)
            {
                this.XmlWriter.WriteAttributeString("m", "http://www.w3.org/2000/xmlns/", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            }
            if (((ODataVersion) base.MessageWriterSettings.Version.Value) >= ODataVersion.V3)
            {
                if ((flags & DefaultNamespaceFlags.GeoRss) == DefaultNamespaceFlags.GeoRss)
                {
                    this.XmlWriter.WriteAttributeString("georss", "http://www.w3.org/2000/xmlns/", "http://www.georss.org/georss");
                }
                if ((flags & DefaultNamespaceFlags.Gml) == DefaultNamespaceFlags.Gml)
                {
                    this.XmlWriter.WriteAttributeString("gml", "http://www.w3.org/2000/xmlns/", "http://www.opengis.net/gml");
                }
            }
        }

        internal void WriteElementWithTextContent(string prefix, string localName, string ns, string textContent)
        {
            this.XmlWriter.WriteStartElement(prefix, localName, ns);
            if (textContent != null)
            {
                ODataAtomWriterUtils.WriteString(this.XmlWriter, textContent);
            }
            this.XmlWriter.WriteEndElement();
        }

        internal void WriteEmptyElement(string prefix, string localName, string ns)
        {
            this.XmlWriter.WriteStartElement(prefix, localName, ns);
            this.XmlWriter.WriteEndElement();
        }

        internal void WritePayloadEnd()
        {
            this.XmlWriter.WriteEndDocument();
        }

        internal void WritePayloadStart()
        {
            this.XmlWriter.WriteStartDocument();
        }

        internal void WriteTopLevelError(ODataError error, bool includeDebugInformation)
        {
            this.WritePayloadStart();
            ODataAtomWriterUtils.WriteError(this.XmlWriter, error, includeDebugInformation, base.MessageWriterSettings.MessageQuotas.MaxNestingDepth);
            this.WritePayloadEnd();
        }

        protected ODataAtomOutputContext AtomOutputContext
        {
            get
            {
                return this.atomOutputContext;
            }
        }

        internal System.Xml.XmlWriter XmlWriter
        {
            get
            {
                return this.atomOutputContext.XmlWriter;
            }
        }

        [Flags]
        internal enum DefaultNamespaceFlags
        {
            All = 0x1f,
            Atom = 4,
            GeoRss = 8,
            Gml = 0x10,
            None = 0,
            OData = 1,
            ODataMetadata = 2
        }
    }
}

