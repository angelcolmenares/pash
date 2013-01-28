namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomPersonMetadata : ODataAnnotatable
    {
        private string email;
        private string name;
        private string uriFromEpm;

        public static implicit operator AtomPersonMetadata(string name)
        {
            return ToAtomPersonMetadata(name);
        }

        public static AtomPersonMetadata ToAtomPersonMetadata(string name)
        {
            return new AtomPersonMetadata { Name = name };
        }

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public System.Uri Uri { get; set; }

        internal string UriFromEpm
        {
            get
            {
                return this.uriFromEpm;
            }
            set
            {
                this.uriFromEpm = value;
            }
        }
    }
}

