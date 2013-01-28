namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;

    internal abstract class ODataAtomMetadataSerializer : ODataAtomSerializer
    {
        internal ODataAtomMetadataSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        internal void WriteAtomLink(AtomLinkMetadata linkMetadata, string etag)
        {
            base.XmlWriter.WriteStartElement("", "link", "http://www.w3.org/2005/Atom");
            this.WriteAtomLinkAttributes(linkMetadata, etag);
            base.XmlWriter.WriteEndElement();
        }

        internal void WriteAtomLinkAttributes(AtomLinkMetadata linkMetadata, string etag)
        {
            string href = (linkMetadata.Href == null) ? null : base.UriToUrlAttributeValue(linkMetadata.Href);
            this.WriteAtomLinkMetadataAttributes(linkMetadata.Relation, href, linkMetadata.HrefLang, linkMetadata.Title, linkMetadata.MediaType, linkMetadata.Length);
            if (etag != null)
            {
                ODataAtomWriterUtils.WriteETag(base.XmlWriter, etag);
            }
        }

        private void WriteAtomLinkMetadataAttributes(string relation, string href, string hrefLang, string title, string mediaType, int? length)
        {
            if (relation != null)
            {
                base.XmlWriter.WriteAttributeString("rel", relation);
            }
            if (mediaType != null)
            {
                base.XmlWriter.WriteAttributeString("type", mediaType);
            }
            if (title != null)
            {
                base.XmlWriter.WriteAttributeString("title", title);
            }
            if (href == null)
            {
                throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkMustSpecifyHref);
            }
            base.XmlWriter.WriteAttributeString("href", href);
            if (hrefLang != null)
            {
                base.XmlWriter.WriteAttributeString("hreflang", hrefLang);
            }
            if (length.HasValue)
            {
                base.XmlWriter.WriteAttributeString("length", length.Value.ToString());
            }
        }

        internal void WriteCategory(AtomCategoryMetadata category)
        {
            this.WriteCategory("", category.Term, category.Scheme, category.Label);
        }

        internal void WriteCategory(string atomPrefix, string term, string scheme, string label)
        {
            base.XmlWriter.WriteStartElement(atomPrefix, "category", "http://www.w3.org/2005/Atom");
            if (term == null)
            {
                throw new ODataException(Strings.ODataAtomWriterMetadataUtils_CategoryMustSpecifyTerm);
            }
            base.XmlWriter.WriteAttributeString("term", term);
            if (scheme != null)
            {
                base.XmlWriter.WriteAttributeString("scheme", scheme);
            }
            if (label != null)
            {
                base.XmlWriter.WriteAttributeString("label", label);
            }
            base.XmlWriter.WriteEndElement();
        }

        internal void WriteEmptyAuthor()
        {
            base.XmlWriter.WriteStartElement("", "author", "http://www.w3.org/2005/Atom");
            base.WriteEmptyElement("", "name", "http://www.w3.org/2005/Atom");
            base.XmlWriter.WriteEndElement();
        }

        internal void WritePersonMetadata(AtomPersonMetadata personMetadata)
        {
            base.WriteElementWithTextContent("", "name", "http://www.w3.org/2005/Atom", personMetadata.Name);
            string uriFromEpm = personMetadata.UriFromEpm;
            if (uriFromEpm == null)
            {
                Uri uri = personMetadata.Uri;
                if (uri != null)
                {
                    uriFromEpm = base.UriToUrlAttributeValue(uri);
                }
            }
            if (uriFromEpm != null)
            {
                base.WriteElementWithTextContent("", "uri", "http://www.w3.org/2005/Atom", uriFromEpm);
            }
            string email = personMetadata.Email;
            if (email != null)
            {
                base.WriteElementWithTextContent("", "email", "http://www.w3.org/2005/Atom", email);
            }
        }

        internal void WriteTextConstruct(string prefix, string localName, string ns, AtomTextConstruct textConstruct)
        {
            base.XmlWriter.WriteStartElement(prefix, localName, ns);
            if (textConstruct != null)
            {
                AtomTextConstructKind kind = textConstruct.Kind;
                base.XmlWriter.WriteAttributeString("type", AtomValueUtils.ToString(textConstruct.Kind));
                string text = textConstruct.Text;
                if (text == null)
                {
                    text = string.Empty;
                }
                if (kind == AtomTextConstructKind.Xhtml)
                {
                    ODataAtomWriterUtils.WriteRaw(base.XmlWriter, text);
                }
                else
                {
                    ODataAtomWriterUtils.WriteString(base.XmlWriter, text);
                }
            }
            base.XmlWriter.WriteEndElement();
        }
    }
}

