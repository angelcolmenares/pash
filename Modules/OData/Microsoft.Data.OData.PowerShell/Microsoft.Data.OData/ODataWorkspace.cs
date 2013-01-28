namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ODataWorkspace : ODataAnnotatable
    {
        public IEnumerable<ODataResourceCollectionInfo> Collections { get; set; }
    }
}

