namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal static class WriterValidationUtils
    {
        internal static IEdmTypeReference ResolveTypeNameForWriting(IEdmModel model, IEdmTypeReference typeReferenceFromMetadata, ref string typeName, EdmTypeKind typeKindFromValue, bool isOpenPropertyType)
        {
            IEdmType type = ValidateValueTypeName(model, typeName, typeKindFromValue, isOpenPropertyType);
            IEdmTypeReference typeReferenceFromValue = type.ToTypeReference();
            if (typeReferenceFromMetadata != null)
            {
                ValidationUtils.ValidateTypeKind(typeKindFromValue, typeReferenceFromMetadata.TypeKind(), (type == null) ? null : type.ODataFullName());
            }
            typeReferenceFromValue = ValidateMetadataType(typeReferenceFromMetadata, typeReferenceFromValue);
            if ((typeKindFromValue == EdmTypeKind.Collection) && (typeReferenceFromValue != null))
            {
                typeReferenceFromValue = ValidationUtils.ValidateCollectionType(typeReferenceFromValue);
            }
            if ((typeName == null) && (typeReferenceFromValue != null))
            {
                typeName = typeReferenceFromValue.ODataFullName();
            }
            return typeReferenceFromValue;
        }

        internal static void ValidateAssociationLink(ODataAssociationLink associationLink, ODataVersion version, bool writingResponse)
        {
            ODataVersionChecker.CheckAssociationLinks(version);
            if (!writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_AssociationLinkInRequest(associationLink.Name));
            }
            ValidationUtils.ValidateAssociationLink(associationLink);
        }

        internal static void ValidateEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink)
        {
            if (entityReferenceLink.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_EntityReferenceLinkUrlMustNotBeNull);
            }
        }

        internal static void ValidateEntityReferenceLinkNotNull(ODataEntityReferenceLink entityReferenceLink)
        {
            if (entityReferenceLink == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_EntityReferenceLinksLinkMustNotBeNull);
            }
        }

        internal static IEdmEntityType ValidateEntityTypeName(IEdmModel model, string typeName)
        {
            if (typeName != null)
            {
                return ValidationUtils.ValidateEntityTypeName(model, typeName);
            }
            if (model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_MissingTypeNameWithMetadata);
            }
            return null;
        }

        internal static void ValidateEntryAtEnd(ODataEntry entry)
        {
            ValidateEntryId(entry.Id);
        }

        internal static void ValidateEntryAtStart(ODataEntry entry)
        {
            ValidateEntryId(entry.Id);
        }

        private static void ValidateEntryId(string id)
        {
            if ((id != null) && (id.Length == 0))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_EntriesMustHaveNonEmptyId);
            }
        }

        internal static void ValidateEntryInExpandedLink(IEdmEntityType entryEntityType, IEdmType parentNavigationPropertyType)
        {
            if (parentNavigationPropertyType != null)
            {
                IEdmEntityType baseType = (parentNavigationPropertyType.TypeKind == EdmTypeKind.Collection) ? ((IEdmEntityType) ((IEdmCollectionType) parentNavigationPropertyType).ElementType.Definition) : ((IEdmEntityType) parentNavigationPropertyType);
                if (!baseType.IsAssignableFrom(entryEntityType))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_EntryTypeInExpandedLinkNotCompatibleWithNavigationPropertyType(entryEntityType.ODataFullName(), baseType.ODataFullName()));
                }
            }
        }

        internal static void ValidateFeedAtEnd(ODataFeed feed, bool writingRequest, ODataVersion version)
        {
            if (feed.NextPageLink != null)
            {
                if (writingRequest)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_NextPageLinkInRequest);
                }
                ODataVersionChecker.CheckNextLink(version);
            }
        }

        internal static void ValidateFeedAtStart(ODataFeed feed)
        {
            if (string.IsNullOrEmpty(feed.Id))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_FeedsMustHaveNonEmptyId);
            }
        }

        internal static void ValidateMessageWriterSettings(ODataMessageWriterSettings messageWriterSettings)
        {
            if ((messageWriterSettings.BaseUri != null) && !messageWriterSettings.BaseUri.IsAbsoluteUri)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_MessageWriterSettingsBaseUriMustBeNullOrAbsolute(UriUtilsCommon.UriToString(messageWriterSettings.BaseUri)));
            }
        }

        private static IEdmTypeReference ValidateMetadataType(IEdmTypeReference typeReferenceFromMetadata, IEdmTypeReference typeReferenceFromValue)
        {
            if (typeReferenceFromMetadata != null)
            {
                if (typeReferenceFromValue == null)
                {
                    return typeReferenceFromMetadata;
                }
                EdmTypeKind expectedTypeKind = typeReferenceFromMetadata.TypeKind();
                ValidationUtils.ValidateTypeKind(typeReferenceFromValue.Definition, expectedTypeKind);
                if (typeReferenceFromValue.IsODataPrimitiveTypeKind())
                {
                    ValidationUtils.ValidateMetadataPrimitiveType(typeReferenceFromMetadata, typeReferenceFromValue);
                    return typeReferenceFromValue;
                }
                if (expectedTypeKind == EdmTypeKind.Entity)
                {
                    ValidationUtils.ValidateEntityTypeIsAssignable((IEdmEntityTypeReference) typeReferenceFromMetadata, (IEdmEntityTypeReference) typeReferenceFromValue);
                    return typeReferenceFromValue;
                }
                if (typeReferenceFromMetadata.ODataFullName() != typeReferenceFromValue.ODataFullName())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncompatibleType(typeReferenceFromValue.ODataFullName(), typeReferenceFromMetadata.ODataFullName()));
                }
            }
            return typeReferenceFromValue;
        }

        internal static IEdmType ValidateNavigationLink(ODataNavigationLink navigationLink, IEdmEntityType declaringEntityType, ODataPayloadKind? expandedPayloadKind)
        {
            if (string.IsNullOrEmpty(navigationLink.Name))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_LinkMustSpecifyName);
            }
            bool flag = ((ODataPayloadKind) expandedPayloadKind) == ODataPayloadKind.EntityReferenceLink;
            bool flag2 = ((ODataPayloadKind) expandedPayloadKind) == ODataPayloadKind.Feed;
            Func<object, string> func = null;
            if ((!flag && navigationLink.IsCollection.HasValue) && (expandedPayloadKind.HasValue && (flag2 != navigationLink.IsCollection.Value)))
            {
                func = (((ODataPayloadKind) expandedPayloadKind.Value) == ODataPayloadKind.Feed) ? new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedContent) : new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryContent);
            }
            IEdmType definition = null;
            if (declaringEntityType != null)
            {
                definition = ValidateNavigationPropertyDefined(navigationLink.Name, declaringEntityType).Type.Definition;
                bool flag3 = definition.TypeKind == EdmTypeKind.Collection;
                if (navigationLink.IsCollection.HasValue)
                {
                    bool flag4 = flag3;
                    if ((flag4 != navigationLink.IsCollection) && ((navigationLink.IsCollection != false) || !flag))
                    {
                        func = flag3 ? new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedMetadata) : new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryMetadata);
                    }
                }
                if ((!flag && expandedPayloadKind.HasValue) && (flag3 != flag2))
                {
                    func = flag3 ? new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkWithEntryPayloadAndFeedMetadata) : new Func<object, string>(Microsoft.Data.OData.Strings.WriterValidationUtils_ExpandedLinkWithFeedPayloadAndEntryMetadata);
                }
            }
            if (func != null)
            {
                string arg = (navigationLink.Url == null) ? "null" : UriUtilsCommon.UriToString(navigationLink.Url);
                throw new ODataException(func(arg));
            }
            return definition;
        }

        internal static IEdmProperty ValidateNavigationPropertyDefined(string propertyName, IEdmEntityType owningEntityType)
        {
            if (owningEntityType == null)
            {
                return null;
            }
            IEdmProperty property = ValidatePropertyDefined(propertyName, owningEntityType);
            if (property == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_OpenNavigationProperty(propertyName, owningEntityType.ODataFullName()));
            }
            if (property.PropertyKind != EdmPropertyKind.Navigation)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NavigationPropertyExpected(propertyName, owningEntityType.ODataFullName(), property.PropertyKind.ToString()));
            }
            return property;
        }

        internal static void ValidateNullPropertyValue(IEdmProperty expectedProperty, ODataWriterBehavior writerBehavior, IEdmModel model)
        {
            if (expectedProperty != null)
            {
                IEdmTypeReference type = expectedProperty.Type;
                if (type.IsNonEntityODataCollectionTypeKind())
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_CollectionPropertiesMustNotHaveNullValue(expectedProperty.Name));
                }
                if (type.IsODataPrimitiveTypeKind())
                {
                    if (!type.IsNullable && !writerBehavior.AllowNullValuesForNonNullablePrimitiveTypes)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue(expectedProperty.Name, expectedProperty.Type.ODataFullName()));
                    }
                }
                else
                {
                    if (type.IsStream())
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_StreamPropertiesMustNotHaveNullValue(expectedProperty.Name));
                    }
                    if ((type.IsODataComplexTypeKind() && ValidationUtils.ShouldValidateComplexPropertyNullValue(model)) && !type.AsComplex().IsNullable)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue(expectedProperty.Name, expectedProperty.Type.ODataFullName()));
                    }
                }
            }
        }

        internal static void ValidateOperation(ODataOperation operation, bool writingResponse)
        {
            if (!writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_OperationInRequest(operation.Metadata));
            }
            ValidationUtils.ValidateOperation(operation);
        }

        internal static void ValidateProperty(ODataProperty property)
        {
            string name = property.Name;
            if (string.IsNullOrEmpty(name))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_PropertiesMustHaveNonEmptyName);
            }
            ValidationUtils.ValidatePropertyName(name);
        }

        internal static IEdmProperty ValidatePropertyDefined(string propertyName, IEdmStructuredType owningStructuredType)
        {
            if (owningStructuredType == null)
            {
                return null;
            }
            IEdmProperty property = owningStructuredType.FindProperty(propertyName);
            if (!owningStructuredType.IsOpen && (property == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, owningStructuredType.ODataFullName()));
            }
            return property;
        }

        internal static void ValidatePropertyNotNull(ODataProperty property)
        {
            if (property == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_PropertyMustNotBeNull);
            }
        }

        internal static void ValidateStreamReferenceProperty(ODataProperty streamProperty, IEdmProperty edmProperty, ODataVersion version, bool writingResponse)
        {
            ODataVersionChecker.CheckStreamReferenceProperty(version);
            ValidationUtils.ValidateStreamReferenceProperty(streamProperty, edmProperty);
            if (!writingResponse)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_StreamPropertyInRequest(streamProperty.Name));
            }
            ODataStreamReferenceValue streamReference = (ODataStreamReferenceValue) streamProperty.Value;
            ValidateStreamReferenceValue(streamReference, false);
        }

        internal static void ValidateStreamReferenceValue(ODataStreamReferenceValue streamReference, bool isDefaultStream)
        {
            if ((streamReference.ContentType != null) && (streamReference.ContentType.Length == 0))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_StreamReferenceValueEmptyContentType);
            }
            if ((isDefaultStream && (streamReference.ReadLink == null)) && (streamReference.ContentType != null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_DefaultStreamWithContentTypeWithoutReadLink);
            }
            if ((isDefaultStream && (streamReference.ReadLink != null)) && (streamReference.ContentType == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_DefaultStreamWithReadLinkWithoutContentType);
            }
            if (((streamReference.EditLink == null) && (streamReference.ReadLink == null)) && !isDefaultStream)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_StreamReferenceValueMustHaveEditLinkOrReadLink);
            }
            if ((streamReference.EditLink == null) && (streamReference.ETag != null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_StreamReferenceValueMustHaveEditLinkToHaveETag);
            }
        }

        internal static IEdmType ValidateValueTypeName(IEdmModel model, string typeName, EdmTypeKind typeKind, bool isOpenPropertyType)
        {
            if (typeName != null)
            {
                return ValidationUtils.ValidateValueTypeName(model, typeName, typeKind);
            }
            if (model.IsUserModel() && isOpenPropertyType)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.WriterValidationUtils_MissingTypeNameWithMetadata);
            }
            return null;
        }
    }
}

