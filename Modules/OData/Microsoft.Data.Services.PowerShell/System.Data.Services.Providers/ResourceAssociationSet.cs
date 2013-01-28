namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("ResourceAssociationSet: ({End1.ResourceSet.Name}, {End1.ResourceType.Name}, {End1.ResourceProperty.Name}) <-> ({End2.ResourceSet.Name}, {End2.ResourceType.Name}, {End2.ResourceProperty.Name})")]
    internal sealed class ResourceAssociationSet
    {
        private Dictionary<string, object> customAnnotations;
        private readonly ResourceAssociationSetEnd end1;
        private readonly ResourceAssociationSetEnd end2;
        private readonly string name;

        public ResourceAssociationSet(string name, ResourceAssociationSetEnd end1, ResourceAssociationSetEnd end2)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckArgumentNull<ResourceAssociationSetEnd>(end1, "end1");
            WebUtil.CheckArgumentNull<ResourceAssociationSetEnd>(end2, "end2");
            if ((end1.ResourceProperty == null) && (end2.ResourceProperty == null))
            {
                throw new ArgumentException(Strings.ResourceAssociationSet_ResourcePropertyCannotBeBothNull);
            }
            if ((end1.ResourceType == end2.ResourceType) && (end1.ResourceProperty == end2.ResourceProperty))
            {
                throw new ArgumentException(Strings.ResourceAssociationSet_SelfReferencingAssociationCannotBeBiDirectional);
            }
            this.name = name;
            this.end1 = end1;
            this.end2 = end2;
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        internal ResourceAssociationSetEnd GetRelatedResourceAssociationSetEnd(ResourceSetWrapper resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            ResourceAssociationSetEnd end = this.GetResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty);
            if (end == null)
            {
                return null;
            }
            if (end != this.End1)
            {
                return this.End1;
            }
            return this.End2;
        }

        internal ResourceAssociationSetEnd GetResourceAssociationSetEnd(ResourceSetWrapper resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
        {
            foreach (ResourceAssociationSetEnd end in new ResourceAssociationSetEnd[] { this.end1, this.end2 })
            {
                if ((end.ResourceSet.Name == resourceSet.Name) && end.ResourceType.IsAssignableFrom(resourceType))
                {
                    if ((end.ResourceProperty == null) && (resourceProperty == null))
                    {
                        return end;
                    }
                    if (((end.ResourceProperty != null) && (resourceProperty != null)) && (end.ResourceProperty.Name == resourceProperty.Name))
                    {
                        return end;
                    }
                }
            }
            return null;
        }

        internal void SetAssociationType(System.Data.Services.Providers.ResourceAssociationType association)
        {
            this.ResourceAssociationType = association;
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

        public ResourceAssociationSetEnd End1
        {
            [DebuggerStepThrough]
            get
            {
                return this.end1;
            }
        }

        public ResourceAssociationSetEnd End2
        {
            [DebuggerStepThrough]
            get
            {
                return this.end2;
            }
        }

        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return this.name;
            }
        }

        internal System.Data.Services.Providers.ResourceAssociationType ResourceAssociationType { get; set; }
    }
}

