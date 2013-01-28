namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal sealed class ODataAtomFeedMetadataDeserializer : ODataAtomMetadataDeserializer
    {
        private readonly string EmptyNamespace;

        internal ODataAtomFeedMetadataDeserializer(ODataAtomInputContext atomInputContext, bool inSourceElement) : base(atomInputContext)
        {
            this.EmptyNamespace = base.XmlReader.NameTable.Add(string.Empty);
            this.InSourceElement = inSourceElement;
        }

        internal void ReadAtomElementAsFeedMetadata(AtomFeedMetadata atomFeedMetadata)
        {
            switch (base.XmlReader.LocalName)
            {
                case "author":
                    this.ReadAuthorElement(atomFeedMetadata);
                    return;

                case "category":
                    this.ReadCategoryElement(atomFeedMetadata);
                    return;

                case "contributor":
                    this.ReadContributorElement(atomFeedMetadata);
                    return;

                case "generator":
                    this.ReadGeneratorElement(atomFeedMetadata);
                    return;

                case "icon":
                    this.ReadIconElement(atomFeedMetadata);
                    return;

                case "id":
                    if (!this.InSourceElement)
                    {
                        base.XmlReader.Skip();
                        return;
                    }
                    this.ReadIdElementAsSourceId(atomFeedMetadata);
                    return;

                case "link":
                    this.ReadLinkElementIntoLinksCollection(atomFeedMetadata);
                    return;

                case "logo":
                    this.ReadLogoElement(atomFeedMetadata);
                    return;

                case "rights":
                    this.ReadRightsElement(atomFeedMetadata);
                    return;

                case "subtitle":
                    this.ReadSubtitleElement(atomFeedMetadata);
                    return;

                case "title":
                    this.ReadTitleElement(atomFeedMetadata);
                    return;

                case "updated":
                    this.ReadUpdatedElement(atomFeedMetadata);
                    return;
            }
            base.XmlReader.Skip();
        }

        internal AtomLinkMetadata ReadAtomLinkElementInFeed(string relation, string hrefStringValue)
        {
            AtomLinkMetadata metadata = new AtomLinkMetadata {
                Relation = relation,
                Href = (hrefStringValue == null) ? null : base.ProcessUriFromPayload(hrefStringValue, base.XmlReader.XmlBaseUri)
            };
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace))
                {
                    switch (base.XmlReader.LocalName)
                    {
                        case "type":
                            metadata.MediaType = base.XmlReader.Value;
                            break;

                        case "hreflang":
                            metadata.HrefLang = base.XmlReader.Value;
                            break;

                        case "title":
                            metadata.Title = base.XmlReader.Value;
                            break;

                        case "length":
                        {
                            int num;
                            string s = base.XmlReader.Value;
                            if (!int.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out num))
                            {
                                throw new ODataException(Strings.EpmSyndicationWriter_InvalidLinkLengthValue(s));
                            }
                            metadata.Length = new int?(num);
                            break;
                        }
                        case "rel":
                            if (metadata.Relation == null)
                            {
                                metadata.Relation = base.XmlReader.Value;
                            }
                            break;

                        case "href":
                            if (metadata.Href == null)
                            {
                                metadata.Href = base.ProcessUriFromPayload(base.XmlReader.Value, base.XmlReader.XmlBaseUri);
                            }
                            break;
                    }
                }
            }
            base.XmlReader.Skip();
            return metadata;
        }

        private void ReadAuthorElement(AtomFeedMetadata atomFeedMetadata)
        {
            AtomMetadataReaderUtils.AddAuthorToFeedMetadata(atomFeedMetadata, base.ReadAtomPersonConstruct(null));
        }

        private void ReadCategoryElement(AtomFeedMetadata atomFeedMetadata)
        {
            AtomCategoryMetadata categoryMetadata = new AtomCategoryMetadata();
            while (base.XmlReader.MoveToNextAttribute())
            {
                string str;
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace) && ((str = base.XmlReader.LocalName) != null))
                {
                    if (!(str == "scheme"))
                    {
                        if (str == "term")
                        {
                            goto Label_0069;
                        }
                        if (str == "label")
                        {
                            goto Label_007C;
                        }
                    }
                    else
                    {
                        categoryMetadata.Scheme = base.XmlReader.Value;
                    }
                }
                continue;
            Label_0069:
                categoryMetadata.Term = base.XmlReader.Value;
                continue;
            Label_007C:
                categoryMetadata.Label = base.XmlReader.Value;
            }
            AtomMetadataReaderUtils.AddCategoryToFeedMetadata(atomFeedMetadata, categoryMetadata);
            base.XmlReader.Skip();
        }

        private void ReadContributorElement(AtomFeedMetadata atomFeedMetadata)
        {
            AtomMetadataReaderUtils.AddContributorToFeedMetadata(atomFeedMetadata, base.ReadAtomPersonConstruct(null));
        }

        private void ReadGeneratorElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Generator);
            AtomGeneratorMetadata metadata = new AtomGeneratorMetadata();
            while (base.XmlReader.MoveToNextAttribute())
            {
                string str;
                if (base.XmlReader.NamespaceEquals(this.EmptyNamespace) && ((str = base.XmlReader.LocalName) != null))
                {
                    if (!(str == "uri"))
                    {
                        if (str == "version")
                        {
                            goto Label_0076;
                        }
                    }
                    else
                    {
                        metadata.Uri = base.ProcessUriFromPayload(base.XmlReader.Value, base.XmlReader.XmlBaseUri);
                    }
                }
                continue;
            Label_0076:
                metadata.Version = base.XmlReader.Value;
            }
            base.XmlReader.MoveToElement();
            if (base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.Skip();
            }
            else
            {
                metadata.Name = base.XmlReader.ReadElementValue();
            }
            atomFeedMetadata.Generator = metadata;
        }

        private void ReadIconElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Icon);
            atomFeedMetadata.Icon = this.ReadUriValuedElement();
        }

        private void ReadIdElementAsSourceId(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.SourceId);
            atomFeedMetadata.SourceId = base.XmlReader.ReadElementValue();
        }

        private void ReadLinkElementIntoLinksCollection(AtomFeedMetadata atomFeedMetadata)
        {
            AtomLinkMetadata linkMetadata = this.ReadAtomLinkElementInFeed(null, null);
            AtomMetadataReaderUtils.AddLinkToFeedMetadata(atomFeedMetadata, linkMetadata);
        }

        private void ReadLogoElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Logo);
            atomFeedMetadata.Logo = this.ReadUriValuedElement();
        }

        private void ReadRightsElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Rights);
            atomFeedMetadata.Rights = base.ReadAtomTextConstruct();
        }

        private void ReadSubtitleElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Subtitle);
            atomFeedMetadata.Subtitle = base.ReadAtomTextConstruct();
        }

        private void ReadTitleElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Title);
            atomFeedMetadata.Title = base.ReadAtomTextConstruct();
        }

        private void ReadUpdatedElement(AtomFeedMetadata atomFeedMetadata)
        {
            this.VerifyNotPreviouslyDefined(atomFeedMetadata.Updated);
            atomFeedMetadata.Updated = base.ReadAtomDateConstruct();
        }

        private Uri ReadUriValuedElement()
        {
            string uriFromPayload = base.XmlReader.ReadElementValue();
            return base.ProcessUriFromPayload(uriFromPayload, base.XmlReader.XmlBaseUri);
        }

        private void VerifyNotPreviouslyDefined(object metadataValue)
        {
            if (metadataValue != null)
            {
                string str = this.InSourceElement ? "source" : "feed";
                throw new ODataException(Strings.ODataAtomMetadataDeserializer_MultipleSingletonMetadataElements(base.XmlReader.LocalName, str));
            }
        }

        private bool InSourceElement { get; set; }
    }
}

