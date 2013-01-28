namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class Int16TypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToInt16(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((short) instance);
        }
    }
}

