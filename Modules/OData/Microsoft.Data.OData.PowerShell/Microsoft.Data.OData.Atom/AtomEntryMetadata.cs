namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class AtomEntryMetadata : ODataAnnotatable
    {
        public IEnumerable<AtomPersonMetadata> Authors { get; set; }

        public IEnumerable<AtomCategoryMetadata> Categories { get; set; }

        public AtomCategoryMetadata CategoryWithTypeName { get; set; }

        public IEnumerable<AtomPersonMetadata> Contributors { get; set; }

        public AtomLinkMetadata EditLink { get; set; }

        public IEnumerable<AtomLinkMetadata> Links { get; set; }

        public DateTimeOffset? Published { get; set; }

        internal string PublishedString { get; set; }

        public AtomTextConstruct Rights { get; set; }

        public AtomLinkMetadata SelfLink { get; set; }

        public AtomFeedMetadata Source { get; set; }

        public AtomTextConstruct Summary { get; set; }

        public AtomTextConstruct Title { get; set; }

        public DateTimeOffset? Updated { get; set; }

        internal string UpdatedString { get; set; }
    }
}

