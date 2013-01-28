namespace Microsoft.Data.OData
{
    using System;

    internal static class UriUtilsCommon
    {
        internal static string UriToString(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return uri.OriginalString;
            }
            return uri.AbsoluteUri;
        }
    }
}

