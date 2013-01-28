namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class BooleanTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToBoolean(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((bool) instance);
        }
    }
}

