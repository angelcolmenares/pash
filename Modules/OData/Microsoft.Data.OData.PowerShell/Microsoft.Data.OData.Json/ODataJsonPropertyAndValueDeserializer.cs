namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Spatial;

    internal class ODataJsonPropertyAndValueDeserializer : ODataJsonDeserializer
    {
        private int recursionDepth;

        internal ODataJsonPropertyAndValueDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
        }

        [Conditional("DEBUG")]
        private void AssertRecursionDepthIsZero()
        {
        }

        private void DecreaseRecursionDepth()
        {
            this.recursionDepth--;
        }

        internal string FindTypeNameInPayload()
        {
            if (base.JsonReader.NodeType == JsonNodeType.PrimitiveValue)
            {
                return null;
            }
            base.JsonReader.StartBuffering();
            base.JsonReader.ReadStartObject();
            string str = null;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                if (string.CompareOrdinal(base.JsonReader.ReadPropertyName(), "__metadata") == 0)
                {
                    str = this.ReadTypeNameFromMetadataPropertyValue();
                    break;
                }
                base.JsonReader.SkipValue();
            }
            base.JsonReader.StopBuffering();
            return str;
        }

        private EdmTypeKind GetNonEntityValueKind()
        {
            EdmTypeKind complex;
            if (base.JsonReader.NodeType == JsonNodeType.PrimitiveValue)
            {
                return EdmTypeKind.Primitive;
            }
            base.JsonReader.StartBuffering();
            try
            {
                base.JsonReader.ReadNext();
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string b = base.JsonReader.ReadPropertyName();
                    if (string.Equals("results", b, StringComparison.Ordinal))
                    {
                        if ((base.JsonReader.NodeType == JsonNodeType.StartArray) && (base.Version >= ODataVersion.V3))
                        {
                            return EdmTypeKind.Collection;
                        }
                        return EdmTypeKind.Complex;
                    }
                    base.JsonReader.SkipValue();
                }
                complex = EdmTypeKind.Complex;
            }
            finally
            {
                base.JsonReader.StopBuffering();
            }
            return complex;
        }

        private void IncreaseRecursionDepth()
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref this.recursionDepth, base.MessageReaderSettings.MessageQuotas.MaxNestingDepth);
        }

        private IEnumerable<object> ReadArrayValue(JsonReader jsonReader)
        {
            this.IncreaseRecursionDepth();
            List<object> list = new List<object>();
            jsonReader.ReadNext();
            while (jsonReader.NodeType != JsonNodeType.EndArray)
            {
                switch (jsonReader.NodeType)
                {
                    case JsonNodeType.StartObject:
                    {
                        list.Add(this.ReadObjectValue(jsonReader));
                        continue;
                    }
                    case JsonNodeType.StartArray:
                    {
                        list.Add(this.ReadArrayValue(jsonReader));
                        continue;
                    }
                    case JsonNodeType.PrimitiveValue:
                    {
                        list.Add(jsonReader.ReadPrimitiveValue());
                        continue;
                    }
                }
                return null;
            }
            jsonReader.ReadEndArray();
            this.DecreaseRecursionDepth();
            return list;
        }

        private ODataCollectionValue ReadCollectionValueImplementation(IEdmCollectionTypeReference collectionValueTypeReference, string payloadTypeName, SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            ODataVersionChecker.CheckCollectionValue(base.Version);
            this.IncreaseRecursionDepth();
            base.JsonReader.ReadStartObject();
            ODataCollectionValue value2 = new ODataCollectionValue {
                TypeName = (collectionValueTypeReference != null) ? collectionValueTypeReference.ODataFullName() : payloadTypeName
            };
            if (serializationTypeNameAnnotation != null)
            {
                value2.SetAnnotation<SerializationTypeNameAnnotation>(serializationTypeNameAnnotation);
            }
            List<object> sourceEnumerable = null;
            bool flag = false;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("__metadata", strB) == 0)
                {
                    if (flag)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_MultiplePropertiesInCollectionWrapper("__metadata"));
                    }
                    flag = true;
                    base.JsonReader.SkipValue();
                }
                else
                {
                    if (string.CompareOrdinal("results", strB) == 0)
                    {
                        if (sourceEnumerable != null)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_MultiplePropertiesInCollectionWrapper("results"));
                        }
                        sourceEnumerable = new List<object>();
                        base.JsonReader.ReadStartArray();
                        DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
                        IEdmTypeReference expectedTypeReference = null;
                        if (collectionValueTypeReference != null)
                        {
                            expectedTypeReference = collectionValueTypeReference.CollectionDefinition().ElementType;
                        }
                        CollectionWithoutExpectedTypeValidator collectionValidator = null;
                        while (base.JsonReader.NodeType != JsonNodeType.EndArray)
                        {
                            object item = this.ReadNonEntityValueImplementation(expectedTypeReference, duplicatePropertyNamesChecker, collectionValidator, true);
                            ValidationUtils.ValidateCollectionItem(item, false);
                            sourceEnumerable.Add(item);
                        }
                        base.JsonReader.ReadEndArray();
                        continue;
                    }
                    base.JsonReader.SkipValue();
                }
            }
            base.JsonReader.ReadEndObject();
            if (sourceEnumerable == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_CollectionWithoutResults);
            }
            value2.Items = new ReadOnlyEnumerable(sourceEnumerable);
            this.DecreaseRecursionDepth();
            return value2;
        }

        private ODataComplexValue ReadComplexValueImplementation(IEdmComplexTypeReference complexValueTypeReference, string payloadTypeName, SerializationTypeNameAnnotation serializationTypeNameAnnotation, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            this.IncreaseRecursionDepth();
            base.JsonReader.ReadStartObject();
            ODataComplexValue value2 = new ODataComplexValue {
                TypeName = (complexValueTypeReference != null) ? complexValueTypeReference.ODataFullName() : payloadTypeName
            };
            if (serializationTypeNameAnnotation != null)
            {
                value2.SetAnnotation<SerializationTypeNameAnnotation>(serializationTypeNameAnnotation);
            }
            if (duplicatePropertyNamesChecker == null)
            {
                duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
            }
            else
            {
                duplicatePropertyNamesChecker.Clear();
            }
            List<ODataProperty> sourceList = new List<ODataProperty>();
            bool flag = false;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("__metadata", strB) == 0)
                {
                    if (flag)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_MultipleMetadataPropertiesInComplexValue);
                    }
                    flag = true;
                    base.JsonReader.SkipValue();
                }
                else if (!ValidationUtils.IsValidPropertyName(strB))
                {
                    base.JsonReader.SkipValue();
                }
                else
                {
                    ODataProperty property = new ODataProperty {
                        Name = strB
                    };
                    IEdmProperty property2 = null;
                    bool ignoreProperty = false;
                    if (complexValueTypeReference != null)
                    {
                        property2 = ReaderValidationUtils.ValidateValuePropertyDefined(strB, complexValueTypeReference.ComplexDefinition(), base.MessageReaderSettings, out ignoreProperty);
                    }
                    if (ignoreProperty)
                    {
                        base.JsonReader.SkipValue();
                        continue;
                    }
                    ODataNullValueBehaviorKind kind = (base.ReadingResponse || (property2 == null)) ? ODataNullValueBehaviorKind.Default : base.Model.NullValueReadBehaviorKind(property2);
                    object obj2 = this.ReadNonEntityValueImplementation((property2 == null) ? null : property2.Type, null, null, kind == ODataNullValueBehaviorKind.Default);
                    if ((kind != ODataNullValueBehaviorKind.IgnoreValue) || (obj2 != null))
                    {
                        duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
                        property.Value = obj2;
                        sourceList.Add(property);
                    }
                }
            }
            base.JsonReader.ReadEndObject();
            value2.Properties = new ReadOnlyEnumerable<ODataProperty>(sourceList);
            this.DecreaseRecursionDepth();
            return value2;
        }

        internal object ReadNonEntityValue(IEdmTypeReference expectedValueTypeReference, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator, bool validateNullValue)
        {
            return this.ReadNonEntityValueImplementation(expectedValueTypeReference, duplicatePropertyNamesChecker, collectionValidator, validateNullValue);
        }

        private object ReadNonEntityValueImplementation(IEdmTypeReference expectedTypeReference, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator, bool validateNullValue)
        {
            object obj2;
            SerializationTypeNameAnnotation annotation;
            EdmTypeKind kind;
            JsonNodeType nodeType = base.JsonReader.NodeType;
            if (nodeType == JsonNodeType.StartArray)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_CannotReadPropertyValue(nodeType));
            }
            if (this.TryReadNullValue(expectedTypeReference, validateNullValue))
            {
                return null;
            }
            string payloadTypeName = this.FindTypeNameInPayload();
            IEdmTypeReference type = ReaderValidationUtils.ResolvePayloadTypeNameAndComputeTargetType(EdmTypeKind.None, null, expectedTypeReference, payloadTypeName, base.Model, base.MessageReaderSettings, base.Version, new Func<EdmTypeKind>(this.GetNonEntityValueKind), out kind, out annotation);
            switch (kind)
            {
                case EdmTypeKind.Primitive:
                {
                    IEdmPrimitiveTypeReference reference2 = (type == null) ? null : type.AsPrimitive();
                    if ((payloadTypeName != null) && !reference2.IsSpatial())
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_InvalidPrimitiveTypeName(payloadTypeName));
                    }
                    obj2 = this.ReadPrimitiveValueImplementation(reference2, validateNullValue);
                    break;
                }
                case EdmTypeKind.Complex:
                    obj2 = this.ReadComplexValueImplementation((type == null) ? null : type.AsComplex(), payloadTypeName, annotation, duplicatePropertyNamesChecker);
                    break;

                case EdmTypeKind.Collection:
                {
                    IEdmCollectionTypeReference collectionValueTypeReference = ValidationUtils.ValidateCollectionType(type);
                    obj2 = this.ReadCollectionValueImplementation(collectionValueTypeReference, payloadTypeName, annotation);
                    break;
                }
                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataJsonPropertyAndValueDeserializer_ReadPropertyValue));
            }
            if (collectionValidator != null)
            {
                string collectionItemTypeName = ODataJsonReaderUtils.GetPayloadTypeName(obj2);
                collectionValidator.ValidateCollectionItem(collectionItemTypeName, kind);
            }
            return obj2;
        }

        private IDictionary<string, object> ReadObjectValue(JsonReader jsonReader)
        {
            this.IncreaseRecursionDepth();
            IDictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
            jsonReader.ReadNext();
            while (jsonReader.NodeType != JsonNodeType.EndObject)
            {
                string key = jsonReader.ReadPropertyName();
                object obj2 = null;
                switch (jsonReader.NodeType)
                {
                    case JsonNodeType.StartObject:
                        obj2 = this.ReadObjectValue(jsonReader);
                        break;

                    case JsonNodeType.StartArray:
                        obj2 = this.ReadArrayValue(jsonReader);
                        break;

                    case JsonNodeType.PrimitiveValue:
                        obj2 = jsonReader.ReadPrimitiveValue();
                        break;

                    default:
                        return null;
                }
                dictionary.Add(key, obj2);
            }
            jsonReader.ReadEndObject();
            this.DecreaseRecursionDepth();
            return dictionary;
        }

        private object ReadPrimitiveValueImplementation(IEdmPrimitiveTypeReference expectedValueTypeReference, bool validateNullValue)
        {
            if ((expectedValueTypeReference != null) && expectedValueTypeReference.IsSpatial())
            {
                return this.ReadSpatialValue(expectedValueTypeReference, validateNullValue);
            }
            object obj2 = base.JsonReader.ReadPrimitiveValue();
            if ((expectedValueTypeReference != null) && !base.MessageReaderSettings.DisablePrimitiveTypeConversion)
            {
                obj2 = ODataJsonReaderUtils.ConvertValue(obj2, expectedValueTypeReference, base.MessageReaderSettings, base.Version, validateNullValue);
            }
            return obj2;
        }

        private ISpatial ReadSpatialValue(IEdmPrimitiveTypeReference expectedValueTypeReference, bool validateNullValue)
        {
            ODataVersionChecker.CheckSpatialValue(base.Version);
            if (this.TryReadNullValue(expectedValueTypeReference, validateNullValue))
            {
                return null;
            }
            ISpatial spatial = null;
            if (base.JsonReader.NodeType == JsonNodeType.StartObject)
            {
                IDictionary<string, object> source = this.ReadObjectValue(base.JsonReader);
                GeoJsonObjectFormatter formatter = SpatialImplementation.CurrentImplementation.CreateGeoJsonObjectFormatter();
                if (expectedValueTypeReference.IsGeographyType())
                {
                    spatial = formatter.Read<Geography>(source);
                }
                else
                {
                    spatial = formatter.Read<Geometry>(source);
                }
            }
            if (spatial == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_CannotReadSpatialPropertyValue);
            }
            return spatial;
        }

        internal ODataProperty ReadTopLevelProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            if (!base.Model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_TopLevelPropertyWithoutMetadata);
            }
            base.ReadPayloadStart(false);
            string propertyName = null;
            object obj2 = null;
            if (this.ShouldReadTopLevelPropertyValueWithoutPropertyWrapper(expectedPropertyTypeReference))
            {
                propertyName = string.Empty;
                obj2 = this.ReadNonEntityValue(expectedPropertyTypeReference, null, null, true);
            }
            else
            {
                base.JsonReader.ReadStartObject();
                bool flag = false;
                string str2 = null;
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    propertyName = base.JsonReader.ReadPropertyName();
                    if (!ValidationUtils.IsValidPropertyName(propertyName))
                    {
                        base.JsonReader.SkipValue();
                    }
                    else
                    {
                        if (flag)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_InvalidTopLevelPropertyPayload);
                        }
                        flag = true;
                        str2 = propertyName;
                        obj2 = this.ReadNonEntityValue(expectedPropertyTypeReference, null, null, true);
                    }
                }
                if (!flag)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_InvalidTopLevelPropertyPayload);
                }
                propertyName = str2;
                base.JsonReader.Read();
            }
            base.ReadPayloadEnd(false);
            return new ODataProperty { Name = propertyName, Value = obj2 };
        }

        internal string ReadTypeNameFromMetadataPropertyValue()
        {
            string str = null;
            if (base.JsonReader.NodeType != JsonNodeType.StartObject)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_MetadataPropertyMustHaveObjectValue(base.JsonReader.NodeType));
            }
            base.JsonReader.ReadStartObject();
            ODataJsonReaderUtils.MetadataPropertyBitMask none = ODataJsonReaderUtils.MetadataPropertyBitMask.None;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("type", strB) == 0)
                {
                    ODataJsonReaderUtils.VerifyMetadataPropertyNotFound(ref none, ODataJsonReaderUtils.MetadataPropertyBitMask.Type, "type");
                    object obj2 = base.JsonReader.ReadPrimitiveValue();
                    str = obj2 as string;
                    if (str == null)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonPropertyAndValueDeserializer_InvalidTypeName(obj2));
                    }
                }
                else
                {
                    base.JsonReader.SkipValue();
                }
            }
            base.JsonReader.ReadEndObject();
            return str;
        }

        private bool ShouldReadTopLevelPropertyValueWithoutPropertyWrapper(IEdmTypeReference expectedPropertyTypeReference)
        {
            if (base.UseServerFormatBehavior && (expectedPropertyTypeReference == null))
            {
                base.JsonReader.StartBuffering();
                try
                {
                    base.JsonReader.ReadStartObject();
                    if (base.JsonReader.NodeType == JsonNodeType.EndObject)
                    {
                        return false;
                    }
                    string strA = base.JsonReader.ReadPropertyName();
                    base.JsonReader.SkipValue();
                    if (base.JsonReader.NodeType != JsonNodeType.EndObject)
                    {
                        return true;
                    }
                    if (string.CompareOrdinal(strA, "__metadata") == 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    base.JsonReader.StopBuffering();
                }
            }
            return false;
        }

        private bool TryReadNullValue(IEdmTypeReference expectedTypeReference, bool validateNullValue)
        {
            if ((base.JsonReader.NodeType == JsonNodeType.PrimitiveValue) && (base.JsonReader.Value == null))
            {
                base.JsonReader.ReadNext();
                ReaderValidationUtils.ValidateNullValue(base.Model, expectedTypeReference, base.MessageReaderSettings, validateNullValue, base.Version);
                return true;
            }
            return false;
        }
    }
}

