namespace System.Data.Services.Client
{
    using System;

    internal sealed class ClrTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return PlatformHelper.GetTypeOrThrow(text);
        }

        internal override string ToString(object instance)
        {
            return ((Type) instance).AssemblyQualifiedName;
        }
    }
}

