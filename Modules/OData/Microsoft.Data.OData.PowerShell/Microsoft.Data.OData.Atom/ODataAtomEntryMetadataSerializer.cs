namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataAtomEntryMetadataSerializer : ODataAtomMetadataSerializer
    {
        private ODataAtomFeedMetadataSerializer sourceMetadataSerializer;

        internal ODataAtomEntryMetadataSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        internal void WriteEntryMetadata(AtomEntryMetadata entryMetadata, AtomEntryMetadata epmEntryMetadata, string updatedTime)
        {
            AtomEntryMetadata metadata = ODataAtomWriterMetadataEpmMergeUtils.MergeCustomAndEpmEntryMetadata(entryMetadata, epmEntryMetadata, base.MessageWriterSettings.WriterBehavior);
            if (metadata == null)
            {
                base.WriteEmptyElement("", "title", "http://www.w3.org/2005/Atom");
                base.WriteElementWithTextContent("", "updated", "http://www.w3.org/2005/Atom", updatedTime);
                base.WriteEmptyAuthor();
            }
            else
            {
                base.WriteTextConstruct("", "title", "http://www.w3.org/2005/Atom", metadata.Title);
                AtomTextConstruct summary = metadata.Summary;
                if (summary != null)
                {
                    base.WriteTextConstruct("", "summary", "http://www.w3.org/2005/Atom", summary);
                }
                string textContent = base.UseClientFormatBehavior ? metadata.PublishedString : (metadata.Published.HasValue ? ODataAtomConvert.ToAtomString(metadata.Published.Value) : null);
                if (textContent != null)
                {
                    base.WriteElementWithTextContent("", "published", "http://www.w3.org/2005/Atom", textContent);
                }
                string str2 = base.UseClientFormatBehavior ? metadata.UpdatedString : (metadata.Updated.HasValue ? ODataAtomConvert.ToAtomString(metadata.Updated.Value) : null);
                str2 = str2 ?? updatedTime;
                base.WriteElementWithTextContent("", "updated", "http://www.w3.org/2005/Atom", str2);
                bool flag = false;
                IEnumerable<AtomPersonMetadata> authors = metadata.Authors;
                if (authors != null)
                {
                    foreach (AtomPersonMetadata metadata2 in authors)
                    {
                        if (metadata2 == null)
                        {
                            throw new ODataException(Strings.ODataAtomWriterMetadataUtils_AuthorMetadataMustNotContainNull);
                        }
                        base.XmlWriter.WriteStartElement("", "author", "http://www.w3.org/2005/Atom");
                        base.WritePersonMetadata(metadata2);
                        base.XmlWriter.WriteEndElement();
                        flag = true;
                    }
                }
                if (!flag)
                {
                    base.WriteEmptyAuthor();
                }
                IEnumerable<AtomPersonMetadata> contributors = metadata.Contributors;
                if (contributors != null)
                {
                    foreach (AtomPersonMetadata metadata3 in contributors)
                    {
                        if (metadata3 == null)
                        {
                            throw new ODataException(Strings.ODataAtomWriterMetadataUtils_ContributorMetadataMustNotContainNull);
                        }
                        base.XmlWriter.WriteStartElement("", "contributor", "http://www.w3.org/2005/Atom");
                        base.WritePersonMetadata(metadata3);
                        base.XmlWriter.WriteEndElement();
                    }
                }
                IEnumerable<AtomLinkMetadata> links = metadata.Links;
                if (links != null)
                {
                    foreach (AtomLinkMetadata metadata4 in links)
                    {
                        if (metadata4 == null)
                        {
                            throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkMetadataMustNotContainNull);
                        }
                        base.WriteAtomLink(metadata4, null);
                    }
                }
                IEnumerable<AtomCategoryMetadata> categories = metadata.Categories;
                if (categories != null)
                {
                    foreach (AtomCategoryMetadata metadata5 in categories)
                    {
                        if (metadata5 == null)
                        {
                            throw new ODataException(Strings.ODataAtomWriterMetadataUtils_CategoryMetadataMustNotContainNull);
                        }
                        base.WriteCategory(metadata5);
                    }
                }
                if (metadata.Rights != null)
                {
                    base.WriteTextConstruct("", "rights", "http://www.w3.org/2005/Atom", metadata.Rights);
                }
                AtomFeedMetadata source = metadata.Source;
                if (source != null)
                {
                    bool flag2;
                    base.XmlWriter.WriteStartElement("", "source", "http://www.w3.org/2005/Atom");
                    this.SourceMetadataSerializer.WriteFeedMetadata(source, null, updatedTime, out flag2);
                    base.XmlWriter.WriteEndElement();
                }
            }
        }

        private ODataAtomFeedMetadataSerializer SourceMetadataSerializer
        {
            get
            {
                return (this.sourceMetadataSerializer ?? (this.sourceMetadataSerializer = new ODataAtomFeedMetadataSerializer(base.AtomOutputContext)));
            }
        }
    }
}

