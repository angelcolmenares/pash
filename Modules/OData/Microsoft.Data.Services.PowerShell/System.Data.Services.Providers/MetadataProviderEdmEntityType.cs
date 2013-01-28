namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;

    internal sealed class MetadataProviderEdmEntityType : EdmEntityType, IEdmEntityType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly object lockObject;
        private Action<MetadataProviderEdmEntityType> propertyLoadAction;

        internal MetadataProviderEdmEntityType(string namespaceName, string name, IEdmEntityType baseType, bool isAbstract, bool isOpen, Action<MetadataProviderEdmEntityType> propertyLoadAction) : base(namespaceName, name, baseType, isAbstract, isOpen)
        {
            this.lockObject = new object();
            this.propertyLoadAction = propertyLoadAction;
        }

        private void EnsurePropertyLoaded()
        {
            lock (this.lockObject)
            {
                if (this.propertyLoadAction != null)
                {
                    this.propertyLoadAction(this);
                    this.propertyLoadAction = null;
                }
            }
        }

        public override IEnumerable<IEdmStructuralProperty> DeclaredKey
        {
            get
            {
                this.EnsurePropertyLoaded();
                return base.DeclaredKey;
            }
        }

        public override IEnumerable<IEdmProperty> DeclaredProperties
        {
            get
            {
                this.EnsurePropertyLoaded();
                return base.DeclaredProperties;
            }
        }
    }
}

