namespace System.Data.Services.Client
{
    using System;

    internal sealed class DateTimeTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return PlatformHelper.ConvertStringToDateTime(text);
        }

        internal override string ToString(object instance)
        {
            return PlatformHelper.ConvertDateTimeToString((DateTime) instance);
        }
    }
}

