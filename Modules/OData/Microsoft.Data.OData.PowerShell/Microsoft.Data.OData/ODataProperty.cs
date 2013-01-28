namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataProperty : ODataAnnotatable
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}

