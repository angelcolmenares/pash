namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class ODataAtomServiceDocumentMetadataDeserializer : ODataAtomMetadataDeserializer
    {
        private readonly string AtomCategoryElementName;
        private readonly string AtomCategoryLabelAttributeName;
        private readonly string AtomCategorySchemeAttributeName;
        private readonly string AtomCategoryTermAttributeName;
        private readonly string AtomHRefAttributeName;
        private readonly string AtomNamespace;
        private readonly string AtomPublishingFixedAttributeName;
        private readonly string EmptyNamespace;

        internal ODataAtomServiceDocumentMetadataDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.AtomNamespace = nameTable.Add("http://www.w3.org/2005/Atom");
            this.AtomCategoryElementName = nameTable.Add("category");
            this.AtomHRefAttributeName = nameTable.Add("href");
            this.AtomPublishingFixedAttributeName = nameTable.Add("fixed");
            this.AtomCategorySchemeAttributeName = nameTable.Add("scheme");
            this.AtomCategoryTermAttributeName = nameTable.Add("term");
            this.AtomCategoryLabelAttributeName = nameTable.Add("label");
            this.EmptyNamespace = nameTable.Add(string.Empty);
        }

        internal void ReadAcceptElementInCollection(AtomResourceCollectionMetadata collectionMetadata)
        {
            if (collectionMetadata.Accept != null)
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentMetadataDeserializer_MultipleAcceptElementsFoundInCollection);
            }
            collectionMetadata.Accept = base.XmlReader.ReadElementValue();
        }

        internal void ReadCategoriesElementInCollection(AtomResourceCollectionMetadata collectionMetadata)
        {
            AtomCategoriesMetadata metadata = new AtomCategoriesMetadata();
            List<AtomCategoryMetadata> sourceList = new List<AtomCategoryMetadata>();
            while (base.XmlReader.MoveToNextAttribute())
            {
                string uriFromPayload = base.XmlReader.Value;
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace))
                {
                    if (base.XmlReader.LocalNameEquals(this.AtomHRefAttributeName))
                    {
                        metadata.Href = base.ProcessUriFromPayload(uriFromPayload, base.XmlReader.XmlBaseUri);
                    }
                    else
                    {
                        if (base.XmlReader.LocalNameEquals(this.AtomPublishingFixedAttributeName))
                        {
                            if (string.CompareOrdinal(uriFromPayload, "yes") != 0)
                            {
                                if (string.CompareOrdinal(uriFromPayload, "no") != 0)
                                {
                                    throw new ODataException(Strings.ODataAtomServiceDocumentMetadataDeserializer_InvalidFixedAttributeValue(uriFromPayload));
                                }
                                metadata.Fixed = false;
                            }
                            else
                            {
                                metadata.Fixed = true;
                            }
                            continue;
                        }
                        if (base.XmlReader.LocalNameEquals(this.AtomCategorySchemeAttributeName))
                        {
                            metadata.Scheme = uriFromPayload;
                        }
                    }
                }
            }
            base.XmlReader.MoveToElement();
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.ReadStartElement();
                do
                {
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(this.AtomNamespace) && base.XmlReader.LocalNameEquals(this.AtomCategoryElementName))
                            {
                                sourceList.Add(this.ReadCategoryElementInCollection());
                            }
                            break;

                        case XmlNodeType.EndElement:
                            break;

                        default:
                            base.XmlReader.Skip();
                            break;
                    }
                }
                while (base.XmlReader.NodeType != XmlNodeType.EndElement);
            }
            base.XmlReader.Read();
            metadata.Categories = new ReadOnlyEnumerable<AtomCategoryMetadata>(sourceList);
            collectionMetadata.Categories = metadata;
        }

        private AtomCategoryMetadata ReadCategoryElementInCollection()
        {
            AtomCategoryMetadata metadata = new AtomCategoryMetadata();
            while (base.XmlReader.MoveToNextAttribute())
            {
                string str = base.XmlReader.Value;
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace))
                {
                    if (base.XmlReader.LocalNameEquals(this.AtomCategoryTermAttributeName))
                    {
                        metadata.Term = str;
                    }
                    else
                    {
                        if (base.XmlReader.LocalNameEquals(this.AtomCategorySchemeAttributeName))
                        {
                            metadata.Scheme = str;
                            continue;
                        }
                        if (base.XmlReader.LocalNameEquals(this.AtomCategoryLabelAttributeName))
                        {
                            metadata.Label = str;
                        }
                    }
                }
            }
            return metadata;
        }

        internal void ReadTitleElementInCollection(AtomResourceCollectionMetadata collectionMetadata)
        {
            if (collectionMetadata.Title != null)
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentMetadataDeserializer_MultipleTitleElementsFound("collection"));
            }
            collectionMetadata.Title = base.ReadTitleElement();
        }

        internal void ReadTitleElementInWorkspace(AtomWorkspaceMetadata workspaceMetadata)
        {
            if (workspaceMetadata.Title != null)
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentMetadataDeserializer_MultipleTitleElementsFound("workspace"));
            }
            workspaceMetadata.Title = base.ReadTitleElement();
        }
    }
}

