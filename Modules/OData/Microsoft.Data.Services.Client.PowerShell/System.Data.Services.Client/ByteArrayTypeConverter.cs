namespace System.Data.Services.Client
{
    using System;

    internal sealed class ByteArrayTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return Convert.FromBase64String(text);
        }

        internal override string ToString(object instance)
        {
            return Convert.ToBase64String((byte[]) instance);
        }
    }
}

