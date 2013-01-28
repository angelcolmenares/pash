namespace Microsoft.Data.OData
{
    using System;

    internal static class Strings
    {
        internal static string AtomValueUtils_CannotConvertValueToAtomPrimitive(object p0)
        {
            return TextRes.GetString("AtomValueUtils_CannotConvertValueToAtomPrimitive", new object[] { p0 });
        }

        internal static string CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeKind(object p0, object p1)
        {
            return TextRes.GetString("CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeKind", new object[] { p0, p1 });
        }

        internal static string CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeName(object p0, object p1)
        {
            return TextRes.GetString("CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeName", new object[] { p0, p1 });
        }

        internal static string CollectionWithoutExpectedTypeValidator_InvalidItemTypeKind(object p0)
        {
            return TextRes.GetString("CollectionWithoutExpectedTypeValidator_InvalidItemTypeKind", new object[] { p0 });
        }

        internal static string DuplicatePropertyNamesChecker_DuplicatePropertyNamesNotAllowed(object p0)
        {
            return TextRes.GetString("DuplicatePropertyNamesChecker_DuplicatePropertyNamesNotAllowed", new object[] { p0 });
        }

        internal static string DuplicatePropertyNamesChecker_MultipleLinksForSingleton(object p0)
        {
            return TextRes.GetString("DuplicatePropertyNamesChecker_MultipleLinksForSingleton", new object[] { p0 });
        }

        internal static string EntityPropertyMapping_EpmAttribute(object p0)
        {
            return TextRes.GetString("EntityPropertyMapping_EpmAttribute", new object[] { p0 });
        }

        internal static string EntityPropertyMapping_InvalidTargetPath(object p0)
        {
            return TextRes.GetString("EntityPropertyMapping_InvalidTargetPath", new object[] { p0 });
        }

        internal static string EntityPropertyMapping_TargetNamespaceUriNotValid(object p0)
        {
            return TextRes.GetString("EntityPropertyMapping_TargetNamespaceUriNotValid", new object[] { p0 });
        }

        internal static string EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmExtensionMethods_AttributeNotAllowedForAtomPubMappingOnType", new object[] { p0, p1 });
        }

        internal static string EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmExtensionMethods_AttributeNotAllowedForCustomMappingOnType", new object[] { p0, p1 });
        }

        internal static string EpmExtensionMethods_CannotConvertEdmAnnotationValue(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_CannotConvertEdmAnnotationValue", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_InvalidKeepInContentOnProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_InvalidKeepInContentOnProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_InvalidKeepInContentOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmExtensionMethods_InvalidKeepInContentOnType", new object[] { p0, p1 });
        }

        internal static string EpmExtensionMethods_InvalidTargetTextContentKindOnProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_InvalidTargetTextContentKindOnProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_InvalidTargetTextContentKindOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmExtensionMethods_InvalidTargetTextContentKindOnType", new object[] { p0, p1 });
        }

        internal static string EpmExtensionMethods_MissingAttributeOnProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmExtensionMethods_MissingAttributeOnProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmExtensionMethods_MissingAttributeOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmExtensionMethods_MissingAttributeOnType", new object[] { p0, p1 });
        }

        internal static string EpmReader_OpenComplexOrCollectionEpmProperty(object p0)
        {
            return TextRes.GetString("EpmReader_OpenComplexOrCollectionEpmProperty", new object[] { p0 });
        }

        internal static string EpmSourceTree_CollectionPropertyCannotBeMapped(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_CollectionPropertyCannotBeMapped", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_DuplicateEpmAttributesWithSameSourceName(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_DuplicateEpmAttributesWithSameSourceName", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_EndsWithNonPrimitiveType(object p0)
        {
            return TextRes.GetString("EpmSourceTree_EndsWithNonPrimitiveType", new object[] { p0 });
        }

        internal static string EpmSourceTree_InvalidSourcePath(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_InvalidSourcePath", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_MissingPropertyOnInstance(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_MissingPropertyOnInstance", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_MissingPropertyOnType(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_MissingPropertyOnType", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_OpenComplexPropertyCannotBeMapped(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_OpenComplexPropertyCannotBeMapped", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_OpenPropertySpatialTypeCannotBeMapped(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_OpenPropertySpatialTypeCannotBeMapped", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_SpatialTypeCannotBeMapped(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_SpatialTypeCannotBeMapped", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_StreamPropertyCannotBeMapped(object p0, object p1)
        {
            return TextRes.GetString("EpmSourceTree_StreamPropertyCannotBeMapped", new object[] { p0, p1 });
        }

        internal static string EpmSourceTree_TraversalOfNonComplexType(object p0)
        {
            return TextRes.GetString("EpmSourceTree_TraversalOfNonComplexType", new object[] { p0 });
        }

        internal static string EpmSyndicationReader_MultipleValuesForNonCollectionProperty(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmSyndicationReader_MultipleValuesForNonCollectionProperty", new object[] { p0, p1, p2 });
        }

        internal static string EpmSyndicationWriter_DateTimePropertyCanNotBeConverted(object p0)
        {
            return TextRes.GetString("EpmSyndicationWriter_DateTimePropertyCanNotBeConverted", new object[] { p0 });
        }

        internal static string EpmSyndicationWriter_EmptyCollectionMappedToAuthor(object p0)
        {
            return TextRes.GetString("EpmSyndicationWriter_EmptyCollectionMappedToAuthor", new object[] { p0 });
        }

        internal static string EpmSyndicationWriter_InvalidLinkLengthValue(object p0)
        {
            return TextRes.GetString("EpmSyndicationWriter_InvalidLinkLengthValue", new object[] { p0 });
        }

        internal static string EpmSyndicationWriter_InvalidValueForCategorySchemeCriteriaAttribute(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmSyndicationWriter_InvalidValueForCategorySchemeCriteriaAttribute", new object[] { p0, p1, p2 });
        }

        internal static string EpmSyndicationWriter_InvalidValueForLinkRelCriteriaAttribute(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmSyndicationWriter_InvalidValueForLinkRelCriteriaAttribute", new object[] { p0, p1, p2 });
        }

        internal static string EpmSyndicationWriter_NullValueForAttributeTarget(object p0, object p1, object p2)
        {
            return TextRes.GetString("EpmSyndicationWriter_NullValueForAttributeTarget", new object[] { p0, p1, p2 });
        }

        internal static string EpmTargetTree_AttributeInMiddle(object p0)
        {
            return TextRes.GetString("EpmTargetTree_AttributeInMiddle", new object[] { p0 });
        }

        internal static string EpmTargetTree_DuplicateEpmAttributesWithSameTargetName(object p0, object p1, object p2, object p3)
        {
            return TextRes.GetString("EpmTargetTree_DuplicateEpmAttributesWithSameTargetName", new object[] { p0, p1, p2, p3 });
        }

        internal static string EpmTargetTree_InvalidTargetPath_EmptySegment(object p0)
        {
            return TextRes.GetString("EpmTargetTree_InvalidTargetPath_EmptySegment", new object[] { p0 });
        }

        internal static string EpmTargetTree_InvalidTargetPath_MixedContent(object p0, object p1)
        {
            return TextRes.GetString("EpmTargetTree_InvalidTargetPath_MixedContent", new object[] { p0, p1 });
        }

        internal static string ExceptionUtils_CheckIntegerNotNegative(object p0)
        {
            return TextRes.GetString("ExceptionUtils_CheckIntegerNotNegative", new object[] { p0 });
        }

        internal static string ExceptionUtils_CheckIntegerPositive(object p0)
        {
            return TextRes.GetString("ExceptionUtils_CheckIntegerPositive", new object[] { p0 });
        }

        internal static string ExceptionUtils_CheckLongPositive(object p0)
        {
            return TextRes.GetString("ExceptionUtils_CheckLongPositive", new object[] { p0 });
        }

        internal static string ExpressionLexer_DigitExpected(object p0, object p1)
        {
            return TextRes.GetString("ExpressionLexer_DigitExpected", new object[] { p0, p1 });
        }

        internal static string ExpressionLexer_ExpectedLiteralToken(object p0)
        {
            return TextRes.GetString("ExpressionLexer_ExpectedLiteralToken", new object[] { p0 });
        }

        internal static string ExpressionLexer_InvalidCharacter(object p0, object p1, object p2)
        {
            return TextRes.GetString("ExpressionLexer_InvalidCharacter", new object[] { p0, p1, p2 });
        }

        internal static string ExpressionLexer_SyntaxError(object p0, object p1)
        {
            return TextRes.GetString("ExpressionLexer_SyntaxError", new object[] { p0, p1 });
        }

        internal static string ExpressionLexer_UnterminatedLiteral(object p0, object p1)
        {
            return TextRes.GetString("ExpressionLexer_UnterminatedLiteral", new object[] { p0, p1 });
        }

        internal static string ExpressionLexer_UnterminatedStringLiteral(object p0, object p1)
        {
            return TextRes.GetString("ExpressionLexer_UnterminatedStringLiteral", new object[] { p0, p1 });
        }

        internal static string ExpressionToken_IdentifierExpected(object p0)
        {
            return TextRes.GetString("ExpressionToken_IdentifierExpected", new object[] { p0 });
        }

        internal static string FeedWithoutExpectedTypeValidator_IncompatibleTypes(object p0, object p1)
        {
            return TextRes.GetString("FeedWithoutExpectedTypeValidator_IncompatibleTypes", new object[] { p0, p1 });
        }

        internal static string General_InternalError(object p0)
        {
            return TextRes.GetString("General_InternalError", new object[] { p0 });
        }

        internal static string HttpUtils_CannotConvertCharToInt(object p0)
        {
            return TextRes.GetString("HttpUtils_CannotConvertCharToInt", new object[] { p0 });
        }

        internal static string HttpUtils_ClosingQuoteNotFound(object p0)
        {
            return TextRes.GetString("HttpUtils_ClosingQuoteNotFound", new object[] { p0 });
        }

        internal static string HttpUtils_EscapeCharAtEnd(object p0)
        {
            return TextRes.GetString("HttpUtils_EscapeCharAtEnd", new object[] { p0 });
        }

        internal static string HttpUtils_EscapeCharWithoutQuotes(object p0)
        {
            return TextRes.GetString("HttpUtils_EscapeCharWithoutQuotes", new object[] { p0 });
        }

        internal static string HttpUtils_ExpectedLiteralNotFoundInString(object p0, object p1, object p2)
        {
            return TextRes.GetString("HttpUtils_ExpectedLiteralNotFoundInString", new object[] { p0, p1, p2 });
        }

        internal static string HttpUtils_InvalidCharacterInQuotedParameterValue(object p0, object p1, object p2)
        {
            return TextRes.GetString("HttpUtils_InvalidCharacterInQuotedParameterValue", new object[] { p0, p1, p2 });
        }

        internal static string HttpUtils_InvalidCharsetName(object p0)
        {
            return TextRes.GetString("HttpUtils_InvalidCharsetName", new object[] { p0 });
        }

        internal static string HttpUtils_InvalidHttpMethodString(object p0)
        {
            return TextRes.GetString("HttpUtils_InvalidHttpMethodString", new object[] { p0 });
        }

        internal static string HttpUtils_InvalidQualityValue(object p0, object p1)
        {
            return TextRes.GetString("HttpUtils_InvalidQualityValue", new object[] { p0, p1 });
        }

        internal static string HttpUtils_InvalidQualityValueStartChar(object p0, object p1)
        {
            return TextRes.GetString("HttpUtils_InvalidQualityValueStartChar", new object[] { p0, p1 });
        }

        internal static string HttpUtils_InvalidSeparatorBetweenCharsets(object p0)
        {
            return TextRes.GetString("HttpUtils_InvalidSeparatorBetweenCharsets", new object[] { p0 });
        }

        internal static string HttpUtils_MediaTypeMissingParameterValue(object p0)
        {
            return TextRes.GetString("HttpUtils_MediaTypeMissingParameterValue", new object[] { p0 });
        }

        internal static string HttpUtils_MediaTypeRequiresSemicolonBeforeParameter(object p0)
        {
            return TextRes.GetString("HttpUtils_MediaTypeRequiresSemicolonBeforeParameter", new object[] { p0 });
        }

        internal static string HttpUtils_MediaTypeRequiresSlash(object p0)
        {
            return TextRes.GetString("HttpUtils_MediaTypeRequiresSlash", new object[] { p0 });
        }

        internal static string HttpUtils_MediaTypeRequiresSubType(object p0)
        {
            return TextRes.GetString("HttpUtils_MediaTypeRequiresSubType", new object[] { p0 });
        }

        internal static string HttpUtils_MediaTypeUnspecified(object p0)
        {
            return TextRes.GetString("HttpUtils_MediaTypeUnspecified", new object[] { p0 });
        }

        internal static string HttpUtils_MissingSeparatorBetweenCharsets(object p0)
        {
            return TextRes.GetString("HttpUtils_MissingSeparatorBetweenCharsets", new object[] { p0 });
        }

        internal static string HttpUtils_NoOrMoreThanOneContentTypeSpecified(object p0)
        {
            return TextRes.GetString("HttpUtils_NoOrMoreThanOneContentTypeSpecified", new object[] { p0 });
        }

        internal static string HttpUtils_UnexpectedEndOfQValue(object p0)
        {
            return TextRes.GetString("HttpUtils_UnexpectedEndOfQValue", new object[] { p0 });
        }

        internal static string JsonReader_InvalidNumberFormat(object p0)
        {
            return TextRes.GetString("JsonReader_InvalidNumberFormat", new object[] { p0 });
        }

        internal static string JsonReader_InvalidPropertyNameOrUnexpectedComma(object p0)
        {
            return TextRes.GetString("JsonReader_InvalidPropertyNameOrUnexpectedComma", new object[] { p0 });
        }

        internal static string JsonReader_MissingColon(object p0)
        {
            return TextRes.GetString("JsonReader_MissingColon", new object[] { p0 });
        }

        internal static string JsonReader_MissingComma(object p0)
        {
            return TextRes.GetString("JsonReader_MissingComma", new object[] { p0 });
        }

        internal static string JsonReader_UnexpectedComma(object p0)
        {
            return TextRes.GetString("JsonReader_UnexpectedComma", new object[] { p0 });
        }

        internal static string JsonReader_UnexpectedToken(object p0)
        {
            return TextRes.GetString("JsonReader_UnexpectedToken", new object[] { p0 });
        }

        internal static string JsonReader_UnrecognizedEscapeSequence(object p0)
        {
            return TextRes.GetString("JsonReader_UnrecognizedEscapeSequence", new object[] { p0 });
        }

        internal static string JsonReaderExtensions_CannotReadPropertyValueAsString(object p0, object p1)
        {
            return TextRes.GetString("JsonReaderExtensions_CannotReadPropertyValueAsString", new object[] { p0, p1 });
        }

        internal static string JsonReaderExtensions_CannotReadValueAsDouble(object p0)
        {
            return TextRes.GetString("JsonReaderExtensions_CannotReadValueAsDouble", new object[] { p0 });
        }

        internal static string JsonReaderExtensions_CannotReadValueAsString(object p0)
        {
            return TextRes.GetString("JsonReaderExtensions_CannotReadValueAsString", new object[] { p0 });
        }

        internal static string JsonReaderExtensions_UnexpectedNodeDetected(object p0, object p1)
        {
            return TextRes.GetString("JsonReaderExtensions_UnexpectedNodeDetected", new object[] { p0, p1 });
        }

        internal static string MediaType_EncodingNotSupported(object p0)
        {
            return TextRes.GetString("MediaType_EncodingNotSupported", new object[] { p0 });
        }

        internal static string MediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads(object p0, object p1)
        {
            return TextRes.GetString("MediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads", new object[] { p0, p1 });
        }

        internal static string MediaTypeUtils_CannotDetermineFormatFromContentType(object p0, object p1)
        {
            return TextRes.GetString("MediaTypeUtils_CannotDetermineFormatFromContentType", new object[] { p0, p1 });
        }

        internal static string MediaTypeUtils_DidNotFindMatchingMediaType(object p0, object p1)
        {
            return TextRes.GetString("MediaTypeUtils_DidNotFindMatchingMediaType", new object[] { p0, p1 });
        }

        internal static string MediaTypeUtils_NoOrMoreThanOneContentTypeSpecified(object p0)
        {
            return TextRes.GetString("MediaTypeUtils_NoOrMoreThanOneContentTypeSpecified", new object[] { p0 });
        }

        internal static string MessageStreamWrappingStream_ByteLimitExceeded(object p0, object p1)
        {
            return TextRes.GetString("MessageStreamWrappingStream_ByteLimitExceeded", new object[] { p0, p1 });
        }

        internal static string MetadataUtils_ResolveTypeName(object p0)
        {
            return TextRes.GetString("MetadataUtils_ResolveTypeName", new object[] { p0 });
        }

        internal static string ODataAtomCollectionDeserializer_TopLevelCollectionElementWrongNamespace(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomCollectionDeserializer_TopLevelCollectionElementWrongNamespace", new object[] { p0, p1 });
        }

        internal static string ODataAtomCollectionDeserializer_WrongCollectionItemElementName(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomCollectionDeserializer_WrongCollectionItemElementName", new object[] { p0, p1 });
        }

        internal static string ODataAtomDeserializer_RelativeUriUsedWithoutBaseUriSpecified(object p0)
        {
            return TextRes.GetString("ODataAtomDeserializer_RelativeUriUsedWithoutBaseUriSpecified", new object[] { p0 });
        }

        internal static string ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinksStartElement(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinksStartElement", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinkStartElement(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinkStartElement", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntityReferenceLinkDeserializer_MultipleEntityReferenceLinksElementsWithSameName(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntityReferenceLinkDeserializer_MultipleEntityReferenceLinksElementsWithSameName", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_ContentWithInvalidNode(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_ContentWithInvalidNode", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_ContentWithWrongType(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_ContentWithWrongType", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_DuplicateElements(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_DuplicateElements", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_ElementExpected(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_ElementExpected", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_EntryElementWrongName(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_EntryElementWrongName", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_FeedElementWrongName(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_FeedElementWrongName", new object[] { p0, p1 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_InvalidTypeAttributeOnAssociationLink(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_InvalidTypeAttributeOnAssociationLink", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_MultipleExpansionsInInline(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_MultipleExpansionsInInline", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_MultipleLinksInEntry(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_MultipleLinksInEntry", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_MultipleLinksInFeed(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_MultipleLinksInFeed", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_OperationMissingMetadataAttribute(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_OperationMissingMetadataAttribute", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_OperationMissingTargetAttribute(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_OperationMissingTargetAttribute", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_StreamPropertyDuplicatePropertyName(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_StreamPropertyDuplicatePropertyName", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleContentTypes(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleContentTypes", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleEditLinks(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleEditLinks", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleReadLinks(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_StreamPropertyWithMultipleReadLinks", new object[] { p0 });
        }

        internal static string ODataAtomEntryAndFeedDeserializer_UnknownElementInInline(object p0)
        {
            return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_UnknownElementInInline", new object[] { p0 });
        }

        internal static string ODataAtomEntryMetadataDeserializer_InvalidTextConstructKind(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomEntryMetadataDeserializer_InvalidTextConstructKind", new object[] { p0, p1 });
        }

        internal static string ODataAtomErrorDeserializer_InvalidRootElement(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomErrorDeserializer_InvalidRootElement", new object[] { p0, p1 });
        }

        internal static string ODataAtomErrorDeserializer_MultipleErrorElementsWithSameName(object p0)
        {
            return TextRes.GetString("ODataAtomErrorDeserializer_MultipleErrorElementsWithSameName", new object[] { p0 });
        }

        internal static string ODataAtomErrorDeserializer_MultipleInnerErrorElementsWithSameName(object p0)
        {
            return TextRes.GetString("ODataAtomErrorDeserializer_MultipleInnerErrorElementsWithSameName", new object[] { p0 });
        }

        internal static string ODataAtomInputContext_NonEmptyElementWithNullAttribute(object p0)
        {
            return TextRes.GetString("ODataAtomInputContext_NonEmptyElementWithNullAttribute", new object[] { p0 });
        }

        internal static string ODataAtomMetadataDeserializer_MultipleSingletonMetadataElements(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomMetadataDeserializer_MultipleSingletonMetadataElements", new object[] { p0, p1 });
        }

        internal static string ODataAtomMetadataEpmMerge_TextKindConflict(object p0, object p1, object p2)
        {
            return TextRes.GetString("ODataAtomMetadataEpmMerge_TextKindConflict", new object[] { p0, p1, p2 });
        }

        internal static string ODataAtomMetadataEpmMerge_TextValueConflict(object p0, object p1, object p2)
        {
            return TextRes.GetString("ODataAtomMetadataEpmMerge_TextValueConflict", new object[] { p0, p1, p2 });
        }

        internal static string ODataAtomPropertyAndValueDeserializer_InvalidCollectionElement(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomPropertyAndValueDeserializer_InvalidCollectionElement", new object[] { p0, p1 });
        }

        internal static string ODataAtomPropertyAndValueDeserializer_NavigationPropertyInProperties(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomPropertyAndValueDeserializer_NavigationPropertyInProperties", new object[] { p0, p1 });
        }

        internal static string ODataAtomPropertyAndValueDeserializer_NonEmptyElementWithNullAttribute(object p0)
        {
            return TextRes.GetString("ODataAtomPropertyAndValueDeserializer_NonEmptyElementWithNullAttribute", new object[] { p0 });
        }

        internal static string ODataAtomPropertyAndValueDeserializer_TopLevelPropertyElementWrongNamespace(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomPropertyAndValueDeserializer_TopLevelPropertyElementWrongNamespace", new object[] { p0, p1 });
        }

        internal static string ODataAtomReader_FeedNavigationLinkForResourceReferenceProperty(object p0)
        {
            return TextRes.GetString("ODataAtomReader_FeedNavigationLinkForResourceReferenceProperty", new object[] { p0 });
        }

        internal static string ODataAtomServiceDocumentDeserializer_ServiceDocumentRootElementWrongNameOrNamespace(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomServiceDocumentDeserializer_ServiceDocumentRootElementWrongNameOrNamespace", new object[] { p0, p1 });
        }

        internal static string ODataAtomServiceDocumentDeserializer_UnexpectedElementInResourceCollection(object p0)
        {
            return TextRes.GetString("ODataAtomServiceDocumentDeserializer_UnexpectedElementInResourceCollection", new object[] { p0 });
        }

        internal static string ODataAtomServiceDocumentDeserializer_UnexpectedElementInServiceDocument(object p0)
        {
            return TextRes.GetString("ODataAtomServiceDocumentDeserializer_UnexpectedElementInServiceDocument", new object[] { p0 });
        }

        internal static string ODataAtomServiceDocumentDeserializer_UnexpectedElementInWorkspace(object p0)
        {
            return TextRes.GetString("ODataAtomServiceDocumentDeserializer_UnexpectedElementInWorkspace", new object[] { p0 });
        }

        internal static string ODataAtomServiceDocumentMetadataDeserializer_InvalidFixedAttributeValue(object p0)
        {
            return TextRes.GetString("ODataAtomServiceDocumentMetadataDeserializer_InvalidFixedAttributeValue", new object[] { p0 });
        }

        internal static string ODataAtomServiceDocumentMetadataDeserializer_MultipleTitleElementsFound(object p0)
        {
            return TextRes.GetString("ODataAtomServiceDocumentMetadataDeserializer_MultipleTitleElementsFound", new object[] { p0 });
        }

        internal static string ODataAtomWriterMetadataUtils_CategorySchemesMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_CategorySchemesMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_CategoryTermsMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_CategoryTermsMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_InvalidAnnotationValue(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_InvalidAnnotationValue", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_LinkHrefsMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkHrefsMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_LinkMediaTypesMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkMediaTypesMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_LinkRelationsMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkRelationsMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataAtomWriterMetadataUtils_LinkTitlesMustMatch(object p0, object p1)
        {
            return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkTitlesMustMatch", new object[] { p0, p1 });
        }

        internal static string ODataBatch_InvalidHttpMethodForChangeSetRequest(object p0)
        {
            return TextRes.GetString("ODataBatch_InvalidHttpMethodForChangeSetRequest", new object[] { p0 });
        }

        internal static string ODataBatch_InvalidHttpMethodForQueryOperation(object p0)
        {
            return TextRes.GetString("ODataBatch_InvalidHttpMethodForQueryOperation", new object[] { p0 });
        }

        internal static string ODataBatchOperationHeaderDictionary_DuplicateCaseInsensitiveKeys(object p0)
        {
            return TextRes.GetString("ODataBatchOperationHeaderDictionary_DuplicateCaseInsensitiveKeys", new object[] { p0 });
        }

        internal static string ODataBatchOperationHeaderDictionary_KeyNotFound(object p0)
        {
            return TextRes.GetString("ODataBatchOperationHeaderDictionary_KeyNotFound", new object[] { p0 });
        }

        internal static string ODataBatchReader_DuplicateContentIDsNotAllowed(object p0)
        {
            return TextRes.GetString("ODataBatchReader_DuplicateContentIDsNotAllowed", new object[] { p0 });
        }

        internal static string ODataBatchReader_InvalidStateForCreateOperationRequestMessage(object p0)
        {
            return TextRes.GetString("ODataBatchReader_InvalidStateForCreateOperationRequestMessage", new object[] { p0 });
        }

        internal static string ODataBatchReader_InvalidStateForCreateOperationResponseMessage(object p0)
        {
            return TextRes.GetString("ODataBatchReader_InvalidStateForCreateOperationResponseMessage", new object[] { p0 });
        }

        internal static string ODataBatchReader_MaxBatchSizeExceeded(object p0)
        {
            return TextRes.GetString("ODataBatchReader_MaxBatchSizeExceeded", new object[] { p0 });
        }

        internal static string ODataBatchReader_MaxChangeSetSizeExceeded(object p0)
        {
            return TextRes.GetString("ODataBatchReader_MaxChangeSetSizeExceeded", new object[] { p0 });
        }

        internal static string ODataBatchReader_ReadOrReadAsyncCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataBatchReader_ReadOrReadAsyncCalledInInvalidState", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_DuplicateHeaderFound(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_DuplicateHeaderFound", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_InvalidContentLengthSpecified(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidContentLengthSpecified", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_InvalidContentTypeSpecified(object p0, object p1, object p2, object p3)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidContentTypeSpecified", new object[] { p0, p1, p2, p3 });
        }

        internal static string ODataBatchReaderStream_InvalidHeaderSpecified(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidHeaderSpecified", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_InvalidHttpVersionSpecified(object p0, object p1)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidHttpVersionSpecified", new object[] { p0, p1 });
        }

        internal static string ODataBatchReaderStream_InvalidRequestLine(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidRequestLine", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_InvalidResponseLine(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_InvalidResponseLine", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_MissingOrInvalidContentEncodingHeader(object p0, object p1)
        {
            return TextRes.GetString("ODataBatchReaderStream_MissingOrInvalidContentEncodingHeader", new object[] { p0, p1 });
        }

        internal static string ODataBatchReaderStream_MultiByteEncodingsNotSupported(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_MultiByteEncodingsNotSupported", new object[] { p0 });
        }

        internal static string ODataBatchReaderStream_NonIntegerHttpStatusCode(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStream_NonIntegerHttpStatusCode", new object[] { p0 });
        }

        internal static string ODataBatchReaderStreamBuffer_BoundaryLineSecurityLimitReached(object p0)
        {
            return TextRes.GetString("ODataBatchReaderStreamBuffer_BoundaryLineSecurityLimitReached", new object[] { p0 });
        }

        internal static string ODataBatchUtils_RelativeUriStartingWithDollarUsedWithoutBaseUriSpecified(object p0)
        {
            return TextRes.GetString("ODataBatchUtils_RelativeUriStartingWithDollarUsedWithoutBaseUriSpecified", new object[] { p0 });
        }

        internal static string ODataBatchUtils_RelativeUriUsedWithoutBaseUriSpecified(object p0)
        {
            return TextRes.GetString("ODataBatchUtils_RelativeUriUsedWithoutBaseUriSpecified", new object[] { p0 });
        }

        internal static string ODataBatchWriter_DuplicateContentIDsNotAllowed(object p0)
        {
            return TextRes.GetString("ODataBatchWriter_DuplicateContentIDsNotAllowed", new object[] { p0 });
        }

        internal static string ODataBatchWriter_MaxBatchSizeExceeded(object p0)
        {
            return TextRes.GetString("ODataBatchWriter_MaxBatchSizeExceeded", new object[] { p0 });
        }

        internal static string ODataBatchWriter_MaxChangeSetSizeExceeded(object p0)
        {
            return TextRes.GetString("ODataBatchWriter_MaxChangeSetSizeExceeded", new object[] { p0 });
        }

        internal static string ODataCollectionReaderCore_ReadOrReadAsyncCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataCollectionReaderCore_ReadOrReadAsyncCalledInInvalidState", new object[] { p0 });
        }

        internal static string ODataCollectionWriter_CannotCreateCollectionWriterForFormat(object p0)
        {
            return TextRes.GetString("ODataCollectionWriter_CannotCreateCollectionWriterForFormat", new object[] { p0 });
        }

        internal static string ODataCollectionWriterCore_InvalidTransitionFromCollection(object p0, object p1)
        {
            return TextRes.GetString("ODataCollectionWriterCore_InvalidTransitionFromCollection", new object[] { p0, p1 });
        }

        internal static string ODataCollectionWriterCore_InvalidTransitionFromItem(object p0, object p1)
        {
            return TextRes.GetString("ODataCollectionWriterCore_InvalidTransitionFromItem", new object[] { p0, p1 });
        }

        internal static string ODataCollectionWriterCore_InvalidTransitionFromStart(object p0, object p1)
        {
            return TextRes.GetString("ODataCollectionWriterCore_InvalidTransitionFromStart", new object[] { p0, p1 });
        }

        internal static string ODataCollectionWriterCore_WriteEndCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataCollectionWriterCore_WriteEndCalledInInvalidState", new object[] { p0 });
        }

        internal static string ODataInputContext_UnsupportedPayloadKindForFormat(object p0, object p1)
        {
            return TextRes.GetString("ODataInputContext_UnsupportedPayloadKindForFormat", new object[] { p0, p1 });
        }

        internal static string ODataJsonCollectionDeserializer_CannotReadCollectionContentStart(object p0)
        {
            return TextRes.GetString("ODataJsonCollectionDeserializer_CannotReadCollectionContentStart", new object[] { p0 });
        }

        internal static string ODataJsonCollectionReader_CannotReadCollectionStart(object p0)
        {
            return TextRes.GetString("ODataJsonCollectionReader_CannotReadCollectionStart", new object[] { p0 });
        }

        internal static string ODataJsonCollectionReader_CannotReadWrappedCollectionStart(object p0)
        {
            return TextRes.GetString("ODataJsonCollectionReader_CannotReadWrappedCollectionStart", new object[] { p0 });
        }

        internal static string ODataJsonDeserializer_RelativeUriUsedWithoutBaseUriSpecified(object p0)
        {
            return TextRes.GetString("ODataJsonDeserializer_RelativeUriUsedWithoutBaseUriSpecified", new object[] { p0 });
        }

        internal static string ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkMustBeObjectValue(object p0)
        {
            return TextRes.GetString("ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkMustBeObjectValue", new object[] { p0 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_CannotReadCollectionNavigationPropertyValue(object p0)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_CannotReadCollectionNavigationPropertyValue", new object[] { p0 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_CannotReadFeedContentStart(object p0)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_CannotReadFeedContentStart", new object[] { p0 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_CannotReadSingletonNavigationPropertyValue(object p0)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_CannotReadSingletonNavigationPropertyValue", new object[] { p0 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MetadataMustHaveArrayValue(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MetadataMustHaveArrayValue", new object[] { p0, p1 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty(object p0)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesForStreamProperty", new object[] { p0 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleOptionalPropertiesInOperation(object p0, object p1, object p2)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleOptionalPropertiesInOperation", new object[] { p0, p1, p2 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleTargetPropertiesInOperation(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleTargetPropertiesInOperation", new object[] { p0, p1 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_OperationMetadataArrayExpectedAnObject(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_OperationMetadataArrayExpectedAnObject", new object[] { p0, p1 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_OperationMissingTargetProperty(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_OperationMissingTargetProperty", new object[] { p0, p1 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_PropertyInEntryMustHaveObjectValue(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_PropertyInEntryMustHaveObjectValue", new object[] { p0, p1 });
        }

        internal static string ODataJsonEntryAndFeedDeserializer_RepeatMetadataValue(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_RepeatMetadataValue", new object[] { p0, p1 });
        }

        internal static string ODataJsonErrorDeserializer_TopLevelErrorMessageValueWithInvalidProperty(object p0)
        {
            return TextRes.GetString("ODataJsonErrorDeserializer_TopLevelErrorMessageValueWithInvalidProperty", new object[] { p0 });
        }

        internal static string ODataJsonErrorDeserializer_TopLevelErrorValueWithInvalidProperty(object p0)
        {
            return TextRes.GetString("ODataJsonErrorDeserializer_TopLevelErrorValueWithInvalidProperty", new object[] { p0 });
        }

        internal static string ODataJsonErrorDeserializer_TopLevelErrorWithInvalidProperty(object p0)
        {
            return TextRes.GetString("ODataJsonErrorDeserializer_TopLevelErrorWithInvalidProperty", new object[] { p0 });
        }

        internal static string ODataJsonInputContext_FunctionImportCannotBeNullForCreateParameterReader(object p0)
        {
            return TextRes.GetString("ODataJsonInputContext_FunctionImportCannotBeNullForCreateParameterReader", new object[] { p0 });
        }

        internal static string ODataJsonParameterReader_NullCollectionExpected(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonParameterReader_NullCollectionExpected", new object[] { p0, p1 });
        }

        internal static string ODataJsonParameterReader_UnsupportedParameterTypeKind(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonParameterReader_UnsupportedParameterTypeKind", new object[] { p0, p1 });
        }

        internal static string ODataJsonParameterReader_UnsupportedPrimitiveParameterType(object p0, object p1)
        {
            return TextRes.GetString("ODataJsonParameterReader_UnsupportedPrimitiveParameterType", new object[] { p0, p1 });
        }

        internal static string ODataJsonPropertyAndValueDeserializer_CannotReadPropertyValue(object p0)
        {
            return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_CannotReadPropertyValue", new object[] { p0 });
        }

        internal static string ODataJsonPropertyAndValueDeserializer_InvalidPrimitiveTypeName(object p0)
        {
            return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_InvalidPrimitiveTypeName", new object[] { p0 });
        }

        internal static string ODataJsonPropertyAndValueDeserializer_InvalidTypeName(object p0)
        {
            return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_InvalidTypeName", new object[] { p0 });
        }

        internal static string ODataJsonPropertyAndValueDeserializer_MetadataPropertyMustHaveObjectValue(object p0)
        {
            return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_MetadataPropertyMustHaveObjectValue", new object[] { p0 });
        }

        internal static string ODataJsonPropertyAndValueDeserializer_MultiplePropertiesInCollectionWrapper(object p0)
        {
            return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_MultiplePropertiesInCollectionWrapper", new object[] { p0 });
        }

        internal static string ODataJsonReader_CannotReadEntriesOfFeed(object p0)
        {
            return TextRes.GetString("ODataJsonReader_CannotReadEntriesOfFeed", new object[] { p0 });
        }

        internal static string ODataJsonReader_CannotReadEntryStart(object p0)
        {
            return TextRes.GetString("ODataJsonReader_CannotReadEntryStart", new object[] { p0 });
        }

        internal static string ODataJsonReader_CannotReadFeedStart(object p0)
        {
            return TextRes.GetString("ODataJsonReader_CannotReadFeedStart", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_CannotConvertBoolean(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_CannotConvertBoolean", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_CannotConvertDateTime(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_CannotConvertDateTime", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_CannotConvertDateTimeOffset(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_CannotConvertDateTimeOffset", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_CannotConvertDouble(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_CannotConvertDouble", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_CannotConvertInt32(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_CannotConvertInt32", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_EntityReferenceLinksInlineCountWithNullValue(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_EntityReferenceLinksInlineCountWithNullValue", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_EntityReferenceLinksPropertyWithNullValue(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_EntityReferenceLinksPropertyWithNullValue", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_FeedPropertyWithNullValue(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_FeedPropertyWithNullValue", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_MediaResourcePropertyWithNullValue(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_MediaResourcePropertyWithNullValue", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_MetadataPropertyWithNullValue(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_MetadataPropertyWithNullValue", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_MultipleEntityReferenceLinksWrapperPropertiesWithSameName(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_MultipleEntityReferenceLinksWrapperPropertiesWithSameName", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_MultipleErrorPropertiesWithSameName(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_MultipleErrorPropertiesWithSameName", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_MultipleMetadataPropertiesWithSameName(object p0)
        {
            return TextRes.GetString("ODataJsonReaderUtils_MultipleMetadataPropertiesWithSameName", new object[] { p0 });
        }

        internal static string ODataJsonReaderUtils_OperationPropertyCannotBeNull(object p0, object p1, object p2)
        {
            return TextRes.GetString("ODataJsonReaderUtils_OperationPropertyCannotBeNull", new object[] { p0, p1, p2 });
        }

        internal static string ODataJsonWriter_UnsupportedValueType(object p0)
        {
            return TextRes.GetString("ODataJsonWriter_UnsupportedValueType", new object[] { p0 });
        }

        internal static string ODataMediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads(object p0, object p1)
        {
            return TextRes.GetString("ODataMediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads", new object[] { p0, p1 });
        }

        internal static string ODataMessageReader_ExpectedCollectionTypeWrongKind(object p0)
        {
            return TextRes.GetString("ODataMessageReader_ExpectedCollectionTypeWrongKind", new object[] { p0 });
        }

        internal static string ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata(object p0)
        {
            return TextRes.GetString("ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata", new object[] { p0 });
        }

        internal static string ODataMessageReader_ExpectedValueTypeWrongKind(object p0)
        {
            return TextRes.GetString("ODataMessageReader_ExpectedValueTypeWrongKind", new object[] { p0 });
        }

        internal static string ODataMessageReader_FunctionImportSpecifiedWithoutMetadata(object p0)
        {
            return TextRes.GetString("ODataMessageReader_FunctionImportSpecifiedWithoutMetadata", new object[] { p0 });
        }

        internal static string ODataMessageReader_WildcardInContentType(object p0)
        {
            return TextRes.GetString("ODataMessageReader_WildcardInContentType", new object[] { p0 });
        }

        internal static string ODataMessageWriter_CannotSetHeadersWithInvalidPayloadKind(object p0)
        {
            return TextRes.GetString("ODataMessageWriter_CannotSetHeadersWithInvalidPayloadKind", new object[] { p0 });
        }

        internal static string ODataMessageWriter_CannotWriteStreamPropertyAsTopLevelProperty(object p0)
        {
            return TextRes.GetString("ODataMessageWriter_CannotWriteStreamPropertyAsTopLevelProperty", new object[] { p0 });
        }

        internal static string ODataMessageWriter_IncompatiblePayloadKinds(object p0, object p1)
        {
            return TextRes.GetString("ODataMessageWriter_IncompatiblePayloadKinds", new object[] { p0, p1 });
        }

        internal static string ODataMessageWriter_InvalidContentTypeForWritingRawValue(object p0)
        {
            return TextRes.GetString("ODataMessageWriter_InvalidContentTypeForWritingRawValue", new object[] { p0 });
        }

        internal static string ODataMetadataInputContext_ErrorReadingMetadata(object p0)
        {
            return TextRes.GetString("ODataMetadataInputContext_ErrorReadingMetadata", new object[] { p0 });
        }

        internal static string ODataMetadataOutputContext_ErrorWritingMetadata(object p0)
        {
            return TextRes.GetString("ODataMetadataOutputContext_ErrorWritingMetadata", new object[] { p0 });
        }

        internal static string ODataOutputContext_UnsupportedPayloadKindForFormat(object p0, object p1)
        {
            return TextRes.GetString("ODataOutputContext_UnsupportedPayloadKindForFormat", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_CreateReaderAlreadyCalled(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_CreateReaderAlreadyCalled", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_DuplicateParametersInPayload(object p0)
        {
            return TextRes.GetString("ODataParameterReaderCore_DuplicateParametersInPayload", new object[] { p0 });
        }

        internal static string ODataParameterReaderCore_InvalidCreateReaderMethodCalledForState(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_InvalidCreateReaderMethodCalledForState", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_ParameterNameNotInMetadata(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_ParameterNameNotInMetadata", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_ParametersMissingInPayload(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_ParametersMissingInPayload", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_ReadOrReadAsyncCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataParameterReaderCore_ReadOrReadAsyncCalledInInvalidState", new object[] { p0 });
        }

        internal static string ODataParameterReaderCore_SubReaderMustBeCreatedAndReadToCompletionBeforeTheNextReadOrReadAsyncCall(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_SubReaderMustBeCreatedAndReadToCompletionBeforeTheNextReadOrReadAsyncCall", new object[] { p0, p1 });
        }

        internal static string ODataParameterReaderCore_SubReaderMustBeInCompletedStateBeforeTheNextReadOrReadAsyncCall(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterReaderCore_SubReaderMustBeInCompletedStateBeforeTheNextReadOrReadAsyncCall", new object[] { p0, p1 });
        }

        internal static string ODataParameterWriterCore_CannotCreateCollectionWriterOnNonCollectionTypeKind(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterWriterCore_CannotCreateCollectionWriterOnNonCollectionTypeKind", new object[] { p0, p1 });
        }

        internal static string ODataParameterWriterCore_CannotWriteValueOnNonSupportedValueType(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterWriterCore_CannotWriteValueOnNonSupportedValueType", new object[] { p0, p1 });
        }

        internal static string ODataParameterWriterCore_CannotWriteValueOnNonValueTypeKind(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterWriterCore_CannotWriteValueOnNonValueTypeKind", new object[] { p0, p1 });
        }

        internal static string ODataParameterWriterCore_DuplicatedParameterNameNotAllowed(object p0)
        {
            return TextRes.GetString("ODataParameterWriterCore_DuplicatedParameterNameNotAllowed", new object[] { p0 });
        }

        internal static string ODataParameterWriterCore_MissingParameterInParameterPayload(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterWriterCore_MissingParameterInParameterPayload", new object[] { p0, p1 });
        }

        internal static string ODataParameterWriterCore_ParameterNameNotFoundInFunctionImport(object p0, object p1)
        {
            return TextRes.GetString("ODataParameterWriterCore_ParameterNameNotFoundInFunctionImport", new object[] { p0, p1 });
        }

        internal static string ODataReaderCore_NoReadCallsAllowed(object p0)
        {
            return TextRes.GetString("ODataReaderCore_NoReadCallsAllowed", new object[] { p0 });
        }

        internal static string ODataReaderCore_ReadOrReadAsyncCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataReaderCore_ReadOrReadAsyncCalledInInvalidState", new object[] { p0 });
        }

        internal static string ODataUriUtils_ConvertFromUriLiteralNullOnNonNullableType(object p0)
        {
            return TextRes.GetString("ODataUriUtils_ConvertFromUriLiteralNullOnNonNullableType", new object[] { p0 });
        }

        internal static string ODataUriUtils_ConvertFromUriLiteralNullTypeVerificationFailure(object p0, object p1)
        {
            return TextRes.GetString("ODataUriUtils_ConvertFromUriLiteralNullTypeVerificationFailure", new object[] { p0, p1 });
        }

        internal static string ODataUriUtils_ConvertFromUriLiteralTypeVerificationFailure(object p0, object p1)
        {
            return TextRes.GetString("ODataUriUtils_ConvertFromUriLiteralTypeVerificationFailure", new object[] { p0, p1 });
        }

        internal static string ODataUriUtils_ConvertToUriLiteralUnsupportedType(object p0)
        {
            return TextRes.GetString("ODataUriUtils_ConvertToUriLiteralUnsupportedType", new object[] { p0 });
        }

        internal static string ODataUtils_CannotConvertValueToRawPrimitive(object p0)
        {
            return TextRes.GetString("ODataUtils_CannotConvertValueToRawPrimitive", new object[] { p0 });
        }

        internal static string ODataUtils_DidNotFindDefaultMediaType(object p0)
        {
            return TextRes.GetString("ODataUtils_DidNotFindDefaultMediaType", new object[] { p0 });
        }

        internal static string ODataUtils_UnsupportedVersionHeader(object p0)
        {
            return TextRes.GetString("ODataUtils_UnsupportedVersionHeader", new object[] { p0 });
        }

        internal static string ODataVersionChecker_AssociationLinksNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_AssociationLinksNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_CollectionNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_CollectionNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_CollectionPropertiesNotSupported(object p0, object p1)
        {
            return TextRes.GetString("ODataVersionChecker_CollectionPropertiesNotSupported", new object[] { p0, p1 });
        }

        internal static string ODataVersionChecker_EpmVersionNotSupported(object p0, object p1, object p2)
        {
            return TextRes.GetString("ODataVersionChecker_EpmVersionNotSupported", new object[] { p0, p1, p2 });
        }

        internal static string ODataVersionChecker_GeographyAndGeometryNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_GeographyAndGeometryNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_InlineCountNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_InlineCountNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_MaxProtocolVersionExceeded(object p0, object p1)
        {
            return TextRes.GetString("ODataVersionChecker_MaxProtocolVersionExceeded", new object[] { p0, p1 });
        }

        internal static string ODataVersionChecker_NextLinkNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_NextLinkNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_ParameterPayloadNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_ParameterPayloadNotSupported", new object[] { p0 });
        }

        internal static string ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX(object p0, object p1)
        {
            return TextRes.GetString("ODataVersionChecker_PropertyNotSupportedForODataVersionGreaterThanX", new object[] { p0, p1 });
        }

        internal static string ODataVersionChecker_StreamPropertiesNotSupported(object p0)
        {
            return TextRes.GetString("ODataVersionChecker_StreamPropertiesNotSupported", new object[] { p0 });
        }

        internal static string ODataWriter_RelativeUriUsedWithoutBaseUriSpecified(object p0)
        {
            return TextRes.GetString("ODataWriter_RelativeUriUsedWithoutBaseUriSpecified", new object[] { p0 });
        }

        internal static string ODataWriter_StreamPropertiesMustBePropertiesOfODataEntry(object p0)
        {
            return TextRes.GetString("ODataWriter_StreamPropertiesMustBePropertiesOfODataEntry", new object[] { p0 });
        }

        internal static string ODataWriterCore_InvalidStateTransition(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidStateTransition", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromCompleted(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromCompleted", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromEntry(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromEntry", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromError(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromError", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromExpandedLink(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromExpandedLink", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromFeed(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromFeed", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromNullEntry(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromNullEntry", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_InvalidTransitionFromStart(object p0, object p1)
        {
            return TextRes.GetString("ODataWriterCore_InvalidTransitionFromStart", new object[] { p0, p1 });
        }

        internal static string ODataWriterCore_WriteEndCalledInInvalidState(object p0)
        {
            return TextRes.GetString("ODataWriterCore_WriteEndCalledInInvalidState", new object[] { p0 });
        }

        internal static string ReaderUtils_EnumerableModified(object p0)
        {
            return TextRes.GetString("ReaderUtils_EnumerableModified", new object[] { p0 });
        }

        internal static string ReaderValidationUtils_CannotConvertPrimitiveValue(object p0)
        {
            return TextRes.GetString("ReaderValidationUtils_CannotConvertPrimitiveValue", new object[] { p0 });
        }

        internal static string ReaderValidationUtils_DerivedComplexTypesAreNotAllowed(object p0, object p1)
        {
            return TextRes.GetString("ReaderValidationUtils_DerivedComplexTypesAreNotAllowed", new object[] { p0, p1 });
        }

        internal static string ReaderValidationUtils_MessageReaderSettingsBaseUriMustBeNullOrAbsolute(object p0)
        {
            return TextRes.GetString("ReaderValidationUtils_MessageReaderSettingsBaseUriMustBeNullOrAbsolute", new object[] { p0 });
        }

        internal static string ReaderValidationUtils_NullValueForNonNullableType(object p0)
        {
            return TextRes.GetString("ReaderValidationUtils_NullValueForNonNullableType", new object[] { p0 });
        }

        internal static string ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedForOpenType(object p0, object p1)
        {
            return TextRes.GetString("ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedForOpenType", new object[] { p0, p1 });
        }

        internal static string UriQueryExpressionParser_UnrecognizedLiteral(object p0, object p1, object p2, object p3)
        {
            return TextRes.GetString("UriQueryExpressionParser_UnrecognizedLiteral", new object[] { p0, p1, p2, p3 });
        }

        internal static string UriUtils_InvalidRelativeUriForEscaping(object p0, object p1)
        {
            return TextRes.GetString("UriUtils_InvalidRelativeUriForEscaping", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_ActionsAndFunctionsMustSpecifyMetadata(object p0)
        {
            return TextRes.GetString("ValidationUtils_ActionsAndFunctionsMustSpecifyMetadata", new object[] { p0 });
        }

        internal static string ValidationUtils_ActionsAndFunctionsMustSpecifyTarget(object p0)
        {
            return TextRes.GetString("ValidationUtils_ActionsAndFunctionsMustSpecifyTarget", new object[] { p0 });
        }

        internal static string ValidationUtils_EntryTypeNotAssignableToExpectedType(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_EntryTypeNotAssignableToExpectedType", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_EntryWithMediaResourceAndNonMLEType(object p0)
        {
            return TextRes.GetString("ValidationUtils_EntryWithMediaResourceAndNonMLEType", new object[] { p0 });
        }

        internal static string ValidationUtils_EntryWithoutMediaResourceAndMLEType(object p0)
        {
            return TextRes.GetString("ValidationUtils_EntryWithoutMediaResourceAndMLEType", new object[] { p0 });
        }

        internal static string ValidationUtils_EnumerableContainsANullItem(object p0)
        {
            return TextRes.GetString("ValidationUtils_EnumerableContainsANullItem", new object[] { p0 });
        }

        internal static string ValidationUtils_IncompatiblePrimitiveItemType(object p0, object p1, object p2, object p3)
        {
            return TextRes.GetString("ValidationUtils_IncompatiblePrimitiveItemType", new object[] { p0, p1, p2, p3 });
        }

        internal static string ValidationUtils_IncompatibleType(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_IncompatibleType", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_IncorrectTypeKind(object p0, object p1, object p2)
        {
            return TextRes.GetString("ValidationUtils_IncorrectTypeKind", new object[] { p0, p1, p2 });
        }

        internal static string ValidationUtils_IncorrectTypeKindNoTypeName(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_IncorrectTypeKindNoTypeName", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_IncorrectValueTypeKind(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_IncorrectValueTypeKind", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_InvalidBatchBoundaryDelimiterLength(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_InvalidBatchBoundaryDelimiterLength", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_InvalidCollectionTypeName(object p0)
        {
            return TextRes.GetString("ValidationUtils_InvalidCollectionTypeName", new object[] { p0 });
        }

        internal static string ValidationUtils_InvalidCollectionTypeReference(object p0)
        {
            return TextRes.GetString("ValidationUtils_InvalidCollectionTypeReference", new object[] { p0 });
        }

        internal static string ValidationUtils_InvalidEtagValue(object p0)
        {
            return TextRes.GetString("ValidationUtils_InvalidEtagValue", new object[] { p0 });
        }

        internal static string ValidationUtils_MaxDepthOfNestedEntriesExceeded(object p0)
        {
            return TextRes.GetString("ValidationUtils_MaxDepthOfNestedEntriesExceeded", new object[] { p0 });
        }

        internal static string ValidationUtils_MaxNumberOfEntityPropertyMappingsExceeded(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_MaxNumberOfEntityPropertyMappingsExceeded", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_MismatchPropertyKindForStreamProperty(object p0)
        {
            return TextRes.GetString("ValidationUtils_MismatchPropertyKindForStreamProperty", new object[] { p0 });
        }

        internal static string ValidationUtils_NavigationPropertyExpected(object p0, object p1, object p2)
        {
            return TextRes.GetString("ValidationUtils_NavigationPropertyExpected", new object[] { p0, p1, p2 });
        }

        internal static string ValidationUtils_NonPrimitiveTypeForPrimitiveValue(object p0)
        {
            return TextRes.GetString("ValidationUtils_NonPrimitiveTypeForPrimitiveValue", new object[] { p0 });
        }

        internal static string ValidationUtils_NullCollectionItemForNonNullableType(object p0)
        {
            return TextRes.GetString("ValidationUtils_NullCollectionItemForNonNullableType", new object[] { p0 });
        }

        internal static string ValidationUtils_OpenCollectionProperty(object p0)
        {
            return TextRes.GetString("ValidationUtils_OpenCollectionProperty", new object[] { p0 });
        }

        internal static string ValidationUtils_OpenNavigationProperty(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_OpenNavigationProperty", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_OpenStreamProperty(object p0)
        {
            return TextRes.GetString("ValidationUtils_OpenStreamProperty", new object[] { p0 });
        }

        internal static string ValidationUtils_PropertiesMustNotContainReservedChars(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_PropertiesMustNotContainReservedChars", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_PropertyDoesNotExistOnType(object p0, object p1)
        {
            return TextRes.GetString("ValidationUtils_PropertyDoesNotExistOnType", new object[] { p0, p1 });
        }

        internal static string ValidationUtils_RecursionDepthLimitReached(object p0)
        {
            return TextRes.GetString("ValidationUtils_RecursionDepthLimitReached", new object[] { p0 });
        }

        internal static string ValidationUtils_UnrecognizedTypeName(object p0)
        {
            return TextRes.GetString("ValidationUtils_UnrecognizedTypeName", new object[] { p0 });
        }

        internal static string ValidationUtils_UnsupportedPrimitiveType(object p0)
        {
            return TextRes.GetString("ValidationUtils_UnsupportedPrimitiveType", new object[] { p0 });
        }

        internal static string WriterValidationUtils_AssociationLinkInRequest(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_AssociationLinkInRequest", new object[] { p0 });
        }

        internal static string WriterValidationUtils_CollectionPropertiesMustNotHaveNullValue(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_CollectionPropertiesMustNotHaveNullValue", new object[] { p0 });
        }

        internal static string WriterValidationUtils_EntryTypeInExpandedLinkNotCompatibleWithNavigationPropertyType(object p0, object p1)
        {
            return TextRes.GetString("WriterValidationUtils_EntryTypeInExpandedLinkNotCompatibleWithNavigationPropertyType", new object[] { p0, p1 });
        }

        internal static string WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedContent(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedContent", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedMetadata(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedMetadata", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryContent(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryContent", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryMetadata(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryMetadata", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ExpandedLinkWithEntryPayloadAndFeedMetadata(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkWithEntryPayloadAndFeedMetadata", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ExpandedLinkWithFeedPayloadAndEntryMetadata(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ExpandedLinkWithFeedPayloadAndEntryMetadata", new object[] { p0 });
        }

        internal static string WriterValidationUtils_MessageWriterSettingsBaseUriMustBeNullOrAbsolute(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_MessageWriterSettingsBaseUriMustBeNullOrAbsolute", new object[] { p0 });
        }

        internal static string WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue(object p0, object p1)
        {
            return TextRes.GetString("WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue", new object[] { p0, p1 });
        }

        internal static string WriterValidationUtils_OperationInRequest(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_OperationInRequest", new object[] { p0 });
        }

        internal static string WriterValidationUtils_ResourceCollectionMustHaveUniqueName(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_ResourceCollectionMustHaveUniqueName", new object[] { p0 });
        }

        internal static string WriterValidationUtils_StreamPropertiesMustNotHaveNullValue(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_StreamPropertiesMustNotHaveNullValue", new object[] { p0 });
        }

        internal static string WriterValidationUtils_StreamPropertyInRequest(object p0)
        {
            return TextRes.GetString("WriterValidationUtils_StreamPropertyInRequest", new object[] { p0 });
        }

        internal static string XmlReaderExtension_InvalidNodeInStringValue(object p0)
        {
            return TextRes.GetString("XmlReaderExtension_InvalidNodeInStringValue", new object[] { p0 });
        }

        internal static string XmlReaderExtension_InvalidRootNode(object p0)
        {
            return TextRes.GetString("XmlReaderExtension_InvalidRootNode", new object[] { p0 });
        }

        internal static string AsyncBufferedStream_WriterDisposedWithoutFlush
        {
            get
            {
                return TextRes.GetString("AsyncBufferedStream_WriterDisposedWithoutFlush");
            }
        }

        internal static string EdmLibraryExtensions_CollectionItemCanBeOnlyPrimitiveOrComplex
        {
            get
            {
                return TextRes.GetString("EdmLibraryExtensions_CollectionItemCanBeOnlyPrimitiveOrComplex");
            }
        }

        internal static string ExceptionUtils_ArgumentStringNullOrEmpty
        {
            get
            {
                return TextRes.GetString("ExceptionUtils_ArgumentStringNullOrEmpty");
            }
        }

        internal static string HttpUtils_ContentTypeMissing
        {
            get
            {
                return TextRes.GetString("HttpUtils_ContentTypeMissing");
            }
        }

        internal static string HttpUtils_MediaTypeMissingParameterName
        {
            get
            {
                return TextRes.GetString("HttpUtils_MediaTypeMissingParameterName");
            }
        }

        internal static string JsonReader_EndOfInputWithOpenScope
        {
            get
            {
                return TextRes.GetString("JsonReader_EndOfInputWithOpenScope");
            }
        }

        internal static string JsonReader_MultipleTopLevelValues
        {
            get
            {
                return TextRes.GetString("JsonReader_MultipleTopLevelValues");
            }
        }

        internal static string JsonReader_UnexpectedEndOfString
        {
            get
            {
                return TextRes.GetString("JsonReader_UnexpectedEndOfString");
            }
        }

        internal static string JsonReader_UnrecognizedToken
        {
            get
            {
                return TextRes.GetString("JsonReader_UnrecognizedToken");
            }
        }

        internal static string ODataAtomCollectionDeserializer_TypeOrNullAttributeNotAllowed
        {
            get
            {
                return TextRes.GetString("ODataAtomCollectionDeserializer_TypeOrNullAttributeNotAllowed");
            }
        }

        internal static string ODataAtomCollectionWriter_CollectionNameMustNotBeNull
        {
            get
            {
                return TextRes.GetString("ODataAtomCollectionWriter_CollectionNameMustNotBeNull");
            }
        }

        internal static string ODataAtomEntryAndFeedDeserializer_ContentWithSourceLinkIsNotEmpty
        {
            get
            {
                return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_ContentWithSourceLinkIsNotEmpty");
            }
        }

        internal static string ODataAtomEntryAndFeedDeserializer_MultipleInlineElementsInLink
        {
            get
            {
                return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_MultipleInlineElementsInLink");
            }
        }

        internal static string ODataAtomEntryAndFeedDeserializer_StreamPropertyWithEmptyName
        {
            get
            {
                return TextRes.GetString("ODataAtomEntryAndFeedDeserializer_StreamPropertyWithEmptyName");
            }
        }

        internal static string ODataAtomReader_DeferredEntryInFeedNavigationLink
        {
            get
            {
                return TextRes.GetString("ODataAtomReader_DeferredEntryInFeedNavigationLink");
            }
        }

        internal static string ODataAtomReader_EntryXmlCustomizationCallbackReturnedSameInstance
        {
            get
            {
                return TextRes.GetString("ODataAtomReader_EntryXmlCustomizationCallbackReturnedSameInstance");
            }
        }

        internal static string ODataAtomReader_ExpandedEntryInFeedNavigationLink
        {
            get
            {
                return TextRes.GetString("ODataAtomReader_ExpandedEntryInFeedNavigationLink");
            }
        }

        internal static string ODataAtomReader_ExpandedFeedInEntryNavigationLink
        {
            get
            {
                return TextRes.GetString("ODataAtomReader_ExpandedFeedInEntryNavigationLink");
            }
        }

        internal static string ODataAtomReader_MediaLinkEntryMismatch
        {
            get
            {
                return TextRes.GetString("ODataAtomReader_MediaLinkEntryMismatch");
            }
        }

        internal static string ODataAtomReaderUtils_InvalidTypeName
        {
            get
            {
                return TextRes.GetString("ODataAtomReaderUtils_InvalidTypeName");
            }
        }

        internal static string ODataAtomServiceDocumentDeserializer_MissingWorkspaceElement
        {
            get
            {
                return TextRes.GetString("ODataAtomServiceDocumentDeserializer_MissingWorkspaceElement");
            }
        }

        internal static string ODataAtomServiceDocumentDeserializer_MultipleWorkspaceElementsFound
        {
            get
            {
                return TextRes.GetString("ODataAtomServiceDocumentDeserializer_MultipleWorkspaceElementsFound");
            }
        }

        internal static string ODataAtomServiceDocumentMetadataDeserializer_MultipleAcceptElementsFoundInCollection
        {
            get
            {
                return TextRes.GetString("ODataAtomServiceDocumentMetadataDeserializer_MultipleAcceptElementsFoundInCollection");
            }
        }

        internal static string ODataAtomWriter_StartEntryXmlCustomizationCallbackReturnedSameInstance
        {
            get
            {
                return TextRes.GetString("ODataAtomWriter_StartEntryXmlCustomizationCallbackReturnedSameInstance");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_AuthorMetadataMustNotContainNull
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_AuthorMetadataMustNotContainNull");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_CategoriesHrefWithOtherValues
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_CategoriesHrefWithOtherValues");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_CategoryMetadataMustNotContainNull
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_CategoryMetadataMustNotContainNull");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_CategoryMustSpecifyTerm
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_CategoryMustSpecifyTerm");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_ContributorMetadataMustNotContainNull
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_ContributorMetadataMustNotContainNull");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_LinkMetadataMustNotContainNull
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkMetadataMustNotContainNull");
            }
        }

        internal static string ODataAtomWriterMetadataUtils_LinkMustSpecifyHref
        {
            get
            {
                return TextRes.GetString("ODataAtomWriterMetadataUtils_LinkMustSpecifyHref");
            }
        }

        internal static string ODataBatchOperationMessage_VerifyNotCompleted
        {
            get
            {
                return TextRes.GetString("ODataBatchOperationMessage_VerifyNotCompleted");
            }
        }

        internal static string ODataBatchOperationStream_Disposed
        {
            get
            {
                return TextRes.GetString("ODataBatchOperationStream_Disposed");
            }
        }

        internal static string ODataBatchReader_AsyncCallOnSyncReader
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_AsyncCallOnSyncReader");
            }
        }

        internal static string ODataBatchReader_CannotCreateRequestOperationWhenReadingResponse
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_CannotCreateRequestOperationWhenReadingResponse");
            }
        }

        internal static string ODataBatchReader_CannotCreateResponseOperationWhenReadingRequest
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_CannotCreateResponseOperationWhenReadingRequest");
            }
        }

        internal static string ODataBatchReader_CannotUseReaderWhileOperationStreamActive
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_CannotUseReaderWhileOperationStreamActive");
            }
        }

        internal static string ODataBatchReader_NoMessageWasCreatedForOperation
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_NoMessageWasCreatedForOperation");
            }
        }

        internal static string ODataBatchReader_OperationRequestMessageAlreadyCreated
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_OperationRequestMessageAlreadyCreated");
            }
        }

        internal static string ODataBatchReader_OperationResponseMessageAlreadyCreated
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_OperationResponseMessageAlreadyCreated");
            }
        }

        internal static string ODataBatchReader_SyncCallOnAsyncReader
        {
            get
            {
                return TextRes.GetString("ODataBatchReader_SyncCallOnAsyncReader");
            }
        }

        internal static string ODataBatchReaderStream_MissingContentTypeHeader
        {
            get
            {
                return TextRes.GetString("ODataBatchReaderStream_MissingContentTypeHeader");
            }
        }

        internal static string ODataBatchReaderStream_NestedChangesetsAreNotSupported
        {
            get
            {
                return TextRes.GetString("ODataBatchReaderStream_NestedChangesetsAreNotSupported");
            }
        }

        internal static string ODataBatchWriter_AsyncCallOnSyncWriter
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_AsyncCallOnSyncWriter");
            }
        }

        internal static string ODataBatchWriter_CannotCompleteBatchWithActiveChangeSet
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotCompleteBatchWithActiveChangeSet");
            }
        }

        internal static string ODataBatchWriter_CannotCompleteChangeSetWithoutActiveChangeSet
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotCompleteChangeSetWithoutActiveChangeSet");
            }
        }

        internal static string ODataBatchWriter_CannotCreateRequestOperationWhenWritingResponse
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotCreateRequestOperationWhenWritingResponse");
            }
        }

        internal static string ODataBatchWriter_CannotCreateResponseOperationWhenWritingRequest
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotCreateResponseOperationWhenWritingRequest");
            }
        }

        internal static string ODataBatchWriter_CannotStartChangeSetWithActiveChangeSet
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotStartChangeSetWithActiveChangeSet");
            }
        }

        internal static string ODataBatchWriter_CannotWriteInStreamErrorForBatch
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_CannotWriteInStreamErrorForBatch");
            }
        }

        internal static string ODataBatchWriter_FlushOrFlushAsyncCalledInStreamRequestedState
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_FlushOrFlushAsyncCalledInStreamRequestedState");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromBatchCompleted
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromBatchCompleted");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromBatchStarted
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromBatchStarted");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromChangeSetCompleted
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromChangeSetCompleted");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromChangeSetStarted
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromChangeSetStarted");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromOperationContentStreamDisposed
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromOperationContentStreamDisposed");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromOperationContentStreamRequested
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromOperationContentStreamRequested");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromOperationCreated
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromOperationCreated");
            }
        }

        internal static string ODataBatchWriter_InvalidTransitionFromStart
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_InvalidTransitionFromStart");
            }
        }

        internal static string ODataBatchWriter_SyncCallOnAsyncWriter
        {
            get
            {
                return TextRes.GetString("ODataBatchWriter_SyncCallOnAsyncWriter");
            }
        }

        internal static string ODataCollectionReaderCore_AsyncCallOnSyncReader
        {
            get
            {
                return TextRes.GetString("ODataCollectionReaderCore_AsyncCallOnSyncReader");
            }
        }

        internal static string ODataCollectionReaderCore_SyncCallOnAsyncReader
        {
            get
            {
                return TextRes.GetString("ODataCollectionReaderCore_SyncCallOnAsyncReader");
            }
        }

        internal static string ODataCollectionWriterCore_AsyncCallOnSyncWriter
        {
            get
            {
                return TextRes.GetString("ODataCollectionWriterCore_AsyncCallOnSyncWriter");
            }
        }

        internal static string ODataCollectionWriterCore_CollectionsMustNotHaveEmptyName
        {
            get
            {
                return TextRes.GetString("ODataCollectionWriterCore_CollectionsMustNotHaveEmptyName");
            }
        }

        internal static string ODataCollectionWriterCore_SyncCallOnAsyncWriter
        {
            get
            {
                return TextRes.GetString("ODataCollectionWriterCore_SyncCallOnAsyncWriter");
            }
        }

        internal static string ODataErrorException_GeneralError
        {
            get
            {
                return TextRes.GetString("ODataErrorException_GeneralError");
            }
        }

        internal static string ODataException_GeneralError
        {
            get
            {
                return TextRes.GetString("ODataException_GeneralError");
            }
        }

        internal static string ODataJsonCollectionDeserializer_MissingResultsPropertyForCollection
        {
            get
            {
                return TextRes.GetString("ODataJsonCollectionDeserializer_MissingResultsPropertyForCollection");
            }
        }

        internal static string ODataJsonCollectionDeserializer_MultipleResultsPropertiesForCollection
        {
            get
            {
                return TextRes.GetString("ODataJsonCollectionDeserializer_MultipleResultsPropertiesForCollection");
            }
        }

        internal static string ODataJsonCollectionReader_ParsingWithoutMetadata
        {
            get
            {
                return TextRes.GetString("ODataJsonCollectionReader_ParsingWithoutMetadata");
            }
        }

        internal static string ODataJsonDeserializer_DataWrapperMultipleProperties
        {
            get
            {
                return TextRes.GetString("ODataJsonDeserializer_DataWrapperMultipleProperties");
            }
        }

        internal static string ODataJsonDeserializer_DataWrapperPropertyNotFound
        {
            get
            {
                return TextRes.GetString("ODataJsonDeserializer_DataWrapperPropertyNotFound");
            }
        }

        internal static string ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkUriCannotBeNull
        {
            get
            {
                return TextRes.GetString("ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkUriCannotBeNull");
            }
        }

        internal static string ODataJsonEntityReferenceLinkDeserializer_ExpectedEntityReferenceLinksResultsPropertyNotFound
        {
            get
            {
                return TextRes.GetString("ODataJsonEntityReferenceLinkDeserializer_ExpectedEntityReferenceLinksResultsPropertyNotFound");
            }
        }

        internal static string ODataJsonEntityReferenceLinkDeserializer_MultipleUriPropertiesInEntityReferenceLink
        {
            get
            {
                return TextRes.GetString("ODataJsonEntityReferenceLinkDeserializer_MultipleUriPropertiesInEntityReferenceLink");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_CannotParseStreamReference
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_CannotParseStreamReference");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_CannotReadNavigationPropertyValue
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_CannotReadNavigationPropertyValue");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_DeferredLinkMissingUri
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_DeferredLinkMissingUri");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_DeferredLinkUriCannotBeNull
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_DeferredLinkUriCannotBeNull");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_ExpectedFeedResultsPropertyNotFound
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_ExpectedFeedResultsPropertyNotFound");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleFeedResultsPropertiesFound
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleFeedResultsPropertiesFound");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesInEntryValue
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleMetadataPropertiesInEntryValue");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_MultipleUriPropertiesInDeferredLink
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_MultipleUriPropertiesInDeferredLink");
            }
        }

        internal static string ODataJsonEntryAndFeedDeserializer_StreamPropertyInRequest
        {
            get
            {
                return TextRes.GetString("ODataJsonEntryAndFeedDeserializer_StreamPropertyInRequest");
            }
        }

        internal static string ODataJsonPropertyAndValueDeserializer_CannotReadSpatialPropertyValue
        {
            get
            {
                return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_CannotReadSpatialPropertyValue");
            }
        }

        internal static string ODataJsonPropertyAndValueDeserializer_CollectionWithoutResults
        {
            get
            {
                return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_CollectionWithoutResults");
            }
        }

        internal static string ODataJsonPropertyAndValueDeserializer_InvalidTopLevelPropertyPayload
        {
            get
            {
                return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_InvalidTopLevelPropertyPayload");
            }
        }

        internal static string ODataJsonPropertyAndValueDeserializer_MultipleMetadataPropertiesInComplexValue
        {
            get
            {
                return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_MultipleMetadataPropertiesInComplexValue");
            }
        }

        internal static string ODataJsonPropertyAndValueDeserializer_TopLevelPropertyWithoutMetadata
        {
            get
            {
                return TextRes.GetString("ODataJsonPropertyAndValueDeserializer_TopLevelPropertyWithoutMetadata");
            }
        }

        internal static string ODataJsonReader_ParsingWithoutMetadata
        {
            get
            {
                return TextRes.GetString("ODataJsonReader_ParsingWithoutMetadata");
            }
        }

        internal static string ODataJsonReaderUtils_CannotConvertInt64OrDecimal
        {
            get
            {
                return TextRes.GetString("ODataJsonReaderUtils_CannotConvertInt64OrDecimal");
            }
        }

        internal static string ODataJsonServiceDocumentDeserializer_MultipleEntitySetsPropertiesForServiceDocument
        {
            get
            {
                return TextRes.GetString("ODataJsonServiceDocumentDeserializer_MultipleEntitySetsPropertiesForServiceDocument");
            }
        }

        internal static string ODataJsonServiceDocumentDeserializer_NoEntitySetsPropertyForServiceDocument
        {
            get
            {
                return TextRes.GetString("ODataJsonServiceDocumentDeserializer_NoEntitySetsPropertyForServiceDocument");
            }
        }

        internal static string ODataMessage_MustNotModifyMessage
        {
            get
            {
                return TextRes.GetString("ODataMessage_MustNotModifyMessage");
            }
        }

        internal static string ODataMessageReader_DetectPayloadKindMultipleTimes
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_DetectPayloadKindMultipleTimes");
            }
        }

        internal static string ODataMessageReader_EntityReferenceLinksInRequestNotAllowed
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_EntityReferenceLinksInRequestNotAllowed");
            }
        }

        internal static string ODataMessageReader_ErrorPayloadInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ErrorPayloadInRequest");
            }
        }

        internal static string ODataMessageReader_ExpectedPropertyTypeEntityKind
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ExpectedPropertyTypeEntityKind");
            }
        }

        internal static string ODataMessageReader_ExpectedPropertyTypeStream
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ExpectedPropertyTypeStream");
            }
        }

        internal static string ODataMessageReader_GetFormatCalledBeforeReadingStarted
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_GetFormatCalledBeforeReadingStarted");
            }
        }

        internal static string ODataMessageReader_MetadataDocumentInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_MetadataDocumentInRequest");
            }
        }

        internal static string ODataMessageReader_NoneOrEmptyContentTypeHeader
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_NoneOrEmptyContentTypeHeader");
            }
        }

        internal static string ODataMessageReader_ParameterPayloadInResponse
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ParameterPayloadInResponse");
            }
        }

        internal static string ODataMessageReader_PayloadKindDetectionInServerMode
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_PayloadKindDetectionInServerMode");
            }
        }

        internal static string ODataMessageReader_PayloadKindDetectionRunning
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_PayloadKindDetectionRunning");
            }
        }

        internal static string ODataMessageReader_ReaderAlreadyUsed
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ReaderAlreadyUsed");
            }
        }

        internal static string ODataMessageReader_ServiceDocumentInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageReader_ServiceDocumentInRequest");
            }
        }

        internal static string ODataMessageWriter_CannotSpecifyFunctionImportWithoutModel
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_CannotSpecifyFunctionImportWithoutModel");
            }
        }

        internal static string ODataMessageWriter_CannotWriteInStreamErrorForRawValues
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_CannotWriteInStreamErrorForRawValues");
            }
        }

        internal static string ODataMessageWriter_CannotWriteMetadataWithoutModel
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_CannotWriteMetadataWithoutModel");
            }
        }

        internal static string ODataMessageWriter_CannotWriteNullInRawFormat
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_CannotWriteNullInRawFormat");
            }
        }

        internal static string ODataMessageWriter_EntityReferenceLinksInRequestNotAllowed
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_EntityReferenceLinksInRequestNotAllowed");
            }
        }

        internal static string ODataMessageWriter_ErrorPayloadInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_ErrorPayloadInRequest");
            }
        }

        internal static string ODataMessageWriter_MetadataDocumentInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_MetadataDocumentInRequest");
            }
        }

        internal static string ODataMessageWriter_ServiceDocumentInRequest
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_ServiceDocumentInRequest");
            }
        }

        internal static string ODataMessageWriter_WriteErrorAlreadyCalled
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_WriteErrorAlreadyCalled");
            }
        }

        internal static string ODataMessageWriter_WriterAlreadyUsed
        {
            get
            {
                return TextRes.GetString("ODataMessageWriter_WriterAlreadyUsed");
            }
        }

        internal static string ODataMessageWriterSettings_MessageWriterSettingsXmlCustomizationCallbacksMustBeSpecifiedBoth
        {
            get
            {
                return TextRes.GetString("ODataMessageWriterSettings_MessageWriterSettingsXmlCustomizationCallbacksMustBeSpecifiedBoth");
            }
        }

        internal static string ODataParameterReaderCore_AsyncCallOnSyncReader
        {
            get
            {
                return TextRes.GetString("ODataParameterReaderCore_AsyncCallOnSyncReader");
            }
        }

        internal static string ODataParameterReaderCore_SyncCallOnAsyncReader
        {
            get
            {
                return TextRes.GetString("ODataParameterReaderCore_SyncCallOnAsyncReader");
            }
        }

        internal static string ODataParameterWriter_CannotCreateParameterWriterOnResponseMessage
        {
            get
            {
                return TextRes.GetString("ODataParameterWriter_CannotCreateParameterWriterOnResponseMessage");
            }
        }

        internal static string ODataParameterWriter_InStreamErrorNotSupported
        {
            get
            {
                return TextRes.GetString("ODataParameterWriter_InStreamErrorNotSupported");
            }
        }

        internal static string ODataParameterWriterCore_AsyncCallOnSyncWriter
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_AsyncCallOnSyncWriter");
            }
        }

        internal static string ODataParameterWriterCore_CannotWriteEnd
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_CannotWriteEnd");
            }
        }

        internal static string ODataParameterWriterCore_CannotWriteInErrorOrCompletedState
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_CannotWriteInErrorOrCompletedState");
            }
        }

        internal static string ODataParameterWriterCore_CannotWriteParameter
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_CannotWriteParameter");
            }
        }

        internal static string ODataParameterWriterCore_CannotWriteStart
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_CannotWriteStart");
            }
        }

        internal static string ODataParameterWriterCore_SyncCallOnAsyncWriter
        {
            get
            {
                return TextRes.GetString("ODataParameterWriterCore_SyncCallOnAsyncWriter");
            }
        }

        internal static string ODataReaderCore_AsyncCallOnSyncReader
        {
            get
            {
                return TextRes.GetString("ODataReaderCore_AsyncCallOnSyncReader");
            }
        }

        internal static string ODataReaderCore_SyncCallOnAsyncReader
        {
            get
            {
                return TextRes.GetString("ODataReaderCore_SyncCallOnAsyncReader");
            }
        }

        internal static string ODataRequestMessage_AsyncNotAvailable
        {
            get
            {
                return TextRes.GetString("ODataRequestMessage_AsyncNotAvailable");
            }
        }

        internal static string ODataRequestMessage_MessageStreamIsNull
        {
            get
            {
                return TextRes.GetString("ODataRequestMessage_MessageStreamIsNull");
            }
        }

        internal static string ODataRequestMessage_StreamTaskIsNull
        {
            get
            {
                return TextRes.GetString("ODataRequestMessage_StreamTaskIsNull");
            }
        }

        internal static string ODataResponseMessage_AsyncNotAvailable
        {
            get
            {
                return TextRes.GetString("ODataResponseMessage_AsyncNotAvailable");
            }
        }

        internal static string ODataResponseMessage_MessageStreamIsNull
        {
            get
            {
                return TextRes.GetString("ODataResponseMessage_MessageStreamIsNull");
            }
        }

        internal static string ODataResponseMessage_StreamTaskIsNull
        {
            get
            {
                return TextRes.GetString("ODataResponseMessage_StreamTaskIsNull");
            }
        }

        internal static string ODataUriUtils_ConvertFromUriLiteralTypeRefWithoutModel
        {
            get
            {
                return TextRes.GetString("ODataUriUtils_ConvertFromUriLiteralTypeRefWithoutModel");
            }
        }

        internal static string ODataUtils_CannotSaveAnnotationsToBuiltInModel
        {
            get
            {
                return TextRes.GetString("ODataUtils_CannotSaveAnnotationsToBuiltInModel");
            }
        }

        internal static string ODataUtils_IsAlwaysBindableAnnotationSetForANonBindableFunctionImport
        {
            get
            {
                return TextRes.GetString("ODataUtils_IsAlwaysBindableAnnotationSetForANonBindableFunctionImport");
            }
        }

        internal static string ODataUtils_NullValueForHttpMethodAnnotation
        {
            get
            {
                return TextRes.GetString("ODataUtils_NullValueForHttpMethodAnnotation");
            }
        }

        internal static string ODataUtils_NullValueForMimeTypeAnnotation
        {
            get
            {
                return TextRes.GetString("ODataUtils_NullValueForMimeTypeAnnotation");
            }
        }

        internal static string ODataUtils_UnexpectedIsAlwaysBindableAnnotationInANonBindableFunctionImport
        {
            get
            {
                return TextRes.GetString("ODataUtils_UnexpectedIsAlwaysBindableAnnotationInANonBindableFunctionImport");
            }
        }

        internal static string ODataUtils_UnsupportedVersionNumber
        {
            get
            {
                return TextRes.GetString("ODataUtils_UnsupportedVersionNumber");
            }
        }

        internal static string ODataVersionChecker_ProtocolVersion3IsNotSupported
        {
            get
            {
                return TextRes.GetString("ODataVersionChecker_ProtocolVersion3IsNotSupported");
            }
        }

        internal static string ODataWriter_NavigationLinkMustSpecifyUrl
        {
            get
            {
                return TextRes.GetString("ODataWriter_NavigationLinkMustSpecifyUrl");
            }
        }

        internal static string ODataWriterCore_AsyncCallOnSyncWriter
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_AsyncCallOnSyncWriter");
            }
        }

        internal static string ODataWriterCore_CannotWriteTopLevelEntryWithFeedWriter
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_CannotWriteTopLevelEntryWithFeedWriter");
            }
        }

        internal static string ODataWriterCore_CannotWriteTopLevelFeedWithEntryWriter
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_CannotWriteTopLevelFeedWithEntryWriter");
            }
        }

        internal static string ODataWriterCore_DeferredLinkInRequest
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_DeferredLinkInRequest");
            }
        }

        internal static string ODataWriterCore_EntityReferenceLinkInResponse
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_EntityReferenceLinkInResponse");
            }
        }

        internal static string ODataWriterCore_EntityReferenceLinkWithoutNavigationLink
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_EntityReferenceLinkWithoutNavigationLink");
            }
        }

        internal static string ODataWriterCore_InlineCountInRequest
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_InlineCountInRequest");
            }
        }

        internal static string ODataWriterCore_LinkMustSpecifyIsCollection
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_LinkMustSpecifyIsCollection");
            }
        }

        internal static string ODataWriterCore_MultipleItemsInNavigationLinkContent
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_MultipleItemsInNavigationLinkContent");
            }
        }

        internal static string ODataWriterCore_OnlyTopLevelFeedsSupportInlineCount
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_OnlyTopLevelFeedsSupportInlineCount");
            }
        }

        internal static string ODataWriterCore_SyncCallOnAsyncWriter
        {
            get
            {
                return TextRes.GetString("ODataWriterCore_SyncCallOnAsyncWriter");
            }
        }

        internal static string ReaderValidationUtils_EntityReferenceLinkMissingUri
        {
            get
            {
                return TextRes.GetString("ReaderValidationUtils_EntityReferenceLinkMissingUri");
            }
        }

        internal static string ReaderValidationUtils_EntryWithoutType
        {
            get
            {
                return TextRes.GetString("ReaderValidationUtils_EntryWithoutType");
            }
        }

        internal static string ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedOnRequest
        {
            get
            {
                return TextRes.GetString("ReaderValidationUtils_UndeclaredPropertyBehaviorKindSpecifiedOnRequest");
            }
        }

        internal static string ReaderValidationUtils_ValueWithoutType
        {
            get
            {
                return TextRes.GetString("ReaderValidationUtils_ValueWithoutType");
            }
        }

        internal static string ValidationUtils_AssociationLinkMustSpecifyName
        {
            get
            {
                return TextRes.GetString("ValidationUtils_AssociationLinkMustSpecifyName");
            }
        }

        internal static string ValidationUtils_AssociationLinkMustSpecifyUrl
        {
            get
            {
                return TextRes.GetString("ValidationUtils_AssociationLinkMustSpecifyUrl");
            }
        }

        internal static string ValidationUtils_LinkMustSpecifyName
        {
            get
            {
                return TextRes.GetString("ValidationUtils_LinkMustSpecifyName");
            }
        }

        internal static string ValidationUtils_NestedCollectionsAreNotSupported
        {
            get
            {
                return TextRes.GetString("ValidationUtils_NestedCollectionsAreNotSupported");
            }
        }

        internal static string ValidationUtils_NonStreamingCollectionElementsMustNotBeNull
        {
            get
            {
                return TextRes.GetString("ValidationUtils_NonStreamingCollectionElementsMustNotBeNull");
            }
        }

        internal static string ValidationUtils_ResourceCollectionMustSpecifyUrl
        {
            get
            {
                return TextRes.GetString("ValidationUtils_ResourceCollectionMustSpecifyUrl");
            }
        }

        internal static string ValidationUtils_ResourceCollectionUrlMustNotBeNull
        {
            get
            {
                return TextRes.GetString("ValidationUtils_ResourceCollectionUrlMustNotBeNull");
            }
        }

        internal static string ValidationUtils_StreamReferenceValuesNotSupportedInCollections
        {
            get
            {
                return TextRes.GetString("ValidationUtils_StreamReferenceValuesNotSupportedInCollections");
            }
        }

        internal static string ValidationUtils_TypeNameMustNotBeEmpty
        {
            get
            {
                return TextRes.GetString("ValidationUtils_TypeNameMustNotBeEmpty");
            }
        }

        internal static string ValidationUtils_WorkspaceCollectionsMustNotContainNullItem
        {
            get
            {
                return TextRes.GetString("ValidationUtils_WorkspaceCollectionsMustNotContainNullItem");
            }
        }

        internal static string WriterValidationUtils_DefaultStreamWithContentTypeWithoutReadLink
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_DefaultStreamWithContentTypeWithoutReadLink");
            }
        }

        internal static string WriterValidationUtils_DefaultStreamWithReadLinkWithoutContentType
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_DefaultStreamWithReadLinkWithoutContentType");
            }
        }

        internal static string WriterValidationUtils_EntityReferenceLinksLinkMustNotBeNull
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_EntityReferenceLinksLinkMustNotBeNull");
            }
        }

        internal static string WriterValidationUtils_EntityReferenceLinkUrlMustNotBeNull
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_EntityReferenceLinkUrlMustNotBeNull");
            }
        }

        internal static string WriterValidationUtils_EntriesMustHaveNonEmptyId
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_EntriesMustHaveNonEmptyId");
            }
        }

        internal static string WriterValidationUtils_FeedsMustHaveNonEmptyId
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_FeedsMustHaveNonEmptyId");
            }
        }

        internal static string WriterValidationUtils_MissingTypeNameWithMetadata
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_MissingTypeNameWithMetadata");
            }
        }

        internal static string WriterValidationUtils_NextPageLinkInRequest
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_NextPageLinkInRequest");
            }
        }

        internal static string WriterValidationUtils_PropertiesMustHaveNonEmptyName
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_PropertiesMustHaveNonEmptyName");
            }
        }

        internal static string WriterValidationUtils_PropertyMustNotBeNull
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_PropertyMustNotBeNull");
            }
        }

        internal static string WriterValidationUtils_StreamReferenceValueEmptyContentType
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_StreamReferenceValueEmptyContentType");
            }
        }

        internal static string WriterValidationUtils_StreamReferenceValueMustHaveEditLinkOrReadLink
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_StreamReferenceValueMustHaveEditLinkOrReadLink");
            }
        }

        internal static string WriterValidationUtils_StreamReferenceValueMustHaveEditLinkToHaveETag
        {
            get
            {
                return TextRes.GetString("WriterValidationUtils_StreamReferenceValueMustHaveEditLinkToHaveETag");
            }
        }
    }
}

