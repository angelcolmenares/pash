namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;

    internal sealed class MetadataProviderEdmNavigationProperty : EdmElement, IEdmNavigationProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly IEdmEntityType declaringType;
        private EdmOnDeleteAction deleteAction;
        private ReadOnlyCollection<IEdmStructuralProperty> dependentProperties;
        private bool isPrinciple;
        private readonly string name;
        private IEdmNavigationProperty partner;
        private IEdmTypeReference type;

        internal MetadataProviderEdmNavigationProperty(EdmEntityType declaringType, string name, IEdmTypeReference type)
        {
            this.declaringType = declaringType;
            this.name = name;
            this.type = type;
        }

        internal void FixUpNavigationProperty(IEdmNavigationProperty partnerProperty, bool isPrincipleEnd, EdmOnDeleteAction propertyDeleteAction)
        {
            this.partner = partnerProperty;
            this.isPrinciple = isPrincipleEnd;
            this.deleteAction = propertyDeleteAction;
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
                return this.declaringType;
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
                return this.isPrinciple;
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
                if (this.partner == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.MetadataSerializer_NoResourceAssociationSetForNavigationProperty(this.name, this.declaringType.FullName()));
                }
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
            internal set
            {
                this.type = value;
            }
        }
    }
}

