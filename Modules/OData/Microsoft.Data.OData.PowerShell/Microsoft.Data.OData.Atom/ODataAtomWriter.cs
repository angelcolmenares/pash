namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml;

    internal sealed class ODataAtomWriter : ODataWriterCore
    {
        private readonly ODataAtomEntryAndFeedSerializer atomEntryAndFeedSerializer;
        private readonly ODataAtomOutputContext atomOutputContext;
        private readonly string updatedTime;

        internal ODataAtomWriter(ODataAtomOutputContext atomOutputContext, bool writingFeed) : base(atomOutputContext, writingFeed)
        {
            this.updatedTime = ODataAtomConvert.ToAtomString(DateTimeOffset.UtcNow);
            this.atomOutputContext = atomOutputContext;
            if (this.atomOutputContext.MessageWriterSettings.WriterBehavior.StartEntryXmlCustomizationCallback != null)
            {
                this.atomOutputContext.InitializeWriterCustomization();
            }
            this.atomEntryAndFeedSerializer = new ODataAtomEntryAndFeedSerializer(this.atomOutputContext);
        }

        private void CheckAndWriteParentNavigationLinkEndForInlineElement()
        {
            if (base.ParentNavigationLink != null)
            {
                this.atomOutputContext.XmlWriter.WriteEndElement();
                this.WriteNavigationLinkEnd();
            }
        }

        private void CheckAndWriteParentNavigationLinkStartForInlineElement()
        {
            ODataNavigationLink parentNavigationLink = base.ParentNavigationLink;
            if (parentNavigationLink != null)
            {
                this.WriteNavigationLinkStart(parentNavigationLink, null);
                this.atomOutputContext.XmlWriter.WriteStartElement("m", "inline", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            }
        }

        protected override ODataWriterCore.EntryScope CreateEntryScope(ODataEntry entry, bool skipWriting)
        {
            return new AtomEntryScope(entry, skipWriting, this.atomOutputContext.WritingResponse, this.atomOutputContext.MessageWriterSettings.WriterBehavior);
        }

        protected override ODataWriterCore.FeedScope CreateFeedScope(ODataFeed feed, bool skipWriting)
        {
            return new AtomFeedScope(feed, skipWriting);
        }

        protected override void EndEntry(ODataEntry entry)
        {
            if (entry == null)
            {
                this.CheckAndWriteParentNavigationLinkEndForInlineElement();
            }
            else
            {
                IEdmEntityType entryEntityType = base.EntryEntityType;
                EntryPropertiesValueCache propertyValueCache = new EntryPropertiesValueCache(entry);
                ODataEntityPropertyMappingCache cache2 = this.atomOutputContext.Model.EnsureEpmCache(entryEntityType, 0x7fffffff);
                if (cache2 != null)
                {
                    EpmWriterUtils.CacheEpmProperties(propertyValueCache, cache2.EpmSourceTree);
                }
                ProjectedPropertiesAnnotation projectedProperties = entry.GetAnnotation<ProjectedPropertiesAnnotation>();
                AtomEntryScope currentEntryScope = this.CurrentEntryScope;
                AtomEntryMetadata entryMetadata = entry.Atom();
                if (!currentEntryScope.IsElementWritten(AtomElement.Id))
                {
                    this.atomEntryAndFeedSerializer.WriteEntryId(entry.Id);
                }
                Uri editLink = entry.EditLink;
                if ((editLink != null) && !currentEntryScope.IsElementWritten(AtomElement.EditLink))
                {
                    this.atomEntryAndFeedSerializer.WriteEntryEditLink(editLink, entryMetadata);
                }
                Uri readLink = entry.ReadLink;
                if ((readLink != null) && !currentEntryScope.IsElementWritten(AtomElement.ReadLink))
                {
                    this.atomEntryAndFeedSerializer.WriteEntryReadLink(readLink, entryMetadata);
                }
                AtomEntryMetadata epmEntryMetadata = null;
                if (cache2 != null)
                {
                    ODataVersionChecker.CheckEntityPropertyMapping(this.atomOutputContext.Version, entryEntityType, this.atomOutputContext.Model);
                    epmEntryMetadata = EpmSyndicationWriter.WriteEntryEpm(cache2.EpmTargetTree, propertyValueCache, entryEntityType.ToTypeReference().AsEntity(), this.atomOutputContext);
                }
                this.atomEntryAndFeedSerializer.WriteEntryMetadata(entryMetadata, epmEntryMetadata, this.updatedTime);
                IEnumerable<ODataProperty> entryStreamProperties = propertyValueCache.EntryStreamProperties;
                if (entryStreamProperties != null)
                {
                    foreach (ODataProperty property in entryStreamProperties)
                    {
                        this.atomEntryAndFeedSerializer.WriteStreamProperty(property, entryEntityType, base.DuplicatePropertyNamesChecker, projectedProperties);
                    }
                }
                IEnumerable<ODataAssociationLink> associationLinks = entry.AssociationLinks;
                if (associationLinks != null)
                {
                    foreach (ODataAssociationLink link in associationLinks)
                    {
                        this.atomEntryAndFeedSerializer.WriteAssociationLink(link, entryEntityType, base.DuplicatePropertyNamesChecker, projectedProperties);
                    }
                }
                IEnumerable<ODataAction> actions = entry.Actions;
                if (actions != null)
                {
                    foreach (ODataAction action in actions)
                    {
                        ValidationUtils.ValidateOperationNotNull(action, true);
                        this.atomEntryAndFeedSerializer.WriteOperation(action);
                    }
                }
                IEnumerable<ODataFunction> functions = entry.Functions;
                if (functions != null)
                {
                    foreach (ODataFunction function in functions)
                    {
                        ValidationUtils.ValidateOperationNotNull(function, false);
                        this.atomEntryAndFeedSerializer.WriteOperation(function);
                    }
                }
                this.WriteEntryContent(entry, entryEntityType, propertyValueCache, (cache2 == null) ? null : cache2.EpmSourceTree.Root, projectedProperties);
                if (cache2 != null)
                {
                    EpmCustomWriter.WriteEntryEpm(this.atomOutputContext.XmlWriter, cache2.EpmTargetTree, propertyValueCache, entryEntityType.ToTypeReference().AsEntity(), this.atomOutputContext);
                }
                this.atomOutputContext.XmlWriter.WriteEndElement();
                this.EndEntryXmlCustomization(entry);
                this.CheckAndWriteParentNavigationLinkEndForInlineElement();
            }
        }

        private void EndEntryXmlCustomization(ODataEntry entry)
        {
            if (this.atomOutputContext.MessageWriterSettings.WriterBehavior.StartEntryXmlCustomizationCallback != null)
            {
                XmlWriter objB = this.atomOutputContext.PopCustomWriter();
                if (!object.ReferenceEquals(this.atomOutputContext.XmlWriter, objB))
                {
                    this.atomOutputContext.MessageWriterSettings.WriterBehavior.EndEntryXmlCustomizationCallback(entry, objB, this.atomOutputContext.XmlWriter);
                }
            }
        }

        protected override void EndFeed(ODataFeed feed)
        {
            AtomFeedScope currentFeedScope = this.CurrentFeedScope;
            if (!currentFeedScope.AuthorWritten && (currentFeedScope.EntryCount == 0))
            {
                this.atomEntryAndFeedSerializer.WriteFeedDefaultAuthor();
            }
            this.atomEntryAndFeedSerializer.WriteFeedNextPageLink(feed);
            this.atomOutputContext.XmlWriter.WriteEndElement();
            this.CheckAndWriteParentNavigationLinkEndForInlineElement();
        }

        protected override void EndNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
        }

        protected override void EndPayload()
        {
            this.atomEntryAndFeedSerializer.WritePayloadEnd();
        }

        protected override Task FlushAsynchronously()
        {
            return this.atomOutputContext.FlushAsync();
        }

        protected override void FlushSynchronously()
        {
            this.atomOutputContext.Flush();
        }

        protected override void StartEntry(ODataEntry entry)
        {
            this.CheckAndWriteParentNavigationLinkStartForInlineElement();
            if (entry != null)
            {
                this.StartEntryXmlCustomization(entry);
                this.atomOutputContext.XmlWriter.WriteStartElement("", "entry", "http://www.w3.org/2005/Atom");
                if (base.IsTopLevel)
                {
                    this.atomEntryAndFeedSerializer.WriteBaseUriAndDefaultNamespaceAttributes();
                }
                string eTag = entry.ETag;
                if (eTag != null)
                {
                    ODataAtomWriterUtils.WriteETag(this.atomOutputContext.XmlWriter, eTag);
                }
                AtomEntryScope currentEntryScope = this.CurrentEntryScope;
                AtomEntryMetadata entryMetadata = entry.Atom();
                string id = entry.Id;
                if (id != null)
                {
                    this.atomEntryAndFeedSerializer.WriteEntryId(id);
                    currentEntryScope.SetWrittenElement(AtomElement.Id);
                }
                string typeName = entry.TypeName;
                SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
                if (annotation != null)
                {
                    typeName = annotation.TypeName;
                }
                this.atomEntryAndFeedSerializer.WriteEntryTypeName(typeName, entryMetadata);
                Uri editLink = entry.EditLink;
                if (editLink != null)
                {
                    this.atomEntryAndFeedSerializer.WriteEntryEditLink(editLink, entryMetadata);
                    currentEntryScope.SetWrittenElement(AtomElement.EditLink);
                }
                Uri readLink = entry.ReadLink;
                if (readLink != null)
                {
                    this.atomEntryAndFeedSerializer.WriteEntryReadLink(readLink, entryMetadata);
                    currentEntryScope.SetWrittenElement(AtomElement.ReadLink);
                }
            }
        }

        private void StartEntryXmlCustomization(ODataEntry entry)
        {
            if (this.atomOutputContext.MessageWriterSettings.WriterBehavior.StartEntryXmlCustomizationCallback != null)
            {
                XmlWriter objB = this.atomOutputContext.MessageWriterSettings.WriterBehavior.StartEntryXmlCustomizationCallback(entry, this.atomOutputContext.XmlWriter);
                if (objB != null)
                {
                    if (object.ReferenceEquals(this.atomOutputContext.XmlWriter, objB))
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomWriter_StartEntryXmlCustomizationCallbackReturnedSameInstance);
                    }
                }
                else
                {
                    objB = this.atomOutputContext.XmlWriter;
                }
                this.atomOutputContext.PushCustomWriter(objB);
            }
        }

        protected override void StartFeed(ODataFeed feed)
        {
            bool flag;
            this.CheckAndWriteParentNavigationLinkStartForInlineElement();
            this.atomOutputContext.XmlWriter.WriteStartElement("", "feed", "http://www.w3.org/2005/Atom");
            if (base.IsTopLevel)
            {
                this.atomEntryAndFeedSerializer.WriteBaseUriAndDefaultNamespaceAttributes();
                if (feed.Count.HasValue)
                {
                    this.atomEntryAndFeedSerializer.WriteCount(feed.Count.Value, false);
                }
            }
            this.atomEntryAndFeedSerializer.WriteFeedMetadata(feed, this.updatedTime, out flag);
            this.CurrentFeedScope.AuthorWritten = flag;
        }

        protected override void StartNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
        }

        protected override void StartPayload()
        {
            this.atomEntryAndFeedSerializer.WritePayloadStart();
        }

        protected override void VerifyNotDisposed()
        {
            this.atomOutputContext.VerifyNotDisposed();
        }

        protected override void WriteDeferredNavigationLink(ODataNavigationLink navigationLink)
        {
            this.WriteNavigationLinkStart(navigationLink, null);
            this.WriteNavigationLinkEnd();
        }

        protected override void WriteEntityReferenceInNavigationLinkContent(ODataNavigationLink parentNavigationLink, ODataEntityReferenceLink entityReferenceLink)
        {
            this.WriteNavigationLinkStart(parentNavigationLink, entityReferenceLink.Url);
            this.WriteNavigationLinkEnd();
        }

        private void WriteEntryContent(ODataEntry entry, IEdmEntityType entryType, EntryPropertiesValueCache propertiesValueCache, EpmSourcePathSegment rootSourcePathSegment, ProjectedPropertiesAnnotation projectedProperties)
        {
            ODataStreamReferenceValue mediaResource = entry.MediaResource;
            if (mediaResource == null)
            {
                this.atomOutputContext.XmlWriter.WriteStartElement("", "content", "http://www.w3.org/2005/Atom");
                this.atomOutputContext.XmlWriter.WriteAttributeString("type", "application/xml");
                this.atomEntryAndFeedSerializer.WriteProperties(entryType, propertiesValueCache.EntryProperties, false, new Action(this.atomEntryAndFeedSerializer.WriteEntryPropertiesStart), new Action(this.atomEntryAndFeedSerializer.WriteEntryPropertiesEnd), base.DuplicatePropertyNamesChecker, propertiesValueCache, rootSourcePathSegment, projectedProperties);
                this.atomOutputContext.XmlWriter.WriteEndElement();
            }
            else
            {
                WriterValidationUtils.ValidateStreamReferenceValue(mediaResource, true);
                this.atomEntryAndFeedSerializer.WriteEntryMediaEditLink(mediaResource);
                if (mediaResource.ReadLink != null)
                {
                    this.atomOutputContext.XmlWriter.WriteStartElement("", "content", "http://www.w3.org/2005/Atom");
                    this.atomOutputContext.XmlWriter.WriteAttributeString("type", mediaResource.ContentType);
                    this.atomOutputContext.XmlWriter.WriteAttributeString("src", this.atomEntryAndFeedSerializer.UriToUrlAttributeValue(mediaResource.ReadLink));
                    this.atomOutputContext.XmlWriter.WriteEndElement();
                }
                this.atomEntryAndFeedSerializer.WriteProperties(entryType, propertiesValueCache.EntryProperties, false, new Action(this.atomEntryAndFeedSerializer.WriteEntryPropertiesStart), new Action(this.atomEntryAndFeedSerializer.WriteEntryPropertiesEnd), base.DuplicatePropertyNamesChecker, propertiesValueCache, rootSourcePathSegment, projectedProperties);
            }
        }

        private void WriteNavigationLinkEnd()
        {
            this.atomOutputContext.XmlWriter.WriteEndElement();
        }

        private void WriteNavigationLinkStart(ODataNavigationLink navigationLink, Uri navigationLinkUrlOverride)
        {
            if (!navigationLink.IsCollection.HasValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriterCore_LinkMustSpecifyIsCollection);
            }
            if (navigationLink.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriter_NavigationLinkMustSpecifyUrl);
            }
            this.atomEntryAndFeedSerializer.WriteNavigationLinkStart(navigationLink, navigationLinkUrlOverride);
        }

        private AtomEntryScope CurrentEntryScope
        {
            get
            {
                return (base.CurrentScope as AtomEntryScope);
            }
        }

        private AtomFeedScope CurrentFeedScope
        {
            get
            {
                return (base.CurrentScope as AtomFeedScope);
            }
        }

        private enum AtomElement
        {
            EditLink = 4,
            Id = 1,
            ReadLink = 2
        }

        private sealed class AtomEntryScope : ODataWriterCore.EntryScope
        {
            private int alreadyWrittenElements;

            internal AtomEntryScope(ODataEntry entry, bool skipWriting, bool writingResponse, ODataWriterBehavior writerBehavior) : base(entry, skipWriting, writingResponse, writerBehavior)
            {
            }

            internal bool IsElementWritten(ODataAtomWriter.AtomElement atomElement)
            {
				return (((ODataAtomWriter.AtomElement)this.alreadyWrittenElements & atomElement) == (ODataAtomWriter.AtomElement)atomElement);
            }

            internal void SetWrittenElement(ODataAtomWriter.AtomElement atomElement)
            {
                this.alreadyWrittenElements |= (int)atomElement;
            }
        }

        private sealed class AtomFeedScope : ODataWriterCore.FeedScope
        {
            private bool authorWritten;

            internal AtomFeedScope(ODataFeed feed, bool skipWriting) : base(feed, skipWriting)
            {
            }

            internal bool AuthorWritten
            {
                get
                {
                    return this.authorWritten;
                }
                set
                {
                    this.authorWritten = value;
                }
            }
        }
    }
}

