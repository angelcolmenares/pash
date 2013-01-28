namespace System.Data.Services.Client.Serializers
{
    using System;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;

    [DebuggerDisplay("EntityPropertyMappingInfo {DefiningType}")]
    internal sealed class EntityPropertyMappingInfo
    {
        private readonly ClientTypeAnnotation actualPropertyType;
        private readonly EntityPropertyMappingAttribute attribute;
        private readonly Type definingType;
        private bool isSyndicationMapping;
        private string[] propertyValuePath;

        public EntityPropertyMappingInfo(EntityPropertyMappingAttribute attribute, Type definingType, ClientTypeAnnotation actualPropertyType)
        {
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

        public ClientTypeAnnotation ActualPropertyType
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

        public Type DefiningType
        {
            get
            {
                return this.definingType;
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

