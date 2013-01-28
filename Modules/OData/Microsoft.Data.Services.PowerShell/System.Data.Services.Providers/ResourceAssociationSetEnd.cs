namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("ResourceAssociationSetEnd: {Name}: ({ResourceSet.Name}, {ResourceType.Name}, {ResourceProperty.Name})")]
    internal sealed class ResourceAssociationSetEnd
    {
        private Dictionary<string, object> customAnnotations;
        private readonly System.Data.Services.Providers.ResourceProperty resourceProperty;
        private readonly System.Data.Services.Providers.ResourceSet resourceSet;
        private readonly System.Data.Services.Providers.ResourceType resourceType;

        public ResourceAssociationSetEnd(System.Data.Services.Providers.ResourceSet resourceSet, System.Data.Services.Providers.ResourceType resourceType, System.Data.Services.Providers.ResourceProperty resourceProperty)
        {
            WebUtil.CheckArgumentNull<System.Data.Services.Providers.ResourceSet>(resourceSet, "resourceSet");
            WebUtil.CheckArgumentNull<System.Data.Services.Providers.ResourceType>(resourceType, "resourceType");
            if ((resourceProperty != null) && ((resourceType.TryResolvePropertyName(resourceProperty.Name) == null) || (resourceProperty.TypeKind != ResourceTypeKind.EntityType)))
            {
                throw new ArgumentException(Strings.ResourceAssociationSetEnd_ResourcePropertyMustBeNavigationPropertyOnResourceType);
            }
            if (!resourceSet.ResourceType.IsAssignableFrom(resourceType) && !resourceType.IsAssignableFrom(resourceSet.ResourceType))
            {
                throw new ArgumentException(Strings.ResourceAssociationSetEnd_ResourceTypeMustBeAssignableToResourceSet);
            }
            if ((resourceProperty != null) && (DataServiceProviderWrapper.GetDeclaringTypeForProperty(resourceType, resourceProperty, null) != resourceType))
            {
                throw new ArgumentException(Strings.ResourceAssociationSetEnd_ResourceTypeMustBeTheDeclaringType(resourceType.FullName, resourceProperty.Name));
            }
            this.resourceSet = resourceSet;
            this.resourceType = resourceType;
            this.resourceProperty = resourceProperty;
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

        public object CustomState { get; set; }

        internal string Name { get; set; }

        public System.Data.Services.Providers.ResourceProperty ResourceProperty
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceProperty;
            }
        }

        public System.Data.Services.Providers.ResourceSet ResourceSet
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceSet;
            }
        }

        public System.Data.Services.Providers.ResourceType ResourceType
        {
            [DebuggerStepThrough]
            get
            {
                return this.resourceType;
            }
        }
    }
}

