namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal class ObjectContextMember : IProviderMember
    {
        private readonly EdmMember edmMember;

        internal ObjectContextMember(EdmMember edmMember)
        {
            this.edmMember = edmMember;
        }

        public EntityType CollectionItemType
        {
            get
            {
                CollectionType edmType = this.EdmType as CollectionType;
                if (edmType != null)
                {
                    return (edmType.TypeUsage.EdmType as EntityType);
                }
                return null;
            }
        }

        private System.Data.Entity.Core.Metadata.Edm.EdmType EdmType
        {
            get
            {
                return this.edmMember.TypeUsage.EdmType;
            }
        }

        public BuiltInTypeKind EdmTypeKind
        {
            get
            {
                return this.EdmType.BuiltInTypeKind;
            }
        }

        public string EdmTypeName
        {
            get
            {
                return this.EdmType.Name;
            }
        }

        public IEnumerable<Facet> Facets
        {
            get
            {
                return this.edmMember.TypeUsage.Facets;
            }
        }

        public bool IsKey
        {
            get
            {
                StructuralType declaringType = this.edmMember.DeclaringType;
                return ((declaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType) && ((EntityType) declaringType).KeyMembers.Contains(this.edmMember));
            }
        }

        public IEnumerable<MetadataProperty> MetadataProperties
        {
            get
            {
                return this.edmMember.MetadataProperties;
            }
        }

        public string MimeType
        {
            get
            {
                MetadataProperty property;
                if (this.edmMember.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata:MimeType", false, out property))
                {
                    return (string) property.Value;
                }
                return null;
            }
        }

        public string Name
        {
            get
            {
                return this.edmMember.Name;
            }
        }
    }
}

