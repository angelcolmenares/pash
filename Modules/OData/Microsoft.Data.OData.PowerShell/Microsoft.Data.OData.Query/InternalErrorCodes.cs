namespace Microsoft.Data.OData.Query
{
    using System;

    internal enum InternalErrorCodes
    {
        TypePromotionUtils_GetFunctionSignatures_Binary_UnreachableCodepath,
        TypePromotionUtils_GetFunctionSignatures_Unary_UnreachableCodepath,
        MetadataBinder_BindServiceOperation,
        QueryExpressionTranslator_TranslateBinaryOperator_UnreachableCodepath,
        UriPrimitiveTypeParser_HexCharToNibble,
        UriQueryExpressionParser_ParseComparison,
        UriPrimitiveTypeParser_TryUriStringToPrimitive,
        QueryNodeUtils_BinaryOperatorResultType_UnreachableCodepath,
        QueryExpressionTranslator_TranslateUnaryOperator_UnreachableCodepath,
        BinaryOperator_GetOperator_UnreachableCodePath,
        ODataUriBuilder_WriteUnary_UnreachableCodePath,
        ODataUriBuilderUtils_ToText_InlineCountKind_UnreachableCodePath
    }
}

