namespace System.Data.Services.Client
{
    using System;

    internal sealed class UriTypeConverter : PrimitiveTypeConverter
    {
        internal override object Parse(string text)
        {
            return Util.CreateUri(text, UriKind.RelativeOrAbsolute);
        }

        internal override string ToString(object instance)
        {
            return CommonUtil.UriToString((Uri) instance);
        }
    }
}

