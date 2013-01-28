namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class EntryPropertiesValueCache : EpmValueCache
    {
        private readonly List<ODataProperty> entryPropertiesCache;

        internal EntryPropertiesValueCache(ODataEntry entry)
        {
            if (entry.Properties != null)
            {
                this.entryPropertiesCache = new List<ODataProperty>(entry.Properties);
            }
        }

        internal IEnumerable<ODataProperty> EntryProperties
        {
            get
            {
                if (this.entryPropertiesCache == null)
                {
                    return null;
                }
                return this.entryPropertiesCache.Where<ODataProperty>(delegate (ODataProperty p) {
                    if (p != null)
                    {
                        return !(p.Value is ODataStreamReferenceValue);
                    }
                    return true;
                });
            }
        }

        internal IEnumerable<ODataProperty> EntryStreamProperties
        {
            get
            {
                if (this.entryPropertiesCache == null)
                {
                    return null;
                }
                return (from p in this.entryPropertiesCache
                    where (p != null) && (p.Value is ODataStreamReferenceValue)
                    select p);
            }
        }
    }
}

