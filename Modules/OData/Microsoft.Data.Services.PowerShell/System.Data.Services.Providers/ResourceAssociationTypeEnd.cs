namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Data.Services;

    internal class ResourceAssociationTypeEnd
    {
        private Dictionary<string, object> customAnnotations;
        private readonly EdmOnDeleteAction deleteAction;
        private readonly string multiplicity;
        private readonly string name;
        private readonly System.Data.Services.Providers.ResourceProperty resourceProperty;
        private readonly System.Data.Services.Providers.ResourceType resourceType;

        internal ResourceAssociationTypeEnd(string name, System.Data.Services.Providers.ResourceType resourceType, System.Data.Services.Providers.ResourceProperty resourceProperty, System.Data.Services.Providers.ResourceProperty fromProperty)
        {
            this.name = name;
            this.resourceType = resourceType;
            this.resourceProperty = resourceProperty;
            if ((fromProperty != null) && (fromProperty.Kind == ResourcePropertyKind.ResourceReference))
            {
                this.multiplicity = "0..1";
            }
            else
            {
                this.multiplicity = "*";
            }
        }

        internal ResourceAssociationTypeEnd(string name, System.Data.Services.Providers.ResourceType resourceType, System.Data.Services.Providers.ResourceProperty resourceProperty, string multiplicity, EdmOnDeleteAction deleteBehavior)
        {
            this.name = name;
            this.resourceType = resourceType;
            this.resourceProperty = resourceProperty;
            this.multiplicity = multiplicity;
            this.deleteAction = deleteBehavior;
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        internal IEnumerable<KeyValuePair<string, object>> CustomAnnotations
        {
            get
            {
                if (this.customAnnotations == null)
                {
                    return WebUtil.EmptyKeyValuePairStringObject;
                }
                return this.customAnnotations;
            }
        }

        internal EdmOnDeleteAction DeleteBehavior
        {
            get
            {
                return this.deleteAction;
            }
        }

        internal string Multiplicity
        {
            get
            {
                return this.multiplicity;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal System.Data.Services.Providers.ResourceProperty ResourceProperty
        {
            get
            {
                return this.resourceProperty;
            }
        }

        internal System.Data.Services.Providers.ResourceType ResourceType
        {
            get
            {
                return this.resourceType;
            }
        }
    }
}

