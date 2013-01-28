namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class ODataOperation : ODataAnnotatable
    {
        protected ODataOperation()
        {
        }

        public Uri Metadata { get; set; }

        public Uri Target { get; set; }

        public string Title { get; set; }
    }
}

