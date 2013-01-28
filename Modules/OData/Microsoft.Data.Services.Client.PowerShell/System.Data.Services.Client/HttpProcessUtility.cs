namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class HttpProcessUtility
    {
        internal static readonly UTF8Encoding EncodingUtf8NoPreamble = new UTF8Encoding(false, true);

        private static Encoding EncodingFromName(string name)
        {
            Encoding encoding;
            if (name == null)
            {
                return MissingEncoding;
            }
            name = name.Trim();
            if (name.Length == 0)
            {
                return MissingEncoding;
            }
            try
            {
                encoding = Encoding.GetEncoding(name);
            }
            catch (ArgumentException)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EncodingNotSupported(name));
            }
            return encoding;
        }

        private static bool IsHttpSeparator(char c)
        {
            if ((((((c != '(') && (c != ')')) && ((c != '<') && (c != '>'))) && (((c != '@') && (c != ',')) && ((c != ';') && (c != ':')))) && ((((c != '\\') && (c != '"')) && ((c != '/') && (c != '['))) && (((c != ']') && (c != '?')) && ((c != '=') && (c != '{'))))) && ((c != '}') && (c != ' ')))
            {
                return (c == '\t');
            }
            return true;
        }

        private static bool IsHttpToken(char c)
        {
            return (((c < '\x007f') && (c > '\x001f')) && !IsHttpSeparator(c));
        }

        internal static KeyValuePair<string, string>[] ReadContentType(string contentType, out string mime, out Encoding encoding)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_ContentTypeMissing);
            }
            MediaType type = ReadMediaType(contentType);
            mime = type.MimeType;
            encoding = type.SelectEncoding();
            return type.Parameters;
        }

        private static MediaType ReadMediaType(string text)
        {
            string str;
            string str2;
            int textIndex = 0;
            ReadMediaTypeAndSubtype(text, ref textIndex, out str, out str2);
            KeyValuePair<string, string>[] parameters = null;
            while (!SkipWhitespace(text, ref textIndex))
            {
                if (text[textIndex] != ';')
                {
                    throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
                }
                textIndex++;
                if (SkipWhitespace(text, ref textIndex))
                {
                    break;
                }
                ReadMediaTypeParameter(text, ref textIndex, ref parameters);
            }
            return new MediaType(str, str2, parameters);
        }

        private static void ReadMediaTypeAndSubtype(string text, ref int textIndex, out string type, out string subType)
        {
            int startIndex = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeUnspecified);
            }
            if (text[textIndex] != '/')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSlash);
            }
            type = text.Substring(startIndex, textIndex - startIndex);
            textIndex++;
            int num2 = textIndex;
            ReadToken(text, ref textIndex);
            if (textIndex == num2)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSubType);
            }
            subType = text.Substring(num2, textIndex - num2);
        }

        private static void ReadMediaTypeParameter(string text, ref int textIndex, ref KeyValuePair<string, string>[] parameters)
        {
            int startIndex = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }
            string parameterName = text.Substring(startIndex, textIndex - startIndex);
            if (text[textIndex] != '=')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }
            textIndex++;
            string str2 = ReadQuotedParameterValue(parameterName, text, ref textIndex);
            if (parameters == null)
            {
                parameters = new KeyValuePair<string, string>[1];
            }
            else
            {
                KeyValuePair<string, string>[] destinationArray = new KeyValuePair<string, string>[parameters.Length + 1];
                Array.Copy(parameters, destinationArray, parameters.Length);
                parameters = destinationArray;
            }
            parameters[parameters.Length - 1] = new KeyValuePair<string, string>(parameterName, str2);
        }

        private static string ReadQuotedParameterValue(string parameterName, string headerText, ref int textIndex)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if ((textIndex < headerText.Length) && (headerText[textIndex] == '"'))
            {
                textIndex++;
                flag = true;
            }
            while (textIndex < headerText.Length)
            {
                char c = headerText[textIndex];
                if ((c == '\\') || (c == '"'))
                {
                    if (!flag)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharWithoutQuotes(parameterName));
                    }
                    textIndex++;
                    if (c == '"')
                    {
                        flag = false;
                        break;
                    }
                    if (textIndex >= headerText.Length)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharAtEnd(parameterName));
                    }
                    c = headerText[textIndex];
                }
                else if (!IsHttpToken(c))
                {
                    break;
                }
                builder.Append(c);
                textIndex++;
            }
            if (flag)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_ClosingQuoteNotFound(parameterName));
            }
            return builder.ToString();
        }

        private static bool ReadToken(string text, ref int textIndex)
        {
            while ((textIndex < text.Length) && IsHttpToken(text[textIndex]))
            {
                textIndex++;
            }
            return (textIndex == text.Length);
        }

        private static bool SkipWhitespace(string text, ref int textIndex)
        {
            while ((textIndex < text.Length) && char.IsWhiteSpace(text, textIndex))
            {
                textIndex++;
            }
            return (textIndex == text.Length);
        }

        internal static bool TryReadVersion(string text, out KeyValuePair<Version, string> result)
        {
            string str;
            string str2;
            int index = text.IndexOf(';');
            if (index >= 0)
            {
                str = text.Substring(0, index);
                str2 = text.Substring(index + 1).Trim();
            }
            else
            {
                str = text;
                str2 = null;
            }
            result = new KeyValuePair<Version, string>();
            str = str.Trim();
            bool flag = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '.')
                {
                    if (flag)
                    {
                        return false;
                    }
                    flag = true;
                }
                else if ((str[i] < '0') || (str[i] > '9'))
                {
                    return false;
                }
            }
            try
            {
                result = new KeyValuePair<Version, string>(new Version(str), str2);
                return true;
            }
            catch (Exception exception)
            {
                if (!CommonUtil.IsCatchableExceptionType(exception) || ((!(exception is FormatException) && !(exception is OverflowException)) && !(exception is ArgumentException)))
                {
                    throw;
                }
                return false;
            }
        }

        internal static Encoding FallbackEncoding
        {
            get
            {
                return EncodingUtf8NoPreamble;
            }
        }

        private static Encoding MissingEncoding
        {
            get
            {
                return Encoding.GetEncoding("ISO-8859-1", new EncoderExceptionFallback(), new DecoderExceptionFallback());
            }
        }

        [DebuggerDisplay("MediaType [{type}/{subType}]")]
        private sealed class MediaType
        {
            private readonly KeyValuePair<string, string>[] parameters;
            private readonly string subType;
            private readonly string type;

            internal MediaType(string type, string subType, KeyValuePair<string, string>[] parameters)
            {
                this.type = type;
                this.subType = subType;
                this.parameters = parameters;
            }

            internal Encoding SelectEncoding()
            {
                if (this.parameters != null)
                {
                    foreach (KeyValuePair<string, string> pair in this.parameters)
                    {
                        if (string.Equals(pair.Key, "charset", StringComparison.OrdinalIgnoreCase) && (pair.Value.Trim().Length > 0))
                        {
                            return HttpProcessUtility.EncodingFromName(pair.Value);
                        }
                    }
                }
                if (string.Equals(this.type, "text", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(this.subType, "xml", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    return HttpProcessUtility.MissingEncoding;
                }
                if (string.Equals(this.type, "application", StringComparison.OrdinalIgnoreCase) && string.Equals(this.subType, "json", StringComparison.OrdinalIgnoreCase))
                {
                    return HttpProcessUtility.FallbackEncoding;
                }
                return null;
            }

            internal string MimeType
            {
                get
                {
                    return (this.type + "/" + this.subType);
                }
            }

            internal KeyValuePair<string, string>[] Parameters
            {
                get
                {
                    return this.parameters;
                }
            }
        }
    }
}

