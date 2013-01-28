namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class ODataAtomEntityReferenceLinkDeserializer : ODataAtomDeserializer
    {
        private readonly string ODataCountElementName;
        private readonly string ODataLinksElementName;
        private readonly string ODataNextElementName;
        private readonly string ODataUriElementName;

        internal ODataAtomEntityReferenceLinkDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.ODataLinksElementName = nameTable.Add("links");
            this.ODataCountElementName = nameTable.Add("count");
            this.ODataNextElementName = nameTable.Add("next");
            this.ODataUriElementName = nameTable.Add("uri");
        }

        internal ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            base.ReadPayloadStart();
            if ((!base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace) && !base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace)) || !base.XmlReader.LocalNameEquals(this.ODataUriElementName))
            {
                throw new ODataException(Strings.ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinkStartElement(base.XmlReader.LocalName, base.XmlReader.NamespaceURI));
            }
            ODataEntityReferenceLink link = this.ReadUriElement();
            base.ReadPayloadEnd();
            return link;
        }

        internal ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            base.ReadPayloadStart();
            if (!base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace) || !base.XmlReader.LocalNameEquals(this.ODataLinksElementName))
            {
                throw new ODataException(Strings.ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinksStartElement(base.XmlReader.LocalName, base.XmlReader.NamespaceURI));
            }
            ODataEntityReferenceLinks links = this.ReadLinksElement();
            base.ReadPayloadEnd();
            return links;
        }

        private ODataEntityReferenceLinks ReadLinksElement()
        {
            ODataEntityReferenceLinks links = new ODataEntityReferenceLinks();
            List<ODataEntityReferenceLink> sourceList = new List<ODataEntityReferenceLink>();
            DuplicateEntityReferenceLinksElementBitMask none = DuplicateEntityReferenceLinksElementBitMask.None;
            if (base.XmlReader.IsEmptyElement)
            {
                goto Label_018C;
            }
            base.XmlReader.Read();
        Label_002A:
            switch (base.XmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if ((base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace) && base.XmlReader.LocalNameEquals(this.ODataCountElementName)) && (base.Version >= ODataVersion.V2))
                    {
                        VerifyEntityReferenceLinksElementNotFound(ref none, DuplicateEntityReferenceLinksElementBitMask.Count, base.XmlReader.ODataMetadataNamespace, "count");
                        long num = (long) AtomValueUtils.ReadPrimitiveValue(base.XmlReader, EdmCoreModel.Instance.GetInt64(false));
                        links.Count = new long?(num);
                        base.XmlReader.Read();
                        goto Label_017A;
                    }
                    if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
                    {
                        if (base.XmlReader.LocalNameEquals(this.ODataUriElementName))
                        {
                            ODataEntityReferenceLink item = this.ReadUriElement();
                            sourceList.Add(item);
                            goto Label_017A;
                        }
                        if (base.XmlReader.LocalNameEquals(this.ODataNextElementName) && (base.Version >= ODataVersion.V2))
                        {
                            VerifyEntityReferenceLinksElementNotFound(ref none, DuplicateEntityReferenceLinksElementBitMask.NextLink, base.XmlReader.ODataNamespace, "next");
                            Uri xmlBaseUri = base.XmlReader.XmlBaseUri;
                            string uriFromPayload = base.XmlReader.ReadElementValue();
                            links.NextPageLink = base.ProcessUriFromPayload(uriFromPayload, xmlBaseUri);
                            goto Label_017A;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    goto Label_017A;
            }
            base.XmlReader.Skip();
        Label_017A:
            if (base.XmlReader.NodeType != XmlNodeType.EndElement)
            {
                goto Label_002A;
            }
        Label_018C:
            base.XmlReader.Read();
            links.Links = new ReadOnlyEnumerable<ODataEntityReferenceLink>(sourceList);
            return links;
        }

        private ODataEntityReferenceLink ReadUriElement()
        {
            ODataEntityReferenceLink link = new ODataEntityReferenceLink();
            Uri xmlBaseUri = base.XmlReader.XmlBaseUri;
            string uriFromPayload = base.XmlReader.ReadElementValue();
            Uri uri2 = base.ProcessUriFromPayload(uriFromPayload, xmlBaseUri);
            link.Url = uri2;
            ReaderValidationUtils.ValidateEntityReferenceLink(link);
            return link;
        }

        private static void VerifyEntityReferenceLinksElementNotFound(ref DuplicateEntityReferenceLinksElementBitMask elementsFoundBitField, DuplicateEntityReferenceLinksElementBitMask elementFoundBitMask, string elementNamespace, string elementName)
        {
            if ((elementsFoundBitField & elementFoundBitMask) == elementFoundBitMask)
            {
                throw new ODataException(Strings.ODataAtomEntityReferenceLinkDeserializer_MultipleEntityReferenceLinksElementsWithSameName(elementNamespace, elementName));
            }
            elementsFoundBitField |= elementFoundBitMask;
        }

        [Flags]
        private enum DuplicateEntityReferenceLinksElementBitMask
        {
            None,
            Count,
            NextLink
        }
    }
}

