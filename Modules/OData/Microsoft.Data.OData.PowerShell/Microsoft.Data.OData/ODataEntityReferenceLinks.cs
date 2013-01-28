namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ODataEntityReferenceLinks : ODataAnnotatable
    {
        public long? Count { get; set; }

        public IEnumerable<ODataEntityReferenceLink> Links { get; set; }

        public Uri NextPageLink { get; set; }
    }
}

