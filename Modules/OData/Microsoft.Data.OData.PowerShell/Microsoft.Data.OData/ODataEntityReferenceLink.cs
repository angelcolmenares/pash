namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Url.OriginalString}")]
    internal sealed class ODataEntityReferenceLink : ODataItem
    {
        public Uri Url { get; set; }
    }
}

