namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataStreamReferenceValue : ODataAnnotatable
    {
        public string ContentType { get; set; }

        public Uri EditLink { get; set; }

        public string ETag { get; set; }

        public Uri ReadLink { get; set; }
    }
}

