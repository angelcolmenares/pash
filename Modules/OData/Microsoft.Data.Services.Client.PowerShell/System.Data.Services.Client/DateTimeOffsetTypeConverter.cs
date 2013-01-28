namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class DateTimeOffsetTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToDateTimeOffset(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((DateTimeOffset) instance);
        }
    }
}

