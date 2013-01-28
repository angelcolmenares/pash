namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class DoubleTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToDouble(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((double) instance);
        }
    }
}

