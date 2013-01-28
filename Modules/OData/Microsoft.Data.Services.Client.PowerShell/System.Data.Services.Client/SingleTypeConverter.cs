namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal sealed class SingleTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XmlConvert.ToSingle(text);
        }

        internal override string ToString(object instance)
        {
            return XmlConvert.ToString((float) instance);
        }
    }
}

