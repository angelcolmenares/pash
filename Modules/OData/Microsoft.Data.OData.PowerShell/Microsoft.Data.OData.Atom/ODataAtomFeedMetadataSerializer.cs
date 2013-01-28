namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal sealed class ODataAtomFeedMetadataSerializer : ODataAtomMetadataSerializer
    {
        internal ODataAtomFeedMetadataSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        internal void WriteFeedMetadata(AtomFeedMetadata feedMetadata, ODataFeed feed, string updatedTime, out bool authorWritten)
        {
            string textContent = (feed == null) ? feedMetadata.SourceId : feed.Id;
            base.WriteElementWithTextContent("", "id", "http://www.w3.org/2005/Atom", textContent);
            base.WriteTextConstruct("", "title", "http://www.w3.org/2005/Atom", feedMetadata.Title);
            if (feedMetadata.Subtitle != null)
            {
                base.WriteTextConstruct("", "subtitle", "http://www.w3.org/2005/Atom", feedMetadata.Subtitle);
            }
            string str2 = feedMetadata.Updated.HasValue ? ODataAtomConvert.ToAtomString(feedMetadata.Updated.Value) : updatedTime;
            base.WriteElementWithTextContent("", "updated", "http://www.w3.org/2005/Atom", str2);
            AtomLinkMetadata selfLink = feedMetadata.SelfLink;
            if (selfLink != null)
            {
                AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(selfLink, "self", null, null, null);
                base.WriteAtomLink(linkMetadata, null);
            }
            IEnumerable<AtomLinkMetadata> links = feedMetadata.Links;
            if (links != null)
            {
                foreach (AtomLinkMetadata metadata3 in links)
                {
                    base.WriteAtomLink(metadata3, null);
                }
            }
            IEnumerable<AtomCategoryMetadata> categories = feedMetadata.Categories;
            if (categories != null)
            {
                foreach (AtomCategoryMetadata metadata4 in categories)
                {
                    base.WriteCategory(metadata4);
                }
            }
            Uri logo = feedMetadata.Logo;
            if (logo != null)
            {
                base.WriteElementWithTextContent("", "logo", "http://www.w3.org/2005/Atom", base.UriToUrlAttributeValue(logo));
            }
            if (feedMetadata.Rights != null)
            {
                base.WriteTextConstruct("", "rights", "http://www.w3.org/2005/Atom", feedMetadata.Rights);
            }
            IEnumerable<AtomPersonMetadata> contributors = feedMetadata.Contributors;
            if (contributors != null)
            {
                foreach (AtomPersonMetadata metadata5 in contributors)
                {
                    base.XmlWriter.WriteStartElement("", "contributor", "http://www.w3.org/2005/Atom");
                    base.WritePersonMetadata(metadata5);
                    base.XmlWriter.WriteEndElement();
                }
            }
            AtomGeneratorMetadata generator = feedMetadata.Generator;
            if (generator != null)
            {
                base.XmlWriter.WriteStartElement("", "generator", "http://www.w3.org/2005/Atom");
                if (generator.Uri != null)
                {
                    base.XmlWriter.WriteAttributeString("uri", base.UriToUrlAttributeValue(generator.Uri));
                }
                if (!string.IsNullOrEmpty(generator.Version))
                {
                    base.XmlWriter.WriteAttributeString("version", generator.Version);
                }
                ODataAtomWriterUtils.WriteString(base.XmlWriter, generator.Name);
                base.XmlWriter.WriteEndElement();
            }
            Uri icon = feedMetadata.Icon;
            if (icon != null)
            {
                base.WriteElementWithTextContent("", "icon", "http://www.w3.org/2005/Atom", base.UriToUrlAttributeValue(icon));
            }
            IEnumerable<AtomPersonMetadata> authors = feedMetadata.Authors;
            authorWritten = false;
            if (authors != null)
            {
                foreach (AtomPersonMetadata metadata7 in authors)
                {
                    authorWritten = true;
                    base.XmlWriter.WriteStartElement("", "author", "http://www.w3.org/2005/Atom");
                    base.WritePersonMetadata(metadata7);
                    base.XmlWriter.WriteEndElement();
                }
            }
        }
    }
}

