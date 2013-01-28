namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Atom;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal static class AtomValueUtils
    {
        internal static object ConvertStringToPrimitive(string text, IEdmPrimitiveTypeReference targetTypeReference)
        {
            object obj2;
            try
            {
                switch (targetTypeReference.PrimitiveKind())
                {
                    case EdmPrimitiveTypeKind.Binary:
                        return Convert.FromBase64String(text);

                    case EdmPrimitiveTypeKind.Boolean:
                        return XmlConvert.ToBoolean(text);

                    case EdmPrimitiveTypeKind.Byte:
                        return XmlConvert.ToByte(text);

                    case EdmPrimitiveTypeKind.DateTime:
                        return Microsoft.Data.OData.PlatformHelper.ConvertStringToDateTime(text);

                    case EdmPrimitiveTypeKind.DateTimeOffset:
                        return XmlConvert.ToDateTimeOffset(text);

                    case EdmPrimitiveTypeKind.Decimal:
                        return XmlConvert.ToDecimal(text);

                    case EdmPrimitiveTypeKind.Double:
                        return XmlConvert.ToDouble(text);

                    case EdmPrimitiveTypeKind.Guid:
                        return new Guid(text);

                    case EdmPrimitiveTypeKind.Int16:
                        return XmlConvert.ToInt16(text);

                    case EdmPrimitiveTypeKind.Int32:
                        return XmlConvert.ToInt32(text);

                    case EdmPrimitiveTypeKind.Int64:
                        return XmlConvert.ToInt64(text);

                    case EdmPrimitiveTypeKind.SByte:
                        return XmlConvert.ToSByte(text);

                    case EdmPrimitiveTypeKind.Single:
                        return XmlConvert.ToSingle(text);

                    case EdmPrimitiveTypeKind.String:
                        return text;

                    case EdmPrimitiveTypeKind.Time:
                        return XmlConvert.ToTimeSpan(text);
                }
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.AtomValueUtils_ConvertStringToPrimitive));
            }
            catch (Exception exception)
            {
                if (!ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ReaderValidationUtils.GetPrimitiveTypeConversionException(targetTypeReference, exception);
            }
            return obj2;
        }

        internal static object ReadPrimitiveValue(XmlReader reader, IEdmPrimitiveTypeReference primitiveTypeReference)
        {
            object obj2;
            if (!PrimitiveConverter.Instance.TryTokenizeFromXml(reader, EdmLibraryExtensions.GetPrimitiveClrType(primitiveTypeReference), out obj2))
            {
                return ConvertStringToPrimitive(reader.ReadElementContentValue(), primitiveTypeReference);
            }
            return obj2;
        }

        internal static string ToString(AtomTextConstructKind textConstructKind)
        {
            switch (textConstructKind)
            {
                case AtomTextConstructKind.Text:
                    return "text";

                case AtomTextConstructKind.Html:
                    return "html";

                case AtomTextConstructKind.Xhtml:
                    return "xhtml";
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataAtomConvert_ToString));
        }

        internal static bool TryConvertPrimitiveToString(object value, out string result)
        {
            result = null;
            switch (Microsoft.Data.OData.PlatformHelper.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    result = ODataAtomConvert.ToString((bool) value);
                    break;

                case TypeCode.SByte:
                    result = ((sbyte) value).ToString();
                    break;

                case TypeCode.Byte:
                    result = ODataAtomConvert.ToString((byte) value);
                    break;

                case TypeCode.Int16:
                    result = ((short) value).ToString();
                    break;

                case TypeCode.Int32:
                    result = ((int) value).ToString();
                    break;

                case TypeCode.Int64:
                    result = ((long) value).ToString();
                    break;

                case TypeCode.Single:
                    result = ((float) value).ToString();
                    break;

                case TypeCode.Double:
                    result = ((double) value).ToString();
                    break;

                case TypeCode.Decimal:
                    result = ODataAtomConvert.ToString((decimal) value);
                    break;

                case TypeCode.DateTime:
                    result = ((DateTime) value).ToString();
                    break;

                case TypeCode.String:
                    result = (string) value;
                    break;

                default:
                {
                    byte[] bytes = value as byte[];
                    if (bytes != null)
                    {
                        result = bytes.ToString();
                    }
                    else if (value is DateTimeOffset)
                    {
                        result = ODataAtomConvert.ToString((DateTimeOffset) value);
                    }
                    else if (value is Guid)
                    {
                        result = ((Guid) value).ToString();
                    }
                    else if (value is TimeSpan)
                    {
                        result = ((TimeSpan) value).ToString();
                    }
                    else
                    {
                        return false;
                    }
                    break;
                }
            }
            return true;
        }

        internal static void WritePrimitiveValue(XmlWriter writer, object value)
        {
            if (!PrimitiveConverter.Instance.TryWriteAtom(value, writer))
            {
                string str;
                if (!TryConvertPrimitiveToString(value, out str))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.AtomValueUtils_CannotConvertValueToAtomPrimitive(value.GetType().FullName));
                }
                ODataAtomWriterUtils.WriteString(writer, str);
            }
        }
    }
}

