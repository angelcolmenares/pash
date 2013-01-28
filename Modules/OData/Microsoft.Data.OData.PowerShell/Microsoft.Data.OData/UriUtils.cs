namespace Microsoft.Data.OData
{
    using System;

    internal static class UriUtils
    {
        internal static Uri EnsureEscapedRelativeUri(Uri uri)
        {
			if (!uri.IsAbsoluteUri) return uri;
            string components = uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
            if (string.CompareOrdinal(uri.OriginalString, components) == 0)
            {
                return uri;
            }
            return new Uri(components, UriKind.Relative);
        }

        internal static Uri UriToAbsoluteUri(Uri baseUri, Uri relativeUri)
        {
            return new Uri(baseUri, relativeUri);
        }
    }
}

