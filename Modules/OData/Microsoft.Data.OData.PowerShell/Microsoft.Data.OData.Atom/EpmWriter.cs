namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Spatial;

    internal abstract class EpmWriter
    {
        private readonly ODataAtomOutputContext atomOutputContext;

        protected EpmWriter(ODataAtomOutputContext atomOutputContext)
        {
            this.atomOutputContext = atomOutputContext;
        }

        private object ReadComplexPropertyValue(EntityPropertyMappingInfo epmInfo, ODataComplexValue complexValue, EpmValueCache epmValueCache, int sourceSegmentIndex, IEdmComplexTypeReference complexType)
        {
            return this.ReadPropertyValue(epmInfo, EpmValueCache.GetComplexValueProperties(epmValueCache, complexValue, false), sourceSegmentIndex, complexType, epmValueCache);
        }

        protected object ReadEntryPropertyValue(EntityPropertyMappingInfo epmInfo, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType)
        {
            return this.ReadPropertyValue(epmInfo, epmValueCache.EntryProperties, 0, entityType, epmValueCache);
        }

        private object ReadPropertyValue(EntityPropertyMappingInfo epmInfo, IEnumerable<ODataProperty> cachedProperties, int sourceSegmentIndex, IEdmStructuredTypeReference structuredTypeReference, EpmValueCache epmValueCache)
        {
            EpmSourcePathSegment segment = epmInfo.PropertyValuePath[sourceSegmentIndex];
            string propertyName = segment.PropertyName;
            bool flag = epmInfo.PropertyValuePath.Length == (sourceSegmentIndex + 1);
            IEdmStructuredType owningStructuredType = structuredTypeReference.StructuredDefinition();
            IEdmProperty expectedProperty = WriterValidationUtils.ValidatePropertyDefined(propertyName, owningStructuredType);
            if (expectedProperty != null)
            {
                if (flag)
                {
                    if (!expectedProperty.Type.IsODataPrimitiveTypeKind() && !expectedProperty.Type.IsNonEntityODataCollectionTypeKind())
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_EndsWithNonPrimitiveType(propertyName));
                    }
                }
                else if (expectedProperty.Type.TypeKind() != EdmTypeKind.Complex)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_TraversalOfNonComplexType(propertyName));
                }
            }
            ODataProperty property2 = (cachedProperties == null) ? null : cachedProperties.FirstOrDefault<ODataProperty>(p => (p.Name == propertyName));
            if (property2 == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_MissingPropertyOnInstance(propertyName, structuredTypeReference.ODataFullName()));
            }
            object obj2 = property2.Value;
            ODataComplexValue complexValue = obj2 as ODataComplexValue;
            if (flag)
            {
                if (obj2 == null)
                {
                    WriterValidationUtils.ValidateNullPropertyValue(expectedProperty, this.WriterBehavior, this.atomOutputContext.Model);
                    return obj2;
                }
                if (complexValue != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_EndsWithNonPrimitiveType(propertyName));
                }
                ODataCollectionValue value3 = obj2 as ODataCollectionValue;
                if (value3 != null)
                {
                    string str = value3.TypeName;
                    WriterValidationUtils.ResolveTypeNameForWriting(this.atomOutputContext.Model, (expectedProperty == null) ? null : expectedProperty.Type, ref str, EdmTypeKind.Collection, expectedProperty == null);
                    return obj2;
                }
                if (obj2 is ODataStreamReferenceValue)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataWriter_StreamPropertiesMustBePropertiesOfODataEntry(propertyName));
                }
                if (obj2 is ISpatial)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_OpenPropertySpatialTypeCannotBeMapped(propertyName, epmInfo.DefiningType.FullName()));
                }
                if (expectedProperty != null)
                {
                    ValidationUtils.ValidateIsExpectedPrimitiveType(obj2, expectedProperty.Type);
                }
                return obj2;
            }
            if (complexValue == null)
            {
                if (obj2 != null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmSourceTree_TraversalOfNonComplexType(propertyName));
                }
                return null;
            }
            string typeName = complexValue.TypeName;
            IEdmComplexTypeReference complexType = WriterValidationUtils.ResolveTypeNameForWriting(this.atomOutputContext.Model, (expectedProperty == null) ? null : expectedProperty.Type, ref typeName, EdmTypeKind.Complex, expectedProperty == null).AsComplexOrNull();
            return this.ReadComplexPropertyValue(epmInfo, complexValue, epmValueCache, sourceSegmentIndex + 1, complexType);
        }

        protected ODataVersion Version
        {
            get
            {
                return this.atomOutputContext.Version;
            }
        }

        protected ODataWriterBehavior WriterBehavior
        {
            get
            {
                return this.atomOutputContext.MessageWriterSettings.WriterBehavior;
            }
        }
    }
}

