namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    internal class ResourceAssociationType
    {
        private Dictionary<string, object> customAnnotations;
        private readonly ResourceAssociationTypeEnd end1;
        private readonly ResourceAssociationTypeEnd end2;
        private readonly string fullName;
        private readonly string name;
        private readonly string namespaceName;

        internal ResourceAssociationType(string name, string namespaceName, ResourceAssociationTypeEnd end1, ResourceAssociationTypeEnd end2)
        {
            this.name = name;
            this.namespaceName = namespaceName;
            this.fullName = namespaceName + "." + name;
            this.end1 = end1;
            this.end2 = end2;
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        internal ResourceAssociationTypeEnd GetEnd(string endName)
        {
            if (this.end1.Name == endName)
            {
                return this.end1;
            }
            if (this.end2.Name == endName)
            {
                return this.end2;
            }
            return null;
        }

        internal ResourceAssociationTypeEnd GetRelatedEnd(string endName)
        {
            if (this.end1.Name == endName)
            {
                return this.end2;
            }
            if (this.end2.Name == endName)
            {
                return this.end1;
            }
            return null;
        }

        internal ResourceAssociationTypeEnd GetRelatedResourceAssociationSetEnd(ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceAssociationTypeEnd resourceAssociationTypeEnd = this.GetResourceAssociationTypeEnd(resourceType, resourceProperty);
            if (resourceAssociationTypeEnd != null)
            {
                foreach (ResourceAssociationTypeEnd end2 in new ResourceAssociationTypeEnd[] { this.end1, this.end2 })
                {
                    if (end2 != resourceAssociationTypeEnd)
                    {
                        return end2;
                    }
                }
            }
            return null;
        }

        internal ResourceAssociationTypeEnd GetResourceAssociationTypeEnd(ResourceType resourceType, ResourceProperty resourceProperty)
        {
            foreach (ResourceAssociationTypeEnd end in new ResourceAssociationTypeEnd[] { this.end1, this.end2 })
            {
                if ((end.ResourceType == resourceType) && (end.ResourceProperty == resourceProperty))
                {
                    return end;
                }
            }
            return null;
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

        internal ResourceAssociationTypeEnd End1
        {
            get
            {
                return this.end1;
            }
        }

        internal ResourceAssociationTypeEnd End2
        {
            get
            {
                return this.end2;
            }
        }

        internal string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal string NamespaceName
        {
            get
            {
                return this.namespaceName;
            }
        }

        internal ResourceReferentialConstraint ReferentialConstraint { get; set; }
    }
}

