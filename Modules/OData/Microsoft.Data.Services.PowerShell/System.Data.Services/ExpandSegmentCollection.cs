namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ExpandSegmentCollection : List<ExpandSegment>
    {
        public ExpandSegmentCollection()
        {
        }

        public ExpandSegmentCollection(int capacity) : base(capacity)
        {
        }

        public bool HasFilter
        {
            get
            {
                return this.Any<ExpandSegment>(segment => segment.HasFilter);
            }
        }
    }
}

