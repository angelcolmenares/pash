namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}")]
    internal sealed class ODataAssociationLink : ODataAnnotatable
    {
        public string Name { get; set; }

        public Uri Url { get; set; }
    }
}

