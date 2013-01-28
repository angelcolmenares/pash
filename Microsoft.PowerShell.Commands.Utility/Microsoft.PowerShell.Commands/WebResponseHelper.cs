namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Net;

    internal static class WebResponseHelper
    {
        internal static string GetCharacterSet(WebResponse response)
        {
            string characterSet = null;
            HttpWebResponse response2 = response as HttpWebResponse;
            if (response2 != null)
            {
                characterSet = response2.CharacterSet;
            }
            return characterSet;
        }

        internal static string GetProtocol(WebResponse response)
        {
            string str = string.Empty;
            HttpWebResponse response2 = response as HttpWebResponse;
            if (response2 != null)
            {
                str = string.Format(CultureInfo.InvariantCulture, "HTTP/{0}", new object[] { response2.ProtocolVersion });
            }
            return str;
        }

        internal static int GetStatusCode(WebResponse response)
        {
            int statusCode = 0;
            HttpWebResponse response2 = response as HttpWebResponse;
            if (response2 != null)
            {
                statusCode = (int) response2.StatusCode;
            }
            return statusCode;
        }

        internal static string GetStatusDescription(WebResponse response)
        {
            string statusDescription = string.Empty;
            HttpWebResponse response2 = response as HttpWebResponse;
            if (response2 != null)
            {
                statusDescription = response2.StatusDescription;
            }
            return statusDescription;
        }

        internal static bool IsText(WebResponse response)
        {
            return ContentHelper.IsText(ContentHelper.GetContentType(response));
        }
    }
}

