namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;

    internal static class ODataJsonWriterUtils
    {
        internal static void WriteError(JsonWriter jsonWriter, ODataError error, bool includeDebugInformation, int maxInnerErrorDepth)
        {
            string str;
            string str2;
            string str3;
            ErrorUtils.GetErrorDetails(error, out str, out str2, out str3);
            ODataInnerError innerError = includeDebugInformation ? error.InnerError : null;
            WriteError(jsonWriter, str, str2, str3, innerError, maxInnerErrorDepth);
        }

        private static void WriteError(JsonWriter jsonWriter, string code, string message, string messageLanguage, ODataInnerError innerError, int maxInnerErrorDepth)
        {
            jsonWriter.StartObjectScope();
            jsonWriter.WriteName("error");
            jsonWriter.StartObjectScope();
            jsonWriter.WriteName("code");
            jsonWriter.WriteValue(code);
            jsonWriter.WriteName("message");
            jsonWriter.StartObjectScope();
            jsonWriter.WriteName("lang");
            jsonWriter.WriteValue(messageLanguage);
            jsonWriter.WriteName("value");
            jsonWriter.WriteValue(message);
            jsonWriter.EndObjectScope();
            if (innerError != null)
            {
                WriteInnerError(jsonWriter, innerError, "innererror", 0, maxInnerErrorDepth);
            }
            jsonWriter.EndObjectScope();
            jsonWriter.EndObjectScope();
        }

        private static void WriteInnerError(JsonWriter jsonWriter, ODataInnerError innerError, string innerErrorPropertyName, int recursionDepth, int maxInnerErrorDepth)
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref recursionDepth, maxInnerErrorDepth);
            jsonWriter.WriteName(innerErrorPropertyName);
            jsonWriter.StartObjectScope();
            jsonWriter.WriteName("message");
            jsonWriter.WriteValue(innerError.Message ?? string.Empty);
            jsonWriter.WriteName("type");
            jsonWriter.WriteValue(innerError.TypeName ?? string.Empty);
            jsonWriter.WriteName("stacktrace");
            jsonWriter.WriteValue(innerError.StackTrace ?? string.Empty);
            if (innerError.InnerError != null)
            {
                WriteInnerError(jsonWriter, innerError.InnerError, "internalexception", recursionDepth, maxInnerErrorDepth);
            }
            jsonWriter.EndObjectScope();
        }
    }
}

