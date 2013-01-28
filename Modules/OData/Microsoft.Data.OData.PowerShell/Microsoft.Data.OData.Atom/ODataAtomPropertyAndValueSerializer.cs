namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
	using System.Linq;
    using System.Spatial;

    internal class ODataAtomPropertyAndValueSerializer : ODataAtomSerializer
    {
        private int recursionDepth;

        internal ODataAtomPropertyAndValueSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertRecursionDepthIsZero()
        {
        }

        private void DecreaseRecursionDepth()
        {
            this.recursionDepth--;
        }

        private void IncreaseRecursionDepth()
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref this.recursionDepth, base.MessageWriterSettings.MessageQuotas.MaxNestingDepth);
        }

        private bool ShouldWritePropertyInContent(IEdmStructuredType owningType, ProjectedPropertiesAnnotation projectedProperties, string propertyName, object propertyValue, EpmSourcePathSegment epmSourcePathSegment)
        {
            bool flag = !projectedProperties.ShouldSkipProperty(propertyName);
            if ((((base.MessageWriterSettings.WriterBehavior != null) && base.MessageWriterSettings.WriterBehavior.UseV1ProviderBehavior) && (owningType != null)) && owningType.IsODataComplexTypeKind())
            {
                IEdmComplexType complexType = (IEdmComplexType) owningType;
                CachedPrimitiveKeepInContentAnnotation annotation = base.Model.EpmCachedKeepPrimitiveInContent(complexType);
                if ((annotation != null) && annotation.IsKeptInContent(propertyName))
                {
                    return flag;
                }
            }
            if ((propertyValue == null) && (epmSourcePathSegment != null))
            {
                return true;
            }
            EntityPropertyMappingAttribute entityPropertyMapping = EpmWriterUtils.GetEntityPropertyMapping(epmSourcePathSegment);
            if (entityPropertyMapping == null)
            {
                return flag;
            }
            string str = propertyValue as string;
            if ((str != null) && (str.Length == 0))
            {
                switch (entityPropertyMapping.TargetSyndicationItem)
                {
                    case SyndicationItemProperty.AuthorEmail:
                    case SyndicationItemProperty.AuthorUri:
                    case SyndicationItemProperty.ContributorEmail:
                    case SyndicationItemProperty.ContributorUri:
                        return true;
                }
            }
            return (entityPropertyMapping.KeepInContent && flag);
        }

        private void WriteCollectionValue(ODataCollectionValue collectionValue, IEdmTypeReference propertyTypeReference, bool isOpenPropertyType, bool isWritingCollection)
        {
            this.IncreaseRecursionDepth();
            string typeName = collectionValue.TypeName;
            IEdmCollectionTypeReference type = (IEdmCollectionTypeReference) WriterValidationUtils.ResolveTypeNameForWriting(base.Model, propertyTypeReference, ref typeName, EdmTypeKind.Collection, isOpenPropertyType);
            string itemTypeNameFromCollection = null;
            if (typeName != null)
            {
                itemTypeNameFromCollection = ValidationUtils.ValidateCollectionTypeName(typeName);
            }
            SerializationTypeNameAnnotation annotation = collectionValue.GetAnnotation<SerializationTypeNameAnnotation>();
            if (annotation != null)
            {
                typeName = annotation.TypeName;
            }
            if (typeName != null)
            {
                this.WritePropertyTypeAttribute(typeName);
            }
            IEdmTypeReference metadataTypeReference = (type == null) ? null : type.ElementType();
            CollectionWithoutExpectedTypeValidator collectionValidator = new CollectionWithoutExpectedTypeValidator(itemTypeNameFromCollection);
            IEnumerable items = collectionValue.Items;
            if (items != null)
            {
                DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = null;
                foreach (object obj2 in items)
                {
                    ValidationUtils.ValidateCollectionItem(obj2, false);
                    base.XmlWriter.WriteStartElement("d", "element", base.MessageWriterSettings.WriterBehavior.ODataNamespace);
                    ODataComplexValue complexValue = obj2 as ODataComplexValue;
                    if (complexValue != null)
                    {
                        if (duplicatePropertyNamesChecker == null)
                        {
                            duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
                        }
                        this.WriteComplexValue(complexValue, metadataTypeReference, false, isWritingCollection, null, null, duplicatePropertyNamesChecker, collectionValidator, null, null, null);
                        duplicatePropertyNamesChecker.Clear();
                    }
                    else
                    {
                        this.WritePrimitiveValue(obj2, collectionValidator, metadataTypeReference);
                    }
                    base.XmlWriter.WriteEndElement();
                }
            }
            this.DecreaseRecursionDepth();
        }

        internal bool WriteComplexValue(ODataComplexValue complexValue, IEdmTypeReference metadataTypeReference, bool isOpenPropertyType, bool isWritingCollection, Action beforeValueAction, Action afterValueAction, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator, EpmValueCache epmValueCache, EpmSourcePathSegment epmSourcePathSegment, ProjectedPropertiesAnnotation projectedProperties)
        {
            Action action2 = null;
            string typeName = complexValue.TypeName;
            if (collectionValidator != null)
            {
                collectionValidator.ValidateCollectionItem(typeName, EdmTypeKind.Complex);
            }
            this.IncreaseRecursionDepth();
            IEdmComplexTypeReference reference = WriterValidationUtils.ResolveTypeNameForWriting(base.Model, metadataTypeReference, ref typeName, EdmTypeKind.Complex, isOpenPropertyType).AsComplexOrNull();
            if (((typeName != null) && (collectionValidator != null)) && (string.CompareOrdinal(collectionValidator.ItemTypeNameFromCollection, typeName) == 0))
            {
                typeName = null;
            }
            SerializationTypeNameAnnotation annotation = complexValue.GetAnnotation<SerializationTypeNameAnnotation>();
            if (annotation != null)
            {
                typeName = annotation.TypeName;
            }
            Action beforePropertiesAction = beforeValueAction;
            if (typeName != null)
            {
                if (beforeValueAction != null)
                {
                    if (action2 == null)
                    {
                        action2 = delegate {
                            beforeValueAction();
                            this.WritePropertyTypeAttribute(typeName);
                        };
                    }
                    beforePropertiesAction = action2;
                }
                else
                {
                    this.WritePropertyTypeAttribute(typeName);
                }
            }
            if (((base.MessageWriterSettings.WriterBehavior != null) && base.MessageWriterSettings.WriterBehavior.UseV1ProviderBehavior) && !object.ReferenceEquals(projectedProperties, ProjectedPropertiesAnnotation.EmptyProjectedPropertiesMarker))
            {
                IEdmComplexType definition = (IEdmComplexType) reference.Definition;
                if (base.Model.EpmCachedKeepPrimitiveInContent(definition) == null)
                {
                    List<string> keptInContentPropertyNames = null;
                    foreach (IEdmProperty property in from p in definition.Properties()
                        where p.Type.IsODataPrimitiveTypeKind()
                        select p)
                    {
                        EntityPropertyMappingAttribute entityPropertyMapping = EpmWriterUtils.GetEntityPropertyMapping(epmSourcePathSegment, property.Name);
                        if ((entityPropertyMapping != null) && entityPropertyMapping.KeepInContent)
                        {
                            if (keptInContentPropertyNames == null)
                            {
                                keptInContentPropertyNames = new List<string>();
                            }
                            keptInContentPropertyNames.Add(property.Name);
                        }
                    }
                    base.Model.SetAnnotationValue<CachedPrimitiveKeepInContentAnnotation>(definition, new CachedPrimitiveKeepInContentAnnotation(keptInContentPropertyNames));
                }
            }
            bool flag = this.WriteProperties((reference == null) ? null : reference.ComplexDefinition(), EpmValueCache.GetComplexValueProperties(epmValueCache, complexValue, true), isWritingCollection, beforePropertiesAction, afterValueAction, duplicatePropertyNamesChecker, epmValueCache, epmSourcePathSegment, projectedProperties);
            this.DecreaseRecursionDepth();
            return flag;
        }

        private void WriteNullAttribute()
        {
            base.XmlWriter.WriteAttributeString("m", "null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
        }

        private void WriteNullPropertyValue(IEdmProperty edmProperty, string propertyName, bool isTopLevel, bool isWritingCollection, Action beforePropertyAction)
        {
            WriterValidationUtils.ValidateNullPropertyValue(edmProperty, base.MessageWriterSettings.WriterBehavior, base.Model);
            this.WritePropertyStart(beforePropertyAction, propertyName, isWritingCollection, isTopLevel);
            if ((edmProperty != null) && !base.UseDefaultFormatBehavior)
            {
                string typeName = edmProperty.Type.ODataFullName();
                if ((typeName != "Edm.String") && (edmProperty.Type.IsODataPrimitiveTypeKind() || base.UseServerFormatBehavior))
                {
                    this.WritePropertyTypeAttribute(typeName);
                }
            }
            this.WriteNullAttribute();
            this.WritePropertyEnd();
        }

        internal void WritePrimitiveValue(object value, CollectionWithoutExpectedTypeValidator collectionValidator, IEdmTypeReference expectedTypeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = EdmLibraryExtensions.GetPrimitiveTypeReference(value.GetType());
            if (primitiveTypeReference == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_UnsupportedPrimitiveType(value.GetType().FullName));
            }
            string collectionItemTypeName = primitiveTypeReference.FullName();
            if (collectionValidator != null)
            {
                collectionValidator.ValidateCollectionItem(collectionItemTypeName, EdmTypeKind.Primitive);
                if (string.CompareOrdinal(collectionValidator.ItemTypeNameFromCollection, collectionItemTypeName) == 0)
                {
                    collectionItemTypeName = null;
                }
            }
            if (expectedTypeReference != null)
            {
                ValidationUtils.ValidateIsExpectedPrimitiveType(value, primitiveTypeReference, expectedTypeReference);
            }
            if ((collectionItemTypeName != null) && (collectionItemTypeName != "Edm.String"))
            {
                this.WritePropertyTypeAttribute(collectionItemTypeName);
            }
            AtomValueUtils.WritePrimitiveValue(base.XmlWriter, value);
        }

        internal bool WriteProperties(IEdmStructuredType owningType, IEnumerable<ODataProperty> cachedProperties, bool isWritingCollection, Action beforePropertiesAction, Action afterPropertiesAction, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, EpmValueCache epmValueCache, EpmSourcePathSegment epmSourcePathSegment, ProjectedPropertiesAnnotation projectedProperties)
        {
            if (cachedProperties == null)
            {
                return false;
            }
            bool flag = false;
            foreach (ODataProperty property in cachedProperties)
            {
                flag |= this.WriteProperty(property, owningType, false, isWritingCollection, flag ? null : beforePropertiesAction, epmValueCache, epmSourcePathSegment, duplicatePropertyNamesChecker, projectedProperties);
            }
            if ((afterPropertiesAction != null) && flag)
            {
                afterPropertiesAction();
            }
            return flag;
        }

        private bool WriteProperty(ODataProperty property, IEdmStructuredType owningType, bool isTopLevel, bool isWritingCollection, Action beforePropertyAction, EpmValueCache epmValueCache, EpmSourcePathSegment epmParentSourcePathSegment, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ProjectedPropertiesAnnotation projectedProperties)
        {
            Action beforeValueAction = null;
            Action afterValueAction = null;
            WriterValidationUtils.ValidatePropertyNotNull(property);
            object propertyValue = property.Value;
            string propertyName = property.Name;
            EpmSourcePathSegment propertySourcePathSegment = EpmWriterUtils.GetPropertySourcePathSegment(epmParentSourcePathSegment, propertyName);
            ODataComplexValue complexValue = propertyValue as ODataComplexValue;
            ProjectedPropertiesAnnotation emptyProjectedPropertiesMarker = null;
            if (!this.ShouldWritePropertyInContent(owningType, projectedProperties, propertyName, propertyValue, propertySourcePathSegment))
            {
                if ((propertySourcePathSegment == null) || (complexValue == null))
                {
                    return false;
                }
                emptyProjectedPropertiesMarker = ProjectedPropertiesAnnotation.EmptyProjectedPropertiesMarker;
            }
            WriterValidationUtils.ValidateProperty(property);
            duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
            IEdmProperty edmProperty = WriterValidationUtils.ValidatePropertyDefined(propertyName, owningType);
            if (propertyValue is ODataStreamReferenceValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriter_StreamPropertiesMustBePropertiesOfODataEntry(propertyName));
            }
            if (((edmProperty != null) && edmProperty.Type.IsSpatial()) || ((edmProperty == null) && (propertyValue is ISpatial)))
            {
                ODataVersionChecker.CheckSpatialValue(base.Version);
            }
            if (propertyValue == null)
            {
                this.WriteNullPropertyValue(edmProperty, propertyName, isTopLevel, isWritingCollection, beforePropertyAction);
                return true;
            }
            bool isOpenPropertyType = ((owningType != null) && owningType.IsOpen) && (edmProperty == null);
            if (isOpenPropertyType)
            {
                ValidationUtils.ValidateOpenPropertyValue(propertyName, propertyValue);
            }
            IEdmTypeReference metadataTypeReference = (edmProperty == null) ? null : edmProperty.Type;
            if (complexValue != null)
            {
                DuplicatePropertyNamesChecker checker = base.CreateDuplicatePropertyNamesChecker();
                if (isTopLevel)
                {
                    this.WritePropertyStart(beforePropertyAction, propertyName, isWritingCollection, isTopLevel);
                    this.WriteComplexValue(complexValue, metadataTypeReference, isOpenPropertyType, isWritingCollection, null, null, checker, null, epmValueCache, propertySourcePathSegment, null);
                    this.WritePropertyEnd();
                    return true;
                }
                if (beforeValueAction == null)
                {
                    beforeValueAction = delegate {
                        this.WritePropertyStart(beforePropertyAction, propertyName, isWritingCollection, isTopLevel);
                    };
                }
                if (afterValueAction == null)
                {
                    afterValueAction = delegate {
                        this.WritePropertyEnd();
                    };
                }
                return this.WriteComplexValue(complexValue, metadataTypeReference, isOpenPropertyType, isWritingCollection, beforeValueAction, afterValueAction, checker, null, epmValueCache, propertySourcePathSegment, emptyProjectedPropertiesMarker);
            }
            ODataCollectionValue collectionValue = propertyValue as ODataCollectionValue;
            if (collectionValue != null)
            {
                ODataVersionChecker.CheckCollectionValueProperties(base.Version, propertyName);
                this.WritePropertyStart(beforePropertyAction, propertyName, isWritingCollection, isTopLevel);
                this.WriteCollectionValue(collectionValue, metadataTypeReference, isOpenPropertyType, isWritingCollection);
                this.WritePropertyEnd();
                return true;
            }
            this.WritePropertyStart(beforePropertyAction, propertyName, isWritingCollection, isTopLevel);
            this.WritePrimitiveValue(propertyValue, null, metadataTypeReference);
            this.WritePropertyEnd();
            return true;
        }

        private void WritePropertyEnd()
        {
            base.XmlWriter.WriteEndElement();
        }

        private void WritePropertyStart(Action beforePropertyCallback, string propertyName, bool isWritingCollection, bool isTopLevel)
        {
            if (beforePropertyCallback != null)
            {
                beforePropertyCallback();
            }
            base.XmlWriter.WriteStartElement(isWritingCollection ? string.Empty : "d", propertyName, base.MessageWriterSettings.WriterBehavior.ODataNamespace);
            if (isTopLevel)
            {
                base.WriteDefaultNamespaceAttributes(ODataAtomSerializer.DefaultNamespaceFlags.Gml | ODataAtomSerializer.DefaultNamespaceFlags.GeoRss | ODataAtomSerializer.DefaultNamespaceFlags.ODataMetadata | ODataAtomSerializer.DefaultNamespaceFlags.OData);
            }
        }

        private void WritePropertyTypeAttribute(string typeName)
        {
            base.XmlWriter.WriteAttributeString("m", "type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", typeName);
        }

        internal void WriteTopLevelProperty(ODataProperty property)
        {
            base.WritePayloadStart();
            this.WriteProperty(property, null, true, false, null, null, null, base.CreateDuplicatePropertyNamesChecker(), null);
            base.WritePayloadEnd();
        }
    }
}

