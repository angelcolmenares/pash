namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Globalization;

    internal sealed class EpmSyndicationWriter : EpmWriter
    {
        private readonly AtomEntryMetadata entryMetadata;
        private readonly EpmTargetTree epmTargetTree;

        private EpmSyndicationWriter(EpmTargetTree epmTargetTree, ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
            this.epmTargetTree = epmTargetTree;
            this.entryMetadata = new AtomEntryMetadata();
        }

        private static AtomTextConstruct CreateAtomTextConstruct(string textValue, SyndicationTextContentKind contentKind)
        {
            AtomTextConstructKind text;
            switch (contentKind)
            {
                case SyndicationTextContentKind.Plaintext:
                    text = AtomTextConstructKind.Text;
                    break;

                case SyndicationTextContentKind.Html:
                    text = AtomTextConstructKind.Html;
                    break;

                case SyndicationTextContentKind.Xhtml:
                    text = AtomTextConstructKind.Xhtml;
                    break;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationWriter_CreateAtomTextConstruct));
            }
            return new AtomTextConstruct { Kind = text, Text = textValue };
        }

        private static string CreateDateTimeStringValue(object propertyValue, ODataWriterBehavior writerBehavior)
        {
            if (propertyValue == null)
            {
                propertyValue = DateTimeOffset.Now;
            }
            if (propertyValue is DateTime)
            {
                propertyValue = new DateTimeOffset((DateTime) propertyValue);
            }
            if (propertyValue is DateTimeOffset)
            {
                return ODataAtomConvert.ToAtomString((DateTimeOffset) propertyValue);
            }
            return EpmWriterUtils.GetPropertyValueAsText(propertyValue);
        }

        private static DateTimeOffset CreateDateTimeValue(object propertyValue, SyndicationItemProperty targetProperty, ODataWriterBehavior writerBehavior)
        {
            DateTimeOffset offset;
            DateTime time;
            if (propertyValue == null)
            {
                return DateTimeOffset.Now;
            }
            if (propertyValue is DateTimeOffset)
            {
                return (DateTimeOffset) propertyValue;
            }
            if (propertyValue is DateTime)
            {
                return new DateTimeOffset((DateTime) propertyValue);
            }
            string input = propertyValue as string;
            if (input == null)
            {
                DateTimeOffset offset2;
                try
                {
                    offset2 = new DateTimeOffset(Convert.ToDateTime(propertyValue, CultureInfo.InvariantCulture));
                }
                catch (Exception exception)
                {
                    if (!ExceptionUtils.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSyndicationWriter_DateTimePropertyCanNotBeConverted(targetProperty.ToString()));
                }
                return offset2;
            }
            if (DateTimeOffset.TryParse(input, out offset))
            {
                return offset;
            }
            if (!DateTime.TryParse(input, out time))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmSyndicationWriter_DateTimePropertyCanNotBeConverted(targetProperty.ToString()));
            }
            return new DateTimeOffset(time);
        }

        private string GetPropertyValueAsText(EpmTargetPathSegment targetSegment, object epmValueCache, IEdmTypeReference typeReference)
        {
            object obj2;
            EntryPropertiesValueCache cache = epmValueCache as EntryPropertiesValueCache;
            if (cache != null)
            {
                obj2 = base.ReadEntryPropertyValue(targetSegment.EpmInfo, cache, typeReference.AsEntity());
            }
            else
            {
                obj2 = epmValueCache;
                ValidationUtils.ValidateIsExpectedPrimitiveType(obj2, typeReference);
            }
            return EpmWriterUtils.GetPropertyValueAsText(obj2);
        }

        private AtomEntryMetadata WriteEntryEpm(EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType)
        {
            EpmTargetPathSegment syndicationRoot = this.epmTargetTree.SyndicationRoot;
            if (syndicationRoot.SubSegments.Count == 0)
            {
                return null;
            }
            foreach (EpmTargetPathSegment segment2 in syndicationRoot.SubSegments)
            {
                if (!segment2.HasContent)
                {
                    goto Label_018C;
                }
                EntityPropertyMappingInfo epmInfo = segment2.EpmInfo;
                object propertyValue = base.ReadEntryPropertyValue(epmInfo, epmValueCache, entityType);
                string propertyValueAsText = EpmWriterUtils.GetPropertyValueAsText(propertyValue);
                switch (epmInfo.Attribute.TargetSyndicationItem)
                {
                    case SyndicationItemProperty.Updated:
                    {
                        if (base.WriterBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesClient)
                        {
                            break;
                        }
                        this.entryMetadata.UpdatedString = CreateDateTimeStringValue(propertyValue, base.WriterBehavior);
                        continue;
                    }
                    case SyndicationItemProperty.Published:
                    {
                        if (base.WriterBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesClient)
                        {
                            goto Label_00FE;
                        }
                        this.entryMetadata.PublishedString = CreateDateTimeStringValue(propertyValue, base.WriterBehavior);
                        continue;
                    }
                    case SyndicationItemProperty.Rights:
                    {
                        this.entryMetadata.Rights = CreateAtomTextConstruct(propertyValueAsText, epmInfo.Attribute.TargetTextContentKind);
                        continue;
                    }
                    case SyndicationItemProperty.Summary:
                    {
                        this.entryMetadata.Summary = CreateAtomTextConstruct(propertyValueAsText, epmInfo.Attribute.TargetTextContentKind);
                        continue;
                    }
                    case SyndicationItemProperty.Title:
                    {
                        this.entryMetadata.Title = CreateAtomTextConstruct(propertyValueAsText, epmInfo.Attribute.TargetTextContentKind);
                        continue;
                    }
                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationWriter_WriteEntryEpm_ContentTarget));
                }
                this.entryMetadata.Updated = new DateTimeOffset?(CreateDateTimeValue(propertyValue, SyndicationItemProperty.Updated, base.WriterBehavior));
                continue;
            Label_00FE:
                this.entryMetadata.Published = new DateTimeOffset?(CreateDateTimeValue(propertyValue, SyndicationItemProperty.Published, base.WriterBehavior));
                continue;
            Label_018C:
                this.WriteParentSegment(segment2, epmValueCache, entityType);
            }
            return this.entryMetadata;
        }

        internal static AtomEntryMetadata WriteEntryEpm(EpmTargetTree epmTargetTree, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference type, ODataAtomOutputContext atomOutputContext)
        {
            EpmSyndicationWriter writer = new EpmSyndicationWriter(epmTargetTree, atomOutputContext);
            return writer.WriteEntryEpm(epmValueCache, type);
        }

        private void WriteParentSegment(EpmTargetPathSegment targetSegment, object epmValueCache, IEdmTypeReference typeReference)
        {
            if (targetSegment.SegmentName == "author")
            {
                AtomPersonMetadata item = this.WritePersonEpm(targetSegment, epmValueCache, typeReference);
                if (item != null)
                {
                    List<AtomPersonMetadata> authors = (List<AtomPersonMetadata>) this.entryMetadata.Authors;
                    if (authors == null)
                    {
                        authors = new List<AtomPersonMetadata>();
                        this.entryMetadata.Authors = authors;
                    }
                    authors.Add(item);
                }
            }
            else
            {
                if (!(targetSegment.SegmentName == "contributor"))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationWriter_WriteParentSegment_TargetSegmentName));
                }
                AtomPersonMetadata metadata2 = this.WritePersonEpm(targetSegment, epmValueCache, typeReference);
                if (metadata2 != null)
                {
                    List<AtomPersonMetadata> contributors = (List<AtomPersonMetadata>) this.entryMetadata.Contributors;
                    if (contributors == null)
                    {
                        contributors = new List<AtomPersonMetadata>();
                        this.entryMetadata.Contributors = contributors;
                    }
                    contributors.Add(metadata2);
                }
            }
        }

        private AtomPersonMetadata WritePersonEpm(EpmTargetPathSegment targetSegment, object epmValueCache, IEdmTypeReference typeReference)
        {
            AtomPersonMetadata metadata = null;
            foreach (EpmTargetPathSegment segment in targetSegment.SubSegments)
            {
                string str = this.GetPropertyValueAsText(segment, epmValueCache, typeReference);
                if (str != null)
                {
                    switch (segment.EpmInfo.Attribute.TargetSyndicationItem)
                    {
                        case SyndicationItemProperty.AuthorEmail:
                        case SyndicationItemProperty.ContributorEmail:
                        {
                            if ((str != null) && (str.Length > 0))
                            {
                                if (metadata == null)
                                {
                                    metadata = new AtomPersonMetadata();
                                }
                                metadata.Email = str;
                            }
                            continue;
                        }
                        case SyndicationItemProperty.AuthorName:
                        case SyndicationItemProperty.ContributorName:
                        {
                            if (str != null)
                            {
                                if (metadata == null)
                                {
                                    metadata = new AtomPersonMetadata();
                                }
                                metadata.Name = str;
                            }
                            continue;
                        }
                        case SyndicationItemProperty.AuthorUri:
                        case SyndicationItemProperty.ContributorUri:
                        {
                            if ((str != null) && (str.Length > 0))
                            {
                                if (metadata == null)
                                {
                                    metadata = new AtomPersonMetadata();
                                }
                                metadata.UriFromEpm = str;
                            }
                            continue;
                        }
                    }
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationWriter_WritePersonEpm));
                }
            }
            return metadata;
        }
    }
}

