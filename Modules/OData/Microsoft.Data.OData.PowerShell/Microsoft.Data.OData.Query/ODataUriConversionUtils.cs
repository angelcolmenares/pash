namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Json;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Spatial;
    using System.Text;
    using System.Xml;

    internal static class ODataUriConversionUtils
    {
        private static string AppendDecimalMarkerToDouble(string input)
        {
            if ((!input.Contains(".") && !input.Contains("INF")) && !input.Contains("NaN"))
            {
                return (input + ".0");
            }
            return input;
        }

        private static object CoerceNumericType(object primitiveValue, IEdmPrimitiveType targetEdmType)
        {
            ExceptionUtils.CheckArgumentNotNull<object>(primitiveValue, "primitiveValue");
            ExceptionUtils.CheckArgumentNotNull<IEdmPrimitiveType>(targetEdmType, "targetEdmType");
            TypeCode typeCode = Microsoft.Data.OData.PlatformHelper.GetTypeCode(primitiveValue.GetType());
            EdmPrimitiveTypeKind primitiveKind = targetEdmType.PrimitiveKind;
            switch (typeCode)
            {
                case TypeCode.SByte:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                            return Convert.ToDecimal((sbyte) primitiveValue);

                        case EdmPrimitiveTypeKind.Double:
                            return Convert.ToDouble((sbyte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int16:
                            return Convert.ToInt16((sbyte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int32:
                            return Convert.ToInt32((sbyte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int64:
                            return Convert.ToInt64((sbyte) primitiveValue);

                        case EdmPrimitiveTypeKind.SByte:
                            return primitiveValue;

                        case EdmPrimitiveTypeKind.Single:
                            return Convert.ToSingle((sbyte) primitiveValue);
                    }
                    break;

                case TypeCode.Byte:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Byte:
                            return primitiveValue;

                        case EdmPrimitiveTypeKind.Decimal:
                            return Convert.ToDecimal((byte) primitiveValue);

                        case EdmPrimitiveTypeKind.Double:
                            return Convert.ToDouble((byte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int16:
                            return Convert.ToInt16((byte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int32:
                            return Convert.ToInt32((byte) primitiveValue);

                        case EdmPrimitiveTypeKind.Int64:
                            return Convert.ToInt64((byte) primitiveValue);

                        case EdmPrimitiveTypeKind.Single:
                            return Convert.ToSingle((byte) primitiveValue);
                    }
                    break;

                case TypeCode.Int16:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                            return Convert.ToDecimal((short) primitiveValue);

                        case EdmPrimitiveTypeKind.Double:
                            return Convert.ToDouble((short) primitiveValue);

                        case EdmPrimitiveTypeKind.Int16:
                            return primitiveValue;

                        case EdmPrimitiveTypeKind.Int32:
                            return Convert.ToInt32((short) primitiveValue);

                        case EdmPrimitiveTypeKind.Int64:
                            return Convert.ToInt64((short) primitiveValue);

                        case EdmPrimitiveTypeKind.Single:
                            return Convert.ToSingle((short) primitiveValue);
                    }
                    break;

                case TypeCode.Int32:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                            return Convert.ToDecimal((int) primitiveValue);

                        case EdmPrimitiveTypeKind.Double:
                            return Convert.ToDouble((int) primitiveValue);

                        case EdmPrimitiveTypeKind.Int32:
                            return primitiveValue;

                        case EdmPrimitiveTypeKind.Int64:
                            return Convert.ToInt64((int) primitiveValue);

                        case EdmPrimitiveTypeKind.Single:
                            return Convert.ToSingle((int) primitiveValue);
                    }
                    break;

                case TypeCode.Int64:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                            return Convert.ToDecimal((long) primitiveValue);

                        case EdmPrimitiveTypeKind.Double:
                            return Convert.ToDouble((long) primitiveValue);

                        case EdmPrimitiveTypeKind.Int64:
                            return primitiveValue;

                        case EdmPrimitiveTypeKind.Single:
                            return Convert.ToSingle((long) primitiveValue);
                    }
                    break;

                case TypeCode.Single:
                {
                    EdmPrimitiveTypeKind kind7 = primitiveKind;
                    if (kind7 == EdmPrimitiveTypeKind.Double)
                    {
                        return Convert.ToDouble((float) primitiveValue);
                    }
                    if (kind7 != EdmPrimitiveTypeKind.Single)
                    {
                        break;
                    }
                    return primitiveValue;
                }
                case TypeCode.Double:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Double:
                            break;
                    }
                    return primitiveValue;

                case TypeCode.Decimal:
                    switch (primitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Decimal:
                            break;
                    }
                    return primitiveValue;
            }
            return null;
        }

        private static string ConvertByteArrayToKeyString(byte[] byteArray)
        {
            StringBuilder builder = new StringBuilder(3 + (byteArray.Length * 2));
            builder.Append("X");
            builder.Append("'");
            for (int i = 0; i < byteArray.Length; i++)
            {
                builder.Append(byteArray[i].ToString("X2", CultureInfo.InvariantCulture));
            }
            builder.Append("'");
            return builder.ToString();
        }

        internal static object ConvertFromComplexOrCollectionValue(string value, ODataVersion version, IEdmModel model, IEdmTypeReference typeReference)
        {
            object obj3;
            ODataMessageReaderSettings messageReaderSettings = new ODataMessageReaderSettings();
            using (StringReader reader = new StringReader(value))
            {
                using (ODataJsonInputContext context = new ODataJsonInputContext(ODataFormat.VerboseJson, reader, messageReaderSettings, version, false, true, model, null))
                {
                    ODataJsonPropertyAndValueDeserializer deserializer = new ODataJsonPropertyAndValueDeserializer(context);
                    deserializer.ReadPayloadStart(false);
                    object obj2 = deserializer.ReadNonEntityValue(typeReference, null, null, true);
                    deserializer.ReadPayloadEnd(false);
                    obj3 = obj2;
                }
            }
            return obj3;
        }

        internal static string ConvertToUriCollectionLiteral(ODataCollectionValue collectionValue, IEdmModel model, ODataVersion version)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataCollectionValue>(collectionValue, "collectionValue");
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ODataVersionChecker.CheckCollectionValue(version);
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                JsonWriter jsonWriter = new JsonWriter(writer, false);
                ODataMessageWriterSettings messageWriterSettings = new ODataMessageWriterSettings {
                    Version = new ODataVersion?(version)
                };
                using (ODataJsonOutputContext context = ODataJsonOutputContext.Create(ODataFormat.VerboseJson, jsonWriter, messageWriterSettings, false, model, null))
                {
                    new ODataJsonPropertyAndValueSerializer(context).WriteCollectionValue(collectionValue, null, false);
                }
            }
            return sb.ToString();
        }

        internal static string ConvertToUriComplexLiteral(ODataComplexValue complexValue, IEdmModel model, ODataVersion version)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataComplexValue>(complexValue, "complexValue");
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                JsonWriter jsonWriter = new JsonWriter(writer, false);
                bool writingResponse = false;
                ODataMessageWriterSettings messageWriterSettings = new ODataMessageWriterSettings {
                    Version = new ODataVersion?(version)
                };
                using (ODataJsonOutputContext context = ODataJsonOutputContext.Create(ODataFormat.VerboseJson, jsonWriter, messageWriterSettings, writingResponse, model, null))
                {
                    ODataJsonPropertyAndValueSerializer serializer = new ODataJsonPropertyAndValueSerializer(context);
                    serializer.WriteComplexValue(complexValue, null, true, serializer.CreateDuplicatePropertyNamesChecker(), null);
                }
            }
            return sb.ToString();
        }

        internal static string ConvertToUriNullValue(ODataUriNullValue nullValue, IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataUriNullValue>(nullValue, "nullValue");
            if (nullValue.TypeName != null)
            {
                ValidationUtils.ValidateTypeName(model, nullValue.TypeName);
                StringBuilder builder = new StringBuilder();
                builder.Append("null");
                builder.Append("'");
                builder.Append(nullValue.TypeName);
                builder.Append("'");
                return builder.ToString();
            }
            return "null";
        }

        internal static string ConvertToUriPrimitiveLiteral(object value, ODataVersion version)
        {
            ExceptionUtils.CheckArgumentNotNull<object>(value, "value");
            StringBuilder builder = new StringBuilder();
            Type nullableType = value.GetType();
            nullableType = Nullable.GetUnderlyingType(nullableType) ?? nullableType;
            switch (Microsoft.Data.OData.PlatformHelper.GetTypeCode(nullableType))
            {
                case TypeCode.Object:
                    if (!(nullableType == typeof(byte[])))
                    {
                        if (nullableType == typeof(Guid))
                        {
                            builder.Append("guid");
                            builder.Append("'");
                            builder.Append(value.ToString());
                            builder.Append("'");
                        }
                        else if (nullableType == typeof(DateTimeOffset))
                        {
                            builder.Append("datetimeoffset");
                            builder.Append("'");
                            builder.Append(XmlConvert.ToString((DateTimeOffset) value));
                            builder.Append("'");
                        }
                        else if (nullableType == typeof(TimeSpan))
                        {
                            builder.Append("time");
                            builder.Append("'");
                            builder.Append(XmlConvert.ToString((TimeSpan) value));
                            builder.Append("'");
                        }
                        else if (typeof(Geography).IsAssignableFrom(nullableType))
                        {
                            ODataVersionChecker.CheckSpatialValue(version);
                            builder.Append("geography");
                            builder.Append("'");
                            builder.Append(WellKnownTextSqlFormatter.Create(true).Write((Geography) value));
                            builder.Append("'");
                        }
                        else
                        {
                            if (!typeof(Geometry).IsAssignableFrom(nullableType))
                            {
                                throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertToUriLiteralUnsupportedType(nullableType.ToString()));
                            }
                            ODataVersionChecker.CheckSpatialValue(version);
                            builder.Append("geometry");
                            builder.Append("'");
                            builder.Append(WellKnownTextSqlFormatter.Create(true).Write((Geometry) value));
                            builder.Append("'");
                        }
                        break;
                    }
                    builder.Append(ConvertByteArrayToKeyString((byte[]) value));
                    break;

                case TypeCode.Boolean:
                    builder.Append(XmlConvert.ToString((bool) value));
                    return builder.ToString();

                case TypeCode.SByte:
                    builder.Append(XmlConvert.ToString((sbyte) value));
                    return builder.ToString();

                case TypeCode.Byte:
                    builder.Append(XmlConvert.ToString((byte) value));
                    return builder.ToString();

                case TypeCode.Int16:
                    builder.Append(XmlConvert.ToString((short) value));
                    return builder.ToString();

                case TypeCode.Int32:
                    builder.Append(XmlConvert.ToString((int) value));
                    return builder.ToString();

                case TypeCode.Int64:
                    builder.Append(XmlConvert.ToString((long) value));
                    builder.Append("L");
                    return builder.ToString();

                case TypeCode.Single:
                    builder.Append(XmlConvert.ToString((float) value));
                    builder.Append("f");
                    return builder.ToString();

                case TypeCode.Double:
                    builder.Append(AppendDecimalMarkerToDouble(XmlConvert.ToString((double) value)));
                    builder.Append("D");
                    return builder.ToString();

                case TypeCode.Decimal:
                    builder.Append(XmlConvert.ToString((decimal) value));
                    builder.Append("M");
                    return builder.ToString();

                case TypeCode.DateTime:
                    builder.Append("datetime");
                    builder.Append("'");
                    builder.Append(Microsoft.Data.OData.PlatformHelper.ConvertDateTimeToString((DateTime) value));
                    builder.Append("'");
                    return builder.ToString();

                case TypeCode.String:
                    builder.Append("'");
                    builder.Append(((string) value).Replace("'", "''"));
                    builder.Append("'");
                    return builder.ToString();

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertToUriLiteralUnsupportedType(nullableType.ToString()));
            }
            return builder.ToString();
        }

        internal static object VerifyAndCoerceUriPrimitiveLiteral(object primitiveValue, IEdmModel model, IEdmTypeReference expectedTypeReference, ODataVersion version)
        {
            ExceptionUtils.CheckArgumentNotNull<object>(primitiveValue, "primitiveValue");
            ExceptionUtils.CheckArgumentNotNull<IEdmModel>(model, "model");
            ExceptionUtils.CheckArgumentNotNull<IEdmTypeReference>(expectedTypeReference, "expectedTypeReference");
            ODataUriNullValue value2 = primitiveValue as ODataUriNullValue;
            if (value2 != null)
            {
                if (!expectedTypeReference.IsNullable)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertFromUriLiteralNullOnNonNullableType(expectedTypeReference.ODataFullName()));
                }
                IEdmType type = ValidationUtils.ValidateValueTypeName(model, value2.TypeName, expectedTypeReference.Definition.TypeKind);
                if (type.IsSpatial())
                {
                    ODataVersionChecker.CheckSpatialValue(version);
                }
                if (!TypePromotionUtils.CanConvertTo(type.ToTypeReference(), expectedTypeReference))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertFromUriLiteralNullTypeVerificationFailure(expectedTypeReference.ODataFullName(), value2.TypeName));
                }
                value2.TypeName = expectedTypeReference.ODataFullName();
                return value2;
            }
            IEdmPrimitiveTypeReference reference = expectedTypeReference.AsPrimitiveOrNull();
            if (reference == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertFromUriLiteralTypeVerificationFailure(expectedTypeReference.ODataFullName(), primitiveValue));
            }
            object obj2 = CoerceNumericType(primitiveValue, reference.PrimitiveDefinition());
            if (obj2 != null)
            {
                return obj2;
            }
            Type c = primitiveValue.GetType();
            if (!TypeUtils.GetNonNullableType(EdmLibraryExtensions.GetPrimitiveClrType(reference)).IsAssignableFrom(c))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataUriUtils_ConvertFromUriLiteralTypeVerificationFailure(reference.ODataFullName(), primitiveValue));
            }
            return primitiveValue;
        }
    }
}

