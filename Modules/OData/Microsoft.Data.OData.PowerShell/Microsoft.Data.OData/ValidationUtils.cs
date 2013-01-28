namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Csdl;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.Linq;

    internal static class ValidationUtils
    {
        internal static readonly char[] InvalidCharactersInPropertyNames = new char[] { ':', '.', '@' };
        private const int MaxBoundaryLength = 70;

        internal static void IncreaseAndValidateRecursionDepth(ref int recursionDepth, int maxDepth)
        {
            recursionDepth++;
            if (recursionDepth > maxDepth)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_RecursionDepthLimitReached(maxDepth));
            }
        }

        internal static bool IsValidPropertyName(string propertyName)
        {
            return (propertyName.IndexOfAny(InvalidCharactersInPropertyNames) < 0);
        }

        internal static bool ShouldValidateComplexPropertyNullValue(IEdmModel model)
        {
            Version edmVersion = model.GetEdmVersion();
            Version dataServiceVersion = model.GetDataServiceVersion();
            if (((edmVersion != null) && (dataServiceVersion != null)) && (edmVersion < ODataVersion.V3.ToDataServiceVersion()))
            {
                return false;
            }
            return true;
        }

        internal static void ValidateAssociationLink(ODataAssociationLink associationLink)
        {
            ValidateAssociationLinkName(associationLink.Name);
            if (associationLink.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_AssociationLinkMustSpecifyUrl);
            }
        }

        internal static void ValidateAssociationLinkName(string associationLinkName)
        {
            if (string.IsNullOrEmpty(associationLinkName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_AssociationLinkMustSpecifyName);
            }
        }

        internal static void ValidateAssociationLinkNotNull(ODataAssociationLink associationLink)
        {
            if (associationLink == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_EnumerableContainsANullItem("ODataEntry.AssociationLinks"));
            }
        }

        internal static void ValidateBoundaryString(string boundary)
        {
            if (((boundary == null) || (boundary.Length == 0)) || (boundary.Length > 70))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_InvalidBatchBoundaryDelimiterLength(boundary, 70));
            }
        }

        internal static void ValidateCollectionItem(object item, bool isStreamable)
        {
            if (!isStreamable && (item == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NonStreamingCollectionElementsMustNotBeNull);
            }
            if (item is ODataCollectionValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NestedCollectionsAreNotSupported);
            }
            if (item is ODataStreamReferenceValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_StreamReferenceValuesNotSupportedInCollections);
            }
        }

        internal static IEdmCollectionTypeReference ValidateCollectionType(IEdmTypeReference typeReference)
        {
            IEdmCollectionTypeReference reference = typeReference.AsCollectionOrNull();
            if ((reference != null) && !typeReference.IsNonEntityODataCollectionTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_InvalidCollectionTypeReference(typeReference.TypeKind()));
            }
            return reference;
        }

        internal static string ValidateCollectionTypeName(string collectionTypeName)
        {
            string collectionItemTypeName = EdmLibraryExtensions.GetCollectionItemTypeName(collectionTypeName);
            if (collectionItemTypeName == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_InvalidCollectionTypeName(collectionTypeName));
            }
            return collectionItemTypeName;
        }

        internal static void ValidateEntityTypeIsAssignable(IEdmEntityTypeReference expectedEntityTypeReference, IEdmEntityTypeReference payloadEntityTypeReference)
        {
            if (!expectedEntityTypeReference.EntityDefinition().IsAssignableFrom(payloadEntityTypeReference.EntityDefinition()))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_EntryTypeNotAssignableToExpectedType(payloadEntityTypeReference.ODataFullName(), expectedEntityTypeReference.ODataFullName()));
            }
        }

        internal static IEdmEntityType ValidateEntityTypeName(IEdmModel model, string typeName)
        {
            IEdmType actualType = ValidateTypeName(model, typeName);
            if (actualType != null)
            {
                ValidateTypeKind(actualType, EdmTypeKind.Entity);
            }
            return (IEdmEntityType) actualType;
        }

        internal static void ValidateEntryMetadata(ODataEntry entry, IEdmEntityType entityType, IEdmModel model, bool validateMediaResource)
        {
            if ((entityType != null) && validateMediaResource)
            {
                bool flag = model.HasDefaultStream(entityType);
                if (entry.MediaResource == null)
                {
                    if (flag)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_EntryWithoutMediaResourceAndMLEType(entityType.ODataFullName()));
                    }
                }
                else if (!flag)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_EntryWithMediaResourceAndNonMLEType(entityType.ODataFullName()));
                }
            }
        }

        internal static void ValidateIsExpectedPrimitiveType(object value, IEdmTypeReference expectedTypeReference)
        {
            IEdmPrimitiveTypeReference primitiveTypeReference = EdmLibraryExtensions.GetPrimitiveTypeReference(value.GetType());
            ValidateIsExpectedPrimitiveType(value, primitiveTypeReference, expectedTypeReference);
        }

        internal static void ValidateIsExpectedPrimitiveType(object value, IEdmPrimitiveTypeReference valuePrimitiveTypeReference, IEdmTypeReference expectedTypeReference)
        {
            if (valuePrimitiveTypeReference == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_UnsupportedPrimitiveType(value.GetType().FullName));
            }
            if (!expectedTypeReference.IsODataPrimitiveTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NonPrimitiveTypeForPrimitiveValue(expectedTypeReference.ODataFullName()));
            }
            ValidateMetadataPrimitiveType(expectedTypeReference, valuePrimitiveTypeReference);
        }

        internal static void ValidateMetadataPrimitiveType(IEdmTypeReference expectedTypeReference, IEdmTypeReference typeReferenceFromValue)
        {
            IEdmPrimitiveType definition = (IEdmPrimitiveType) expectedTypeReference.Definition;
            IEdmPrimitiveType subtype = (IEdmPrimitiveType) typeReferenceFromValue.Definition;
            bool flag = ((expectedTypeReference.IsNullable == typeReferenceFromValue.IsNullable) || (expectedTypeReference.IsNullable && !typeReferenceFromValue.IsNullable)) || !typeReferenceFromValue.IsODataValueType();
            bool flag2 = definition.IsAssignableFrom(subtype);
            if (!flag || !flag2)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncompatiblePrimitiveItemType(typeReferenceFromValue.ODataFullName(), typeReferenceFromValue.IsNullable, expectedTypeReference.ODataFullName(), expectedTypeReference.IsNullable));
            }
        }

        internal static void ValidateNullCollectionItem(IEdmTypeReference expectedItemType, ODataWriterBehavior writerBehavior)
        {
            if (((expectedItemType != null) && expectedItemType.IsODataPrimitiveTypeKind()) && (!expectedItemType.IsNullable && !writerBehavior.AllowNullValuesForNonNullablePrimitiveTypes))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_NullCollectionItemForNonNullableType(expectedItemType.ODataFullName()));
            }
        }

        internal static void ValidateOpenPropertyValue(string propertyName, object value)
        {
            if (value is ODataCollectionValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_OpenCollectionProperty(propertyName));
            }
            if (value is ODataStreamReferenceValue)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_OpenStreamProperty(propertyName));
            }
        }

        internal static void ValidateOperation(ODataOperation operation)
        {
            if (operation.Metadata == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_ActionsAndFunctionsMustSpecifyMetadata(operation.GetType().Name));
            }
            if (operation.Target == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_ActionsAndFunctionsMustSpecifyTarget(operation.GetType().Name));
            }
        }

        internal static void ValidateOperationNotNull(ODataOperation operation, bool isAction)
        {
            if (operation == null)
            {
                string str = isAction ? "ODataEntry.Actions" : "ODataEntry.Functions";
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_EnumerableContainsANullItem(str));
            }
        }

        internal static void ValidatePropertyName(string propertyName)
        {
            if (!IsValidPropertyName(propertyName))
            {
                string str = string.Join(", ", (from c in InvalidCharactersInPropertyNames select string.Format(CultureInfo.InvariantCulture, "'{0}'", new object[] { c })).ToArray<string>());
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_PropertiesMustNotContainReservedChars(propertyName, str));
            }
        }

        internal static void ValidateResourceCollectionInfo(ODataResourceCollectionInfo collectionInfo)
        {
            if (collectionInfo == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_WorkspaceCollectionsMustNotContainNullItem);
            }
            if (collectionInfo.Url == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_ResourceCollectionMustSpecifyUrl);
            }
        }

        internal static void ValidateResourceCollectionInfoUrl(string collectionInfoUrl)
        {
            if (collectionInfoUrl == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_ResourceCollectionUrlMustNotBeNull);
            }
        }

        internal static void ValidateStreamReferenceProperty(ODataProperty streamProperty, IEdmProperty edmProperty)
        {
            if ((edmProperty != null) && !edmProperty.Type.IsStream())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_MismatchPropertyKindForStreamProperty(streamProperty.Name));
            }
        }

        internal static int ValidateTotalEntityPropertyMappingCount(ODataEntityPropertyMappingCache baseCache, ODataEntityPropertyMappingCollection mappings, int maxMappingCount)
        {
            int num = (baseCache == null) ? 0 : baseCache.TotalMappingCount;
            int num2 = (mappings == null) ? 0 : mappings.Count;
            int num3 = num + num2;
            if (num3 > maxMappingCount)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_MaxNumberOfEntityPropertyMappingsExceeded(num3, maxMappingCount));
            }
            return num3;
        }

        internal static void ValidateTypeKind(IEdmType actualType, EdmTypeKind expectedTypeKind)
        {
            ValidateTypeKind(actualType.TypeKind, expectedTypeKind, actualType.ODataFullName());
        }

        internal static void ValidateTypeKind(EdmTypeKind actualTypeKind, EdmTypeKind expectedTypeKind, string typeName)
        {
            if (actualTypeKind != expectedTypeKind)
            {
                if (typeName == null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncorrectTypeKindNoTypeName(actualTypeKind.ToString(), expectedTypeKind.ToString()));
                }
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncorrectTypeKind(typeName, expectedTypeKind.ToString(), actualTypeKind.ToString()));
            }
        }

        internal static IEdmType ValidateTypeName(IEdmModel model, string typeName)
        {
            if (typeName.Length == 0)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_TypeNameMustNotBeEmpty);
            }
            IEdmType type = MetadataUtils.ResolveTypeNameForWrite(model, typeName);
            if (model.IsUserModel() && (type == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_UnrecognizedTypeName(typeName));
            }
            return type;
        }

        internal static void ValidateValueTypeKind(EdmTypeKind typeKind, string typeName)
        {
            if (((typeKind != EdmTypeKind.Primitive) && (typeKind != EdmTypeKind.Complex)) && (typeKind != EdmTypeKind.Collection))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ValidationUtils_IncorrectValueTypeKind(typeName, typeKind.ToString()));
            }
        }

        internal static IEdmType ValidateValueTypeName(IEdmModel model, string typeName, EdmTypeKind typeKind)
        {
            IEdmType actualType = ValidateTypeName(model, typeName);
            if (actualType != null)
            {
                ValidateTypeKind(actualType, typeKind);
            }
            return actualType;
        }
    }
}

