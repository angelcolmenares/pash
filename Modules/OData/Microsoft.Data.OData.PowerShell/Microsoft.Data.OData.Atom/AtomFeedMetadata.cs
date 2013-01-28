namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class AtomFeedMetadata : ODataAnnotatable
    {
        public IEnumerable<AtomPersonMetadata> Authors { get; set; }

        public IEnumerable<AtomCategoryMetadata> Categories { get; set; }

        public IEnumerable<AtomPersonMetadata> Contributors { get; set; }

        public AtomGeneratorMetadata Generator { get; set; }

        public Uri Icon { get; set; }

        public IEnumerable<AtomLinkMetadata> Links { get; set; }

        public Uri Logo { get; set; }

        public AtomLinkMetadata NextPageLink { get; set; }

        public AtomTextConstruct Rights { get; set; }

        public AtomLinkMetadata SelfLink { get; set; }

        public string SourceId { get; set; }

        public AtomTextConstruct Subtitle { get; set; }

        public AtomTextConstruct Title { get; set; }

        public DateTimeOffset? Updated { get; set; }
    }
}

