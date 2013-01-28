namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ODataComplexValue : ODataAnnotatable
    {
        public IEnumerable<ODataProperty> Properties { get; set; }

        public string TypeName { get; set; }
    }
}

