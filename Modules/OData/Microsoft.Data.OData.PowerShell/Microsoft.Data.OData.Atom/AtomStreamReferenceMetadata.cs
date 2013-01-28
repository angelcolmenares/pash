namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomStreamReferenceMetadata : ODataAnnotatable
    {
        public AtomLinkMetadata EditLink { get; set; }

        public AtomLinkMetadata SelfLink { get; set; }
    }
}

