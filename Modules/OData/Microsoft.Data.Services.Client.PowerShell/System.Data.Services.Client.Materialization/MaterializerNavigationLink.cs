namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;

    internal class MaterializerNavigationLink
    {
        private readonly object feedOrEntry;
        private readonly ODataNavigationLink link;

        private MaterializerNavigationLink(ODataNavigationLink link, object materializedFeedOrEntry)
        {
            this.link = link;
            this.feedOrEntry = materializedFeedOrEntry;
        }

        public static MaterializerNavigationLink CreateLink(ODataNavigationLink link, ODataFeed feed)
        {
            MaterializerNavigationLink annotation = new MaterializerNavigationLink(link, feed);
            link.SetAnnotation<MaterializerNavigationLink>(annotation);
            return annotation;
        }

        public static MaterializerNavigationLink CreateLink(ODataNavigationLink link, MaterializerEntry entry)
        {
            MaterializerNavigationLink annotation = new MaterializerNavigationLink(link, entry);
            link.SetAnnotation<MaterializerNavigationLink>(annotation);
            return annotation;
        }

        public static MaterializerNavigationLink GetLink(ODataNavigationLink link)
        {
            return link.GetAnnotation<MaterializerNavigationLink>();
        }

        public MaterializerEntry Entry
        {
            get
            {
                return (this.feedOrEntry as MaterializerEntry);
            }
        }

        public ODataFeed Feed
        {
            get
            {
                return (this.feedOrEntry as ODataFeed);
            }
        }

        public ODataNavigationLink Link
        {
            get
            {
                return this.link;
            }
        }
    }
}

