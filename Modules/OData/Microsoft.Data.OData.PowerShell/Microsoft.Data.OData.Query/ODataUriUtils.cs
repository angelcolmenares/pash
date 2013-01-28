namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Spatial;

    internal static class ODataUriUtils
    {
        public static object ConvertFromUriLiteral(string value, ODataVersion version)
        {
            return ConvertFromUriLiteral(value, version, null, null);
        }

        public static object ConvertFromUriLiteral(string value, ODataVersion version, IEdmModel model, IEdmTypeReference typeReference)
        {
            Exception exception;
            ExpressionToken token;
            ExceptionUtils.CheckArgumentNotNull<string>(value, "value");
            if ((typeReference != null) && (model == null))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertFromUriLiteralTypeRefWithoutModel);
            }
            if (model == null)
            {
                model = EdmCoreModel.Instance;
            }
            ExpressionLexer lexer = new ExpressionLexer(value, false);
            if (!lexer.TryPeekNextToken(out token, out exception))
            {
                return ODataUriConversionUtils.ConvertFromComplexOrCollectionValue(value, version, model, typeReference);
            }
            object primitiveValue = lexer.ReadLiteralToken();
            if (typeReference != null)
            {
                primitiveValue = ODataUriConversionUtils.VerifyAndCoerceUriPrimitiveLiteral(primitiveValue, model, typeReference, version);
            }
            if (primitiveValue is ISpatial)
            {
                ODataVersionChecker.CheckSpatialValue(version);
            }
            return primitiveValue;
        }

        public static string ConvertToUriLiteral(object value, ODataVersion version)
        {
            return ConvertToUriLiteral(value, version, null);
        }

        public static string ConvertToUriLiteral(object value, ODataVersion version, IEdmModel model)
        {
            if (value == null)
            {
                value = new ODataUriNullValue();
            }
            if (model == null)
            {
                model = EdmCoreModel.Instance;
            }
            ODataUriNullValue nullValue = value as ODataUriNullValue;
            if (nullValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriNullValue(nullValue, model);
            }
            ODataCollectionValue collectionValue = value as ODataCollectionValue;
            if (collectionValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriCollectionLiteral(collectionValue, model, version);
            }
            ODataComplexValue complexValue = value as ODataComplexValue;
            if (complexValue != null)
            {
                return ODataUriConversionUtils.ConvertToUriComplexLiteral(complexValue, model, version);
            }
            return ODataUriConversionUtils.ConvertToUriPrimitiveLiteral(value, version);
        }
    }
}

