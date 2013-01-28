namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    internal class ResourceReferentialConstraint
    {
        private Dictionary<string, object> customAnnotations;

        internal ResourceReferentialConstraint(ResourceAssociationTypeEnd principalEnd, IEnumerable<ResourceProperty> dependentProperties)
        {
            this.PrincipalEnd = principalEnd;
            this.DependentProperties = dependentProperties;
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

        internal IEnumerable<ResourceProperty> DependentProperties { get; private set; }

        internal ResourceAssociationTypeEnd PrincipalEnd { get; private set; }
    }
}

