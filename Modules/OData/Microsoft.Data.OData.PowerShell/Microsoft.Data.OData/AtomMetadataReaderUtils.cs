namespace Microsoft.Data.OData
{
    using Microsoft.Data.OData.Atom;
    using System;

    internal static class AtomMetadataReaderUtils
    {
        private static readonly ReadOnlyEnumerable<AtomCategoryMetadata> EmptyCategoriesList = new ReadOnlyEnumerable<AtomCategoryMetadata>();
        private static readonly ReadOnlyEnumerable<AtomLinkMetadata> EmptyLinksList = new ReadOnlyEnumerable<AtomLinkMetadata>();
        private static readonly ReadOnlyEnumerable<AtomPersonMetadata> EmptyPersonsList = new ReadOnlyEnumerable<AtomPersonMetadata>();

        internal static void AddAuthorToEntryMetadata(AtomEntryMetadata entryMetadata, AtomPersonMetadata authorMetadata)
        {
            if (object.ReferenceEquals(entryMetadata.Authors, EmptyPersonsList))
            {
                entryMetadata.Authors = new ReadOnlyEnumerable<AtomPersonMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomPersonMetadata>(entryMetadata.Authors, "Authors").Add(authorMetadata);
        }

        internal static void AddAuthorToFeedMetadata(AtomFeedMetadata feedMetadata, AtomPersonMetadata authorMetadata)
        {
            if (object.ReferenceEquals(feedMetadata.Authors, EmptyPersonsList))
            {
                feedMetadata.Authors = new ReadOnlyEnumerable<AtomPersonMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomPersonMetadata>(feedMetadata.Authors, "Authors").Add(authorMetadata);
        }

        internal static void AddCategoryToEntryMetadata(AtomEntryMetadata entryMetadata, AtomCategoryMetadata categoryMetadata)
        {
            if (object.ReferenceEquals(entryMetadata.Categories, EmptyCategoriesList))
            {
                entryMetadata.Categories = new ReadOnlyEnumerable<AtomCategoryMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomCategoryMetadata>(entryMetadata.Categories, "Categories").Add(categoryMetadata);
        }

        internal static void AddCategoryToFeedMetadata(AtomFeedMetadata feedMetadata, AtomCategoryMetadata categoryMetadata)
        {
            if (object.ReferenceEquals(feedMetadata.Categories, EmptyCategoriesList))
            {
                feedMetadata.Categories = new ReadOnlyEnumerable<AtomCategoryMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomCategoryMetadata>(feedMetadata.Categories, "Categories").Add(categoryMetadata);
        }

        internal static void AddContributorToEntryMetadata(AtomEntryMetadata entryMetadata, AtomPersonMetadata contributorMetadata)
        {
            if (object.ReferenceEquals(entryMetadata.Contributors, EmptyPersonsList))
            {
                entryMetadata.Contributors = new ReadOnlyEnumerable<AtomPersonMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomPersonMetadata>(entryMetadata.Contributors, "Contributors").Add(contributorMetadata);
        }

        internal static void AddContributorToFeedMetadata(AtomFeedMetadata feedMetadata, AtomPersonMetadata contributorMetadata)
        {
            if (object.ReferenceEquals(feedMetadata.Contributors, EmptyPersonsList))
            {
                feedMetadata.Contributors = new ReadOnlyEnumerable<AtomPersonMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomPersonMetadata>(feedMetadata.Contributors, "Contributors").Add(contributorMetadata);
        }

        internal static void AddLinkToEntryMetadata(AtomEntryMetadata entryMetadata, AtomLinkMetadata linkMetadata)
        {
            if (object.ReferenceEquals(entryMetadata.Links, EmptyLinksList))
            {
                entryMetadata.Links = new ReadOnlyEnumerable<AtomLinkMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomLinkMetadata>(entryMetadata.Links, "Links").Add(linkMetadata);
        }

        internal static void AddLinkToFeedMetadata(AtomFeedMetadata feedMetadata, AtomLinkMetadata linkMetadata)
        {
            if (object.ReferenceEquals(feedMetadata.Links, EmptyLinksList))
            {
                feedMetadata.Links = new ReadOnlyEnumerable<AtomLinkMetadata>();
            }
            ReaderUtils.GetSourceListOfEnumerable<AtomLinkMetadata>(feedMetadata.Links, "Links").Add(linkMetadata);
        }

        internal static AtomEntryMetadata CreateNewAtomEntryMetadata()
        {
            return new AtomEntryMetadata { Authors = EmptyPersonsList, Categories = EmptyCategoriesList, Contributors = EmptyPersonsList, Links = EmptyLinksList };
        }

        internal static AtomFeedMetadata CreateNewAtomFeedMetadata()
        {
            return new AtomFeedMetadata { Authors = EmptyPersonsList, Categories = EmptyCategoriesList, Contributors = EmptyPersonsList, Links = EmptyLinksList };
        }
    }
}

