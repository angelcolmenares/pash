namespace System.Data.Services.Client.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;

    internal sealed class MetadataProviderEdmComplexType : EdmComplexType, IEdmComplexType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly object lockObject;
        private Action<MetadataProviderEdmComplexType> propertyLoadAction;

        internal MetadataProviderEdmComplexType(string namespaceName, string name, IEdmComplexType baseType, bool isAbstract, Action<MetadataProviderEdmComplexType> propertyLoadAction) : base(namespaceName, name, baseType, isAbstract)
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

