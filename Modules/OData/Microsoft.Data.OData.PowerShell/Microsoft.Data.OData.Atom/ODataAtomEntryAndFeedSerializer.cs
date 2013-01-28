namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class ODataAtomEntryAndFeedSerializer : ODataAtomPropertyAndValueSerializer
    {
        private readonly ODataAtomEntryMetadataSerializer atomEntryMetadataSerializer;
        private readonly ODataAtomFeedMetadataSerializer atomFeedMetadataSerializer;

        internal ODataAtomEntryAndFeedSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
            this.atomEntryMetadataSerializer = new ODataAtomEntryMetadataSerializer(atomOutputContext);
            this.atomFeedMetadataSerializer = new ODataAtomFeedMetadataSerializer(atomOutputContext);
        }

        internal void WriteAssociationLink(ODataAssociationLink associationLink, IEdmEntityType owningType, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ProjectedPropertiesAnnotation projectedProperties)
        {
            ValidationUtils.ValidateAssociationLinkNotNull(associationLink);
            if (!projectedProperties.ShouldSkipProperty(associationLink.Name))
            {
                base.ValidateAssociationLink(associationLink, owningType);
                duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(associationLink);
                AtomLinkMetadata annotation = associationLink.GetAnnotation<AtomLinkMetadata>();
                string relation = AtomUtils.ComputeODataAssociationLinkRelation(associationLink);
                AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(annotation, relation, associationLink.Url, associationLink.Name, "application/xml");
                this.atomEntryMetadataSerializer.WriteAtomLink(linkMetadata, null);
            }
        }

        internal void WriteEntryEditLink(Uri editLink, AtomEntryMetadata entryMetadata)
        {
            AtomLinkMetadata linkMetadata = (entryMetadata == null) ? null : entryMetadata.EditLink;
            this.WriteReadOrEditLink(editLink, linkMetadata, "edit");
        }

        internal void WriteEntryId(string entryId)
        {
            base.WriteElementWithTextContent("", "id", "http://www.w3.org/2005/Atom", entryId);
        }

        internal void WriteEntryMediaEditLink(ODataStreamReferenceValue mediaResource)
        {
            Uri editLink = mediaResource.EditLink;
            if (editLink != null)
            {
                AtomStreamReferenceMetadata annotation = mediaResource.GetAnnotation<AtomStreamReferenceMetadata>();
                AtomLinkMetadata metadata = (annotation == null) ? null : annotation.EditLink;
                AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(metadata, "edit-media", editLink, null, null);
                this.atomEntryMetadataSerializer.WriteAtomLink(linkMetadata, mediaResource.ETag);
            }
        }

        internal void WriteEntryMetadata(AtomEntryMetadata entryMetadata, AtomEntryMetadata epmEntryMetadata, string updatedTime)
        {
            this.atomEntryMetadataSerializer.WriteEntryMetadata(entryMetadata, epmEntryMetadata, updatedTime);
        }

        internal void WriteEntryPropertiesEnd()
        {
            base.XmlWriter.WriteEndElement();
        }

        internal void WriteEntryPropertiesStart()
        {
            base.XmlWriter.WriteStartElement("m", "properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
        }

        internal void WriteEntryReadLink(Uri readLink, AtomEntryMetadata entryMetadata)
        {
            AtomLinkMetadata linkMetadata = (entryMetadata == null) ? null : entryMetadata.SelfLink;
            this.WriteReadOrEditLink(readLink, linkMetadata, "self");
        }

        internal void WriteEntryTypeName(string typeName, AtomEntryMetadata entryMetadata)
        {
            if (typeName != null)
            {
                AtomCategoryMetadata category = ODataAtomWriterMetadataUtils.MergeCategoryMetadata((entryMetadata == null) ? null : entryMetadata.CategoryWithTypeName, typeName, base.MessageWriterSettings.WriterBehavior.ODataTypeScheme);
                this.atomEntryMetadataSerializer.WriteCategory(category);
            }
        }

        internal void WriteFeedDefaultAuthor()
        {
            this.atomFeedMetadataSerializer.WriteEmptyAuthor();
        }

        internal void WriteFeedMetadata(ODataFeed feed, string updatedTime, out bool authorWritten)
        {
            AtomFeedMetadata annotation = feed.GetAnnotation<AtomFeedMetadata>();
            if (annotation == null)
            {
                base.WriteElementWithTextContent("", "id", "http://www.w3.org/2005/Atom", feed.Id);
                base.WriteEmptyElement("", "title", "http://www.w3.org/2005/Atom");
                base.WriteElementWithTextContent("", "updated", "http://www.w3.org/2005/Atom", updatedTime);
                authorWritten = false;
            }
            else
            {
                this.atomFeedMetadataSerializer.WriteFeedMetadata(annotation, feed, updatedTime, out authorWritten);
            }
        }

        internal void WriteFeedNextPageLink(ODataFeed feed)
        {
            Uri nextPageLink = feed.NextPageLink;
            if (nextPageLink != null)
            {
                AtomFeedMetadata annotation = feed.GetAnnotation<AtomFeedMetadata>();
                AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata((annotation == null) ? null : annotation.NextPageLink, "next", nextPageLink, null, null);
                this.atomFeedMetadataSerializer.WriteAtomLink(linkMetadata, null);
            }
        }

        internal void WriteNavigationLinkStart(ODataNavigationLink navigationLink, Uri navigationLinkUrlOverride)
        {
            base.XmlWriter.WriteStartElement("", "link", "http://www.w3.org/2005/Atom");
            string relation = AtomUtils.ComputeODataNavigationLinkRelation(navigationLink);
            string mediaType = AtomUtils.ComputeODataNavigationLinkType(navigationLink);
            string name = navigationLink.Name;
            Uri href = navigationLinkUrlOverride ?? navigationLink.Url;
            AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(navigationLink.GetAnnotation<AtomLinkMetadata>(), relation, href, name, mediaType);
            this.atomEntryMetadataSerializer.WriteAtomLinkAttributes(linkMetadata, null);
        }

        internal void WriteOperation(ODataOperation operation)
        {
            string str;
            WriterValidationUtils.ValidateOperation(operation, base.WritingResponse);
            if (operation is ODataAction)
            {
                str = "action";
            }
            else
            {
                str = "function";
            }
            base.XmlWriter.WriteStartElement("m", str, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            string str2 = base.UriToUrlAttributeValue(operation.Metadata, false);
            base.XmlWriter.WriteAttributeString("metadata", str2);
            if (operation.Title != null)
            {
                base.XmlWriter.WriteAttributeString("title", operation.Title);
            }
            string str3 = base.UriToUrlAttributeValue(operation.Target);
            base.XmlWriter.WriteAttributeString("target", str3);
            base.XmlWriter.WriteEndElement();
        }

        private void WriteReadOrEditLink(Uri link, AtomLinkMetadata linkMetadata, string linkRelation)
        {
            if (link != null)
            {
                AtomLinkMetadata metadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(linkMetadata, linkRelation, link, null, null);
                this.atomEntryMetadataSerializer.WriteAtomLink(metadata, null);
            }
        }

        internal void WriteStreamProperty(ODataProperty streamProperty, IEdmEntityType owningType, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ProjectedPropertiesAnnotation projectedProperties)
        {
            WriterValidationUtils.ValidatePropertyNotNull(streamProperty);
            string name = streamProperty.Name;
            if (!projectedProperties.ShouldSkipProperty(name))
            {
                WriterValidationUtils.ValidateProperty(streamProperty);
                duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(streamProperty);
                IEdmProperty edmProperty = WriterValidationUtils.ValidatePropertyDefined(streamProperty.Name, owningType);
                WriterValidationUtils.ValidateStreamReferenceProperty(streamProperty, edmProperty, base.Version, base.WritingResponse);
                ODataStreamReferenceValue value2 = (ODataStreamReferenceValue) streamProperty.Value;
                if (((owningType != null) && owningType.IsOpen) && (edmProperty == null))
                {
                    ValidationUtils.ValidateOpenPropertyValue(streamProperty.Name, value2);
                }
                AtomStreamReferenceMetadata annotation = value2.GetAnnotation<AtomStreamReferenceMetadata>();
                string contentType = value2.ContentType;
                string title = streamProperty.Name;
                Uri readLink = value2.ReadLink;
                if (readLink != null)
                {
                    string relation = AtomUtils.ComputeStreamPropertyRelation(streamProperty, false);
                    AtomLinkMetadata metadata = (annotation == null) ? null : annotation.SelfLink;
                    AtomLinkMetadata linkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(metadata, relation, readLink, title, contentType);
                    this.atomEntryMetadataSerializer.WriteAtomLink(linkMetadata, null);
                }
                Uri editLink = value2.EditLink;
                if (editLink != null)
                {
                    string str5 = AtomUtils.ComputeStreamPropertyRelation(streamProperty, true);
                    AtomLinkMetadata metadata4 = (annotation == null) ? null : annotation.EditLink;
                    AtomLinkMetadata metadata5 = ODataAtomWriterMetadataUtils.MergeLinkMetadata(metadata4, str5, editLink, title, contentType);
                    this.atomEntryMetadataSerializer.WriteAtomLink(metadata5, value2.ETag);
                }
            }
        }
    }
}

