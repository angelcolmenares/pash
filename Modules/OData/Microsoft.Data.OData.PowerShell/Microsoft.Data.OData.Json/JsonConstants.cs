namespace Microsoft.Data.OData.Json
{
    using System;

    internal static class JsonConstants
    {
        internal const string ArrayElementSeparator = ",";
        internal const string EndArrayScope = "]";
        internal const string EndObjectScope = "}";
        internal const string JsonFalseLiteral = "false";
        internal const string JsonNullLiteral = "null";
        internal const string JsonTrueLiteral = "true";
        internal const string NameValueSeparator = ":";
        internal const string ObjectMemberSeparator = ",";
        internal const string ODataActionsMetadataName = "actions";
        internal const string ODataCountName = "__count";
        internal const string ODataDataWrapper = "\"d\":";
        internal const string ODataDataWrapperPropertyName = "d";
        internal const string ODataDateTimeFormat = @"\/Date({0})\/";
        internal const string ODataDateTimeOffsetFormat = @"\/Date({0}{1}{2:D4})\/";
        internal const string ODataDateTimeOffsetPlusSign = "+";
        internal const string ODataDeferredName = "__deferred";
        internal const string ODataEntryIdName = "id";
        internal const string ODataErrorCodeName = "code";
        internal const string ODataErrorInnerErrorInnerErrorName = "internalexception";
        internal const string ODataErrorInnerErrorMessageName = "message";
        internal const string ODataErrorInnerErrorName = "innererror";
        internal const string ODataErrorInnerErrorStackTraceName = "stacktrace";
        internal const string ODataErrorInnerErrorTypeNameName = "type";
        internal const string ODataErrorMessageLanguageName = "lang";
        internal const string ODataErrorMessageName = "message";
        internal const string ODataErrorMessageValueName = "value";
        internal const string ODataErrorName = "error";
        internal const string ODataFunctionsMetadataName = "functions";
        internal const string ODataMetadataContentTypeName = "content_type";
        internal const string ODataMetadataEditMediaName = "edit_media";
        internal const string ODataMetadataETagName = "etag";
        internal const string ODataMetadataMediaETagName = "media_etag";
        internal const string ODataMetadataMediaResourceName = "__mediaresource";
        internal const string ODataMetadataMediaUriName = "media_src";
        internal const string ODataMetadataName = "__metadata";
        internal const string ODataMetadataPropertiesAssociationUriName = "associationuri";
        internal const string ODataMetadataPropertiesName = "properties";
        internal const string ODataMetadataTypeName = "type";
        internal const string ODataMetadataUriName = "uri";
        internal const string ODataNavigationLinkUriName = "uri";
        internal const string ODataNextLinkName = "__next";
        internal const string ODataOperationMetadataName = "metadata";
        internal const string ODataOperationTargetName = "target";
        internal const string ODataOperationTitleName = "title";
        internal const string ODataResultsName = "results";
        internal const string ODataServiceDocumentEntitySetsName = "EntitySets";
        internal const string ODataUriName = "uri";
        internal const char QuoteCharacter = '"';
        internal const string StartArrayScope = "[";
        internal const string StartObjectScope = "{";
    }
}

