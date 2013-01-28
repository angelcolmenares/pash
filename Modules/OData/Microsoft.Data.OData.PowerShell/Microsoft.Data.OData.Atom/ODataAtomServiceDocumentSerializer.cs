namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataAtomServiceDocumentSerializer : ODataAtomSerializer
    {
        private readonly ODataAtomServiceDocumentMetadataSerializer atomServiceDocumentMetadataSerializer;

        internal ODataAtomServiceDocumentSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
            this.atomServiceDocumentMetadataSerializer = new ODataAtomServiceDocumentMetadataSerializer(atomOutputContext);
        }

        internal void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            IEnumerable<ODataResourceCollectionInfo> collections = defaultWorkspace.Collections;
            base.WritePayloadStart();
            base.XmlWriter.WriteStartElement(string.Empty, "service", "http://www.w3.org/2007/app");
            if (base.MessageWriterSettings.BaseUri != null)
            {
                base.XmlWriter.WriteAttributeString("base", "http://www.w3.org/XML/1998/namespace", base.MessageWriterSettings.BaseUri.AbsoluteUri);
            }
            base.XmlWriter.WriteAttributeString("xmlns", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2007/app");
            base.XmlWriter.WriteAttributeString("atom", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2005/Atom");
            base.XmlWriter.WriteStartElement(string.Empty, "workspace", "http://www.w3.org/2007/app");
            this.atomServiceDocumentMetadataSerializer.WriteWorkspaceMetadata(defaultWorkspace);
            if (collections != null)
            {
                foreach (ODataResourceCollectionInfo info in collections)
                {
                    ValidationUtils.ValidateResourceCollectionInfo(info);
                    base.XmlWriter.WriteStartElement(string.Empty, "collection", "http://www.w3.org/2007/app");
                    base.XmlWriter.WriteAttributeString("href", base.UriToUrlAttributeValue(info.Url));
                    this.atomServiceDocumentMetadataSerializer.WriteResourceCollectionMetadata(info);
                    base.XmlWriter.WriteEndElement();
                }
            }
            base.XmlWriter.WriteEndElement();
            base.XmlWriter.WriteEndElement();
            base.WritePayloadEnd();
        }
    }
}

