namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;

    internal sealed class ODataEntityPropertyMappingCollection : IEnumerable<EntityPropertyMappingAttribute>, IEnumerable
    {
        private readonly List<EntityPropertyMappingAttribute> mappings;

        public ODataEntityPropertyMappingCollection()
        {
            this.mappings = new List<EntityPropertyMappingAttribute>();
        }

        public ODataEntityPropertyMappingCollection(IEnumerable<EntityPropertyMappingAttribute> other)
        {
            ExceptionUtils.CheckArgumentNotNull<IEnumerable<EntityPropertyMappingAttribute>>(other, "other");
            this.mappings = new List<EntityPropertyMappingAttribute>(other);
        }

        public void Add(EntityPropertyMappingAttribute mapping)
        {
            ExceptionUtils.CheckArgumentNotNull<EntityPropertyMappingAttribute>(mapping, "mapping");
            this.mappings.Add(mapping);
        }

        public IEnumerator<EntityPropertyMappingAttribute> GetEnumerator()
        {
            return this.mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.mappings.GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return this.mappings.Count;
            }
        }
    }
}

