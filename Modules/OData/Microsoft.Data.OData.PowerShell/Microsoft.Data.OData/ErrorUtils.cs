namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal static class ErrorUtils
    {
        internal const string ODataErrorMessageDefaultLanguage = "en-US";

        internal static void GetErrorDetails(ODataError error, out string code, out string message, out string messageLanguage)
        {
            code = error.ErrorCode ?? string.Empty;
            message = error.Message ?? string.Empty;
            messageLanguage = error.MessageLanguage ?? "en-US";
        }

        internal static void WriteXmlError(XmlWriter writer, ODataError error, bool includeDebugInformation, int maxInnerErrorDepth)
        {
            string str;
            string str2;
            string str3;
            GetErrorDetails(error, out str, out str2, out str3);
            ODataInnerError innerError = includeDebugInformation ? error.InnerError : null;
            WriteXmlError(writer, str, str2, str3, innerError, maxInnerErrorDepth);
        }

        private static void WriteXmlError(XmlWriter writer, string code, string message, string messageLanguage, ODataInnerError innerError, int maxInnerErrorDepth)
        {
            writer.WriteStartElement("m", "error", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteElementString("m", "code", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", code);
            writer.WriteStartElement("m", "message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", messageLanguage);
            writer.WriteString(message);
            writer.WriteEndElement();
            if (innerError != null)
            {
                WriteXmlInnerError(writer, innerError, "innererror", 0, maxInnerErrorDepth);
            }
            writer.WriteEndElement();
        }

        private static void WriteXmlInnerError(XmlWriter writer, ODataInnerError innerError, string innerErrorElementName, int recursionDepth, int maxInnerErrorDepth)
        {
            recursionDepth++;
            if (recursionDepth > maxInnerErrorDepth)
            {
                throw new ODataException(Strings.ValidationUtils_RecursionDepthLimitReached(maxInnerErrorDepth));
            }
            writer.WriteStartElement("m", innerErrorElementName, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            string text = innerError.Message ?? string.Empty;
            writer.WriteStartElement("message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteString(text);
            writer.WriteEndElement();
            string str2 = innerError.TypeName ?? string.Empty;
            writer.WriteStartElement("type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteString(str2);
            writer.WriteEndElement();
            string str3 = innerError.StackTrace ?? string.Empty;
            writer.WriteStartElement("stacktrace", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteString(str3);
            writer.WriteEndElement();
            if (innerError.InnerError != null)
            {
                WriteXmlInnerError(writer, innerError.InnerError, "internalexception", recursionDepth, maxInnerErrorDepth);
            }
            writer.WriteEndElement();
        }
    }
}

