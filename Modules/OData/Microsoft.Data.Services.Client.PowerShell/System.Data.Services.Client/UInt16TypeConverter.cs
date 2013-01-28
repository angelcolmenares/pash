namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class UInt16TypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToUInt16(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((ushort) instance);
        }
    }
}

