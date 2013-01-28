namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;

    internal sealed class ODataAtomReaderNavigationLinkDescriptor
    {
        private ODataNavigationLink navigationLink;
        private IEdmNavigationProperty navigationProperty;

        internal ODataAtomReaderNavigationLinkDescriptor(ODataNavigationLink navigationLink, IEdmNavigationProperty navigationProperty)
        {
            this.navigationLink = navigationLink;
            this.navigationProperty = navigationProperty;
        }

        internal ODataNavigationLink NavigationLink
        {
            get
            {
                return this.navigationLink;
            }
        }

        internal IEdmNavigationProperty NavigationProperty
        {
            get
            {
                return this.navigationProperty;
            }
        }
    }
}

