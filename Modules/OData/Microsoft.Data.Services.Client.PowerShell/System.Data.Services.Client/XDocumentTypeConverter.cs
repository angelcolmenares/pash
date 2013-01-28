namespace System.Data.Services.Client
{
    using System;
    using System.Xml.Linq;

    internal sealed class XDocumentTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            if (text.Length <= 0)
            {
                return new XDocument();
            }
            return XDocument.Parse(text);
        }

        internal override string ToString(object instance)
        {
            return instance.ToString();
        }
    }
}

