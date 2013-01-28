namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataFeed : ODataItem
    {
        public long? Count { get; set; }

        public string Id { get; set; }

        public Uri NextPageLink { get; set; }
    }
}

