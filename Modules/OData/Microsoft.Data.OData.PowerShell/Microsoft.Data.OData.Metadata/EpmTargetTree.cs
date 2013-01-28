namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class EpmTargetTree
    {
        private int countOfNonContentV2Mappings;
        private readonly EpmTargetPathSegment nonSyndicationRoot = new EpmTargetPathSegment();
        private readonly EpmTargetPathSegment syndicationRoot = new EpmTargetPathSegment();

        internal EpmTargetTree()
        {
        }

        internal void Add(EntityPropertyMappingInfo epmInfo)
        {
            string targetPath = epmInfo.Attribute.TargetPath;
            string namespaceUri = epmInfo.Attribute.TargetNamespaceUri;
            string targetNamespacePrefix = epmInfo.Attribute.TargetNamespacePrefix;
            EpmTargetPathSegment parentSegment = epmInfo.IsSyndicationMapping ? this.SyndicationRoot : this.NonSyndicationRoot;
            IList<EpmTargetPathSegment> subSegments = parentSegment.SubSegments;
            string[] strArray = targetPath.Split(new char[] { '/' });
            EpmTargetPathSegment segment2 = null;
            for (int i = 0; i < strArray.Length; i++)
            {
                string targetSegment = strArray[i];
                if (targetSegment.Length == 0)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmTargetTree_InvalidTargetPath_EmptySegment(targetPath));
                }
                if ((targetSegment[0] == '@') && (i != (strArray.Length - 1)))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmTargetTree_AttributeInMiddle(targetSegment));
                }
                segment2 = subSegments.SingleOrDefault<EpmTargetPathSegment>(delegate (EpmTargetPathSegment segment) {
                    if (!(segment.SegmentName == targetSegment))
                    {
                        return false;
                    }
                    if (!epmInfo.IsSyndicationMapping)
                    {
                        return segment.SegmentNamespaceUri == namespaceUri;
                    }
                    return true;
                });
                if (segment2 != null)
                {
                    parentSegment = segment2;
                }
                else
                {
                    parentSegment = new EpmTargetPathSegment(targetSegment, namespaceUri, targetNamespacePrefix, parentSegment);
                    if (targetSegment[0] == '@')
                    {
                        subSegments.Insert(0, parentSegment);
                    }
                    else
                    {
                        subSegments.Add(parentSegment);
                    }
                }
                subSegments = parentSegment.SubSegments;
            }
            if (parentSegment.EpmInfo != null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmTargetTree_DuplicateEpmAttributesWithSameTargetName(parentSegment.EpmInfo.DefiningType.ODataFullName(), GetPropertyNameFromEpmInfo(parentSegment.EpmInfo), parentSegment.EpmInfo.Attribute.SourcePath, epmInfo.Attribute.SourcePath));
            }
            if (!epmInfo.Attribute.KeepInContent)
            {
                this.countOfNonContentV2Mappings++;
            }
            parentSegment.EpmInfo = epmInfo;
            List<EntityPropertyMappingAttribute> ancestorsWithContent = new List<EntityPropertyMappingAttribute>(2);
            if (HasMixedContent(this.NonSyndicationRoot, ancestorsWithContent))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmTargetTree_InvalidTargetPath_MixedContent(ancestorsWithContent[0].TargetPath, ancestorsWithContent[1].TargetPath));
            }
        }

        private static string GetPropertyNameFromEpmInfo(EntityPropertyMappingInfo epmInfo)
        {
            if (epmInfo.Attribute.TargetSyndicationItem == SyndicationItemProperty.CustomProperty)
            {
                return epmInfo.Attribute.TargetPath;
            }
            return epmInfo.Attribute.TargetSyndicationItem.ToString();
        }

        private static bool HasMixedContent(EpmTargetPathSegment currentSegment, List<EntityPropertyMappingAttribute> ancestorsWithContent)
        {
            foreach (EpmTargetPathSegment segment in from s in currentSegment.SubSegments
                where !s.IsAttribute
                select s)
            {
                if (segment.HasContent && (ancestorsWithContent.Count == 1))
                {
                    ancestorsWithContent.Add(segment.EpmInfo.Attribute);
                    return true;
                }
                if (segment.HasContent)
                {
                    ancestorsWithContent.Add(segment.EpmInfo.Attribute);
                }
                if (HasMixedContent(segment, ancestorsWithContent))
                {
                    return true;
                }
                if (segment.HasContent)
                {
                    ancestorsWithContent.Clear();
                }
            }
            return false;
        }

        internal void Remove(EntityPropertyMappingInfo epmInfo)
        {
            string targetPath = epmInfo.Attribute.TargetPath;
            string namespaceUri = epmInfo.Attribute.TargetNamespaceUri;
            EpmTargetPathSegment item = epmInfo.IsSyndicationMapping ? this.SyndicationRoot : this.NonSyndicationRoot;
            List<EpmTargetPathSegment> subSegments = item.SubSegments;
            string[] strArray = targetPath.Split(new char[] { '/' });
            for (int i = 0; i < strArray.Length; i++)
            {
                string targetSegment = strArray[i];
                EpmTargetPathSegment segment2 = subSegments.FirstOrDefault<EpmTargetPathSegment>(delegate (EpmTargetPathSegment segment) {
                    if (!(segment.SegmentName == targetSegment))
                    {
                        return false;
                    }
                    if (!epmInfo.IsSyndicationMapping)
                    {
                        return segment.SegmentNamespaceUri == namespaceUri;
                    }
                    return true;
                });
                if (segment2 != null)
                {
                    item = segment2;
                }
                else
                {
                    return;
                }
                subSegments = item.SubSegments;
            }
            if (item.EpmInfo != null)
            {
                if (!item.EpmInfo.Attribute.KeepInContent)
                {
                    this.countOfNonContentV2Mappings--;
                }
                EpmTargetPathSegment parentSegment = null;
                do
                {
                    parentSegment = item.ParentSegment;
                    parentSegment.SubSegments.Remove(item);
                    item = parentSegment;
                }
                while (((item.ParentSegment != null) && !item.HasContent) && (item.SubSegments.Count == 0));
            }
        }

        [Conditional("DEBUG")]
        internal void Validate()
        {
        }

        internal ODataVersion MinimumODataProtocolVersion
        {
            get
            {
                if (this.countOfNonContentV2Mappings > 0)
                {
                    return ODataVersion.V2;
                }
                return ODataVersion.V1;
            }
        }

        internal EpmTargetPathSegment NonSyndicationRoot
        {
            get
            {
                return this.nonSyndicationRoot;
            }
        }

        internal EpmTargetPathSegment SyndicationRoot
        {
            get
            {
                return this.syndicationRoot;
            }
        }
    }
}

