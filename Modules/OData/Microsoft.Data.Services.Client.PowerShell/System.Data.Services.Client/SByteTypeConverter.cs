namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class SByteTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToSByte(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((sbyte) instance);
        }
    }
}

