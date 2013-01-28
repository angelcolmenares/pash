namespace Microsoft.Data.OData.Atom
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomResourceCollectionMetadata
    {
        public string Accept { get; set; }

        public AtomCategoriesMetadata Categories { get; set; }

        public AtomTextConstruct Title { get; set; }
    }
}

