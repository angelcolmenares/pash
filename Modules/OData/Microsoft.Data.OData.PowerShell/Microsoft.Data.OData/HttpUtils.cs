namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal static class HttpUtils
    {
        private static IEnumerable<CharsetPart> AcceptCharsetParts(string headerValue)
        {
            bool iteratorVariable0 = false;
            int textIndex = 0;
        Label_PostSwitchInIterator:;
            while (textIndex < headerValue.Length)
            {
                if (SkipWhitespace(headerValue, ref textIndex))
                {
                    break;
                }
                if (headerValue[textIndex] == ',')
                {
                    iteratorVariable0 = false;
                    textIndex++;
                }
                else
                {
                    int iteratorVariable5;
                    int iteratorVariable4;
                    if (iteratorVariable0)
                    {
                        throw new ODataException(Strings.HttpUtils_MissingSeparatorBetweenCharsets(headerValue));
                    }
                    int startIndex = textIndex;
                    int iteratorVariable3 = startIndex;
                    bool iteratorVariable6 = ReadToken(headerValue, ref iteratorVariable3);
                    if (iteratorVariable3 == textIndex)
                    {
                        throw new ODataException(Strings.HttpUtils_InvalidCharsetName(headerValue));
                    }
                    if (iteratorVariable6)
                    {
                        iteratorVariable5 = 0x3e8;
                        iteratorVariable4 = iteratorVariable3;
                    }
                    else
                    {
                        char c = headerValue[iteratorVariable3];
                        if (!IsHttpSeparator(c))
                        {
                            throw new ODataException(Strings.HttpUtils_InvalidSeparatorBetweenCharsets(headerValue));
                        }
                        if (c == ';')
                        {
                            if (ReadLiteral(headerValue, iteratorVariable3, ";q="))
                            {
                                throw new ODataException(Strings.HttpUtils_UnexpectedEndOfQValue(headerValue));
                            }
                            iteratorVariable4 = iteratorVariable3 + 3;
                            ReadQualityValue(headerValue, ref iteratorVariable4, out iteratorVariable5);
                        }
                        else
                        {
                            iteratorVariable5 = 0x3e8;
                            iteratorVariable4 = iteratorVariable3;
                        }
                    }
                    yield return new CharsetPart(headerValue.Substring(startIndex, iteratorVariable3 - startIndex), iteratorVariable5);
                    iteratorVariable0 = true;
                    textIndex = iteratorVariable4;
                    goto Label_PostSwitchInIterator;
                }
            }
        }

        internal static string BuildContentType(MediaType mediaType, Encoding encoding)
        {
            return mediaType.ToText(encoding);
        }

        internal static bool CompareMediaTypeNames(string mediaTypeName1, string mediaTypeName2)
        {
            return string.Equals(mediaTypeName1, mediaTypeName2, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool CompareMediaTypeParameterNames(string parameterName1, string parameterName2)
        {
            return string.Equals(parameterName1, parameterName2, StringComparison.OrdinalIgnoreCase);
        }

        private static int DigitToInt32(char c)
        {
            if ((c >= '0') && (c <= '9'))
            {
                return (c - '0');
            }
            if (!IsHttpElementSeparator(c))
            {
                throw new ODataException(Strings.HttpUtils_CannotConvertCharToInt(c));
            }
            return -1;
        }

        internal static Encoding EncodingFromAcceptableCharsets(string acceptableCharsets, MediaType mediaType, Encoding utf8Encoding, Encoding defaultEncoding)
        {
            Encoding encodingFromCharsetName = null;
            if (!string.IsNullOrEmpty(acceptableCharsets))
            {
                foreach (KeyValuePair<int, CharsetPart> pair in new List<CharsetPart>(AcceptCharsetParts(acceptableCharsets)).ToArray().StableSort<CharsetPart>((x, y) => y.Quality - x.Quality))
                {
                    CharsetPart part = pair.Value;
                    if (part.Quality > 0)
                    {
                        if (string.Compare("utf-8", part.Charset, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            encodingFromCharsetName = utf8Encoding;
                            break;
                        }
                        encodingFromCharsetName = GetEncodingFromCharsetName(part.Charset);
                        if (encodingFromCharsetName != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (encodingFromCharsetName == null)
            {
                encodingFromCharsetName = mediaType.SelectEncoding();
                if (encodingFromCharsetName == null)
                {
                    return defaultEncoding;
                }
            }
            return encodingFromCharsetName;
        }

        internal static Encoding GetEncodingFromCharsetName(string charsetName)
        {
            try
            {
                return Encoding.GetEncoding(charsetName, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        internal static string GetStatusMessage(int statusCode)
        {
            switch (statusCode)
            {
                case 100:
                    return "Continue";

                case 0x65:
                    return "Switching Protocols";

                case 200:
                    return "OK";

                case 0xc9:
                    return "Created";

                case 0xca:
                    return "Accepted";

                case 0xcb:
                    return "Non-Authoritative Information";

                case 0xcc:
                    return "No Content";

                case 0xcd:
                    return "Reset Content";

                case 0xce:
                    return "Partial Content";

                case 300:
                    return "Multiple Choices";

                case 0x12d:
                    return "Moved Permanently";

                case 0x12e:
                    return "Found";

                case 0x12f:
                    return "See Other";

                case 0x130:
                    return "Not Modified";

                case 0x131:
                    return "Use Proxy";

                case 0x133:
                    return "Temporary Redirect";

                case 400:
                    return "Bad Request";

                case 0x191:
                    return "Unauthorized";

                case 0x192:
                    return "Payment Required";

                case 0x193:
                    return "Forbidden";

                case 0x194:
                    return "Not Found";

                case 0x195:
                    return "Method Not Allowed";

                case 0x196:
                    return "Not Acceptable";

                case 0x197:
                    return "Proxy Authentication Required";

                case 0x198:
                    return "Request Time-out";

                case 0x199:
                    return "Conflict";

                case 410:
                    return "Gone";

                case 0x19b:
                    return "Length Required";

                case 0x19c:
                    return "Precondition Failed";

                case 0x19d:
                    return "Request Entity Too Large";

                case 0x19e:
                    return "Request-URI Too Large";

                case 0x19f:
                    return "Unsupported Media Type";

                case 0x1a0:
                    return "Requested range not satisfiable";

                case 0x1a1:
                    return "Expectation Failed";

                case 500:
                    return "Internal Server Error";

                case 0x1f5:
                    return "Not Implemented";

                case 0x1f6:
                    return "Bad Gateway";

                case 0x1f7:
                    return "Service Unavailable";

                case 0x1f8:
                    return "Gateway Time-out";

                case 0x1f9:
                    return "HTTP Version not supported";
            }
            return "Unknown Status Code";
        }

        private static bool IsHttpElementSeparator(char c)
        {
            if ((c != ',') && (c != ' '))
            {
                return (c == '\t');
            }
            return true;
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

        private static bool IsValidInQuotedHeaderValue(char c)
        {
            int num = c;
            return ((((num >= 0x20) || (c == ' ')) || (c == '\t')) && (num != 0x7f));
        }

        internal static IList<KeyValuePair<MediaType, string>> MediaTypesFromString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            return ReadMediaTypes(text);
        }

        private static bool ReadLiteral(string text, int textIndex, string literal)
        {
            if (string.Compare(text, textIndex, literal, 0, literal.Length, StringComparison.Ordinal) != 0)
            {
                throw new ODataException(Strings.HttpUtils_ExpectedLiteralNotFoundInString(literal, textIndex, text));
            }
            return ((textIndex + literal.Length) == text.Length);
        }

        private static void ReadMediaTypeAndSubtype(string mediaTypeName, ref int textIndex, out string type, out string subType)
        {
            int startIndex = textIndex;
            if (ReadToken(mediaTypeName, ref textIndex))
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeUnspecified(mediaTypeName));
            }
            if (mediaTypeName[textIndex] != '/')
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeRequiresSlash(mediaTypeName));
            }
            type = mediaTypeName.Substring(startIndex, textIndex - startIndex);
            textIndex++;
            int num2 = textIndex;
            ReadToken(mediaTypeName, ref textIndex);
            if (textIndex == num2)
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeRequiresSubType(mediaTypeName));
            }
            subType = mediaTypeName.Substring(num2, textIndex - num2);
        }

        private static void ReadMediaTypeParameter(string text, ref int textIndex, ref List<KeyValuePair<string, string>> parameters, ref string charset)
        {
            int startIndex = textIndex;
            bool flag = ReadToken(text, ref textIndex);
            string str = text.Substring(startIndex, textIndex - startIndex);
            if (str.Length == 0)
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeMissingParameterName);
            }
            if (flag)
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeMissingParameterValue(str));
            }
            if (text[textIndex] != '=')
            {
                throw new ODataException(Strings.HttpUtils_MediaTypeMissingParameterValue(str));
            }
            textIndex++;
            string str2 = ReadQuotedParameterValue(str, text, ref textIndex);
            if (CompareMediaTypeParameterNames("charset", str))
            {
                charset = str2;
            }
            else
            {
                if (parameters == null)
                {
                    parameters = new List<KeyValuePair<string, string>>(1);
                }
                parameters.Add(new KeyValuePair<string, string>(str, str2));
            }
        }

        private static IList<KeyValuePair<MediaType, string>> ReadMediaTypes(string text)
        {
            List<KeyValuePair<string, string>> parameters = null;
            List<KeyValuePair<MediaType, string>> list2 = new List<KeyValuePair<MediaType, string>>();
            int textIndex = 0;
            while (!SkipWhitespace(text, ref textIndex))
            {
                string str;
                string str2;
                ReadMediaTypeAndSubtype(text, ref textIndex, out str, out str2);
                string charset = null;
                while (!SkipWhitespace(text, ref textIndex))
                {
                    if (text[textIndex] == ',')
                    {
                        textIndex++;
                        break;
                    }
                    if (text[textIndex] != ';')
                    {
                        throw new ODataException(Strings.HttpUtils_MediaTypeRequiresSemicolonBeforeParameter(text));
                    }
                    textIndex++;
                    if (SkipWhitespace(text, ref textIndex))
                    {
                        break;
                    }
                    ReadMediaTypeParameter(text, ref textIndex, ref parameters, ref charset);
                }
                list2.Add(new KeyValuePair<MediaType, string>(new MediaType(str, str2, (parameters == null) ? null : ((IList<KeyValuePair<string, string>>) parameters.ToArray())), charset));
                parameters = null;
            }
            return list2;
        }

        internal static IList<KeyValuePair<string, string>> ReadMimeType(string contentType, out string mediaTypeName, out string mediaTypeCharset)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ODataException(Strings.HttpUtils_ContentTypeMissing);
            }
            IList<KeyValuePair<MediaType, string>> list = ReadMediaTypes(contentType);
            if (list.Count != 1)
            {
                throw new ODataException(Strings.HttpUtils_NoOrMoreThanOneContentTypeSpecified(contentType));
            }
            KeyValuePair<MediaType, string> pair = list[0];
            MediaType key = pair.Key;
            MediaTypeUtils.CheckMediaTypeForWildCards(key);
            mediaTypeName = key.FullTypeName;
            KeyValuePair<MediaType, string> pair2 = list[0];
            mediaTypeCharset = pair2.Value;
            return key.Parameters;
        }

        internal static void ReadQualityValue(string text, ref int textIndex, out int qualityValue)
        {
            char ch = text[textIndex++];
            switch (ch)
            {
                case '0':
                    qualityValue = 0;
                    break;

                case '1':
                    qualityValue = 1;
                    break;

                default:
                    throw new ODataException(Strings.HttpUtils_InvalidQualityValueStartChar(text, ch));
            }
            if ((textIndex >= text.Length) || (text[textIndex] != '.'))
            {
                qualityValue *= 0x3e8;
            }
            else
            {
                textIndex++;
                int num = 0x3e8;
                while ((num > 1) && (textIndex < text.Length))
                {
                    char c = text[textIndex];
                    int num2 = DigitToInt32(c);
                    if (num2 < 0)
                    {
                        break;
                    }
                    textIndex++;
                    num /= 10;
                    qualityValue *= 10;
                    qualityValue += num2;
                }
                qualityValue *= num;
                if (qualityValue > 0x3e8)
                {
                    throw new ODataException(Strings.HttpUtils_InvalidQualityValue((int) (qualityValue / 0x3e8), text));
                }
            }
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
                        throw new ODataException(Strings.HttpUtils_EscapeCharWithoutQuotes(parameterName));
                    }
                    textIndex++;
                    if (c == '"')
                    {
                        flag = false;
                        break;
                    }
                    if (textIndex >= headerText.Length)
                    {
                        throw new ODataException(Strings.HttpUtils_EscapeCharAtEnd(parameterName));
                    }
                    c = headerText[textIndex];
                }
                else
                {
                    if (!flag && !IsHttpToken(c))
                    {
                        break;
                    }
                    if (flag && !IsValidInQuotedHeaderValue(c))
                    {
                        throw new ODataException(Strings.HttpUtils_InvalidCharacterInQuotedParameterValue(headerText, c, (int) textIndex));
                    }
                }
                builder.Append(c);
                textIndex++;
            }
            if (flag)
            {
                throw new ODataException(Strings.HttpUtils_ClosingQuoteNotFound(parameterName));
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

        internal static string ToText(this MediaType mediaType)
        {
            return mediaType.ToText(null);
        }

        internal static string ToText(this MediaType mediaType, Encoding encoding)
        {
            if ((mediaType.Parameters == null) || (mediaType.Parameters.Count == 0))
            {
                string fullTypeName = mediaType.FullTypeName;
                if (encoding != null)
                {
                    fullTypeName = fullTypeName + ";" + "charset" + "=" + encoding.WebName;
                }
                return fullTypeName;
            }
            StringBuilder builder = new StringBuilder(mediaType.FullTypeName);
            for (int i = 0; i < mediaType.Parameters.Count; i++)
            {
                KeyValuePair<string, string> pair = mediaType.Parameters[i];
                if (!CompareMediaTypeParameterNames("charset", pair.Key))
                {
                    builder.Append(";");
                    builder.Append(pair.Key);
                    builder.Append("=");
                    builder.Append(pair.Value);
                }
            }
            if (encoding != null)
            {
                builder.Append(";");
                builder.Append("charset");
                builder.Append("=");
                builder.Append(encoding.WebName);
            }
            return builder.ToString();
        }

        internal static void ValidateHttpMethod(string httpMethodString)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(httpMethodString, "httpMethodString");
            if ((((string.CompareOrdinal(httpMethodString, "GET") != 0) && (string.CompareOrdinal(httpMethodString, "DELETE") != 0)) && ((string.CompareOrdinal(httpMethodString, "MERGE") != 0) && (string.CompareOrdinal(httpMethodString, "PATCH") != 0))) && ((string.CompareOrdinal(httpMethodString, "POST") != 0) && (string.CompareOrdinal(httpMethodString, "PUT") != 0)))
            {
                throw new ODataException(Strings.HttpUtils_InvalidHttpMethodString(httpMethodString));
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        private struct CharsetPart
        {
            internal readonly string Charset;
            internal readonly int Quality;
            internal CharsetPart(string charset, int quality)
            {
                this.Charset = charset;
                this.Quality = quality;
            }
        }
    }
}

