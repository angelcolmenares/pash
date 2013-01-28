namespace System.Data.Services.Client
{
    using System;

    internal sealed class CharArrayTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return text.ToCharArray();
        }

        internal override string ToString(object instance)
        {
            return new string((char[]) instance);
        }
    }
}

