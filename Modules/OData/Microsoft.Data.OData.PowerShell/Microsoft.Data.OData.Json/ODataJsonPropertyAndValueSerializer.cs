namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Spatial;

    internal class ODataJsonPropertyAndValueSerializer : ODataJsonSerializer
    {
        private int recursionDepth;

        internal ODataJsonPropertyAndValueSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
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

        internal void WriteCollectionValue(ODataCollectionValue collectionValue, IEdmTypeReference metadataTypeReference, bool isOpenPropertyType)
        {
            this.IncreaseRecursionDepth();
            base.JsonWriter.StartObjectScope();
            string typeName = collectionValue.TypeName;
            IEdmCollectionTypeReference type = (IEdmCollectionTypeReference) WriterValidationUtils.ResolveTypeNameForWriting(base.Model, metadataTypeReference, ref typeName, EdmTypeKind.Collection, isOpenPropertyType);
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
                base.JsonWriter.WriteName("__metadata");
                base.JsonWriter.StartObjectScope();
                base.JsonWriter.WriteName("type");
                base.JsonWriter.WriteValue(typeName);
                base.JsonWriter.EndObjectScope();
            }
            base.JsonWriter.WriteDataArrayName();
            base.JsonWriter.StartArrayScope();
            IEnumerable items = collectionValue.Items;
            if (items != null)
            {
                IEdmTypeReference propertyTypeReference = (type == null) ? null : type.ElementType();
                CollectionWithoutExpectedTypeValidator collectionValidator = new CollectionWithoutExpectedTypeValidator(itemTypeNameFromCollection);
                DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = null;
                foreach (object obj2 in items)
                {
                    ValidationUtils.ValidateCollectionItem(obj2, false);
                    ODataComplexValue complexValue = obj2 as ODataComplexValue;
                    if (complexValue != null)
                    {
                        if (duplicatePropertyNamesChecker == null)
                        {
                            duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
                        }
                        this.WriteComplexValue(complexValue, propertyTypeReference, false, duplicatePropertyNamesChecker, collectionValidator);
                        duplicatePropertyNamesChecker.Clear();
                    }
                    else
                    {
                        this.WritePrimitiveValue(obj2, collectionValidator, propertyTypeReference);
                    }
                }
            }
            base.JsonWriter.EndArrayScope();
            base.JsonWriter.EndObjectScope();
            this.DecreaseRecursionDepth();
        }

        internal void WriteComplexValue(ODataComplexValue complexValue, IEdmTypeReference propertyTypeReference, bool isOpenPropertyType, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator)
        {
            this.IncreaseRecursionDepth();
            base.JsonWriter.StartObjectScope();
            string typeName = complexValue.TypeName;
            if (collectionValidator != null)
            {
                collectionValidator.ValidateCollectionItem(typeName, EdmTypeKind.Complex);
            }
            IEdmComplexTypeReference type = WriterValidationUtils.ResolveTypeNameForWriting(base.Model, propertyTypeReference, ref typeName, EdmTypeKind.Complex, isOpenPropertyType).AsComplexOrNull();
            if (((typeName != null) && (collectionValidator != null)) && (string.CompareOrdinal(collectionValidator.ItemTypeNameFromCollection, typeName) == 0))
            {
                typeName = null;
            }
            SerializationTypeNameAnnotation annotation = complexValue.GetAnnotation<SerializationTypeNameAnnotation>();
            if (annotation != null)
            {
                typeName = annotation.TypeName;
            }
            if (typeName != null)
            {
                base.JsonWriter.WriteName("__metadata");
                base.JsonWriter.StartObjectScope();
                base.JsonWriter.WriteName("type");
                base.JsonWriter.WriteValue(typeName);
                base.JsonWriter.EndObjectScope();
            }
            this.WriteProperties((type == null) ? null : type.ComplexDefinition(), complexValue.Properties, true, duplicatePropertyNamesChecker, null);
            base.JsonWriter.EndObjectScope();
            this.DecreaseRecursionDepth();
        }

        internal void WriteETag(string etagName, string etagValue)
        {
            base.JsonWriter.WriteName(etagName);
            base.JsonWriter.WriteValue(etagValue);
        }

        internal void WritePrimitiveValue(object value, CollectionWithoutExpectedTypeValidator collectionValidator, IEdmTypeReference expectedTypeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = EdmLibraryExtensions.GetPrimitiveTypeReference(value.GetType());
            if (collectionValidator != null)
            {
                if (primitiveTypeReference == null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_UnsupportedPrimitiveType(value.GetType().FullName));
                }
                collectionValidator.ValidateCollectionItem(primitiveTypeReference.FullName(), EdmTypeKind.Primitive);
            }
            if (expectedTypeReference != null)
            {
                ValidationUtils.ValidateIsExpectedPrimitiveType(value, primitiveTypeReference, expectedTypeReference);
            }
            if ((primitiveTypeReference != null) && primitiveTypeReference.IsSpatial())
            {
                string typeName = primitiveTypeReference.FullName();
                PrimitiveConverter.Instance.WriteJson(value, base.JsonWriter, typeName, base.Version);
            }
            else
            {
                base.JsonWriter.WritePrimitiveValue(value, base.Version);
            }
        }

        internal void WriteProperties(IEdmStructuredType owningType, IEnumerable<ODataProperty> properties, bool isComplexValue, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ProjectedPropertiesAnnotation projectedProperties)
        {
            if (properties != null)
            {
                foreach (ODataProperty property in properties)
                {
                    this.WriteProperty(property, owningType, !isComplexValue, duplicatePropertyNamesChecker, projectedProperties);
                }
            }
        }

        private void WriteProperty(ODataProperty property, IEdmStructuredType owningType, bool allowStreamProperty, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ProjectedPropertiesAnnotation projectedProperties)
        {
            WriterValidationUtils.ValidatePropertyNotNull(property);
            if (!projectedProperties.ShouldSkipProperty(property.Name))
            {
                WriterValidationUtils.ValidateProperty(property);
                duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
                IEdmProperty expectedProperty = WriterValidationUtils.ValidatePropertyDefined(property.Name, owningType);
                if (((expectedProperty != null) && expectedProperty.Type.IsSpatial()) || ((expectedProperty == null) && (property.Value is ISpatial)))
                {
                    ODataVersionChecker.CheckSpatialValue(base.Version);
                }
                base.JsonWriter.WriteName(property.Name);
                object obj2 = property.Value;
                if (obj2 == null)
                {
                    WriterValidationUtils.ValidateNullPropertyValue(expectedProperty, base.MessageWriterSettings.WriterBehavior, base.Model);
                    base.JsonWriter.WriteValue((string) null);
                }
                else
                {
                    bool isOpenPropertyType = ((owningType != null) && owningType.IsOpen) && (expectedProperty == null);
                    if (isOpenPropertyType)
                    {
                        ValidationUtils.ValidateOpenPropertyValue(property.Name, obj2);
                    }
                    IEdmTypeReference propertyTypeReference = (expectedProperty == null) ? null : expectedProperty.Type;
                    ODataComplexValue complexValue = obj2 as ODataComplexValue;
                    if (complexValue != null)
                    {
                        this.WriteComplexValue(complexValue, propertyTypeReference, isOpenPropertyType, base.CreateDuplicatePropertyNamesChecker(), null);
                    }
                    else
                    {
                        ODataCollectionValue collectionValue = obj2 as ODataCollectionValue;
                        if (collectionValue != null)
                        {
                            ODataVersionChecker.CheckCollectionValueProperties(base.Version, property.Name);
                            this.WriteCollectionValue(collectionValue, propertyTypeReference, isOpenPropertyType);
                        }
                        else if (obj2 is ODataStreamReferenceValue)
                        {
                            if (!allowStreamProperty)
                            {
                                throw new ODataException(Microsoft.Data.OData.Strings.ODataWriter_StreamPropertiesMustBePropertiesOfODataEntry(property.Name));
                            }
                            WriterValidationUtils.ValidateStreamReferenceProperty(property, expectedProperty, base.Version, base.WritingResponse);
                            this.WriteStreamReferenceValue((ODataStreamReferenceValue) property.Value);
                        }
                        else
                        {
                            this.WritePrimitiveValue(obj2, null, propertyTypeReference);
                        }
                    }
                }
            }
        }

        private void WriteStreamReferenceValue(ODataStreamReferenceValue streamReferenceValue)
        {
            base.JsonWriter.StartObjectScope();
            base.JsonWriter.WriteName("__mediaresource");
            base.JsonWriter.StartObjectScope();
            this.WriteStreamReferenceValueContent(streamReferenceValue);
            base.JsonWriter.EndObjectScope();
            base.JsonWriter.EndObjectScope();
        }

        internal void WriteStreamReferenceValueContent(ODataStreamReferenceValue streamReferenceValue)
        {
            Uri editLink = streamReferenceValue.EditLink;
            if (editLink != null)
            {
                base.JsonWriter.WriteName("edit_media");
                base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(editLink));
            }
            if (streamReferenceValue.ReadLink != null)
            {
                base.JsonWriter.WriteName("media_src");
                base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(streamReferenceValue.ReadLink));
            }
            if (streamReferenceValue.ContentType != null)
            {
                base.JsonWriter.WriteName("content_type");
                base.JsonWriter.WriteValue(streamReferenceValue.ContentType);
            }
            string eTag = streamReferenceValue.ETag;
            if (eTag != null)
            {
                this.WriteETag("media_etag", eTag);
            }
        }

        internal void WriteTopLevelProperty(ODataProperty property)
        {
            base.WriteTopLevelPayload(delegate {
                this.JsonWriter.StartObjectScope();
                this.WriteProperty(property, null, false, this.CreateDuplicatePropertyNamesChecker(), null);
                this.JsonWriter.EndObjectScope();
            });
        }
    }
}

