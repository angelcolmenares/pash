namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("{kind}: {name}")]
    internal class ResourceProperty
    {
        private bool canReflectOnInstanceTypeProperty;
        private Dictionary<string, object> customAnnotations;
        private bool isReadOnly;
        private ResourcePropertyKind kind;
        private string mimeType;
        private readonly string name;
        private readonly System.Data.Services.Providers.ResourceType propertyResourceType;

        public ResourceProperty(string name, ResourcePropertyKind kind, System.Data.Services.Providers.ResourceType propertyResourceType)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckArgumentNull<System.Data.Services.Providers.ResourceType>(propertyResourceType, "propertyResourceType");
            ValidatePropertyParameters(kind, propertyResourceType);
            this.kind = kind;
            this.name = name;
            this.propertyResourceType = propertyResourceType;
            this.canReflectOnInstanceTypeProperty = !kind.HasFlag(ResourcePropertyKind.Stream);
        }

        internal void AddCustomAnnotation(string annotationNamespace, string annotationName, object annotationValue)
        {
            WebUtil.ValidateAndAddAnnotation(ref this.customAnnotations, annotationNamespace, annotationName, annotationValue);
        }

        private static void CheckResourcePropertyKind(ResourcePropertyKind kind, string parameterName)
        {
            if ((((kind != ResourcePropertyKind.ResourceReference) && (kind != ResourcePropertyKind.ResourceSetReference)) && ((kind != ResourcePropertyKind.ComplexType) && (kind != ResourcePropertyKind.Primitive))) && (((kind != ResourcePropertyKind.Collection) && (kind != ResourcePropertyKind.Stream)) && ((kind != (ResourcePropertyKind.Key | ResourcePropertyKind.Primitive)) && (kind != (ResourcePropertyKind.ETag | ResourcePropertyKind.Primitive)))))
            {
                throw new ArgumentException(Strings.InvalidEnumValue(kind.GetType().Name), parameterName);
            }
        }

        internal bool IsOfKind(ResourcePropertyKind checkKind)
        {
            return IsOfKind(this.kind, checkKind);
        }

        private static bool IsOfKind(ResourcePropertyKind propertyKind, ResourcePropertyKind kind)
        {
            return ((propertyKind & kind) == kind);
        }

        public void SetReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.ResourceType.SetReadOnly();
                this.isReadOnly = true;
            }
        }

        private void ThrowIfSealed()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(Strings.ResourceProperty_Sealed(this.Name));
            }
        }

        private static void ValidatePropertyParameters(ResourcePropertyKind kind, System.Data.Services.Providers.ResourceType propertyResourceType)
        {
            CheckResourcePropertyKind(kind, "kind");
            if ((IsOfKind(kind, ResourcePropertyKind.ResourceReference) || IsOfKind(kind, ResourcePropertyKind.ResourceSetReference)) && (propertyResourceType.ResourceTypeKind != ResourceTypeKind.EntityType))
            {
                throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
            }
            if (IsOfKind(kind, ResourcePropertyKind.Primitive) && (propertyResourceType.ResourceTypeKind != ResourceTypeKind.Primitive))
            {
                throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
            }
            if (IsOfKind(kind, ResourcePropertyKind.ComplexType) && (propertyResourceType.ResourceTypeKind != ResourceTypeKind.ComplexType))
            {
                throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
            }
            if (IsOfKind(kind, ResourcePropertyKind.Collection) && (propertyResourceType.ResourceTypeKind != ResourceTypeKind.Collection))
            {
                throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
            }
            if (IsOfKind(kind, ResourcePropertyKind.Stream))
            {
                if (kind != ResourcePropertyKind.Stream)
                {
                    throw new ArgumentException(Strings.ResourceProperty_NamedStreamKindMustBeUsedAlone);
                }
                if (propertyResourceType != System.Data.Services.Providers.ResourceType.PrimitiveResourceTypeMap.GetPrimitive(typeof(Stream)))
                {
                    throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
                }
            }
            else if (propertyResourceType == System.Data.Services.Providers.ResourceType.PrimitiveResourceTypeMap.GetPrimitive(typeof(Stream)))
            {
                throw new ArgumentException(Strings.ResourceProperty_PropertyKindAndResourceTypeKindMismatch("kind", "propertyResourceType"));
            }
            if (IsOfKind(kind, ResourcePropertyKind.Key) && (Nullable.GetUnderlyingType(propertyResourceType.InstanceType) != null))
            {
                throw new ArgumentException(Strings.ResourceProperty_KeyPropertiesCannotBeNullable);
            }
        }

        public bool CanReflectOnInstanceTypeProperty
        {
            get
            {
                return this.canReflectOnInstanceTypeProperty;
            }
            set
            {
                this.ThrowIfSealed();
                if (this.IsOfKind(ResourcePropertyKind.Stream))
                {
                    throw new InvalidOperationException(Strings.ResourceProperty_NamedStreamCannotReflect);
                }
                this.canReflectOnInstanceTypeProperty = value;
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

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public ResourcePropertyKind Kind
        {
            [DebuggerStepThrough]
            get
            {
                return this.kind;
            }
            [DebuggerStepThrough]
            internal set
            {
                this.kind = value;
            }
        }

        public string MimeType
        {
            [DebuggerStepThrough]
            get
            {
                return this.mimeType;
            }
            set
            {
                this.ThrowIfSealed();
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(Strings.ResourceProperty_MimeTypeAttributeEmpty(this.Name));
                }
                if (!this.IsOfKind(ResourcePropertyKind.Primitive))
                {
                    throw new InvalidOperationException(Strings.ResourceProperty_MimeTypeAttributeOnNonPrimitive(this.Name, this.Kind));
                }
                if (!WebUtil.IsValidMimeType(value))
                {
                    throw new InvalidOperationException(Strings.ResourceProperty_MimeTypeNotValid(value, this.Name));
                }
                this.mimeType = value;
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

        public System.Data.Services.Providers.ResourceType ResourceType
        {
            [DebuggerStepThrough]
            get
            {
                return this.propertyResourceType;
            }
        }

        internal System.Type Type
        {
            get
            {
                if (this.Kind != ResourcePropertyKind.ResourceSetReference)
                {
                    return this.propertyResourceType.InstanceType;
                }
                return typeof(IEnumerable<>).MakeGenericType(new System.Type[] { this.propertyResourceType.InstanceType });
            }
        }

        internal ResourceTypeKind TypeKind
        {
            get
            {
                return this.ResourceType.ResourceTypeKind;
            }
        }
    }
}

