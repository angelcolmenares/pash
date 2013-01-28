namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    internal sealed class ODataAtomEntryMetadataDeserializer : ODataAtomEpmDeserializer
    {
        private readonly string AtomNamespace;
        private readonly string EmptyNamespace;
        private ODataAtomFeedMetadataDeserializer sourceMetadataDeserializer;

        internal ODataAtomEntryMetadataDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.EmptyNamespace = nameTable.Add(string.Empty);
            this.AtomNamespace = nameTable.Add("http://www.w3.org/2005/Atom");
        }

        internal AtomCategoryMetadata ReadAtomCategoryElement()
        {
            AtomCategoryMetadata metadata = new AtomCategoryMetadata();
            while (base.XmlReader.MoveToNextAttribute())
            {
                string str2;
                if (!base.XmlReader.NamespaceEquals(this.EmptyNamespace))
                {
                    goto Label_00AF;
                }
                string localName = base.XmlReader.LocalName;
                if (localName != null)
                {
                    if (!(localName == "scheme"))
                    {
                        if (localName == "term")
                        {
                            goto Label_007F;
                        }
                        if (localName == "label")
                        {
                            goto Label_009C;
                        }
                    }
                    else
                    {
                        metadata.Scheme = metadata.Scheme ?? base.XmlReader.Value;
                    }
                }
                continue;
            Label_007F:;
                metadata.Term = metadata.Term ?? base.XmlReader.Value;
                continue;
            Label_009C:
                metadata.Label = base.XmlReader.Value;
                continue;
            Label_00AF:
                if ((base.UseClientFormatBehavior && base.XmlReader.NamespaceEquals(this.AtomNamespace)) && ((str2 = base.XmlReader.LocalName) != null))
                {
                    if (!(str2 == "scheme"))
                    {
                        if (str2 == "term")
                        {
                            goto Label_0108;
                        }
                    }
                    else
                    {
                        metadata.Scheme = base.XmlReader.Value;
                    }
                }
                continue;
            Label_0108:
                metadata.Term = base.XmlReader.Value;
            }
            base.XmlReader.Skip();
            return metadata;
        }

        internal void ReadAtomCategoryElementInEntryContent(IODataAtomReaderEntryState entryState)
        {
            ODataEntityPropertyMappingCache cachedEpm = entryState.CachedEpm;
            EpmTargetPathSegment syndicationRoot = null;
            if (cachedEpm != null)
            {
                syndicationRoot = cachedEpm.EpmTargetTree.SyndicationRoot;
            }
            if (syndicationRoot == null)
            {
            }
            bool flag = syndicationRoot.SubSegments.Any<EpmTargetPathSegment>();
            if (base.ReadAtomMetadata || flag)
            {
                AtomCategoryMetadata categoryMetadata = this.ReadAtomCategoryElement();
                AtomMetadataReaderUtils.AddCategoryToEntryMetadata(entryState.AtomEntryMetadata, categoryMetadata);
            }
            else
            {
                base.XmlReader.Skip();
            }
        }

        internal void ReadAtomElementInEntryContent(IODataAtomReaderEntryState entryState)
        {
            EpmTargetPathSegment segment2;
            ODataEntityPropertyMappingCache cachedEpm = entryState.CachedEpm;
            EpmTargetPathSegment parentSegment = null;
            if (cachedEpm != null)
            {
                parentSegment = cachedEpm.EpmTargetTree.SyndicationRoot;
            }
            if (base.ShouldReadElement(parentSegment, base.XmlReader.LocalName, out segment2))
            {
                switch (base.XmlReader.LocalName)
                {
                    case "author":
                        this.ReadAuthorElement(entryState, segment2);
                        return;

                    case "contributor":
                        this.ReadContributorElement(entryState, segment2);
                        return;

                    case "updated":
                    {
                        AtomEntryMetadata atomEntryMetadata = entryState.AtomEntryMetadata;
                        if (!base.UseClientFormatBehavior)
                        {
                            if (!this.ShouldReadSingletonElement(atomEntryMetadata.Updated.HasValue))
                            {
                                break;
                            }
                            atomEntryMetadata.Updated = base.ReadAtomDateConstruct();
                            return;
                        }
                        if (!this.ShouldReadSingletonElement(atomEntryMetadata.UpdatedString != null))
                        {
                            break;
                        }
                        atomEntryMetadata.UpdatedString = base.ReadAtomDateConstructAsString();
                        return;
                    }
                    case "published":
                    {
                        AtomEntryMetadata metadata2 = entryState.AtomEntryMetadata;
                        if (!base.UseClientFormatBehavior)
                        {
                            if (this.ShouldReadSingletonElement(metadata2.Published.HasValue))
                            {
                                metadata2.Published = base.ReadAtomDateConstruct();
                                return;
                            }
                            break;
                        }
                        if (!this.ShouldReadSingletonElement(metadata2.PublishedString != null))
                        {
                            break;
                        }
                        metadata2.PublishedString = base.ReadAtomDateConstructAsString();
                        return;
                    }
                    case "rights":
                        if (!this.ShouldReadSingletonElement(entryState.AtomEntryMetadata.Rights != null))
                        {
                            break;
                        }
                        entryState.AtomEntryMetadata.Rights = base.ReadAtomTextConstruct();
                        return;

                    case "source":
                        if (!this.ShouldReadSingletonElement(entryState.AtomEntryMetadata.Source != null))
                        {
                            break;
                        }
                        entryState.AtomEntryMetadata.Source = this.ReadAtomSourceInEntryContent();
                        return;

                    case "summary":
                        if (!this.ShouldReadSingletonElement(entryState.AtomEntryMetadata.Summary != null))
                        {
                            break;
                        }
                        entryState.AtomEntryMetadata.Summary = base.ReadAtomTextConstruct();
                        return;

                    case "title":
                        if (!this.ShouldReadSingletonElement(entryState.AtomEntryMetadata.Title != null))
                        {
                            break;
                        }
                        entryState.AtomEntryMetadata.Title = base.ReadAtomTextConstruct();
                        return;
                }
            }
            base.XmlReader.Skip();
        }

        internal AtomLinkMetadata ReadAtomLinkElementInEntryContent(string relation, string hrefStringValue)
        {
            AtomLinkMetadata metadata = null;
            if (base.ReadAtomMetadata)
            {
                metadata = new AtomLinkMetadata {
                    Relation = relation
                };
                if (base.ReadAtomMetadata)
                {
                    metadata.Href = (hrefStringValue == null) ? null : base.ProcessUriFromPayload(hrefStringValue, base.XmlReader.XmlBaseUri);
                }
                while (base.XmlReader.MoveToNextAttribute())
                {
                    string str2;
                    if (base.XmlReader.NamespaceEquals(this.EmptyNamespace) && ((str2 = base.XmlReader.LocalName) != null))
                    {
                        if (!(str2 == "type"))
                        {
                            if (str2 == "hreflang")
                            {
                                goto Label_00B9;
                            }
                            if (str2 == "title")
                            {
                                goto Label_00CC;
                            }
                            if (str2 == "length")
                            {
                                goto Label_00DF;
                            }
                        }
                        else
                        {
                            metadata.MediaType = base.XmlReader.Value;
                        }
                    }
                    continue;
                Label_00B9:
                    metadata.HrefLang = base.XmlReader.Value;
                    continue;
                Label_00CC:
                    metadata.Title = base.XmlReader.Value;
                    continue;
                Label_00DF:
                    if (base.ReadAtomMetadata)
                    {
                        int num;
                        string s = base.XmlReader.Value;
                        if (!int.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out num))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.EpmSyndicationWriter_InvalidLinkLengthValue(s));
                        }
                        metadata.Length = new int?(num);
                    }
                }
            }
            base.XmlReader.MoveToElement();
            return metadata;
        }

        internal AtomFeedMetadata ReadAtomSourceInEntryContent()
        {
            AtomFeedMetadata atomFeedMetadata = AtomMetadataReaderUtils.CreateNewAtomFeedMetadata();
            if (base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.Read();
                return atomFeedMetadata;
            }
            base.XmlReader.Read();
            while (base.XmlReader.NodeType != XmlNodeType.EndElement)
            {
                if (base.XmlReader.NodeType != XmlNodeType.Element)
                {
                    base.XmlReader.Skip();
                }
                else
                {
                    if (base.XmlReader.NamespaceEquals(this.AtomNamespace))
                    {
                        this.SourceMetadataDeserializer.ReadAtomElementAsFeedMetadata(atomFeedMetadata);
                        continue;
                    }
                    base.XmlReader.Skip();
                }
            }
            base.XmlReader.Read();
            return atomFeedMetadata;
        }

        private void ReadAuthorElement(IODataAtomReaderEntryState entryState, EpmTargetPathSegment epmTargetPathSegment)
        {
            if (this.ShouldReadCollectionElement(entryState.AtomEntryMetadata.Authors.Any<AtomPersonMetadata>()))
            {
                AtomMetadataReaderUtils.AddAuthorToEntryMetadata(entryState.AtomEntryMetadata, base.ReadAtomPersonConstruct(epmTargetPathSegment));
            }
            else
            {
                base.XmlReader.Skip();
            }
        }

        private void ReadContributorElement(IODataAtomReaderEntryState entryState, EpmTargetPathSegment epmTargetPathSegment)
        {
            if (this.ShouldReadCollectionElement(entryState.AtomEntryMetadata.Contributors.Any<AtomPersonMetadata>()))
            {
                AtomMetadataReaderUtils.AddContributorToEntryMetadata(entryState.AtomEntryMetadata, base.ReadAtomPersonConstruct(epmTargetPathSegment));
            }
            else
            {
                base.XmlReader.Skip();
            }
        }

        private bool ShouldReadCollectionElement(bool someAlreadyExist)
        {
            if (!base.ReadAtomMetadata)
            {
                return !someAlreadyExist;
            }
            return true;
        }

        private bool ShouldReadSingletonElement(bool alreadyExists)
        {
            if (!alreadyExists)
            {
                return true;
            }
            if (base.ReadAtomMetadata || base.AtomInputContext.UseDefaultFormatBehavior)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomMetadataDeserializer_MultipleSingletonMetadataElements(base.XmlReader.LocalName, "entry"));
            }
            return false;
        }

        private ODataAtomFeedMetadataDeserializer SourceMetadataDeserializer
        {
            get
            {
                return (this.sourceMetadataDeserializer ?? (this.sourceMetadataDeserializer = new ODataAtomFeedMetadataDeserializer(base.AtomInputContext, true)));
            }
        }
    }
}

