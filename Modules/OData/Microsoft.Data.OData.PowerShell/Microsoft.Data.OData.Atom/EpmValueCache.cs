namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal class EpmValueCache
    {
        private Dictionary<object, object> epmValuesCache;

        internal EpmValueCache()
        {
        }

        internal IEnumerable<ODataProperty> CacheComplexValueProperties(ODataComplexValue complexValue)
        {
            if (complexValue == null)
            {
                return null;
            }
            IEnumerable<ODataProperty> properties = complexValue.Properties;
            List<ODataProperty> list = null;
            if (properties != null)
            {
                list = new List<ODataProperty>(properties);
            }
            if (this.epmValuesCache == null)
            {
                this.epmValuesCache = new Dictionary<object, object>(ReferenceEqualityComparer<object>.Instance);
            }
            this.epmValuesCache.Add(complexValue, list);
            return list;
        }

        private IEnumerable<ODataProperty> GetComplexValueProperties(ODataComplexValue complexValue, bool writingContent)
        {
            object obj2;
            if ((this.epmValuesCache != null) && this.epmValuesCache.TryGetValue(complexValue, out obj2))
            {
                return (IEnumerable<ODataProperty>) obj2;
            }
            return complexValue.Properties;
        }

        internal static IEnumerable<ODataProperty> GetComplexValueProperties(EpmValueCache epmValueCache, ODataComplexValue complexValue, bool writingContent)
        {
            if (epmValueCache == null)
            {
                return complexValue.Properties;
            }
            return epmValueCache.GetComplexValueProperties(complexValue, writingContent);
        }
    }
}

