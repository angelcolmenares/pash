namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    internal static class JsonValueUtils
    {
        private static readonly long JsonDateTimeMinTimeTicks;

        static JsonValueUtils()
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            JsonDateTimeMinTimeTicks = time.Ticks;
        }

        private static long DateTimeTicksToJsonTicks(long ticks)
        {
            return ((ticks - JsonDateTimeMinTimeTicks) / 0x2710L);
        }

        private static DateTime GetUniversalDate(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    value = new DateTime(value.Ticks, DateTimeKind.Utc);
                    return value;

                case DateTimeKind.Utc:
                    return value;

                case DateTimeKind.Local:
                    value = value.ToUniversalTime();
                    return value;
            }
            return value;
        }

        internal static long JsonTicksToDateTimeTicks(long ticks)
        {
            return ((ticks * 0x2710L) + JsonDateTimeMinTimeTicks);
        }

        internal static void WriteEscapedJsonString(TextWriter writer, string inputString)
        {
            writer.Write('"');
            int startIndex = 0;
            int length = 0;
            for (int i = 0; i < inputString.Length; i++)
            {
                char ch = inputString[i];
                if ((((ch == '\r') || (ch == '\t')) || ((ch == '"') || (ch == '\\'))) || (((ch == '\n') || (ch < ' ')) || (((ch > '\x007f') || (ch == '\b')) || (ch == '\f'))))
                {
                    writer.Write(inputString.Substring(startIndex, length));
                    startIndex = i + 1;
                    length = 0;
                }
                else
                {
                    length++;
                    continue;
                }
                switch (ch)
                {
                    case '\b':
                    {
                        writer.Write(@"\b");
                        continue;
                    }
                    case '\t':
                    {
                        writer.Write(@"\t");
                        continue;
                    }
                    case '\n':
                    {
                        writer.Write(@"\n");
                        continue;
                    }
                    case '\f':
                    {
                        writer.Write(@"\f");
                        continue;
                    }
                    case '\r':
                    {
                        writer.Write(@"\r");
                        continue;
                    }
                    case '"':
                    {
                        writer.Write("\\\"");
                        continue;
                    }
                    case '\\':
                    {
                        writer.Write(@"\\");
                        continue;
                    }
                }
                string str = string.Format(CultureInfo.InvariantCulture, @"\u{0:x4}", new object[] { (int) ch });
                writer.Write(str);
            }
            if (length > 0)
            {
                writer.Write(inputString.Substring(startIndex, length));
            }
            writer.Write('"');
        }

        private static void WriteQuoted(TextWriter writer, string text)
        {
            writer.Write('"');
            writer.Write(text);
            writer.Write('"');
        }

        internal static void WriteValue(TextWriter writer, bool value)
        {
            writer.Write(value ? "true" : "false");
        }

        internal static void WriteValue(TextWriter writer, byte value)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, decimal value)
        {
            WriteQuoted(writer, value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                WriteQuoted(writer, value.ToString(null, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.Write(XmlConvert.ToString(value));
            }
        }

        internal static void WriteValue(TextWriter writer, Guid value)
        {
            WriteQuoted(writer, value.ToString());
        }

        internal static void WriteValue(TextWriter writer, short value)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, int value)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, long value)
        {
            WriteQuoted(writer, value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, sbyte value)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        internal static void WriteValue(TextWriter writer, float value)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                WriteQuoted(writer, value.ToString(null, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.Write(XmlConvert.ToString(value));
            }
        }

        internal static void WriteValue(TextWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write("null");
            }
            else
            {
                WriteEscapedJsonString(writer, value);
            }
        }

        internal static void WriteValue(TextWriter writer, TimeSpan value)
        {
            WriteQuoted(writer, XmlConvert.ToString(value));
        }

        internal static void WriteValue(TextWriter writer, DateTime value, ODataJsonDateTimeFormat dateTimeFormat)
        {
            switch (dateTimeFormat)
            {
                case ODataJsonDateTimeFormat.ODataDateTime:
                {
                    value = GetUniversalDate(value);
                    string text = string.Format(CultureInfo.InvariantCulture, @"\/Date({0})\/", new object[] { DateTimeTicksToJsonTicks(value.Ticks) });
                    WriteQuoted(writer, text);
                    return;
                }
                case ODataJsonDateTimeFormat.ISO8601DateTime:
                {
                    string str = PlatformHelper.ConvertDateTimeToString(value);
                    WriteQuoted(writer, str);
                    return;
                }
            }
        }

        internal static void WriteValue(TextWriter writer, DateTimeOffset value, ODataJsonDateTimeFormat dateTimeFormat)
        {
            int totalMinutes = (int) value.Offset.TotalMinutes;
            switch (dateTimeFormat)
            {
                case ODataJsonDateTimeFormat.ODataDateTime:
                {
                    string text = string.Format(CultureInfo.InvariantCulture, @"\/Date({0}{1}{2:D4})\/", new object[] { DateTimeTicksToJsonTicks(value.Ticks), (totalMinutes >= 0) ? "+" : string.Empty, totalMinutes });
                    WriteQuoted(writer, text);
                    return;
                }
                case ODataJsonDateTimeFormat.ISO8601DateTime:
                {
                    string str = XmlConvert.ToString(value);
                    WriteQuoted(writer, str);
                    return;
                }
            }
        }
    }
}

