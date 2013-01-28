namespace Microsoft.Data.OData.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("EpmTargetPathSegment {SegmentName} HasContent={HasContent}")]
    internal sealed class EpmTargetPathSegment
    {
        private EntityPropertyMappingInfo epmInfo;
        private readonly EpmTargetPathSegment parentSegment;
        private readonly string segmentAttributeName;
        private readonly string segmentName;
        private readonly string segmentNamespacePrefix;
        private readonly string segmentNamespaceUri;
        private readonly List<EpmTargetPathSegment> subSegments;

        internal EpmTargetPathSegment()
        {
            this.subSegments = new List<EpmTargetPathSegment>();
        }

        internal EpmTargetPathSegment(string segmentName, string segmentNamespaceUri, string segmentNamespacePrefix, EpmTargetPathSegment parentSegment) : this()
        {
            this.segmentName = segmentName;
            this.segmentNamespaceUri = segmentNamespaceUri;
            this.segmentNamespacePrefix = segmentNamespacePrefix;
            this.parentSegment = parentSegment;
            if (!string.IsNullOrEmpty(segmentName) && (segmentName[0] == '@'))
            {
                this.segmentAttributeName = segmentName.Substring(1);
            }
        }

        internal string AttributeName
        {
            get
            {
                return this.segmentAttributeName;
            }
        }

        internal EntityPropertyMappingInfo EpmInfo
        {
            get
            {
                return this.epmInfo;
            }
            set
            {
                this.epmInfo = value;
            }
        }

        internal bool HasContent
        {
            get
            {
                return (this.EpmInfo != null);
            }
        }

        internal bool IsAttribute
        {
            get
            {
                return (this.segmentAttributeName != null);
            }
        }

        internal EpmTargetPathSegment ParentSegment
        {
            get
            {
                return this.parentSegment;
            }
        }

        internal string SegmentName
        {
            get
            {
                return this.segmentName;
            }
        }

        internal string SegmentNamespacePrefix
        {
            get
            {
                return this.segmentNamespacePrefix;
            }
        }

        internal string SegmentNamespaceUri
        {
            get
            {
                return this.segmentNamespaceUri;
            }
        }

        internal List<EpmTargetPathSegment> SubSegments
        {
            get
            {
                return this.subSegments;
            }
        }
    }
}

