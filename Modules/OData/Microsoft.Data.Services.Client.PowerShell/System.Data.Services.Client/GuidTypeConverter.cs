namespace System.Data.Services.Client
{
    using System;

    internal sealed class GuidTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return new Guid(text);
        }

        internal override string ToString(object instance)
        {
            return instance.ToString();
        }
    }
}

