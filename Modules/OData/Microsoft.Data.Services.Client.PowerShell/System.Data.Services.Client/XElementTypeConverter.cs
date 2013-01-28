namespace System.Data.Services.Client
{
    using System;
    using System.Xml.Linq;

    internal sealed class XElementTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return XElement.Parse(text);
        }

        internal override string ToString(object instance)
        {
            return instance.ToString();
        }
    }
}

