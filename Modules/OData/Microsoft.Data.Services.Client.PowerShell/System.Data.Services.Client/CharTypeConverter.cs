namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class CharTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToChar(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((char) instance);
        }
    }
}

