namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Xml;

    internal sealed class EpmSyndicationReader : EpmReader
    {
        private EpmSyndicationReader(IODataAtomReaderEntryState entryState, ODataAtomInputContext inputContext) : base(entryState, inputContext)
        {
        }

        private void ReadEntryEpm()
        {
            AtomEntryMetadata atomEntryMetadata = base.EntryState.AtomEntryMetadata;
            EpmTargetPathSegment syndicationRoot = base.EntryState.CachedEpm.EpmTargetTree.SyndicationRoot;
            if (syndicationRoot.SubSegments.Count != 0)
            {
                foreach (EpmTargetPathSegment segment2 in syndicationRoot.SubSegments)
                {
                    if (segment2.HasContent)
                    {
                        this.ReadPropertyValueSegment(segment2, atomEntryMetadata);
                    }
                    else
                    {
                        this.ReadParentSegment(segment2, atomEntryMetadata);
                    }
                }
            }
        }

        internal static void ReadEntryEpm(IODataAtomReaderEntryState entryState, ODataAtomInputContext inputContext)
        {
            new EpmSyndicationReader(entryState, inputContext).ReadEntryEpm();
        }

        private void ReadParentSegment(EpmTargetPathSegment targetSegment, AtomEntryMetadata entryMetadata)
        {
            switch (targetSegment.SegmentName)
            {
                case "author":
                {
                    AtomPersonMetadata personMetadata = entryMetadata.Authors.FirstOrDefault<AtomPersonMetadata>();
                    if (personMetadata != null)
                    {
                        this.ReadPersonEpm(ReaderUtils.GetPropertiesList(base.EntryState.Entry.Properties), base.EntryState.EntityType.ToTypeReference(), targetSegment, personMetadata);
                    }
                    return;
                }
                case "contributor":
                {
                    AtomPersonMetadata metadata2 = entryMetadata.Contributors.FirstOrDefault<AtomPersonMetadata>();
                    if (metadata2 != null)
                    {
                        this.ReadPersonEpm(ReaderUtils.GetPropertiesList(base.EntryState.Entry.Properties), base.EntryState.EntityType.ToTypeReference(), targetSegment, metadata2);
                    }
                    return;
                }
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationReader_ReadParentSegment_TargetSegmentName));
        }

        private void ReadPersonEpm(IList targetList, IEdmTypeReference targetTypeReference, EpmTargetPathSegment targetSegment, AtomPersonMetadata personMetadata)
        {
            foreach (EpmTargetPathSegment segment in targetSegment.SubSegments)
            {
                switch (segment.EpmInfo.Attribute.TargetSyndicationItem)
                {
                    case SyndicationItemProperty.AuthorEmail:
                    case SyndicationItemProperty.ContributorEmail:
                    {
                        string email = personMetadata.Email;
                        if (email != null)
                        {
                            base.SetEpmValue(targetList, targetTypeReference, segment.EpmInfo, email);
                        }
                        break;
                    }
                    case SyndicationItemProperty.AuthorName:
                    case SyndicationItemProperty.ContributorName:
                    {
                        string name = personMetadata.Name;
                        if (name != null)
                        {
                            base.SetEpmValue(targetList, targetTypeReference, segment.EpmInfo, name);
                        }
                        break;
                    }
                    case SyndicationItemProperty.AuthorUri:
                    case SyndicationItemProperty.ContributorUri:
                    {
                        string uriFromEpm = personMetadata.UriFromEpm;
                        if (uriFromEpm != null)
                        {
                            base.SetEpmValue(targetList, targetTypeReference, segment.EpmInfo, uriFromEpm);
                        }
                        break;
                    }
                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationReader_ReadPersonEpm));
                }
            }
        }

        private void ReadPropertyValueSegment(EpmTargetPathSegment targetSegment, AtomEntryMetadata entryMetadata)
        {
            switch (targetSegment.EpmInfo.Attribute.TargetSyndicationItem)
            {
                case SyndicationItemProperty.Updated:
                    if (base.MessageReaderSettings.ReaderBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesClient)
                    {
                        if (!entryMetadata.Updated.HasValue)
                        {
                            break;
                        }
                        base.SetEntryEpmValue(targetSegment.EpmInfo, XmlConvert.ToString(entryMetadata.Updated.Value));
                        return;
                    }
                    if (entryMetadata.UpdatedString == null)
                    {
                        break;
                    }
                    base.SetEntryEpmValue(targetSegment.EpmInfo, entryMetadata.UpdatedString);
                    return;

                case SyndicationItemProperty.Published:
                    if (base.MessageReaderSettings.ReaderBehavior.FormatBehaviorKind != ODataBehaviorKind.WcfDataServicesClient)
                    {
                        if (entryMetadata.Published.HasValue)
                        {
                            base.SetEntryEpmValue(targetSegment.EpmInfo, XmlConvert.ToString(entryMetadata.Published.Value));
                            return;
                        }
                        break;
                    }
                    if (entryMetadata.PublishedString == null)
                    {
                        break;
                    }
                    base.SetEntryEpmValue(targetSegment.EpmInfo, entryMetadata.PublishedString);
                    return;

                case SyndicationItemProperty.Rights:
                    this.ReadTextConstructEpm(targetSegment, entryMetadata.Rights);
                    return;

                case SyndicationItemProperty.Summary:
                    this.ReadTextConstructEpm(targetSegment, entryMetadata.Summary);
                    return;

                case SyndicationItemProperty.Title:
                    this.ReadTextConstructEpm(targetSegment, entryMetadata.Title);
                    return;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmSyndicationReader_ReadEntryEpm_ContentTarget));
            }
        }

        private void ReadTextConstructEpm(EpmTargetPathSegment targetSegment, AtomTextConstruct textConstruct)
        {
            if ((textConstruct != null) && (textConstruct.Text != null))
            {
                base.SetEntryEpmValue(targetSegment.EpmInfo, textConstruct.Text);
            }
        }
    }
}

