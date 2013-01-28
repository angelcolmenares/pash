namespace System.Data.Services.Serializers
{
    using System;
    using System.Data.Services.Common;
    using System.Data.Services.Providers;
    using System.Diagnostics;

    [DebuggerDisplay("EntityPropertyMappingInfo {DefiningType}")]
    internal sealed class EntityPropertyMappingInfo
    {
        private readonly ResourceType actualPropertyType;
        private readonly EntityPropertyMappingAttribute attribute;
        private readonly ResourceType definingType;
        private readonly bool isEFProvider;
        private bool isSyndicationMapping;
        private string[] propertyValuePath;

        public EntityPropertyMappingInfo(EntityPropertyMappingAttribute attribute, ResourceType definingType, ResourceType actualPropertyType, bool isEFProvider)
        {
            this.isEFProvider = isEFProvider;
            this.attribute = attribute;
            this.definingType = definingType;
            this.actualPropertyType = actualPropertyType;
            this.propertyValuePath = attribute.SourcePath.Split(new char[] { '/' });
            this.isSyndicationMapping = this.attribute.TargetSyndicationItem != SyndicationItemProperty.CustomProperty;
        }

        internal bool DefiningTypesAreEqual(EntityPropertyMappingInfo other)
        {
            return (this.DefiningType == other.DefiningType);
        }

        public ResourceType ActualPropertyType
        {
            get
            {
                return this.actualPropertyType;
            }
        }

        public EntityPropertyMappingAttribute Attribute
        {
            get
            {
                return this.attribute;
            }
        }

        public ResourceType DefiningType
        {
            get
            {
                return this.definingType;
            }
        }

        public bool IsEFProvider
        {
            get
            {
                return this.isEFProvider;
            }
        }

        public bool IsSyndicationMapping
        {
            get
            {
                return this.isSyndicationMapping;
            }
        }

        public string[] PropertyValuePath
        {
            get
            {
                return this.propertyValuePath;
            }
        }
    }
}

