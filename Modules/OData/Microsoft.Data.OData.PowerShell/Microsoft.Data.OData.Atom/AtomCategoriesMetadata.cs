namespace Microsoft.Data.OData.Atom
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class AtomCategoriesMetadata
    {
        public IEnumerable<AtomCategoryMetadata> Categories { get; set; }

        public bool? Fixed { get; set; }

        public Uri Href { get; set; }

        public string Scheme { get; set; }
    }
}

