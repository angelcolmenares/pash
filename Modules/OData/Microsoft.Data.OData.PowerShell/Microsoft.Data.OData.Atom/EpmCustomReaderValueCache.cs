namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class EpmCustomReaderValueCache
    {
        private readonly List<KeyValuePair<EntityPropertyMappingInfo, string>> customEpmValues = new List<KeyValuePair<EntityPropertyMappingInfo, string>>();

        internal EpmCustomReaderValueCache()
        {
        }

        internal void Add(EntityPropertyMappingInfo epmInfo, string value)
        {
            this.customEpmValues.Add(new KeyValuePair<EntityPropertyMappingInfo, string>(epmInfo, value));
        }

        internal bool Contains(EntityPropertyMappingInfo epmInfo)
        {
            return this.customEpmValues.Any<KeyValuePair<EntityPropertyMappingInfo, string>>(epmValue => object.ReferenceEquals(epmValue.Key, epmInfo));
        }

        internal IEnumerable<KeyValuePair<EntityPropertyMappingInfo, string>> CustomEpmValues
        {
            get
            {
                return this.customEpmValues;
            }
        }
    }
}

