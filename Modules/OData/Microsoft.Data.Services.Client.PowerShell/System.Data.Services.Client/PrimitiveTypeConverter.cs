namespace System.Data.Services.Client
{
    using System;
    using System.Xml;

    internal class PrimitiveTypeConverter
    {
        protected PrimitiveTypeConverter()
        {
        }

        internal virtual object Parse(string text)
        {
            return text;
        }

        internal virtual PrimitiveParserToken TokenizeFromText(string text)
        {
            return new TextPrimitiveParserToken(text);
        }

        internal virtual PrimitiveParserToken TokenizeFromXml(XmlReader reader)
        {
            string text = MaterializeAtom.ReadElementString(reader, true);
            if (text != null)
            {
                return new TextPrimitiveParserToken(text);
            }
            return null;
        }

        internal virtual string ToString(object instance)
        {
            throw new NotImplementedException();
        }
    }
}

