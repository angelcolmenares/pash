namespace System.Data.Services
{
    using System;
    using System.Globalization;

    internal static class UriUtil
    {
        private static Uri CreateBaseComparableUri(Uri uri)
        {
			var strUri = CommonUtil.UriToString(uri).ToUpper(CultureInfo.InvariantCulture);
			if (strUri.EndsWith (".svc", StringComparison.OrdinalIgnoreCase)) strUri += "/";
			uri = new Uri(strUri, UriKind.RelativeOrAbsolute);
            UriBuilder builder = new UriBuilder(uri) {
                Host = "h",
                Port = 80,
                Scheme = "http"
            };
            return builder.Uri;
        }

        internal static bool IsBaseOf(Uri baseUriWithSlash, Uri requestUri)
        {
            return baseUriWithSlash.IsBaseOf(requestUri);
        }

        internal static string ReadSegmentValue(string segment)
        {
            if ((segment.Length == 0) || !(segment != "/"))
            {
                return null;
            }
            if (segment[segment.Length - 1] == '/')
            {
                segment = segment.Substring(0, segment.Length - 1);
            }
            return Uri.UnescapeDataString(segment);
        }

        internal static bool UriInvariantInsensitiveIsBaseOf(Uri current, Uri uri)
        {
            Uri baseUriWithSlash = CreateBaseComparableUri(current);
            Uri requestUri = CreateBaseComparableUri(uri);
            return IsBaseOf(baseUriWithSlash, requestUri);
        }
    }
}

