namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomLinkMetadata : ODataAnnotatable
    {
        private string hrefFromEpm;

        public AtomLinkMetadata()
        {
        }

        internal AtomLinkMetadata(AtomLinkMetadata other)
        {
            if (other != null)
            {
                this.Relation = other.Relation;
                this.Href = other.Href;
                this.HrefLang = other.HrefLang;
                this.Title = other.Title;
                this.MediaType = other.MediaType;
                this.Length = other.Length;
                this.hrefFromEpm = other.hrefFromEpm;
            }
        }

        public Uri Href { get; set; }

        public string HrefLang { get; set; }

        public int? Length { get; set; }

        public string MediaType { get; set; }

        public string Relation { get; set; }

        public string Title { get; set; }
    }
}

