namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ReaderValidationUtils
    {
        private static EdmTypeKind ComputeTargetTypeKind(IEdmTypeReference expectedTypeReference, bool forEntityValue, string payloadTypeName, EdmTypeKind payloadTypeKind, ODataMessageReaderSettings messageReaderSettings, Func<EdmTypeKind> typeKindFromPayloadFunc)
        {
            EdmTypeKind kind;
            bool flag = (messageReaderSettings.ReaderBehavior.TypeResolver != null) && (payloadTypeKind != EdmTypeKind.None);
            if (((expectedTypeReference != null) && !flag) && (!expectedTypeReference.IsODataPrimitiveTypeKind() || !messageReaderSettings.DisablePrimitiveTypeConversion))
            {
                kind = expectedTypeReference.TypeKind();
            }
            else if (payloadTypeKind != EdmTypeKind.None)
            {
                if (!forEntityValue)
                {
                    ValidationUtils.ValidateValueTypeKind(payloadTypeKind, payloadTypeName);
                }
                kind = payloadTypeKind;
            }
            else
            {
                kind = typeKindFromPayloadFunc();
            }
            if (ShouldValidatePayloadTypeKind(messageReaderSettings, expectedTypeReference, payloadTypeKind))
            {
                ValidationUtils.ValidateTypeKind(kind, expectedTypeReference.TypeKind(), payloadTypeName);
            }
            return kind;
        }

        private static SerializationTypeNameAnnotation CreateSerializationTypeNameAnnotation(string payloadTypeName, IEdmTypeReference targetTypeReference)
        {
            if ((payloadTypeName != null) && (string.CompareOrdinal(payloadTypeName, targetTypeReference.ODataFullName()) != 0))
            {
                return new SerializationTypeNameAnnotation { TypeName = payloadTypeName };
            }
            if (payloadTypeName == null)
            {
                return new SerializationTypeNameAnnotation { TypeName = null };
            }
            return null;
        }

        internal static IEdmProperty FindDefinedProperty(string propertyName, IEdmStructuredType owningStructuredType, ODataMessageReaderSettings messageReaderSettings)
        {
            if (owningStructuredType == null)
            {
                return null;
            }
            IEdmProperty property = owningStructuredType.FindProperty(propertyName);
            if (((property == null) && owningStructuredType.IsOpen) && (messageReaderSettings.UndeclaredPropertyBehaviorKinds != ODataUndeclaredPropertyBehaviorKinds.None))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedForOpenType(propertyName, owningStructuredType.ODataFullName()));
            }
            return property;
        }

        private static IEdmTypeReference GetNullablePayloadTypeReference(IEdmType payloadType)
        {
            if (payloadType != null)
            {
                return payloadType.ToTypeReference(true);
            }
            return null;
        }

        internal static ODataException GetPrimitiveTypeConversionException(IEdmPrimitiveTypeReference targetTypeReference, Exception innerException)
        {
            return new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_CannotConvertPrimitiveValue(targetTypeReference.ODataFullName()), innerException);
        }

        internal static IEdmTypeReference ResolveAndValidateNonPrimitiveTargetType(EdmTypeKind expectedTypeKind, IEdmTypeReference expectedTypeReference, EdmTypeKind payloadTypeKind, IEdmType payloadType, string payloadTypeName, IEdmModel model, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, out SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            bool flag = (messageReaderSettings.ReaderBehavior.TypeResolver != null) && (payloadType != null);
            if (!flag)
            {
                ValidateTypeSupported(expectedTypeReference, version);
                if (model.IsUserModel() && ((expectedTypeReference == null) || !messageReaderSettings.DisableStrictMetadataValidation))
                {
                    VerifyPayloadTypeDefined(payloadTypeName, payloadType);
                }
            }
            else
            {
                ValidateTypeSupported((payloadType == null) ? null : payloadType.ToTypeReference(true), version);
            }
            if ((payloadTypeKind != EdmTypeKind.None) && (!messageReaderSettings.DisableStrictMetadataValidation || (expectedTypeReference == null)))
            {
                ValidationUtils.ValidateTypeKind(payloadTypeKind, expectedTypeKind, payloadTypeName);
            }
            serializationTypeNameAnnotation = null;
            if (!model.IsUserModel())
            {
                return null;
            }
            if ((expectedTypeReference == null) || flag)
            {
                return ResolveAndValidateTargetTypeWithNoExpectedType(expectedTypeKind, payloadType, payloadTypeName, out serializationTypeNameAnnotation);
            }
            if (messageReaderSettings.DisableStrictMetadataValidation)
            {
                return ResolveAndValidateTargetTypeStrictValidationDisabled(expectedTypeKind, expectedTypeReference, payloadType, payloadTypeName, out serializationTypeNameAnnotation);
            }
            return ResolveAndValidateTargetTypeStrictValidationEnabled(expectedTypeKind, expectedTypeReference, payloadType, payloadTypeName, out serializationTypeNameAnnotation);
        }

        internal static IEdmTypeReference ResolveAndValidatePrimitiveTargetType(IEdmTypeReference expectedTypeReference, EdmTypeKind payloadTypeKind, IEdmType payloadType, string payloadTypeName, IEdmType defaultPayloadType, IEdmModel model, ODataMessageReaderSettings messageReaderSettings, ODataVersion version)
        {
            bool flag = (messageReaderSettings.ReaderBehavior.TypeResolver != null) && (payloadType != null);
            if ((expectedTypeReference != null) && !flag)
            {
                ValidateTypeSupported(expectedTypeReference, version);
            }
            if ((payloadTypeKind != EdmTypeKind.None) && (messageReaderSettings.DisablePrimitiveTypeConversion || !messageReaderSettings.DisableStrictMetadataValidation))
            {
                ValidationUtils.ValidateTypeKind(payloadTypeKind, EdmTypeKind.Primitive, payloadTypeName);
            }
            if (!model.IsUserModel())
            {
                return GetNullablePayloadTypeReference(payloadType ?? defaultPayloadType);
            }
            if (((expectedTypeReference == null) || flag) || messageReaderSettings.DisablePrimitiveTypeConversion)
            {
                return GetNullablePayloadTypeReference(payloadType ?? defaultPayloadType);
            }
            if (!messageReaderSettings.DisableStrictMetadataValidation && ((payloadType != null) && !MetadataUtilsCommon.CanConvertPrimitiveTypeTo((IEdmPrimitiveType) payloadType, (IEdmPrimitiveType) expectedTypeReference.Definition)))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncompatibleType(payloadTypeName, expectedTypeReference.ODataFullName()));
            }
            return expectedTypeReference;
        }

        private static IEdmTypeReference ResolveAndValidateTargetTypeStrictValidationDisabled(EdmTypeKind expectedTypeKind, IEdmTypeReference expectedTypeReference, IEdmType payloadType, string payloadTypeName, out SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            switch (expectedTypeKind)
            {
                case EdmTypeKind.Entity:
                {
                    if (((payloadType == null) || (expectedTypeKind != payloadType.TypeKind)) || !expectedTypeReference.AsEntity().EntityDefinition().IsAssignableFrom(((IEdmEntityType) payloadType)))
                    {
                        break;
                    }
                    IEdmTypeReference targetTypeReference = payloadType.ToTypeReference(true);
                    serializationTypeNameAnnotation = CreateSerializationTypeNameAnnotation(payloadTypeName, targetTypeReference);
                    return targetTypeReference;
                }
                case EdmTypeKind.Complex:
                    if ((payloadType != null) && (expectedTypeKind == payloadType.TypeKind))
                    {
                        VerifyComplexType(expectedTypeReference, payloadType, false);
                    }
                    break;

                case EdmTypeKind.Collection:
                    if ((payloadType != null) && (expectedTypeKind == payloadType.TypeKind))
                    {
                        VerifyCollectionComplexItemType(expectedTypeReference, payloadType);
                    }
                    break;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ReaderValidationUtils_ResolveAndValidateTypeName_Strict_TypeKind));
            }
            serializationTypeNameAnnotation = CreateSerializationTypeNameAnnotation(payloadTypeName, expectedTypeReference);
            return expectedTypeReference;
        }

        private static IEdmTypeReference ResolveAndValidateTargetTypeStrictValidationEnabled(EdmTypeKind expectedTypeKind, IEdmTypeReference expectedTypeReference, IEdmType payloadType, string payloadTypeName, out SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            switch (expectedTypeKind)
            {
                case EdmTypeKind.Entity:
                {
                    if (payloadType == null)
                    {
                        break;
                    }
                    IEdmTypeReference targetTypeReference = payloadType.ToTypeReference(true);
                    ValidationUtils.ValidateEntityTypeIsAssignable((IEdmEntityTypeReference) expectedTypeReference, (IEdmEntityTypeReference) targetTypeReference);
                    serializationTypeNameAnnotation = CreateSerializationTypeNameAnnotation(payloadTypeName, targetTypeReference);
                    return targetTypeReference;
                }
                case EdmTypeKind.Complex:
                    if (payloadType != null)
                    {
                        VerifyComplexType(expectedTypeReference, payloadType, true);
                    }
                    break;

                case EdmTypeKind.Collection:
                    if ((payloadType != null) && (string.CompareOrdinal(payloadType.ODataFullName(), expectedTypeReference.ODataFullName()) != 0))
                    {
                        VerifyCollectionComplexItemType(expectedTypeReference, payloadType);
                        throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncompatibleType(payloadType.ODataFullName(), expectedTypeReference.ODataFullName()));
                    }
                    break;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ReaderValidationUtils_ResolveAndValidateTypeName_Strict_TypeKind));
            }
            serializationTypeNameAnnotation = CreateSerializationTypeNameAnnotation(payloadTypeName, expectedTypeReference);
            return expectedTypeReference;
        }

        private static IEdmTypeReference ResolveAndValidateTargetTypeWithNoExpectedType(EdmTypeKind expectedTypeKind, IEdmType payloadType, string payloadTypeName, out SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            serializationTypeNameAnnotation = null;
            if (payloadType == null)
            {
                if (expectedTypeKind == EdmTypeKind.Entity)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_EntryWithoutType);
                }
                throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_ValueWithoutType);
            }
            IEdmTypeReference targetTypeReference = payloadType.ToTypeReference(true);
            serializationTypeNameAnnotation = CreateSerializationTypeNameAnnotation(payloadTypeName, targetTypeReference);
            return targetTypeReference;
        }

        internal static IEdmType ResolvePayloadTypeName(IEdmModel model, IEdmTypeReference expectedTypeReference, string payloadTypeName, EdmTypeKind expectedTypeKind, ODataReaderBehavior readerBehavior, ODataVersion version, out EdmTypeKind payloadTypeKind)
        {
            if (payloadTypeName == null)
            {
                payloadTypeKind = EdmTypeKind.None;
                return null;
            }
            if (payloadTypeName.Length == 0)
            {
                payloadTypeKind = expectedTypeKind;
                return null;
            }
            IEdmType type = MetadataUtils.ResolveTypeNameForRead(model, (expectedTypeReference == null) ? null : expectedTypeReference.Definition, payloadTypeName, readerBehavior, version, out payloadTypeKind);
            if (payloadTypeKind == EdmTypeKind.None)
            {
                payloadTypeKind = expectedTypeKind;
            }
            return type;
        }

        internal static IEdmTypeReference ResolvePayloadTypeNameAndComputeTargetType(EdmTypeKind expectedTypeKind, IEdmType defaultPrimitivePayloadType, IEdmTypeReference expectedTypeReference, string payloadTypeName, IEdmModel model, ODataMessageReaderSettings messageReaderSettings, ODataVersion version, Func<EdmTypeKind> typeKindFromPayloadFunc, out EdmTypeKind targetTypeKind, out SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            EdmTypeKind kind;
            IEdmTypeReference reference;
            serializationTypeNameAnnotation = null;
            IEdmType payloadType = ResolvePayloadTypeName(model, expectedTypeReference, payloadTypeName, EdmTypeKind.Complex, messageReaderSettings.ReaderBehavior, version, out kind);
            targetTypeKind = ComputeTargetTypeKind(expectedTypeReference, expectedTypeKind == EdmTypeKind.Entity, payloadTypeName, kind, messageReaderSettings, typeKindFromPayloadFunc);
            if (targetTypeKind == EdmTypeKind.Primitive)
            {
                reference = ResolveAndValidatePrimitiveTargetType(expectedTypeReference, kind, payloadType, payloadTypeName, defaultPrimitivePayloadType, model, messageReaderSettings, version);
            }
            else
            {
                reference = ResolveAndValidateNonPrimitiveTargetType(targetTypeKind, expectedTypeReference, kind, payloadType, payloadTypeName, model, messageReaderSettings, version, out serializationTypeNameAnnotation);
            }
            if ((expectedTypeKind != EdmTypeKind.None) && (reference != null))
            {
                ValidationUtils.ValidateTypeKind(targetTypeKind, expectedTypeKind, payloadTypeName);
            }
            return reference;
        }

        private static bool ShouldValidatePayloadTypeKind(ODataMessageReaderSettings messageReaderSettings, IEdmTypeReference expectedValueTypeReference, EdmTypeKind payloadTypeKind)
        {
            bool flag = (messageReaderSettings.ReaderBehavior.TypeResolver != null) && (payloadTypeKind != EdmTypeKind.None);
            if (expectedValueTypeReference == null)
            {
                return false;
            }
            return ((!messageReaderSettings.DisableStrictMetadataValidation || flag) || (expectedValueTypeReference.IsODataPrimitiveTypeKind() && messageReaderSettings.DisablePrimitiveTypeConversion));
        }

        internal static void ValidateEncodingSupportedInBatch(Encoding encoding)
        {
            if (!encoding.IsSingleByte && (Encoding.UTF8.CodePage != encoding.CodePage))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataBatchReaderStream_MultiByteEncodingsNotSupported(encoding.WebName));
            }
        }

        internal static void ValidateEntityReferenceLink(ODataEntityReferenceLink link)
        {
            if (link.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_EntityReferenceLinkMissingUri);
            }
        }

        internal static void ValidateEntry(ODataEntry entry)
        {
        }

        internal static IEdmProperty ValidateLinkPropertyDefined(string propertyName, IEdmStructuredType owningStructuredType, ODataMessageReaderSettings messageReaderSettings)
        {
            if (owningStructuredType == null)
            {
                return null;
            }
            IEdmProperty property = FindDefinedProperty(propertyName, owningStructuredType, messageReaderSettings);
            if (((property == null) && !owningStructuredType.IsOpen) && !messageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.ReportUndeclaredLinkProperty))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, owningStructuredType.ODataFullName()));
            }
            return property;
        }

        internal static void ValidateMessageReaderSettings(ODataMessageReaderSettings messageReaderSettings, bool readingResponse)
        {
            if ((messageReaderSettings.BaseUri != null) && !messageReaderSettings.BaseUri.IsAbsoluteUri)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_MessageReaderSettingsBaseUriMustBeNullOrAbsolute(UriUtilsCommon.UriToString(messageReaderSettings.BaseUri)));
            }
            if (!readingResponse && (messageReaderSettings.UndeclaredPropertyBehaviorKinds != ODataUndeclaredPropertyBehaviorKinds.None))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedOnRequest);
            }
            if (!string.IsNullOrEmpty(messageReaderSettings.ReaderBehavior.ODataTypeScheme) && !string.Equals(messageReaderSettings.ReaderBehavior.ODataTypeScheme, "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"))
            {
                ODataVersionChecker.CheckCustomTypeScheme(messageReaderSettings.MaxProtocolVersion);
            }
            if (!string.IsNullOrEmpty(messageReaderSettings.ReaderBehavior.ODataNamespace) && !string.Equals(messageReaderSettings.ReaderBehavior.ODataNamespace, "http://schemas.microsoft.com/ado/2007/08/dataservices"))
            {
                ODataVersionChecker.CheckCustomDataNamespace(messageReaderSettings.MaxProtocolVersion);
            }
        }

        internal static IEdmNavigationProperty ValidateNavigationPropertyDefined(string propertyName, IEdmEntityType owningEntityType, ODataMessageReaderSettings messageReaderSettings)
        {
            if (owningEntityType == null)
            {
                return null;
            }
            IEdmProperty property = ValidateLinkPropertyDefined(propertyName, owningEntityType, messageReaderSettings);
            if (property == null)
            {
                if (owningEntityType.IsOpen)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_OpenNavigationProperty(propertyName, owningEntityType.ODataFullName()));
                }
            }
            else if (property.PropertyKind != EdmPropertyKind.Navigation)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NavigationPropertyExpected(propertyName, owningEntityType.ODataFullName(), property.PropertyKind.ToString()));
            }
            return (IEdmNavigationProperty) property;
        }

        internal static void ValidateNullValue(IEdmModel model, IEdmTypeReference expectedTypeReference, ODataMessageReaderSettings messageReaderSettings, bool validateNullValue, ODataVersion version)
        {
            if (expectedTypeReference != null)
            {
                ValidateTypeSupported(expectedTypeReference, version);
                if (!messageReaderSettings.DisablePrimitiveTypeConversion || (expectedTypeReference.TypeKind() != EdmTypeKind.Primitive))
                {
                    ValidateNullValueAllowed(expectedTypeReference, validateNullValue, model);
                }
            }
        }

        private static void ValidateNullValueAllowed(IEdmTypeReference expectedValueTypeReference, bool validateNullValue, IEdmModel model)
        {
            if (validateNullValue && (expectedValueTypeReference != null))
            {
                if (expectedValueTypeReference.IsODataPrimitiveTypeKind())
                {
                    if (!expectedValueTypeReference.IsNullable)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_NullValueForNonNullableType(expectedValueTypeReference.ODataFullName()));
                    }
                }
                else
                {
                    if (expectedValueTypeReference.IsNonEntityODataCollectionTypeKind())
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_NullValueForNonNullableType(expectedValueTypeReference.ODataFullName()));
                    }
                    if ((expectedValueTypeReference.IsODataComplexTypeKind() && ValidationUtils.ShouldValidateComplexPropertyNullValue(model)) && !expectedValueTypeReference.AsComplex().IsNullable)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_NullValueForNonNullableType(expectedValueTypeReference.ODataFullName()));
                    }
                }
            }
        }

        internal static void ValidateStreamReferenceProperty(ODataProperty streamProperty, IEdmStructuredType structuredType, IEdmProperty streamEdmProperty)
        {
            ValidationUtils.ValidateStreamReferenceProperty(streamProperty, streamEdmProperty);
            if (((structuredType != null) && structuredType.IsOpen) && (streamEdmProperty == null))
            {
                ValidationUtils.ValidateOpenPropertyValue(streamProperty.Name, streamProperty.Value);
            }
        }

        internal static void ValidateTypeSupported(IEdmTypeReference typeReference, ODataVersion version)
        {
            if (typeReference != null)
            {
                if (typeReference.IsNonEntityODataCollectionTypeKind())
                {
                    ODataVersionChecker.CheckCollectionValue(version);
                }
                else if (typeReference.IsSpatial())
                {
                    ODataVersionChecker.CheckSpatialValue(version);
                }
            }
        }

        internal static IEdmProperty ValidateValuePropertyDefined(string propertyName, IEdmStructuredType owningStructuredType, ODataMessageReaderSettings messageReaderSettings, out bool ignoreProperty)
        {
            ignoreProperty = false;
            if (owningStructuredType == null)
            {
                return null;
            }
            IEdmProperty property = FindDefinedProperty(propertyName, owningStructuredType, messageReaderSettings);
            if ((property == null) && !owningStructuredType.IsOpen)
            {
                if (!messageReaderSettings.UndeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, owningStructuredType.ODataFullName()));
                }
                ignoreProperty = true;
            }
            return property;
        }

        private static void VerifyCollectionComplexItemType(IEdmTypeReference expectedTypeReference, IEdmType payloadType)
        {
            IEdmTypeReference collectionItemType = ValidationUtils.ValidateCollectionType(expectedTypeReference).GetCollectionItemType();
            if ((collectionItemType != null) && collectionItemType.IsODataComplexTypeKind())
            {
                IEdmTypeReference typeReference = ValidationUtils.ValidateCollectionType(payloadType.ToTypeReference()).GetCollectionItemType();
                if ((typeReference != null) && typeReference.IsODataComplexTypeKind())
                {
                    VerifyComplexType(collectionItemType, typeReference.Definition, false);
                }
            }
        }

        private static void VerifyComplexType(IEdmTypeReference expectedTypeReference, IEdmType payloadType, bool failIfNotRelated)
        {
            IEdmStructuredType thisType = expectedTypeReference.AsStructured().StructuredDefinition();
            IEdmStructuredType otherType = (IEdmStructuredType) payloadType;
            if (!thisType.IsEquivalentTo(otherType))
            {
                if (thisType.IsAssignableFrom(otherType))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ReaderValidationUtils_DerivedComplexTypesAreNotAllowed(thisType.ODataFullName(), otherType.ODataFullName()));
                }
                if (failIfNotRelated)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncompatibleType(otherType.ODataFullName(), thisType.ODataFullName()));
                }
            }
        }

        private static void VerifyPayloadTypeDefined(string payloadTypeName, IEdmType payloadType)
        {
            if ((payloadTypeName != null) && (payloadType == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_UnrecognizedTypeName(payloadTypeName));
            }
        }
    }
}

