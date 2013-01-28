namespace Microsoft.Data.OData.Metadata
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;

    internal sealed class ODataEntityPropertyMappingCache
    {
        private readonly Microsoft.Data.OData.Metadata.EpmSourceTree epmSourceTree;
        private readonly Microsoft.Data.OData.Metadata.EpmTargetTree epmTargetTree;
        private readonly ODataEntityPropertyMappingCollection mappings;
        private readonly List<EntityPropertyMappingAttribute> mappingsForDeclaredProperties;
        private readonly List<EntityPropertyMappingAttribute> mappingsForInheritedProperties;
        private readonly IEdmModel model;
        private readonly int totalMappingCount;

        internal ODataEntityPropertyMappingCache(ODataEntityPropertyMappingCollection mappings, IEdmModel model, int totalMappingCount)
        {
            this.mappings = mappings;
            this.model = model;
            this.totalMappingCount = totalMappingCount;
            this.mappingsForInheritedProperties = new List<EntityPropertyMappingAttribute>();
            this.mappingsForDeclaredProperties = (mappings == null) ? new List<EntityPropertyMappingAttribute>() : new List<EntityPropertyMappingAttribute>(mappings);
            this.epmTargetTree = new Microsoft.Data.OData.Metadata.EpmTargetTree();
            this.epmSourceTree = new Microsoft.Data.OData.Metadata.EpmSourceTree(this.epmTargetTree);
        }

        internal void BuildEpmForType(IEdmEntityType definingEntityType, IEdmEntityType affectedEntityType)
        {
            if (definingEntityType.BaseType != null)
            {
                this.BuildEpmForType(definingEntityType.BaseEntityType(), affectedEntityType);
            }
            ODataEntityPropertyMappingCollection entityPropertyMappings = this.model.GetEntityPropertyMappings(definingEntityType);
            if (entityPropertyMappings != null)
            {
                foreach (EntityPropertyMappingAttribute attribute in entityPropertyMappings)
                {
                    this.epmSourceTree.Add(new EntityPropertyMappingInfo(attribute, definingEntityType, affectedEntityType));
                    if ((definingEntityType == affectedEntityType) && !PropertyExistsOnType(affectedEntityType, attribute))
                    {
                        this.MappingsForInheritedProperties.Add(attribute);
                        this.MappingsForDeclaredProperties.Remove(attribute);
                    }
                }
            }
        }

        internal bool IsDirty(ODataEntityPropertyMappingCollection propertyMappings)
        {
            if ((this.mappings == null) && (propertyMappings == null))
            {
                return false;
            }
            return (!object.ReferenceEquals(this.mappings, propertyMappings) || (this.mappings.Count != propertyMappings.Count));
        }

        private static bool PropertyExistsOnType(IEdmStructuredType structuredType, EntityPropertyMappingAttribute epmAttribute)
        {
            int index = epmAttribute.SourcePath.IndexOf('/');
            string propertyToLookFor = (index == -1) ? epmAttribute.SourcePath : epmAttribute.SourcePath.Substring(0, index);
            return structuredType.DeclaredProperties.Any<IEdmProperty>(p => (p.Name == propertyToLookFor));
        }

        internal IEnumerable<EntityPropertyMappingAttribute> AllMappings
        {
            get
            {
                return this.MappingsForDeclaredProperties.Concat<EntityPropertyMappingAttribute>(this.MappingsForInheritedProperties);
            }
        }

        internal Microsoft.Data.OData.Metadata.EpmSourceTree EpmSourceTree
        {
            get
            {
                return this.epmSourceTree;
            }
        }

        internal Microsoft.Data.OData.Metadata.EpmTargetTree EpmTargetTree
        {
            get
            {
                return this.epmTargetTree;
            }
        }

        internal List<EntityPropertyMappingAttribute> MappingsForDeclaredProperties
        {
            get
            {
                return this.mappingsForDeclaredProperties;
            }
        }

        internal List<EntityPropertyMappingAttribute> MappingsForInheritedProperties
        {
            get
            {
                return this.mappingsForInheritedProperties;
            }
        }

        internal int TotalMappingCount
        {
            get
            {
                return this.totalMappingCount;
            }
        }
    }
}

