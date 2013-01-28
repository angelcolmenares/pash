namespace System.Data.Services.Client
{
    using System;

    internal sealed class LinkInfo
    {
        private Uri associationLink;
        private string name;
        private Uri navigationLink;

        internal LinkInfo(string propertyName)
        {
            this.name = propertyName;
        }

        public Uri AssociationLink
        {
            get
            {
                return this.associationLink;
            }
            internal set
            {
                this.associationLink = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Uri NavigationLink
        {
            get
            {
                return this.navigationLink;
            }
            internal set
            {
                this.navigationLink = value;
            }
        }
    }
}

