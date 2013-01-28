namespace System.Data.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [Obsolete("The IExpandProvider interface is deprecated.")]
    internal interface IExpandProvider
    {
        IEnumerable ApplyExpansions(IQueryable queryable, ICollection<ExpandSegmentCollection> expandPaths);
    }
}

