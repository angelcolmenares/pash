namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class EpmTargetTree
    {
        private int countOfNonContentV2mappings;

        internal EpmTargetTree()
        {
            this.SyndicationRoot = new EpmTargetPathSegment();
            this.NonSyndicationRoot = new EpmTargetPathSegment();
        }

        internal void Add(EntityPropertyMappingInfo epmInfo)
        {
            string targetPath = epmInfo.Attribute.TargetPath;
            string namespaceUri = epmInfo.Attribute.TargetNamespaceUri;
            EpmTargetPathSegment parentSegment = epmInfo.IsSyndicationMapping ? this.SyndicationRoot : this.NonSyndicationRoot;
            IList<EpmTargetPathSegment> subSegments = parentSegment.SubSegments;
            string[] strArray = targetPath.Split(new char[] { '/' });
            EpmTargetPathSegment segment2 = null;
            for (int i = 0; i < strArray.Length; i++)
            {
                string targetSegment = strArray[i];
                if (targetSegment.Length == 0)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmTargetTree_InvalidTargetPath(targetPath));
                }
                if ((targetSegment[0] == '@') && (i != (strArray.Length - 1)))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.EpmTargetTree_AttributeInMiddle(targetSegment));
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
                    parentSegment = new EpmTargetPathSegment(targetSegment, namespaceUri, parentSegment);
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
                throw new ArgumentException(System.Data.Services.Strings.EpmTargetTree_DuplicateEpmAttrsWithSameTargetName(GetPropertyNameFromEpmInfo(parentSegment.EpmInfo), parentSegment.EpmInfo.DefiningType.Name, parentSegment.EpmInfo.Attribute.SourcePath, epmInfo.Attribute.SourcePath));
            }
            if (!epmInfo.Attribute.KeepInContent)
            {
                this.countOfNonContentV2mappings++;
            }
            parentSegment.EpmInfo = epmInfo;
            if (HasMixedContent(this.NonSyndicationRoot, false))
            {
                throw new InvalidOperationException(System.Data.Services.Strings.EpmTargetTree_InvalidTargetPath(targetPath));
            }
        }

        private static string GetPropertyNameFromEpmInfo(EntityPropertyMappingInfo epmInfo)
        {
            if (epmInfo.Attribute.TargetSyndicationItem == SyndicationItemProperty.CustomProperty)
            {
                return epmInfo.Attribute.TargetPath;
            }
            if (!epmInfo.IsEFProvider)
            {
                return epmInfo.Attribute.TargetSyndicationItem.ToString();
            }
            return EpmTranslate.MapSyndicationPropertyToEpmTargetPath(epmInfo.Attribute.TargetSyndicationItem);
        }

        private static bool HasMixedContent(EpmTargetPathSegment currentSegment, bool ancestorHasContent)
        {
            foreach (EpmTargetPathSegment segment in from s in currentSegment.SubSegments
                where !s.IsAttribute
                select s)
            {
                if (segment.HasContent && ancestorHasContent)
                {
                    return true;
                }
                if (HasMixedContent(segment, segment.HasContent || ancestorHasContent))
                {
                    return true;
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
                    this.countOfNonContentV2mappings--;
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

        internal DataServiceProtocolVersion MinimumDataServiceProtocolVersion
        {
            get
            {
                if (this.countOfNonContentV2mappings > 0)
                {
                    return DataServiceProtocolVersion.V2;
                }
                return DataServiceProtocolVersion.V1;
            }
        }

        internal EpmTargetPathSegment NonSyndicationRoot { get; private set; }

        internal EpmTargetPathSegment SyndicationRoot { get; private set; }
    }
}

