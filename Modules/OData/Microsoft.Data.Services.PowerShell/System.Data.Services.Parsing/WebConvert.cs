namespace System.Data.Services.Parsing
{
    using System;
    using System.Data.Linq;
    using System.Data.Services;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    internal static class WebConvert
    {
        private const string HexValues = "0123456789ABCDEF";
        private static readonly char[] XmlWhitespaceChars = new char[] { ' ', '\t', '\n', '\r' };

        internal static string ConvertByteArrayToKeyString(byte[] byteArray)
        {
            StringBuilder builder = new StringBuilder(3 + (byteArray.Length * 2));
            builder.Append("X");
            builder.Append("'");
            for (int i = 0; i < byteArray.Length; i++)
            {
                builder.Append("0123456789ABCDEF"[byteArray[i] >> 4]);
                builder.Append("0123456789ABCDEF"[byteArray[i] & 15]);
            }
            builder.Append("'");
            return builder.ToString();
        }

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
            throw new InvalidOperationException();
        }

        internal static bool IsCharHexDigit(char c)
        {
            if (((c < '0') || (c > '9')) && ((c < 'a') || (c > 'f')))
            {
                return ((c >= 'A') && (c <= 'F'));
            }
            return true;
        }

        internal static bool IsKeyTypeQuoted(Type type)
        {
            if (!(type == typeof(XElement)))
            {
                return (type == typeof(string));
            }
            return true;
        }

        internal static bool IsKeyValueQuoted(string text)
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

        internal static T ParseSpatialLiteral<T>(string literalText, string prefix, WellKnownTextSqlFormatter formatter) where T: class, ISpatial
        {
            string str;
            if (!TryExtractWellKnownTextSqlFromSpatialLiteral(literalText, prefix, out str))
            {
                str = literalText;
            }
            using (StringReader reader = new StringReader(str))
            {
                return formatter.Read<T>(reader);
            }
        }

        internal static string RemoveQuotes(string text)
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

        internal static object StringToPrimitive(string text, Type targetType)
        {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (typeof(string) == targetType)
            {
                return text;
            }
            if (typeof(bool) == targetType)
            {
                return XmlConvert.ToBoolean(text);
            }
            if (typeof(byte) == targetType)
            {
                return XmlConvert.ToByte(text);
            }
            if (typeof(byte[]) == targetType)
            {
                return Convert.FromBase64String(text);
            }
            if (typeof(Binary) == targetType)
            {
                return new Binary(Convert.FromBase64String(text));
            }
            if (typeof(sbyte) == targetType)
            {
                return XmlConvert.ToSByte(text);
            }
            if (typeof(DateTime) == targetType)
            {
                return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.RoundtripKind);
            }
            if (typeof(DateTimeOffset) == targetType)
            {
                return XmlConvert.ToDateTimeOffset(text);
            }
            if (typeof(TimeSpan) == targetType)
            {
                return XmlConvert.ToTimeSpan(text);
            }
            if (typeof(decimal) == targetType)
            {
                return XmlConvert.ToDecimal(text);
            }
            if (typeof(double) == targetType)
            {
                return XmlConvert.ToDouble(text);
            }
            if (typeof(Guid) == targetType)
            {
                return new Guid(text);
            }
            if (typeof(short) == targetType)
            {
                return XmlConvert.ToInt16(text);
            }
            if (typeof(int) == targetType)
            {
                return XmlConvert.ToInt32(text);
            }
            if (typeof(long) == targetType)
            {
                return XmlConvert.ToInt64(text);
            }
            if (typeof(XElement) == targetType)
            {
                return XElement.Parse(text, LoadOptions.PreserveWhitespace);
            }
            return XmlConvert.ToSingle(text);
        }

        private static bool TryExtractWellKnownTextSqlFromSpatialLiteral(string spatialLiteral, string prefix, out string wellKnownTextSql)
        {
            if (!TryRemoveLiteralPrefix(prefix, ref spatialLiteral))
            {
                wellKnownTextSql = null;
                return false;
            }
            if (!TryRemoveQuotes(ref spatialLiteral))
            {
                wellKnownTextSql = null;
                return false;
            }
            wellKnownTextSql = spatialLiteral;
            return true;
        }

        internal static bool TryKeyPrimitiveToString(object value, out string result)
        {
            if (value.GetType() == typeof(byte[]))
            {
                result = ConvertByteArrayToKeyString((byte[]) value);
            }
            else
            {
                if (value.GetType() == typeof(Binary))
                {
                    return TryKeyPrimitiveToString(((Binary) value).ToArray(), out result);
                }
                if (!TryXmlPrimitiveToString(value, out result))
                {
                    return false;
                }
                if (IsKeyTypeQuoted(value.GetType()))
                {
                    result = result.Replace("'", "''");
                }
                result = Uri.EscapeDataString(result);
                if (value.GetType() == typeof(DateTime))
                {
                    result = "datetime'" + result + "'";
                }
                else if (value.GetType() == typeof(DateTimeOffset))
                {
                    result = "datetimeoffset'" + result + "'";
                }
                else if (value.GetType() == typeof(decimal))
                {
                    result = result + "M";
                }
                else if (value.GetType() == typeof(Guid))
                {
                    result = "guid'" + result + "'";
                }
                else if (value.GetType() == typeof(long))
                {
                    result = result + "L";
                }
                else if (value.GetType() == typeof(float))
                {
                    result = result + "f";
                }
                else if (value.GetType() == typeof(double))
                {
                    double d = (double) value;
                    if (!double.IsInfinity(d) && !double.IsNaN(d))
                    {
                        result = result + "D";
                    }
                }
                else if (typeof(Geography).IsAssignableFrom(value.GetType()))
                {
                    result = "geography'" + result + "'";
                }
                else if (typeof(Geometry).IsAssignableFrom(value.GetType()))
                {
                    result = "geometry'" + result + "'";
                }
                else if (value.GetType() == typeof(TimeSpan))
                {
                    result = "time'" + result + "'";
                }
                else if (IsKeyTypeQuoted(value.GetType()))
                {
                    result = "'" + result + "'";
                }
            }
            return true;
        }

        internal static bool TryKeyStringToByteArray(string text, out byte[] targetValue)
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

        internal static bool TryKeyStringToDateTime(string text, out DateTime targetValue)
        {
            return TryKeyStringToType<DateTime>(text, "datetime", s => XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind), out targetValue);
        }

        internal static bool TryKeyStringToDateTimeOffset(string text, out DateTimeOffset targetValue)
        {
            return TryKeyStringToType<DateTimeOffset>(text, "datetimeoffset", s => XmlConvert.ToDateTimeOffset(s), out targetValue);
        }

        internal static bool TryKeyStringToGuid(string text, out Guid targetValue)
        {
            return TryKeyStringToType<Guid>(text, "guid", s => XmlConvert.ToGuid(s), out targetValue);
        }

        internal static bool TryKeyStringToPrimitive(string text, Type targetType, out object targetValue)
        {
            byte[] buffer;
            bool flag7;
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            bool flag = TryKeyStringToByteArray(text, out buffer);
            if ((targetType == typeof(byte[])) || (targetType == typeof(Binary)))
            {
                targetValue = ((buffer != null) && (targetType == typeof(Binary))) ? ((object) new Binary(buffer)) : ((object) buffer);
                return flag;
            }
            if (flag)
            {
                return TryKeyStringToPrimitive(Encoding.UTF8.GetString(buffer), targetType, out targetValue);
            }
            if (targetType == typeof(Guid))
            {
                Guid guid;
                bool flag2 = TryKeyStringToGuid(text, out guid);
                targetValue = guid;
                return flag2;
            }
            if (targetType == typeof(DateTime))
            {
                DateTime time;
                bool flag3 = TryKeyStringToDateTime(text, out time);
                targetValue = time;
                return flag3;
            }
            if (targetType == typeof(DateTimeOffset))
            {
                DateTimeOffset offset;
                bool flag4 = TryKeyStringToDateTimeOffset(text, out offset);
                targetValue = offset;
                return flag4;
            }
            if (targetType == typeof(TimeSpan))
            {
                TimeSpan span;
                bool flag5 = TryKeyStringToTimeSpan(text, out span);
                targetValue = span;
                return flag5;
            }
            bool flag6 = IsKeyTypeQuoted(targetType);
            if (flag6 != IsKeyValueQuoted(text))
            {
                targetValue = null;
                return false;
            }
            if (flag6)
            {
                text = RemoveQuotes(text);
            }
            try
            {
                if (typeof(string) == targetType)
                {
                    targetValue = text;
                }
                else if (typeof(bool) == targetType)
                {
                    targetValue = XmlConvert.ToBoolean(text);
                }
                else if (typeof(byte) == targetType)
                {
                    targetValue = XmlConvert.ToByte(text);
                }
                else if (typeof(sbyte) == targetType)
                {
                    targetValue = XmlConvert.ToSByte(text);
                }
                else if (typeof(short) == targetType)
                {
                    targetValue = XmlConvert.ToInt16(text);
                }
                else if (typeof(int) == targetType)
                {
                    targetValue = XmlConvert.ToInt32(text);
                }
                else if (typeof(long) == targetType)
                {
                    if (!TryRemoveLiteralSuffix("L", ref text))
                    {
                        targetValue = 0L;
                        return false;
                    }
                    targetValue = XmlConvert.ToInt64(text);
                }
                else if (typeof(float) == targetType)
                {
                    if (!TryRemoveLiteralSuffix("f", ref text))
                    {
                        targetValue = 0f;
                        return false;
                    }
                    targetValue = XmlConvert.ToSingle(text);
                }
                else if (typeof(double) == targetType)
                {
                    TryRemoveLiteralSuffix("D", ref text);
                    targetValue = XmlConvert.ToDouble(text);
                }
                else
                {
                    if (typeof(decimal) == targetType)
                    {
                        if (TryRemoveLiteralSuffix("M", ref text))
                        {
                            try
                            {
                                targetValue = XmlConvert.ToDecimal(text);
                                goto Label_0350;
                            }
                            catch (FormatException)
                            {
                                decimal num;
                                if (decimal.TryParse(text, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out num))
                                {
                                    targetValue = num;
                                    goto Label_0350;
                                }
                                targetValue = 0M;
                                return false;
                            }
                        }
                        targetValue = 0M;
                        return false;
                    }
                    if (targetType.IsSpatial())
                    {
                        return TryParseSpatialLiteral(targetType, text, out targetValue);
                    }
                    targetValue = XElement.Parse(text, LoadOptions.PreserveWhitespace);
                }
            Label_0350:
                flag7 = true;
            }
            catch (FormatException)
            {
                targetValue = null;
                flag7 = false;
            }
            return flag7;
        }

        internal static bool TryKeyStringToTimeSpan(string text, out TimeSpan targetValue)
        {
            return TryKeyStringToType<TimeSpan>(text, "time", s => XmlConvert.ToTimeSpan(s), out targetValue);
        }

        internal static bool TryKeyStringToType<T>(string text, string literalPrefix, Func<string, T> convertMethod, out T targetValue)
        {
            if (!TryRemoveLiteralPrefix(literalPrefix, ref text))
            {
                targetValue = default(T);
                return false;
            }
            if (!TryRemoveQuotes(ref text))
            {
                targetValue = default(T);
                return false;
            }
            try
            {
                targetValue = convertMethod(text);
                return true;
            }
            catch (FormatException)
            {
                targetValue = default(T);
                return false;
            }
        }

        private static bool TryParseSpatialLiteral(Type literalType, string literalText, out object literalValue)
        {
            Geometry geometry;
            if (typeof(Geography).IsAssignableFrom(literalType))
            {
                Geography geography;
                bool flag = TryParseSpatialLiteral<Geography>(literalText, "geography", WellKnownTextSqlFormatter.Create(), out geography);
                literalValue = geography;
                return flag;
            }
            bool flag2 = TryParseSpatialLiteral<Geometry>(literalText, "geometry", WellKnownTextSqlFormatter.Create(), out geometry);
            literalValue = geometry;
            return flag2;
        }

        private static bool TryParseSpatialLiteral<T>(string literalText, string prefix, WellKnownTextSqlFormatter formatter, out T literalValue) where T: class, ISpatial
        {
            string str;
            bool flag2;
            if (!TryExtractWellKnownTextSqlFromSpatialLiteral(literalText, prefix, out str))
            {
                literalValue = default(T);
                return false;
            }
            StringReader input = new StringReader(str);
            try
            {
                literalValue = formatter.Read<T>(input);
                flag2 = true;
            }
            catch (Exception exception)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                literalValue = default(T);
                flag2 = false;
            }
            finally
            {
                if (input != null)
                {
                    input.Dispose();
                }
            }
            return flag2;
        }

        internal static bool TryRemoveLiteralPrefix(string prefix, ref string text)
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
            text = text.Trim(XmlWhitespaceChars);
            if ((text.Length <= suffix.Length) || !text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            text = text.Substring(0, text.Length - suffix.Length);
            return true;
        }

        internal static bool TryRemoveQuotes(ref string text)
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

        internal static bool TryXmlPrimitiveToString(object value, out string result)
        {
            Type nullableType = value.GetType();
            nullableType = Nullable.GetUnderlyingType(nullableType) ?? nullableType;
            if (typeof(string) == nullableType)
            {
                result = (string) value;
            }
            else if (typeof(bool) == nullableType)
            {
                result = XmlConvert.ToString((bool) value);
            }
            else if (typeof(byte) == nullableType)
            {
                result = XmlConvert.ToString((byte) value);
            }
            else if (typeof(DateTime) == nullableType)
            {
                result = XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            else if (typeof(decimal) == nullableType)
            {
                result = XmlConvert.ToString((decimal) value);
            }
            else if (typeof(double) == nullableType)
            {
                result = XmlConvert.ToString((double) value);
            }
            else if (typeof(Guid) == nullableType)
            {
                result = value.ToString();
            }
            else if (typeof(short) == nullableType)
            {
                result = XmlConvert.ToString((short) value);
            }
            else if (typeof(int) == nullableType)
            {
                result = XmlConvert.ToString((int) value);
            }
            else if (typeof(long) == nullableType)
            {
                result = XmlConvert.ToString((long) value);
            }
            else if (typeof(sbyte) == nullableType)
            {
                result = XmlConvert.ToString((sbyte) value);
            }
            else if (typeof(float) == nullableType)
            {
                result = XmlConvert.ToString((float) value);
            }
            else if (typeof(byte[]) == nullableType)
            {
                byte[] inArray = (byte[]) value;
                result = Convert.ToBase64String(inArray);
            }
            else
            {
                if (typeof(Binary) == nullableType)
                {
                    return TryXmlPrimitiveToString(((Binary) value).ToArray(), out result);
                }
                if (typeof(XElement) == nullableType)
                {
                    result = ((XElement) value).ToString(SaveOptions.None);
                }
                else if (typeof(DateTimeOffset) == nullableType)
                {
                    result = XmlConvert.ToString((DateTimeOffset) value);
                }
                else if (typeof(TimeSpan) == nullableType)
                {
                    result = XmlConvert.ToString((TimeSpan) value);
                }
                else if (typeof(Geography).IsAssignableFrom(nullableType))
                {
                    result = WellKnownTextSqlFormatter.Create(true).Write((Geography) value);
                }
                else if (typeof(Geometry).IsAssignableFrom(nullableType))
                {
                    result = WellKnownTextSqlFormatter.Create(true).Write((Geometry) value);
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            return true;
        }
    }
}

