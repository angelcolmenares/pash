namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class ByteTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToByte(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((byte) instance);
        }
    }
}

