namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class ODataAtomServiceDocumentDeserializer : ODataAtomDeserializer
    {
        private readonly string AtomHRefAttributeName;
        private readonly string AtomNamespace;
        private readonly string AtomPublishingAcceptElementName;
        private readonly string AtomPublishingCategoriesElementName;
        private readonly string AtomPublishingCollectionElementName;
        private readonly string AtomPublishingNamespace;
        private readonly string AtomPublishingServiceElementName;
        private readonly string AtomPublishingWorkspaceElementName;
        private readonly string AtomTitleElementName;
        private readonly string EmptyNamespace;
        private ODataAtomServiceDocumentMetadataDeserializer serviceDocumentMetadataDeserializer;

        internal ODataAtomServiceDocumentDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.AtomPublishingServiceElementName = nameTable.Add("service");
            this.AtomPublishingWorkspaceElementName = nameTable.Add("workspace");
            this.AtomPublishingCollectionElementName = nameTable.Add("collection");
            this.AtomPublishingAcceptElementName = nameTable.Add("accept");
            this.AtomPublishingCategoriesElementName = nameTable.Add("categories");
            this.AtomHRefAttributeName = nameTable.Add("href");
            this.AtomPublishingNamespace = nameTable.Add("http://www.w3.org/2007/app");
            this.AtomNamespace = nameTable.Add("http://www.w3.org/2005/Atom");
            this.AtomTitleElementName = nameTable.Add("title");
            this.EmptyNamespace = nameTable.Add(string.Empty);
        }

        private ODataResourceCollectionInfo ReadCollectionElement()
        {
            ODataResourceCollectionInfo info = new ODataResourceCollectionInfo();
            string attribute = base.XmlReader.GetAttribute(this.AtomHRefAttributeName, this.EmptyNamespace);
            ValidationUtils.ValidateResourceCollectionInfoUrl(attribute);
            info.Url = base.ProcessUriFromPayload(attribute, base.XmlReader.XmlBaseUri);
            bool enableAtomMetadataReading = base.MessageReaderSettings.EnableAtomMetadataReading;
            AtomResourceCollectionMetadata collectionMetadata = null;
            if (enableAtomMetadataReading)
            {
                collectionMetadata = new AtomResourceCollectionMetadata();
            }
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.ReadStartElement();
                do
                {
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(this.AtomPublishingNamespace))
                            {
                                if (base.XmlReader.LocalNameEquals(this.AtomPublishingCategoriesElementName))
                                {
                                    if (enableAtomMetadataReading)
                                    {
                                        this.ServiceDocumentMetadataDeserializer.ReadCategoriesElementInCollection(collectionMetadata);
                                    }
                                    else
                                    {
                                        base.XmlReader.Skip();
                                    }
                                }
                                else
                                {
                                    if (!base.XmlReader.LocalNameEquals(this.AtomPublishingAcceptElementName))
                                    {
                                        throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_UnexpectedElementInResourceCollection(base.XmlReader.LocalName));
                                    }
                                    if (enableAtomMetadataReading)
                                    {
                                        this.ServiceDocumentMetadataDeserializer.ReadAcceptElementInCollection(collectionMetadata);
                                    }
                                    else
                                    {
                                        base.XmlReader.Skip();
                                    }
                                }
                            }
                            else if (base.XmlReader.NamespaceEquals(this.AtomNamespace))
                            {
                                if (enableAtomMetadataReading && base.XmlReader.LocalNameEquals(this.AtomTitleElementName))
                                {
                                    this.ServiceDocumentMetadataDeserializer.ReadTitleElementInCollection(collectionMetadata);
                                }
                                else
                                {
                                    base.XmlReader.Skip();
                                }
                            }
                            else
                            {
                                base.XmlReader.Skip();
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
            if (enableAtomMetadataReading)
            {
                info.SetAnnotation<AtomResourceCollectionMetadata>(collectionMetadata);
            }
            return info;
        }

        internal ODataWorkspace ReadServiceDocument()
        {
            base.ReadPayloadStart();
            if (!base.XmlReader.NamespaceEquals(this.AtomPublishingNamespace) || !base.XmlReader.LocalNameEquals(this.AtomPublishingServiceElementName))
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_ServiceDocumentRootElementWrongNameOrNamespace(base.XmlReader.LocalName, base.XmlReader.NamespaceURI));
            }
            ODataWorkspace workspace = null;
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.Read();
                workspace = this.ReadWorkspace();
            }
            if (workspace == null)
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_MissingWorkspaceElement);
            }
            this.SkipToElementInAtomPublishingNamespace();
            if (base.XmlReader.NodeType == XmlNodeType.Element)
            {
                if (base.XmlReader.LocalNameEquals(this.AtomPublishingWorkspaceElementName))
                {
                    throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_MultipleWorkspaceElementsFound);
                }
                throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_UnexpectedElementInServiceDocument(base.XmlReader.LocalName));
            }
            base.XmlReader.Read();
            base.ReadPayloadEnd();
            return workspace;
        }

        private ODataWorkspace ReadWorkspace()
        {
            bool enableAtomMetadataReading = base.AtomInputContext.MessageReaderSettings.EnableAtomMetadataReading;
            this.SkipToElementInAtomPublishingNamespace();
            if (base.XmlReader.NodeType == XmlNodeType.EndElement)
            {
                return null;
            }
            if (!base.XmlReader.LocalNameEquals(this.AtomPublishingWorkspaceElementName))
            {
                throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_UnexpectedElementInServiceDocument(base.XmlReader.LocalName));
            }
            List<ODataResourceCollectionInfo> sourceList = new List<ODataResourceCollectionInfo>();
            AtomWorkspaceMetadata workspaceMetadata = null;
            if (enableAtomMetadataReading)
            {
                workspaceMetadata = new AtomWorkspaceMetadata();
            }
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.ReadStartElement();
                do
                {
                    base.XmlReader.SkipInsignificantNodes();
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(this.AtomPublishingNamespace))
                            {
                                if (!base.XmlReader.LocalNameEquals(this.AtomPublishingCollectionElementName))
                                {
                                    throw new ODataException(Strings.ODataAtomServiceDocumentDeserializer_UnexpectedElementInWorkspace(base.XmlReader.LocalName));
                                }
                                ODataResourceCollectionInfo item = this.ReadCollectionElement();
                                sourceList.Add(item);
                            }
                            else if (enableAtomMetadataReading && base.XmlReader.NamespaceEquals(this.AtomNamespace))
                            {
                                if (base.XmlReader.LocalNameEquals(this.AtomTitleElementName))
                                {
                                    this.ServiceDocumentMetadataDeserializer.ReadTitleElementInWorkspace(workspaceMetadata);
                                }
                                else
                                {
                                    base.XmlReader.Skip();
                                }
                            }
                            else
                            {
                                base.XmlReader.Skip();
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
            ODataWorkspace workspace = new ODataWorkspace {
                Collections = new ReadOnlyEnumerable<ODataResourceCollectionInfo>(sourceList)
            };
            if (enableAtomMetadataReading)
            {
                workspace.SetAnnotation<AtomWorkspaceMetadata>(workspaceMetadata);
            }
            return workspace;
        }

        private void SkipToElementInAtomPublishingNamespace()
        {
        Label_0000:
            switch (base.XmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (base.XmlReader.NamespaceEquals(this.AtomPublishingNamespace))
                    {
                        return;
                    }
                    base.XmlReader.Skip();
                    goto Label_0000;

                case XmlNodeType.EndElement:
                    return;
            }
            base.XmlReader.Skip();
            goto Label_0000;
        }

        private ODataAtomServiceDocumentMetadataDeserializer ServiceDocumentMetadataDeserializer
        {
            get
            {
                if (this.serviceDocumentMetadataDeserializer == null)
                {
                    this.serviceDocumentMetadataDeserializer = new ODataAtomServiceDocumentMetadataDeserializer(base.AtomInputContext);
                }
                return this.serviceDocumentMetadataDeserializer;
            }
        }
    }
}

