namespace System.Data.Services.Client
{
    using System;

    internal sealed class StringTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return text;
        }

        internal override string ToString(object instance)
        {
            return (string) instance;
        }
    }
}

