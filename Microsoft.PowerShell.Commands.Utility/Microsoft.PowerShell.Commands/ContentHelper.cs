namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Net;
    using System.Text;

    internal static class ContentHelper
    {
        private static readonly char[] _contentTypeParamSeparator = new char[] { ';' };
        private const string _defaultCodePage = "ISO-8859-1";

        private static bool CheckIsJson(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            return (contentType.Equals("application/json", StringComparison.OrdinalIgnoreCase) | (((contentType.Equals("text/json", StringComparison.OrdinalIgnoreCase) || contentType.Equals("application/x-javascript", StringComparison.OrdinalIgnoreCase)) || (contentType.Equals("text/x-javascript", StringComparison.OrdinalIgnoreCase) || contentType.Equals("application/javascript", StringComparison.OrdinalIgnoreCase))) || contentType.Equals("text/javascript", StringComparison.OrdinalIgnoreCase)));
        }

        private static bool CheckIsText(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            bool flag = (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) || CheckIsXml(contentType)) || CheckIsJson(contentType);
            if (!flag)
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + contentType))
                {
                    if (key == null)
                    {
                        return flag;
                    }
                    string name = key.GetValue("Extension") as string;
                    if (name == null)
                    {
                        return flag;
                    }
                    using (RegistryKey key2 = Registry.ClassesRoot.OpenSubKey(name))
                    {
                        if (key2 != null)
                        {
                            string str2 = key2.GetValue("PerceivedType") as string;
                            flag = str2 == "text";
                        }
                    }
                }
            }
            return flag;
        }

        private static bool CheckIsXml(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            bool flag = (contentType.Equals("application/xml", StringComparison.OrdinalIgnoreCase) || contentType.Equals("application/xml-external-parsed-entity", StringComparison.OrdinalIgnoreCase)) || contentType.Equals("application/xml-dtd", StringComparison.OrdinalIgnoreCase);
            return (flag | contentType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase));
        }

        internal static string GetContentType(WebResponse response)
        {
            string contentType = null;
            try
            {
                contentType = response.ContentType;
            }
            catch (NotImplementedException)
            {
            }
            return contentType;
        }

        private static string GetContentTypeSignature(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return null;
            }
            return contentType.Split(_contentTypeParamSeparator, 2)[0].ToUpperInvariant();
        }

        internal static Encoding GetDefaultEncoding()
        {
            return GetEncoding((string) null);
        }

        internal static Encoding GetEncoding(WebResponse response)
        {
            string characterSet = null;
            HttpWebResponse response2 = response as HttpWebResponse;
            if (response2 != null)
            {
                characterSet = response2.CharacterSet;
            }
            return GetEncoding(characterSet);
        }

        internal static Encoding GetEncoding(string characterSet)
        {
            string name = string.IsNullOrEmpty(characterSet) ? "ISO-8859-1" : characterSet;
            return Encoding.GetEncoding(name);
        }

        internal static StringBuilder GetRawContentHeader(WebResponse baseResponse)
        {
            StringBuilder builder = new StringBuilder();
            string protocol = WebResponseHelper.GetProtocol(baseResponse);
            if (!string.IsNullOrEmpty(protocol))
            {
                int statusCode = WebResponseHelper.GetStatusCode(baseResponse);
                string statusDescription = WebResponseHelper.GetStatusDescription(baseResponse);
                builder.AppendFormat("{0} {1} {2}", protocol, statusCode, statusDescription);
                builder.AppendLine();
            }
            foreach (string str3 in baseResponse.Headers.AllKeys)
            {
                string str4 = baseResponse.Headers[str3];
                builder.AppendFormat("{0}: {1}", str3, str4);
                builder.AppendLine();
            }
            builder.AppendLine();
            return builder;
        }

        internal static bool IsJson(string contentType)
        {
            contentType = GetContentTypeSignature(contentType);
            return CheckIsJson(contentType);
        }

        internal static bool IsText(string contentType)
        {
            contentType = GetContentTypeSignature(contentType);
            return CheckIsText(contentType);
        }

        internal static bool IsXml(string contentType)
        {
            contentType = GetContentTypeSignature(contentType);
            return CheckIsXml(contentType);
        }
    }
}

