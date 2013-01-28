namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using System;
    using System.Data.Services.Common;
    using System.Diagnostics;

    [DebuggerDisplay("EntityPropertyMappingInfo {DefiningType}")]
    internal sealed class EntityPropertyMappingInfo
    {
        private readonly IEdmEntityType actualPropertyType;
        private readonly EntityPropertyMappingAttribute attribute;
        private readonly IEdmEntityType definingType;
        private bool isSyndicationMapping;
        private EpmSourcePathSegment[] propertyValuePath;

        internal EntityPropertyMappingInfo(EntityPropertyMappingAttribute attribute, IEdmEntityType definingType, IEdmEntityType actualTypeDeclaringProperty)
        {
            this.attribute = attribute;
            this.definingType = definingType;
            this.actualPropertyType = actualTypeDeclaringProperty;
            this.isSyndicationMapping = this.attribute.TargetSyndicationItem != SyndicationItemProperty.CustomProperty;
        }

        internal bool DefiningTypesAreEqual(EntityPropertyMappingInfo other)
        {
            return ((IEdmType) this.DefiningType).IsEquivalentTo(((IEdmType) other.DefiningType));
        }

        internal void SetPropertyValuePath(EpmSourcePathSegment[] path)
        {
            this.propertyValuePath = path;
        }

        internal IEdmEntityType ActualPropertyType
        {
            get
            {
                return this.actualPropertyType;
            }
        }

        internal EntityPropertyMappingAttribute Attribute
        {
            get
            {
                return this.attribute;
            }
        }

        internal IEdmEntityType DefiningType
        {
            get
            {
                return this.definingType;
            }
        }

        internal bool IsSyndicationMapping
        {
            get
            {
                return this.isSyndicationMapping;
            }
        }

        internal EpmSourcePathSegment[] PropertyValuePath
        {
            get
            {
                return this.propertyValuePath;
            }
        }
    }
}

