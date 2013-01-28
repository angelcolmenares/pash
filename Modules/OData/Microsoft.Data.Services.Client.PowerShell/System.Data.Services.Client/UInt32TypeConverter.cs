namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class UInt32TypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToUInt32(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((uint) instance);
        }
    }
}

