namespace Microsoft.Data.OData.Query
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Text;
    using System.Xml;

    internal static class UriPrimitiveTypeParser
    {
        private static char[] WhitespaceChars = new char[] { ' ', '\t', '\n', '\r' };

        private static byte HexCharToNibble(char c)
        {
            switch (c)
            {
                case '0':
                    return 0;

                case '1':
                    return 1;

                case '2':
                    return 2;

                case '3':
                    return 3;

                case '4':
                    return 4;

                case '5':
                    return 5;

                case '6':
                    return 6;

                case '7':
                    return 7;

                case '8':
                    return 8;

                case '9':
                    return 9;

                case 'A':
                case 'a':
                    return 10;

                case 'B':
                case 'b':
                    return 11;

                case 'C':
                case 'c':
                    return 12;

                case 'D':
                case 'd':
                    return 13;

                case 'E':
                case 'e':
                    return 14;

                case 'F':
                case 'f':
                    return 15;
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(Microsoft.Data.OData.Query.InternalErrorCodes.UriPrimitiveTypeParser_HexCharToNibble));
        }

        internal static bool IsCharHexDigit(char c)
        {
            if (((c < '0') || (c > '9')) && ((c < 'a') || (c > 'f')))
            {
                return ((c >= 'A') && (c <= 'F'));
            }
            return true;
        }

        private static bool IsUriValueQuoted(string text)
        {
            int num2;
            if (((text.Length < 2) || (text[0] != '\'')) || (text[text.Length - 1] != '\''))
            {
                return false;
            }
            for (int i = 1; i < (text.Length - 1); i = num2 + 2)
            {
                num2 = text.IndexOf('\'', i, (text.Length - i) - 1);
                if (num2 == -1)
                {
                    break;
                }
                if ((num2 == (text.Length - 2)) || (text[num2 + 1] != '\''))
                {
                    return false;
                }
            }
            return true;
        }

        private static string RemoveQuotes(string text)
        {
            char ch = text[0];
            string str = text.Substring(1, text.Length - 2);
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOf(ch, startIndex);
                if (index < 0)
                {
                    return str;
                }
                str = str.Remove(index, 1);
                startIndex = index + 1;
            }
        }

        private static bool TryRemoveLiteralPrefix(string prefix, ref string text)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text.Remove(0, prefix.Length);
                return true;
            }
            return false;
        }

        private static bool TryRemoveLiteralSuffix(string suffix, ref string text)
        {
            text = text.Trim(WhitespaceChars);
            if ((text.Length <= suffix.Length) || !text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            text = text.Substring(0, text.Length - suffix.Length);
            return true;
        }

        private static bool TryRemoveQuotes(ref string text)
        {
            if (text.Length < 2)
            {
                return false;
            }
            char ch = text[0];
            if ((ch != '\'') || (text[text.Length - 1] != ch))
            {
                return false;
            }
            string str = text.Substring(1, text.Length - 2);
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOf(ch, startIndex);
                if (index < 0)
                {
                    break;
                }
                str = str.Remove(index, 1);
                if ((str.Length < (index + 1)) || (str[index] != ch))
                {
                    return false;
                }
                startIndex = index + 1;
            }
            text = str;
            return true;
        }

        private static bool TryUriStringToByteArray(string text, out byte[] targetValue)
        {
            if (!TryRemoveLiteralPrefix("binary", ref text) && !TryRemoveLiteralPrefix("X", ref text))
            {
                targetValue = null;
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = null;
                return false;
            }
            if ((text.Length % 2) != 0)
            {
                targetValue = null;
                return false;
            }
            byte[] buffer = new byte[text.Length / 2];
            int index = 0;
            int num2 = 0;
            while (index < buffer.Length)
            {
                char c = text[num2];
                char ch2 = text[num2 + 1];
                if (!IsCharHexDigit(c) || !IsCharHexDigit(ch2))
                {
                    targetValue = null;
                    return false;
                }
                buffer[index] = (byte) (((byte) (HexCharToNibble(c) << 4)) + HexCharToNibble(ch2));
                num2 += 2;
                index++;
            }
            targetValue = buffer;
            return true;
        }

        private static bool TryUriStringToDateTime(string text, out DateTime targetValue)
        {
            if (!TryRemoveLiteralPrefix("datetime", ref text))
            {
                targetValue = new DateTime();
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = new DateTime();
                return false;
            }
            try
            {
                targetValue = Microsoft.Data.OData.PlatformHelper.ConvertStringToDateTime(text);
                return true;
            }
            catch (FormatException)
            {
                targetValue = new DateTime();
                return false;
            }
        }

        private static bool TryUriStringToDateTimeOffset(string text, out DateTimeOffset targetValue)
        {
            if (!TryRemoveLiteralPrefix("datetimeoffset", ref text))
            {
                targetValue = new DateTimeOffset();
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = new DateTimeOffset();
                return false;
            }
            try
            {
                targetValue = XmlConvert.ToDateTimeOffset(text);
                return true;
            }
            catch (FormatException)
            {
                targetValue = new DateTimeOffset();
                return false;
            }
        }

        private static bool TryUriStringToGeography(string text, out Geography targetValue)
        {
            if (!TryRemoveLiteralPrefix("geography", ref text))
            {
                targetValue = null;
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = null;
                return false;
            }
            try
            {
                targetValue = LiteralUtils.ParseGeography(text);
                return true;
            }
            catch (ParseErrorException)
            {
                targetValue = null;
                return false;
            }
        }

        private static bool TryUriStringToGeometry(string text, out Geometry targetValue)
        {
            if (!TryRemoveLiteralPrefix("geometry", ref text))
            {
                targetValue = null;
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = null;
                return false;
            }
            try
            {
                targetValue = LiteralUtils.ParseGeometry(text);
                return true;
            }
            catch (ParseErrorException)
            {
                targetValue = null;
                return false;
            }
        }

        private static bool TryUriStringToGuid(string text, out Guid targetValue)
        {
            if (!TryRemoveLiteralPrefix("guid", ref text))
            {
                targetValue = new Guid();
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = new Guid();
                return false;
            }
            try
            {
                targetValue = XmlConvert.ToGuid(text);
                return true;
            }
            catch (FormatException)
            {
                targetValue = new Guid();
                return false;
            }
        }

        internal static bool TryUriStringToNonNegativeInteger(string text, out int nonNegativeInteger)
        {
            object obj2;
            if (!TryUriStringToPrimitive(text, EdmCoreModel.Instance.GetInt32(false), out obj2))
            {
                nonNegativeInteger = -1;
                return false;
            }
            nonNegativeInteger = (int) obj2;
            if (nonNegativeInteger < 0)
            {
                return false;
            }
            return true;
        }

        internal static bool TryUriStringToPrimitive(string text, IEdmTypeReference targetType, out object targetValue)
        {
            byte[] buffer;
            bool flag9;
            if (targetType.IsNullable && (text == "null"))
            {
                targetValue = null;
                return true;
            }
            IEdmPrimitiveTypeReference type = targetType.AsPrimitiveOrNull();
            if (type == null)
            {
                targetValue = null;
                return false;
            }
            EdmPrimitiveTypeKind kind = type.PrimitiveKind();
            bool flag = TryUriStringToByteArray(text, out buffer);
            if (kind == EdmPrimitiveTypeKind.Binary)
            {
                targetValue = buffer;
                return flag;
            }
            if (flag)
            {
                return TryUriStringToPrimitive(Encoding.UTF8.GetString(buffer, 0, buffer.Length), targetType, out targetValue);
            }
            switch (kind)
            {
                case EdmPrimitiveTypeKind.Guid:
                {
                    Guid guid;
                    bool flag2 = TryUriStringToGuid(text, out guid);
                    targetValue = guid;
                    return flag2;
                }
                case EdmPrimitiveTypeKind.DateTime:
                {
                    DateTime time;
                    bool flag3 = TryUriStringToDateTime(text, out time);
                    targetValue = time;
                    return flag3;
                }
                case EdmPrimitiveTypeKind.DateTimeOffset:
                {
                    DateTimeOffset offset;
                    bool flag4 = TryUriStringToDateTimeOffset(text, out offset);
                    targetValue = offset;
                    return flag4;
                }
                case EdmPrimitiveTypeKind.Time:
                {
                    TimeSpan span;
                    bool flag5 = TryUriStringToTime(text, out span);
                    targetValue = span;
                    return flag5;
                }
                case EdmPrimitiveTypeKind.Geography:
                {
                    Geography geography;
                    bool flag6 = TryUriStringToGeography(text, out geography);
                    targetValue = geography;
                    return flag6;
                }
                case EdmPrimitiveTypeKind.Geometry:
                {
                    Geometry geometry;
                    bool flag7 = TryUriStringToGeometry(text, out geometry);
                    targetValue = geometry;
                    return flag7;
                }
            }
            bool flag8 = kind == EdmPrimitiveTypeKind.String;
            if (flag8 != IsUriValueQuoted(text))
            {
                targetValue = null;
                return false;
            }
            if (flag8)
            {
                text = RemoveQuotes(text);
            }
            try
            {
                switch (kind)
                {
                    case EdmPrimitiveTypeKind.Boolean:
                        targetValue = XmlConvert.ToBoolean(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Byte:
                        targetValue = XmlConvert.ToByte(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Decimal:
                        if (TryRemoveLiteralSuffix("M", ref text))
                        {
                            try
                            {
                                targetValue = XmlConvert.ToDecimal(text);
                                goto Label_02B3;
                            }
                            catch (FormatException)
                            {
                                decimal num;
                                if (decimal.TryParse(text, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out num))
                                {
                                    targetValue = num;
                                    goto Label_02B3;
                                }
                                targetValue = 0M;
                                return false;
                            }
                        }
                        targetValue = 0M;
                        return false;

                    case EdmPrimitiveTypeKind.Double:
                        TryRemoveLiteralSuffix("D", ref text);
                        targetValue = XmlConvert.ToDouble(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Int16:
                        targetValue = XmlConvert.ToInt16(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Int32:
                        targetValue = XmlConvert.ToInt32(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Int64:
                        if (!TryRemoveLiteralSuffix("L", ref text))
                        {
                            break;
                        }
                        targetValue = XmlConvert.ToInt64(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.SByte:
                        targetValue = XmlConvert.ToSByte(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.Single:
                        if (!TryRemoveLiteralSuffix("f", ref text))
                        {
                            goto Label_020F;
                        }
                        targetValue = XmlConvert.ToSingle(text);
                        goto Label_02B3;

                    case EdmPrimitiveTypeKind.String:
                        targetValue = text;
                        goto Label_02B3;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(Microsoft.Data.OData.Query.InternalErrorCodes.UriPrimitiveTypeParser_TryUriStringToPrimitive));
                }
                targetValue = 0L;
                return false;
            Label_020F:
                targetValue = 0f;
                return false;
            Label_02B3:
                flag9 = true;
            }
            catch (FormatException)
            {
                targetValue = null;
                flag9 = false;
            }
            catch (OverflowException)
            {
                targetValue = null;
                flag9 = false;
            }
            return flag9;
        }

        private static bool TryUriStringToTime(string text, out TimeSpan targetValue)
        {
            if (!TryRemoveLiteralPrefix("time", ref text))
            {
                targetValue = new TimeSpan();
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = new TimeSpan();
                return false;
            }
            try
            {
                targetValue = XmlConvert.ToTimeSpan(text);
                return true;
            }
            catch (FormatException)
            {
                targetValue = new TimeSpan();
                return false;
            }
        }
    }
}

