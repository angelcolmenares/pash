namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataAtomEntityReferenceLinkSerializer : ODataAtomSerializer
    {
        internal ODataAtomEntityReferenceLinkSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        internal void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink)
        {
            base.WritePayloadStart();
            this.WriteEntityReferenceLink(entityReferenceLink, true);
            base.WritePayloadEnd();
        }

        private void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink, bool isTopLevel)
        {
            WriterValidationUtils.ValidateEntityReferenceLink(entityReferenceLink);
            string ns = (base.UseClientFormatBehavior && isTopLevel) ? "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" : base.MessageWriterSettings.WriterBehavior.ODataNamespace;
            base.XmlWriter.WriteStartElement(string.Empty, "uri", ns);
            if (isTopLevel)
            {
                base.XmlWriter.WriteAttributeString("xmlns", ns);
            }
            base.XmlWriter.WriteString(base.UriToUrlAttributeValue(entityReferenceLink.Url));
            base.XmlWriter.WriteEndElement();
        }

        internal void WriteEntityReferenceLinks(ODataEntityReferenceLinks entityReferenceLinks)
        {
            base.WritePayloadStart();
            base.XmlWriter.WriteStartElement(string.Empty, "links", base.MessageWriterSettings.WriterBehavior.ODataNamespace);
            base.XmlWriter.WriteAttributeString("xmlns", base.MessageWriterSettings.WriterBehavior.ODataNamespace);
            if (entityReferenceLinks.Count.HasValue)
            {
                base.WriteCount(entityReferenceLinks.Count.Value, true);
            }
            IEnumerable<ODataEntityReferenceLink> links = entityReferenceLinks.Links;
            if (links != null)
            {
                foreach (ODataEntityReferenceLink link in links)
                {
                    WriterValidationUtils.ValidateEntityReferenceLinkNotNull(link);
                    this.WriteEntityReferenceLink(link, false);
                }
            }
            if (entityReferenceLinks.NextPageLink != null)
            {
                string str = base.UriToUrlAttributeValue(entityReferenceLinks.NextPageLink);
                base.XmlWriter.WriteElementString(string.Empty, "next", base.MessageWriterSettings.WriterBehavior.ODataNamespace, str);
            }
            base.XmlWriter.WriteEndElement();
            base.WritePayloadEnd();
        }
    }
}

