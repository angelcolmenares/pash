namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    internal sealed class ODataCollectionValue : ODataAnnotatable
    {
        public IEnumerable Items { get; set; }

        public string TypeName { get; set; }
    }
}

