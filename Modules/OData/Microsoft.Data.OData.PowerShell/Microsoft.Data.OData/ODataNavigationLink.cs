namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}")]
    internal sealed class ODataNavigationLink : ODataItem
    {
        public bool? IsCollection { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }
    }
}

