namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class DecimalTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToDecimal(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((decimal) instance);
        }
    }
}

