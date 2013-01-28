namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class MetadataProviderEdmSilentNavigationProperty : EdmElement, IEdmNavigationProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly EdmOnDeleteAction deleteAction;
        private ReadOnlyCollection<IEdmStructuralProperty> dependentProperties;
        private readonly string name;
        private readonly IEdmNavigationProperty partner;
        private readonly IEdmTypeReference type;

        public MetadataProviderEdmSilentNavigationProperty(IEdmNavigationProperty partnerProperty, EdmOnDeleteAction propertyDeleteAction, EdmMultiplicity multiplicity, string name)
        {
            this.partner = partnerProperty;
            this.deleteAction = propertyDeleteAction;
            this.name = name;
            switch (multiplicity)
            {
                case EdmMultiplicity.ZeroOrOne:
                    this.type = new EdmEntityTypeReference(this.partner.DeclaringEntityType(), true);
                    return;

                case EdmMultiplicity.One:
                    this.type = new EdmEntityTypeReference(this.partner.DeclaringEntityType(), false);
                    return;

                case EdmMultiplicity.Many:
                    this.type = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(this.partner.DeclaringEntityType(), false)), false);
                    return;
            }
        }

        internal void SetDependentProperties(IList<IEdmStructuralProperty> properties)
        {
            this.dependentProperties = new ReadOnlyCollection<IEdmStructuralProperty>(properties);
        }

        public bool ContainsTarget
        {
            get
            {
                return false;
            }
        }

        public IEdmStructuredType DeclaringType
        {
            get
            {
                return this.partner.ToEntityType();
            }
        }

        public IEnumerable<IEdmStructuralProperty> DependentProperties
        {
            get
            {
                return this.dependentProperties;
            }
        }

        public bool IsPrincipal
        {
            get
            {
                return (this.partner.DependentProperties != null);
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public EdmOnDeleteAction OnDelete
        {
            get
            {
                return this.deleteAction;
            }
        }

        public IEdmNavigationProperty Partner
        {
            get
            {
                return this.partner;
            }
        }

        public EdmPropertyKind PropertyKind
        {
            get
            {
                return EdmPropertyKind.Navigation;
            }
        }

        public IEdmTypeReference Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

