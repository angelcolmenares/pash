namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{Name}: {ResourceType}")]
    internal class ResourceSet
    {
        private Dictionary<string, object> customAnnotations;
        private readonly System.Data.Services.Providers.ResourceType elementType;
        private string entityContainerName;
        private bool isReadOnly;
        private readonly string name;
        private readonly Type queryRootType;
        private Func<object, IQueryable> readFromContextDelegate;
        private bool useMetadataKeyOrder;

        public ResourceSet(string name, System.Data.Services.Providers.ResourceType elementType)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckArgumentNull<System.Data.Services.Providers.ResourceType>(elementType, "elementType");
            if (elementType.ResourceTypeKind != ResourceTypeKind.EntityType)
            {
                throw new ArgumentException(System.Data.Services.Strings.ResourceContainer_ContainerMustBeAssociatedWithEntityType);
            }
            this.name = name;
            this.elementType = elementType;
            this.queryRootType = typeof(IQueryable<>).MakeGenericType(new Type[] { elementType.InstanceType });
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        public void SetReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.elementType.SetReadOnly();
                this.isReadOnly = true;
            }
        }

        private void ThrowIfSealed()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ResourceSet_Sealed(this.Name));
            }
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

        internal string EntityContainerName
        {
            get
            {
                return this.entityContainerName;
            }
            set
            {
                this.entityContainerName = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal Type QueryRootType
        {
            get
            {
                return this.queryRootType;
            }
        }

        internal Func<object, IQueryable> ReadFromContextDelegate
        {
            get
            {
                return this.readFromContextDelegate;
            }
            set
            {
                this.readFromContextDelegate = value;
            }
        }

        public System.Data.Services.Providers.ResourceType ResourceType
        {
            get
            {
                return this.elementType;
            }
        }

        public bool UseMetadataKeyOrder
        {
            get
            {
                return this.useMetadataKeyOrder;
            }
            set
            {
                this.ThrowIfSealed();
                this.useMetadataKeyOrder = value;
            }
        }
    }
}

