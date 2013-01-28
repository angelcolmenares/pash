namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class UInt64TypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToUInt64(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((ulong) instance);
        }
    }
}

