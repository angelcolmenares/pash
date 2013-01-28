namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("EpmTargetPathSegment {SegmentName} HasContent={HasContent}")]
    internal class EpmTargetPathSegment
    {
        private readonly EpmTargetPathSegment parentSegment;
        private readonly string segmentName;
        private readonly string segmentNamespaceUri;
        private readonly List<EpmTargetPathSegment> subSegments;

        internal EpmTargetPathSegment()
        {
            this.subSegments = new List<EpmTargetPathSegment>();
        }

        internal EpmTargetPathSegment(string segmentName, string segmentNamespaceUri, EpmTargetPathSegment parentSegment) : this()
        {
            this.segmentName = segmentName;
            this.segmentNamespaceUri = segmentNamespaceUri;
            this.parentSegment = parentSegment;
        }

        internal string AttributeName
        {
            get
            {
                return this.SegmentName.Substring(1);
            }
        }

        internal EntityPropertyMappingInfo EpmInfo { get; set; }

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
                return (!string.IsNullOrEmpty(this.SegmentName) && (this.SegmentName[0] == '@'));
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

