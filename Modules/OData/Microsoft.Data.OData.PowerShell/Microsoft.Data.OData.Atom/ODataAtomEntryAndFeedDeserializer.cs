namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class ODataAtomEntryAndFeedDeserializer : ODataAtomPropertyAndValueDeserializer
    {
        private readonly string AtomCategoryElementName;
        private readonly string AtomCategorySchemeAttributeName;
        private readonly string AtomCategoryTermAttributeName;
        private readonly string AtomContentElementName;
        private readonly string AtomEntryElementName;
        private readonly string AtomFeedElementName;
        private readonly string AtomIdElementName;
        private readonly string AtomLinkElementName;
        private readonly string AtomLinkHrefAttributeName;
        private readonly string AtomLinkRelationAttributeName;
        private readonly string AtomNamespace;
        private readonly string AtomPropertiesElementName;
        private ODataAtomEntryMetadataDeserializer entryMetadataDeserializer;
        private ODataAtomFeedMetadataDeserializer feedMetadataDeserializer;
        private readonly string MediaLinkEntryContentSourceAttributeName;
        private readonly string ODataActionElementName;
        private readonly string ODataCountElementName;
        private readonly string ODataETagAttributeName;
        private readonly string ODataFunctionElementName;
        private readonly string ODataInlineElementName;
        private readonly string ODataOperationMetadataAttribute;
        private readonly string ODataOperationTargetAttribute;
        private readonly string ODataOperationTitleAttribute;

        internal ODataAtomEntryAndFeedDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.AtomNamespace = nameTable.Add("http://www.w3.org/2005/Atom");
            this.AtomEntryElementName = nameTable.Add("entry");
            this.AtomCategoryElementName = nameTable.Add("category");
            this.AtomCategoryTermAttributeName = nameTable.Add("term");
            this.AtomCategorySchemeAttributeName = nameTable.Add("scheme");
            this.AtomContentElementName = nameTable.Add("content");
            this.AtomLinkElementName = nameTable.Add("link");
            this.AtomPropertiesElementName = nameTable.Add("properties");
            this.AtomFeedElementName = nameTable.Add("feed");
            this.AtomIdElementName = nameTable.Add("id");
            this.AtomLinkRelationAttributeName = nameTable.Add("rel");
            this.AtomLinkHrefAttributeName = nameTable.Add("href");
            this.MediaLinkEntryContentSourceAttributeName = nameTable.Add("src");
            this.ODataETagAttributeName = nameTable.Add("etag");
            this.ODataCountElementName = nameTable.Add("count");
            this.ODataInlineElementName = nameTable.Add("inline");
            this.ODataActionElementName = nameTable.Add("action");
            this.ODataFunctionElementName = nameTable.Add("function");
            this.ODataOperationMetadataAttribute = nameTable.Add("metadata");
            this.ODataOperationTitleAttribute = nameTable.Add("title");
            this.ODataOperationTargetAttribute = nameTable.Add("target");
        }

        internal static void EnsureMediaResource(IODataAtomReaderEntryState entryState, bool validateMLEPresence)
        {
            if (validateMLEPresence)
            {
                entryState.MediaLinkEntry = true;
            }
            ODataEntry entry = entryState.Entry;
            if (entry.MediaResource == null)
            {
                entry.MediaResource = new ODataStreamReferenceValue();
            }
        }

        internal string FindTypeName()
        {
            base.XmlReader.MoveToElement();
            base.XmlReader.StartBuffering();
            try
            {
                if (base.XmlReader.IsEmptyElement)
                {
                    goto Label_015F;
                }
                base.XmlReader.Read();
            Label_0033:
                switch (base.XmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (base.XmlReader.NamespaceEquals(this.AtomNamespace) && base.XmlReader.LocalNameEquals(this.AtomCategoryElementName))
                        {
                            string str = null;
                            bool flag = false;
                            while (base.XmlReader.MoveToNextAttribute())
                            {
                                bool flag2 = base.XmlReader.NamespaceEquals(base.EmptyNamespace);
                                if (flag2 || (base.UseClientFormatBehavior && base.XmlReader.NamespaceEquals(this.AtomNamespace)))
                                {
                                    if (base.XmlReader.LocalNameEquals(this.AtomCategorySchemeAttributeName))
                                    {
                                        if (string.CompareOrdinal(base.XmlReader.Value, base.MessageReaderSettings.ReaderBehavior.ODataTypeScheme) == 0)
                                        {
                                            flag = true;
                                        }
                                    }
                                    else if (base.XmlReader.LocalNameEquals(this.AtomCategoryTermAttributeName) && ((str == null) || !flag2))
                                    {
                                        str = base.XmlReader.Value;
                                    }
                                }
                            }
                            if (flag)
                            {
                                return str;
                            }
                        }
                        base.XmlReader.Skip();
                        goto Label_0033;

                    case XmlNodeType.EndElement:
                        return null;
                }
                base.XmlReader.Skip();
                goto Label_0033;
            }
            finally
            {
                base.XmlReader.StopBuffering();
            }
        Label_015F:
            return null;
        }

        private ODataStreamReferenceValue GetNewOrExistingStreamPropertyValue(IODataAtomReaderEntryState entryState, string streamPropertyName)
        {
            ODataStreamReferenceValue value2;
            List<ODataProperty> propertiesList = ReaderUtils.GetPropertiesList(entryState.Entry.Properties);
            ODataProperty streamProperty = propertiesList.FirstOrDefault<ODataProperty>(p => string.CompareOrdinal(p.Name, streamPropertyName) == 0);
            if (streamProperty == null)
            {
                IEdmProperty streamEdmProperty = ReaderValidationUtils.ValidateLinkPropertyDefined(streamPropertyName, entryState.EntityType, base.MessageReaderSettings);
                value2 = new ODataStreamReferenceValue();
                streamProperty = new ODataProperty {
                    Name = streamPropertyName,
                    Value = value2
                };
                ReaderValidationUtils.ValidateStreamReferenceProperty(streamProperty, entryState.EntityType, streamEdmProperty);
                entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(streamProperty);
                propertiesList.Add(streamProperty);
                return value2;
            }
            value2 = streamProperty.Value as ODataStreamReferenceValue;
            if (value2 == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_StreamPropertyDuplicatePropertyName(streamPropertyName));
            }
            return value2;
        }

        internal bool IsReaderOnInlineEndElement()
        {
            if (!base.XmlReader.LocalNameEquals(this.ODataInlineElementName) || !base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
            {
                return false;
            }
            return (((base.XmlReader.NodeType == XmlNodeType.Element) && base.XmlReader.IsEmptyElement) || (base.XmlReader.NodeType == XmlNodeType.EndElement));
        }

        private void ReadAtomContentAttributes(out string contentType, out string contentSource)
        {
            contentType = null;
            contentSource = null;
            while (base.XmlReader.MoveToNextAttribute())
            {
                bool flag = base.XmlReader.NamespaceEquals(base.EmptyNamespace);
                if (flag || (base.UseClientFormatBehavior && base.XmlReader.NamespaceEquals(this.AtomNamespace)))
                {
                    if (base.XmlReader.LocalNameEquals(base.AtomTypeAttributeName))
                    {
                        if (!flag || (contentType == null))
                        {
                            contentType = base.XmlReader.Value;
                        }
                    }
                    else if (base.XmlReader.LocalNameEquals(this.MediaLinkEntryContentSourceAttributeName) && (!flag || (contentSource == null)))
                    {
                        contentSource = base.XmlReader.Value;
                    }
                }
            }
        }

        private void ReadAtomContentElement(IODataAtomReaderEntryState entryState)
        {
            string str;
            string str2;
            this.ValidateDuplicateElement(entryState.HasContent && base.AtomInputContext.UseDefaultFormatBehavior);
            if (base.AtomInputContext.UseClientFormatBehavior)
            {
                entryState.HasProperties = false;
            }
            this.ReadAtomContentAttributes(out str, out str2);
            if (str2 != null)
            {
                ODataEntry entry = entryState.Entry;
                EnsureMediaResource(entryState, true);
                if (!base.AtomInputContext.UseServerFormatBehavior)
                {
                    entry.MediaResource.ReadLink = base.ProcessUriFromPayload(str2, base.XmlReader.XmlBaseUri);
                }
                entry.MediaResource.ContentType = str;
                if (!base.XmlReader.TryReadEmptyElement())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_ContentWithSourceLinkIsNotEmpty);
                }
            }
            else
            {
                bool flag = string.IsNullOrEmpty(str);
                if (flag && base.AtomInputContext.UseClientFormatBehavior)
                {
                    base.XmlReader.SkipElementContent();
                }
                string str3 = str;
                if (!flag)
                {
                    str3 = this.VerifyAtomContentMediaType(str);
                }
                entryState.MediaLinkEntry = false;
                base.XmlReader.MoveToElement();
                if (!base.XmlReader.IsEmptyElement && (base.XmlReader.NodeType != XmlNodeType.EndElement))
                {
                    if (string.IsNullOrEmpty(str3))
                    {
                        base.XmlReader.ReadElementContentValue();
                    }
                    else
                    {
                        base.XmlReader.ReadStartElement();
                        while (base.XmlReader.NodeType != XmlNodeType.EndElement)
                        {
                            switch (base.XmlReader.NodeType)
                            {
                                case XmlNodeType.Element:
                                {
                                    if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
                                    {
                                        if (!base.XmlReader.LocalNameEquals(this.AtomPropertiesElementName))
                                        {
                                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_ContentWithInvalidNode(base.XmlReader.LocalName));
                                        }
                                        this.ValidateDuplicateElement(entryState.HasProperties && base.AtomInputContext.UseDefaultFormatBehavior);
                                        if (base.UseClientFormatBehavior && entryState.HasProperties)
                                        {
                                            base.XmlReader.SkipElementContent();
                                        }
                                        else
                                        {
                                            base.ReadProperties(entryState.EntityType, ReaderUtils.GetPropertiesList(entryState.Entry.Properties), entryState.DuplicatePropertyNamesChecker, entryState.CachedEpm != null);
                                        }
                                        entryState.HasProperties = true;
                                    }
                                    else
                                    {
                                        base.XmlReader.SkipElementContent();
                                    }
                                    base.XmlReader.Read();
                                    continue;
                                }
                                case XmlNodeType.EndElement:
                                {
                                    continue;
                                }
                            }
                            base.XmlReader.Skip();
                        }
                    }
                }
            }
            base.XmlReader.Read();
            entryState.HasContent = true;
        }

        private ODataAtomReaderNavigationLinkDescriptor ReadAtomElementInEntry(IODataAtomReaderEntryState entryState)
        {
            if (base.XmlReader.LocalNameEquals(this.AtomContentElementName))
            {
                this.ReadAtomContentElement(entryState);
            }
            else if (base.XmlReader.LocalNameEquals(this.AtomIdElementName))
            {
                this.ReadAtomIdElementInEntry(entryState);
            }
            else if (base.XmlReader.LocalNameEquals(this.AtomCategoryElementName))
            {
                string attribute = base.XmlReader.GetAttribute(this.AtomCategorySchemeAttributeName, base.EmptyNamespace);
                if ((attribute != null) && (string.CompareOrdinal(attribute, base.MessageReaderSettings.ReaderBehavior.ODataTypeScheme) == 0))
                {
                    this.ValidateDuplicateElement(entryState.HasTypeNameCategory && base.AtomInputContext.UseDefaultFormatBehavior);
                    if (this.ReadAtomMetadata)
                    {
                        entryState.AtomEntryMetadata.CategoryWithTypeName = this.EntryMetadataDeserializer.ReadAtomCategoryElement();
                    }
                    else
                    {
                        base.XmlReader.Skip();
                    }
                    entryState.HasTypeNameCategory = true;
                }
                else if ((entryState.CachedEpm != null) || this.ReadAtomMetadata)
                {
                    this.EntryMetadataDeserializer.ReadAtomCategoryElementInEntryContent(entryState);
                }
                else
                {
                    base.XmlReader.Skip();
                }
            }
            else
            {
                if (base.XmlReader.LocalNameEquals(this.AtomLinkElementName))
                {
                    return this.ReadAtomLinkElementInEntry(entryState);
                }
                if ((entryState.CachedEpm != null) || this.ReadAtomMetadata)
                {
                    this.EntryMetadataDeserializer.ReadAtomElementInEntryContent(entryState);
                }
                else
                {
                    base.XmlReader.Skip();
                }
            }
            return null;
        }

        private bool ReadAtomElementInFeed(IODataAtomReaderFeedState feedState)
        {
            if (base.XmlReader.LocalNameEquals(this.AtomEntryElementName))
            {
                return true;
            }
            if (base.XmlReader.LocalNameEquals(this.AtomLinkElementName))
            {
                string str;
                string str2;
                this.ReadAtomLinkRelationAndHRef(out str, out str2);
                if (str != null)
                {
                    if (this.ReadAtomStandardRelationLinkInFeed(feedState, str, str2))
                    {
                        return false;
                    }
                    if (AtomUtils.UnescapeAtomLinkRelationAttribute(str) != null)
                    {
                        string nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(str, "http://www.iana.org/assignments/relation/");
                        if ((nameFromAtomLinkRelationAttribute != null) && this.ReadAtomStandardRelationLinkInFeed(feedState, nameFromAtomLinkRelationAttribute, str2))
                        {
                            return false;
                        }
                    }
                }
                if (this.ReadAtomMetadata)
                {
                    AtomLinkMetadata linkMetadata = this.FeedMetadataDeserializer.ReadAtomLinkElementInFeed(str, str2);
                    AtomMetadataReaderUtils.AddLinkToFeedMetadata(feedState.AtomFeedMetadata, linkMetadata);
                }
                else
                {
                    base.XmlReader.Skip();
                }
            }
            else if (base.XmlReader.LocalNameEquals(this.AtomIdElementName))
            {
                string str5 = base.XmlReader.ReadElementValue();
                feedState.Feed.Id = str5;
            }
            else if (this.ReadAtomMetadata)
            {
                this.FeedMetadataDeserializer.ReadAtomElementAsFeedMetadata(feedState.AtomFeedMetadata);
            }
            else
            {
                base.XmlReader.Skip();
            }
            return false;
        }

        private void ReadAtomIdElementInEntry(IODataAtomReaderEntryState entryState)
        {
            this.ValidateDuplicateElement(entryState.HasId && base.AtomInputContext.UseDefaultFormatBehavior);
            string str = base.XmlReader.ReadElementValue();
            if (!base.AtomInputContext.UseClientFormatBehavior || !entryState.HasId)
            {
                entryState.Entry.Id = ((str != null) && (str.Length == 0)) ? null : str;
            }
            entryState.HasId = true;
        }

        private ODataAtomReaderNavigationLinkDescriptor ReadAtomLinkElementInEntry(IODataAtomReaderEntryState entryState)
        {
            string str;
            string str2;
            this.ReadAtomLinkRelationAndHRef(out str, out str2);
            if (str != null)
            {
                bool isStreamPropertyLink = false;
                if (!base.AtomInputContext.UseServerFormatBehavior && this.TryReadAtomStandardRelationLinkInEntry(entryState, str, str2))
                {
                    return null;
                }
                string relation = AtomUtils.UnescapeAtomLinkRelationAttribute(str);
                if (relation != null)
                {
                    if (!base.AtomInputContext.UseServerFormatBehavior)
                    {
                        string nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(relation, "http://www.iana.org/assignments/relation/");
                        if ((nameFromAtomLinkRelationAttribute != null) && this.TryReadAtomStandardRelationLinkInEntry(entryState, nameFromAtomLinkRelationAttribute, str2))
                        {
                            return null;
                        }
                    }
                    ODataAtomReaderNavigationLinkDescriptor descriptor = this.TryReadNavigationLinkInEntry(entryState, relation, str2);
                    if (descriptor != null)
                    {
                        return descriptor;
                    }
                    if (this.TryReadStreamPropertyLinkInEntry(entryState, relation, str2, out isStreamPropertyLink))
                    {
                        return null;
                    }
                    if (!isStreamPropertyLink && this.TryReadAssociationLinkInEntry(entryState, relation, str2))
                    {
                        return null;
                    }
                }
            }
            if ((entryState.CachedEpm != null) || this.ReadAtomMetadata)
            {
                AtomLinkMetadata linkMetadata = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(str, str2);
                if (linkMetadata != null)
                {
                    AtomMetadataReaderUtils.AddLinkToEntryMetadata(entryState.AtomEntryMetadata, linkMetadata);
                }
            }
            base.XmlReader.Skip();
            return null;
        }

        private void ReadAtomLinkRelationAndHRef(out string linkRelation, out string linkHRef)
        {
            linkRelation = null;
            linkHRef = null;
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(base.EmptyNamespace))
                {
                    if (base.XmlReader.LocalNameEquals(this.AtomLinkRelationAttributeName))
                    {
                        linkRelation = base.XmlReader.Value;
                        if (linkHRef == null)
                        {
                            continue;
                        }
                        break;
                    }
                    if (base.XmlReader.LocalNameEquals(this.AtomLinkHrefAttributeName))
                    {
                        linkHRef = base.XmlReader.Value;
                        if (linkRelation != null)
                        {
                            break;
                        }
                    }
                }
            }
            base.XmlReader.MoveToElement();
        }

        private bool ReadAtomStandardRelationLinkInFeed(IODataAtomReaderFeedState feedState, string linkRelation, string linkHRef)
        {
            if (string.CompareOrdinal(linkRelation, "next") == 0)
            {
                if (!base.ReadingResponse || (base.Version < ODataVersion.V2))
                {
                    return false;
                }
                if (feedState.HasNextPageLink)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleLinksInFeed("next"));
                }
                if (linkHRef != null)
                {
                    feedState.Feed.NextPageLink = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
                }
                feedState.HasNextPageLink = true;
                if (this.ReadAtomMetadata)
                {
                    AtomLinkMetadata metadata = this.FeedMetadataDeserializer.ReadAtomLinkElementInFeed(linkRelation, linkHRef);
                    feedState.AtomFeedMetadata.NextPageLink = metadata;
                }
                else
                {
                    base.XmlReader.Skip();
                }
                return true;
            }
            if (string.CompareOrdinal(linkRelation, "self") != 0)
            {
                return false;
            }
            if (feedState.HasReadLink && base.AtomInputContext.UseDefaultFormatBehavior)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleLinksInFeed("self"));
            }
            if (this.ReadAtomMetadata)
            {
                AtomLinkMetadata metadata2 = this.FeedMetadataDeserializer.ReadAtomLinkElementInFeed(linkRelation, linkHRef);
                feedState.AtomFeedMetadata.SelfLink = metadata2;
            }
            else
            {
                base.XmlReader.Skip();
            }
            feedState.HasReadLink = true;
            return true;
        }

        internal ODataAtomReaderNavigationLinkDescriptor ReadEntryContent(IODataAtomReaderEntryState entryState)
        {
            ODataAtomReaderNavigationLinkDescriptor descriptor = null;
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
                        descriptor = this.ReadAtomElementInEntry(entryState);
                        if (descriptor != null)
                        {
                            entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNamesOnNavigationLinkStart(descriptor.NavigationLink);
                            return descriptor;
                        }
                        continue;
                    }
                    if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
                    {
                        if (base.XmlReader.LocalNameEquals(this.AtomPropertiesElementName))
                        {
                            this.ValidateDuplicateElement(entryState.HasProperties && base.AtomInputContext.UseDefaultFormatBehavior);
                            EnsureMediaResource(entryState, true);
                            base.ReadProperties(entryState.EntityType, ReaderUtils.GetPropertiesList(entryState.Entry.Properties), entryState.DuplicatePropertyNamesChecker, entryState.CachedEpm != null);
                            base.XmlReader.Read();
                            entryState.HasProperties = true;
                        }
                        else if ((((base.MessageReaderSettings.MaxProtocolVersion < ODataVersion.V3) || !base.ReadingResponse) || !this.TryReadOperation(entryState)) && !this.EntryMetadataDeserializer.TryReadExtensionElementInEntryContent(entryState))
                        {
                            base.XmlReader.Skip();
                        }
                        continue;
                    }
                    if (!this.EntryMetadataDeserializer.TryReadExtensionElementInEntryContent(entryState))
                    {
                        base.XmlReader.Skip();
                    }
                }
            }
            return descriptor;
        }

        internal void ReadEntryEnd()
        {
            base.XmlReader.Read();
        }

        internal void ReadEntryStart(ODataEntry entry)
        {
            this.VerifyEntryStart();
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace) && base.XmlReader.LocalNameEquals(this.ODataETagAttributeName))
                {
                    entry.ETag = base.XmlReader.Value;
                    break;
                }
            }
            base.XmlReader.MoveToElement();
        }

        internal bool ReadFeedContent(IODataAtomReaderFeedState feedState, bool isExpandedLinkContent)
        {
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
                        if (this.ReadAtomElementInFeed(feedState))
                        {
                            return true;
                        }
                        continue;
                    }
                    if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
                    {
                        if ((base.ReadingResponse && (base.Version >= ODataVersion.V2)) && (!isExpandedLinkContent && base.XmlReader.LocalNameEquals(this.ODataCountElementName)))
                        {
                            this.ValidateDuplicateElement(feedState.HasCount);
                            long num = (long) AtomValueUtils.ReadPrimitiveValue(base.XmlReader, EdmCoreModel.Instance.GetInt64(true));
                            feedState.Feed.Count = new long?(num);
                            base.XmlReader.Read();
                            feedState.HasCount = true;
                        }
                        else
                        {
                            base.XmlReader.Skip();
                        }
                        continue;
                    }
                    base.XmlReader.Skip();
                }
            }
            return false;
        }

        internal void ReadFeedEnd()
        {
            base.XmlReader.Read();
        }

        internal void ReadFeedStart()
        {
            if (!base.XmlReader.NamespaceEquals(this.AtomNamespace) || !base.XmlReader.LocalNameEquals(this.AtomFeedElementName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_FeedElementWrongName(base.XmlReader.LocalName, base.XmlReader.NamespaceURI));
            }
        }

        private ODataAtomDeserializerExpandedNavigationLinkContent ReadInlineElementContent()
        {
        Label_0000:
            switch (base.XmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (base.XmlReader.NamespaceEquals(this.AtomNamespace))
                    {
                        if (base.XmlReader.LocalNameEquals(this.AtomEntryElementName))
                        {
                            return ODataAtomDeserializerExpandedNavigationLinkContent.Entry;
                        }
                        if (!base.XmlReader.LocalNameEquals(this.AtomFeedElementName))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_UnknownElementInInline(base.XmlReader.LocalName));
                        }
                        return ODataAtomDeserializerExpandedNavigationLinkContent.Feed;
                    }
                    base.XmlReader.Skip();
                    goto Label_0000;

                case XmlNodeType.EndElement:
                    return ODataAtomDeserializerExpandedNavigationLinkContent.Empty;
            }
            base.XmlReader.Skip();
            goto Label_0000;
        }

        private bool ReadNavigationLinkContent()
        {
        Label_0000:
            switch (base.XmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (base.XmlReader.LocalNameEquals(this.ODataInlineElementName) && base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
                    {
                        return true;
                    }
                    base.XmlReader.Skip();
                    goto Label_0000;

                case XmlNodeType.EndElement:
                    return false;
            }
            base.XmlReader.Skip();
            goto Label_0000;
        }

        internal void ReadNavigationLinkContentAfterExpansion(bool emptyInline)
        {
            if (!emptyInline)
            {
                ODataAtomDeserializerExpandedNavigationLinkContent content = this.ReadInlineElementContent();
                if (content != ODataAtomDeserializerExpandedNavigationLinkContent.Empty)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleExpansionsInInline(content.ToString()));
                }
            }
            base.XmlReader.Read();
            if (this.ReadNavigationLinkContent())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleInlineElementsInLink);
            }
        }

        internal ODataAtomDeserializerExpandedNavigationLinkContent ReadNavigationLinkContentBeforeExpansion()
        {
            if (!this.ReadNavigationLinkContent())
            {
                return ODataAtomDeserializerExpandedNavigationLinkContent.None;
            }
            if (base.XmlReader.IsEmptyElement)
            {
                return ODataAtomDeserializerExpandedNavigationLinkContent.Empty;
            }
            base.XmlReader.Read();
            return this.ReadInlineElementContent();
        }

        internal void ReadNavigationLinkEnd()
        {
            base.XmlReader.Read();
        }

        private bool ReadStreamPropertyLinkInEntry(IODataAtomReaderEntryState entryState, string streamPropertyName, string linkRelation, string linkHRef, bool editLink)
        {
            if (!base.ReadingResponse || (base.Version < ODataVersion.V3))
            {
                return false;
            }
            if (streamPropertyName.Length == 0)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_StreamPropertyWithEmptyName);
            }
            ODataStreamReferenceValue newOrExistingStreamPropertyValue = this.GetNewOrExistingStreamPropertyValue(entryState, streamPropertyName);
            AtomStreamReferenceMetadata annotation = null;
            if (this.ReadAtomMetadata)
            {
                annotation = newOrExistingStreamPropertyValue.GetAnnotation<AtomStreamReferenceMetadata>();
                if (annotation == null)
                {
                    annotation = new AtomStreamReferenceMetadata();
                    newOrExistingStreamPropertyValue.SetAnnotation<AtomStreamReferenceMetadata>(annotation);
                }
            }
            Uri uri = (linkHRef == null) ? null : base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
            if (editLink)
            {
                if (newOrExistingStreamPropertyValue.EditLink != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleEditLinks(streamPropertyName));
                }
                newOrExistingStreamPropertyValue.EditLink = uri;
                if (this.ReadAtomMetadata)
                {
                    annotation.EditLink = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
                }
            }
            else
            {
                if (newOrExistingStreamPropertyValue.ReadLink != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleReadLinks(streamPropertyName));
                }
                newOrExistingStreamPropertyValue.ReadLink = uri;
                if (this.ReadAtomMetadata)
                {
                    annotation.SelfLink = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
                }
            }
            string attribute = base.XmlReader.GetAttribute(base.AtomTypeAttributeName, base.EmptyNamespace);
            if (((attribute != null) && (newOrExistingStreamPropertyValue.ContentType != null)) && !HttpUtils.CompareMediaTypeNames(attribute, newOrExistingStreamPropertyValue.ContentType))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleContentTypes(streamPropertyName));
            }
            newOrExistingStreamPropertyValue.ContentType = attribute;
            if (editLink)
            {
                string str2 = base.XmlReader.GetAttribute(this.ODataETagAttributeName, base.XmlReader.ODataMetadataNamespace);
                newOrExistingStreamPropertyValue.ETag = str2;
            }
            base.XmlReader.Skip();
            return true;
        }

        internal void SkipNavigationLinkContentOnExpansion()
        {
            do
            {
                base.XmlReader.Skip();
            }
            while (((base.XmlReader.NodeType != XmlNodeType.EndElement) || !base.XmlReader.LocalNameEquals(this.AtomLinkElementName)) || !base.XmlReader.NamespaceEquals(this.AtomNamespace));
        }

        private bool TryReadAssociationLinkInEntry(IODataAtomReaderEntryState entryState, string linkRelation, string linkHRef)
        {
            string nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(linkRelation, "http://schemas.microsoft.com/ado/2007/08/dataservices/relatedlinks/");
            if ((string.IsNullOrEmpty(nameFromAtomLinkRelationAttribute) || !base.ReadingResponse) || (base.MessageReaderSettings.MaxProtocolVersion < ODataVersion.V3))
            {
                return false;
            }
            ReaderValidationUtils.ValidateNavigationPropertyDefined(nameFromAtomLinkRelationAttribute, entryState.EntityType, base.MessageReaderSettings);
            string attribute = base.XmlReader.GetAttribute(base.AtomTypeAttributeName, base.EmptyNamespace);
            if ((attribute != null) && !HttpUtils.CompareMediaTypeNames(attribute, "application/xml"))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_InvalidTypeAttributeOnAssociationLink(nameFromAtomLinkRelationAttribute));
            }
            ODataAssociationLink associationLink = new ODataAssociationLink {
                Name = nameFromAtomLinkRelationAttribute
            };
            if (linkHRef != null)
            {
                associationLink.Url = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
            }
            entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(associationLink);
            ReaderUtils.AddAssociationLinkToEntry(entryState.Entry, associationLink);
            AtomLinkMetadata annotation = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
            if (annotation != null)
            {
                associationLink.SetAnnotation<AtomLinkMetadata>(annotation);
            }
            base.XmlReader.Skip();
            return true;
        }

        private bool TryReadAtomStandardRelationLinkInEntry(IODataAtomReaderEntryState entryState, string linkRelation, string linkHRef)
        {
            if (string.CompareOrdinal(linkRelation, "edit") == 0)
            {
                if (entryState.HasEditLink && base.AtomInputContext.UseDefaultFormatBehavior)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleLinksInEntry("edit"));
                }
                if ((linkHRef != null) && (!base.AtomInputContext.UseClientFormatBehavior || !entryState.HasEditLink))
                {
                    entryState.Entry.EditLink = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
                }
                if (this.ReadAtomMetadata)
                {
                    entryState.AtomEntryMetadata.EditLink = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
                }
                entryState.HasEditLink = true;
                base.XmlReader.Skip();
                return true;
            }
            if (string.CompareOrdinal(linkRelation, "self") == 0)
            {
                if (entryState.HasReadLink && base.AtomInputContext.UseDefaultFormatBehavior)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleLinksInEntry("self"));
                }
                if ((linkHRef != null) && (!base.AtomInputContext.UseClientFormatBehavior || !entryState.HasReadLink))
                {
                    entryState.Entry.ReadLink = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
                }
                if (this.ReadAtomMetadata)
                {
                    entryState.AtomEntryMetadata.SelfLink = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
                }
                entryState.HasReadLink = true;
                base.XmlReader.Skip();
                return true;
            }
            if (string.CompareOrdinal(linkRelation, "edit-media") != 0)
            {
                return false;
            }
            if (entryState.HasEditMediaLink && base.AtomInputContext.UseDefaultFormatBehavior)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_MultipleLinksInEntry("edit-media"));
            }
            if (!base.AtomInputContext.UseClientFormatBehavior || !entryState.HasEditMediaLink)
            {
                EnsureMediaResource(entryState, !base.UseClientFormatBehavior);
                ODataEntry entry = entryState.Entry;
                if (linkHRef != null)
                {
                    entry.MediaResource.EditLink = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
                }
                string attribute = base.XmlReader.GetAttribute(this.ODataETagAttributeName, base.XmlReader.ODataMetadataNamespace);
                if (attribute != null)
                {
                    entry.MediaResource.ETag = attribute;
                }
                if (this.ReadAtomMetadata)
                {
                    AtomLinkMetadata metadata = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
                    AtomStreamReferenceMetadata annotation = new AtomStreamReferenceMetadata {
                        EditLink = metadata
                    };
                    entry.MediaResource.SetAnnotation<AtomStreamReferenceMetadata>(annotation);
                }
            }
            entryState.HasEditMediaLink = true;
            base.XmlReader.Skip();
            return true;
        }

        private ODataAtomReaderNavigationLinkDescriptor TryReadNavigationLinkInEntry(IODataAtomReaderEntryState entryState, string linkRelation, string linkHRef)
        {
            string nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(linkRelation, "http://schemas.microsoft.com/ado/2007/08/dataservices/related/");
            if (string.IsNullOrEmpty(nameFromAtomLinkRelationAttribute))
            {
                return null;
            }
            IEdmNavigationProperty navigationProperty = ReaderValidationUtils.ValidateNavigationPropertyDefined(nameFromAtomLinkRelationAttribute, entryState.EntityType, base.MessageReaderSettings);
            ODataNavigationLink navigationLink = new ODataNavigationLink {
                Name = nameFromAtomLinkRelationAttribute
            };
            string attribute = base.XmlReader.GetAttribute(base.AtomTypeAttributeName, base.EmptyNamespace);
            if (!string.IsNullOrEmpty(attribute))
            {
                bool flag;
                bool flag2;
                if (!AtomUtils.IsExactNavigationLinkTypeMatch(attribute, out flag, out flag2))
                {
                    string str3;
                    string str4;
                    IList<KeyValuePair<string, string>> list = HttpUtils.ReadMimeType(attribute, out str3, out str4);
                    if (!HttpUtils.CompareMediaTypeNames(str3, "application/atom+xml"))
                    {
                        return null;
                    }
                    string strA = null;
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            KeyValuePair<string, string> pair = list[i];
                            if (HttpUtils.CompareMediaTypeParameterNames("type", pair.Key))
                            {
                                strA = pair.Value;
                                break;
                            }
                        }
                    }
                    if (strA != null)
                    {
                        if (string.Compare(strA, "entry", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = true;
                        }
                        else if (string.Compare(strA, "feed", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag2 = true;
                        }
                    }
                }
                if (flag)
                {
                    if (!base.UseClientFormatBehavior)
                    {
                        navigationLink.IsCollection = false;
                    }
                }
                else if (flag2)
                {
                    navigationLink.IsCollection = true;
                }
            }
            if (linkHRef != null)
            {
                navigationLink.Url = base.ProcessUriFromPayload(linkHRef, base.XmlReader.XmlBaseUri);
            }
            base.XmlReader.MoveToElement();
            AtomLinkMetadata annotation = this.EntryMetadataDeserializer.ReadAtomLinkElementInEntryContent(linkRelation, linkHRef);
            if (annotation != null)
            {
                navigationLink.SetAnnotation<AtomLinkMetadata>(annotation);
            }
            return new ODataAtomReaderNavigationLinkDescriptor(navigationLink, navigationProperty);
        }

        private bool TryReadOperation(IODataAtomReaderEntryState entryState)
        {
            ODataOperation operation;
            bool flag = false;
            if (base.XmlReader.LocalNameEquals(this.ODataActionElementName))
            {
                flag = true;
            }
            else if (!base.XmlReader.LocalNameEquals(this.ODataFunctionElementName))
            {
                return false;
            }
            if (flag)
            {
                operation = new ODataAction();
                ReaderUtils.AddActionToEntry(entryState.Entry, (ODataAction) operation);
            }
            else
            {
                operation = new ODataFunction();
                ReaderUtils.AddFunctionToEntry(entryState.Entry, (ODataFunction) operation);
            }
            string localName = base.XmlReader.LocalName;
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(base.EmptyNamespace))
                {
                    string uriFromPayload = base.XmlReader.Value;
                    if (base.XmlReader.LocalNameEquals(this.ODataOperationMetadataAttribute))
                    {
                        operation.Metadata = base.ProcessUriFromPayload(uriFromPayload, base.XmlReader.XmlBaseUri, false);
                    }
                    else
                    {
                        if (base.XmlReader.LocalNameEquals(this.ODataOperationTargetAttribute))
                        {
                            operation.Target = base.ProcessUriFromPayload(uriFromPayload, base.XmlReader.XmlBaseUri);
                            continue;
                        }
                        if (base.XmlReader.LocalNameEquals(this.ODataOperationTitleAttribute))
                        {
                            operation.Title = base.XmlReader.Value;
                        }
                    }
                }
            }
            if (operation.Metadata == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_OperationMissingMetadataAttribute(localName));
            }
            if (operation.Target == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_OperationMissingTargetAttribute(localName));
            }
            base.XmlReader.Skip();
            return true;
        }

        private bool TryReadStreamPropertyLinkInEntry(IODataAtomReaderEntryState entryState, string linkRelation, string linkHRef, out bool isStreamPropertyLink)
        {
            string nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(linkRelation, "http://schemas.microsoft.com/ado/2007/08/dataservices/edit-media/");
            if (nameFromAtomLinkRelationAttribute != null)
            {
                isStreamPropertyLink = true;
                return this.ReadStreamPropertyLinkInEntry(entryState, nameFromAtomLinkRelationAttribute, linkRelation, linkHRef, true);
            }
            nameFromAtomLinkRelationAttribute = AtomUtils.GetNameFromAtomLinkRelationAttribute(linkRelation, "http://schemas.microsoft.com/ado/2007/08/dataservices/mediaresource/");
            if (nameFromAtomLinkRelationAttribute != null)
            {
                isStreamPropertyLink = true;
                return this.ReadStreamPropertyLinkInEntry(entryState, nameFromAtomLinkRelationAttribute, linkRelation, linkHRef, false);
            }
            isStreamPropertyLink = false;
            return false;
        }

        private void ValidateDuplicateElement(bool duplicateElementFound)
        {
            if (duplicateElementFound)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_DuplicateElements(base.XmlReader.NamespaceURI, base.XmlReader.LocalName));
            }
        }

        private string VerifyAtomContentMediaType(string contentType)
        {
            if (!HttpUtils.CompareMediaTypeNames("application/xml", contentType) && !HttpUtils.CompareMediaTypeNames("application/atom+xml", contentType))
            {
                string str;
                string str2;
                HttpUtils.ReadMimeType(contentType, out str, out str2);
                if (HttpUtils.CompareMediaTypeNames(str, "application/xml") || HttpUtils.CompareMediaTypeNames(str, "application/atom+xml"))
                {
                    return contentType;
                }
                if (!base.AtomInputContext.UseClientFormatBehavior)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_ContentWithWrongType(str));
                }
                base.XmlReader.SkipElementContent();
            }
            return contentType;
        }

        internal void VerifyEntryStart()
        {
            if (base.XmlReader.NodeType != XmlNodeType.Element)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_ElementExpected(base.XmlReader.NodeType));
            }
            if (!base.XmlReader.NamespaceEquals(this.AtomNamespace) || !base.XmlReader.LocalNameEquals(this.AtomEntryElementName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomEntryAndFeedDeserializer_EntryElementWrongName(base.XmlReader.LocalName, base.XmlReader.NamespaceURI));
            }
        }

        private ODataAtomEntryMetadataDeserializer EntryMetadataDeserializer
        {
            get
            {
                return (this.entryMetadataDeserializer ?? (this.entryMetadataDeserializer = new ODataAtomEntryMetadataDeserializer(base.AtomInputContext)));
            }
        }

        private ODataAtomFeedMetadataDeserializer FeedMetadataDeserializer
        {
            get
            {
                return (this.feedMetadataDeserializer ?? (this.feedMetadataDeserializer = new ODataAtomFeedMetadataDeserializer(base.AtomInputContext, false)));
            }
        }

        private bool ReadAtomMetadata
        {
            get
            {
                return base.AtomInputContext.MessageReaderSettings.EnableAtomMetadataReading;
            }
        }
    }
}

