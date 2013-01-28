namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomCategoryMetadata : ODataAnnotatable
    {
        public AtomCategoryMetadata()
        {
        }

        internal AtomCategoryMetadata(AtomCategoryMetadata other)
        {
            if (other != null)
            {
                this.Term = other.Term;
                this.Scheme = other.Scheme;
                this.Label = other.Label;
            }
        }

        public string Label { get; set; }

        public string Scheme { get; set; }

        public string Term { get; set; }
    }
}

