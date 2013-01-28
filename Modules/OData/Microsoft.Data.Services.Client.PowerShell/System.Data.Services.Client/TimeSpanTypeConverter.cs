namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class TimeSpanTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToTimeSpan(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((TimeSpan) instance);
        }
    }
}

