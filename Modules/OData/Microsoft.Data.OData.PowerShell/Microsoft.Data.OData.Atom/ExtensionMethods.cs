namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal static class ExtensionMethods
    {
        public static AtomLinkMetadata Atom(this ODataAssociationLink associationLink)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataAssociationLink>(associationLink, "associationLink");
            AtomLinkMetadata annotation = associationLink.GetAnnotation<AtomLinkMetadata>();
            if (annotation == null)
            {
                annotation = new AtomLinkMetadata();
                associationLink.SetAnnotation<AtomLinkMetadata>(annotation);
            }
            return annotation;
        }

        public static AtomEntryMetadata Atom(this ODataEntry entry)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataEntry>(entry, "entry");
            AtomEntryMetadata annotation = entry.GetAnnotation<AtomEntryMetadata>();
            if (annotation == null)
            {
                annotation = new AtomEntryMetadata();
                entry.SetAnnotation<AtomEntryMetadata>(annotation);
            }
            return annotation;
        }

        public static AtomFeedMetadata Atom(this ODataFeed feed)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataFeed>(feed, "feed");
            AtomFeedMetadata annotation = feed.GetAnnotation<AtomFeedMetadata>();
            if (annotation == null)
            {
                annotation = new AtomFeedMetadata();
                feed.SetAnnotation<AtomFeedMetadata>(annotation);
            }
            return annotation;
        }

        public static AtomLinkMetadata Atom(this ODataNavigationLink navigationLink)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataNavigationLink>(navigationLink, "navigationLink");
            AtomLinkMetadata annotation = navigationLink.GetAnnotation<AtomLinkMetadata>();
            if (annotation == null)
            {
                annotation = new AtomLinkMetadata();
                navigationLink.SetAnnotation<AtomLinkMetadata>(annotation);
            }
            return annotation;
        }

        public static AtomResourceCollectionMetadata Atom(this ODataResourceCollectionInfo collection)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataResourceCollectionInfo>(collection, "collection");
            AtomResourceCollectionMetadata annotation = collection.GetAnnotation<AtomResourceCollectionMetadata>();
            if (annotation == null)
            {
                annotation = new AtomResourceCollectionMetadata();
                collection.SetAnnotation<AtomResourceCollectionMetadata>(annotation);
            }
            return annotation;
        }

        public static AtomWorkspaceMetadata Atom(this ODataWorkspace workspace)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataWorkspace>(workspace, "workspace");
            AtomWorkspaceMetadata annotation = workspace.GetAnnotation<AtomWorkspaceMetadata>();
            if (annotation == null)
            {
                annotation = new AtomWorkspaceMetadata();
                workspace.SetAnnotation<AtomWorkspaceMetadata>(annotation);
            }
            return annotation;
        }
    }
}

