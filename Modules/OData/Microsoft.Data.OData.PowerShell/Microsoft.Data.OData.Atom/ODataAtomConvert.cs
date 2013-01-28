namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal static class ODataAtomConvert
    {
        private static readonly TimeSpan zeroOffset = new TimeSpan(0, 0, 0);

        internal static string ToAtomString(DateTimeOffset dateTime)
        {
            if (dateTime.Offset == zeroOffset)
            {
                return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
            return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        }

        internal static string ToString(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        internal static string ToString(bool b)
        {
            if (!b)
            {
                return "false";
            }
            return "true";
        }

        internal static string ToString(byte b)
        {
            return XmlConvert.ToString(b);
        }

        internal static string ToString(this DateTime dt)
        {
            return PlatformHelper.ConvertDateTimeToString(dt);
        }

        internal static string ToString(DateTimeOffset dateTime)
        {
            return XmlConvert.ToString(dateTime);
        }

        internal static string ToString(decimal d)
        {
            return XmlConvert.ToString(d);
        }

        internal static string ToString(this double d)
        {
            return XmlConvert.ToString(d);
        }

        internal static string ToString(this Guid guid)
        {
            return XmlConvert.ToString(guid);
        }

        internal static string ToString(this short i)
        {
            return XmlConvert.ToString(i);
        }

        internal static string ToString(this int i)
        {
            return XmlConvert.ToString(i);
        }

        internal static string ToString(this long i)
        {
            return XmlConvert.ToString(i);
        }

        internal static string ToString(this sbyte sb)
        {
            return XmlConvert.ToString(sb);
        }

        internal static string ToString(this float s)
        {
            return XmlConvert.ToString(s);
        }

        internal static string ToString(this TimeSpan ts)
        {
            return XmlConvert.ToString(ts);
        }
    }
}

