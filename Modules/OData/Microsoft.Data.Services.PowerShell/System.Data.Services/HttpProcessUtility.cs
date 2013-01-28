namespace System.Data.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal static class HttpProcessUtility
    {
        internal static readonly UTF8Encoding EncodingUtf8NoPreamble = new UTF8Encoding(false, true);

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
                        throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
                    }
                    int startIndex = textIndex;
                    int iteratorVariable3 = startIndex;
                    bool iteratorVariable6 = ReadToken(headerValue, ref iteratorVariable3);
                    if (iteratorVariable3 == textIndex)
                    {
                        throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
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
                            throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
                        }
                        if (c == ';')
                        {
                            if (ReadLiteral(headerValue, iteratorVariable3, ";q="))
                            {
                                throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
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

        internal static string BuildContentType(string mime, Encoding encoding)
        {
            if (encoding == null)
            {
                return mime;
            }
            return (mime + ";charset=" + encoding.WebName);
        }

        private static DataServiceException CreateParsingException(string message)
        {
            return System.Data.Services.Error.HttpHeaderFailure(400, message);
        }

        private static int DigitToInt32(char c)
        {
            if ((c >= '0') && (c <= '9'))
            {
                return (c - '0');
            }
            if (!IsHttpElementSeparator(c))
            {
                throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
            }
            return -1;
        }

        internal static Encoding EncodingFromAcceptCharset(string acceptCharset)
        {
            Encoding fallbackEncoding = null;
            if (!string.IsNullOrEmpty(acceptCharset))
            {
                List<CharsetPart> list = new List<CharsetPart>(AcceptCharsetParts(acceptCharset));
                list.Sort((Comparison<CharsetPart>) ((x, y) => (y.Quality - x.Quality)));
                EncoderExceptionFallback encoderFallback = new EncoderExceptionFallback();
                DecoderExceptionFallback decoderFallback = new DecoderExceptionFallback();
                foreach (CharsetPart part in list)
                {
                    if (part.Quality > 0)
                    {
                        if (string.Compare("UTF-8", part.Charset, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            fallbackEncoding = FallbackEncoding;
                            break;
                        }
                        try
                        {
                            fallbackEncoding = Encoding.GetEncoding(part.Charset, encoderFallback, decoderFallback);
                            break;
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }
            }
            return (fallbackEncoding ?? FallbackEncoding);
        }

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
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_EncodingNotSupported(name));
            }
            return encoding;
        }

        internal static bool? GetReturnContentPreference(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int textIndex = 0;
                if (ReadToken(value, ref textIndex))
                {
                    string a = value.Substring(0, textIndex);
                    if (string.Equals(a, "return-content", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (string.Equals(a, "return-no-content", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return null;
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

        private static IEnumerable<MediaType> MimeTypesFromAcceptHeader(string text)
        {
            List<MediaType> list = new List<MediaType>();
            int textIndex = 0;
            while (!SkipWhitespace(text, ref textIndex))
            {
                string str;
                string str2;
                ReadMediaTypeAndSubtype(text, ref textIndex, out str, out str2);
                KeyValuePair<string, string>[] parameters = null;
                while (!SkipWhitespace(text, ref textIndex))
                {
                    if (text[textIndex] == ',')
                    {
                        textIndex++;
                        break;
                    }
                    if (text[textIndex] != ';')
                    {
                        throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
                    }
                    textIndex++;
                    if (SkipWhitespace(text, ref textIndex))
                    {
                        break;
                    }
                    ReadMediaTypeParameter(text, ref textIndex, ref parameters);
                }
                list.Add(new MediaType(str, str2, parameters));
            }
            return list;
        }

        internal static KeyValuePair<string, string>[] ReadContentType(string contentType, out string mime, out Encoding encoding)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_ContentTypeMissing);
            }
            MediaType type = ReadMediaType(contentType);
            mime = type.MimeType;
            encoding = type.SelectEncoding();
            return type.Parameters;
        }

        private static bool ReadLiteral(string text, int textIndex, string literal)
        {
            if (string.Compare(text, textIndex, literal, 0, literal.Length, StringComparison.Ordinal) != 0)
            {
                throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
            }
            return ((textIndex + literal.Length) == text.Length);
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
                    throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
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
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeUnspecified);
            }
            if (text[textIndex] != '/')
            {
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeRequiresSlash);
            }
            type = text.Substring(startIndex, textIndex - startIndex);
            textIndex++;
            int num2 = textIndex;
            ReadToken(text, ref textIndex);
            if (textIndex == num2)
            {
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeRequiresSubType);
            }
            subType = text.Substring(num2, textIndex - num2);
        }

        private static void ReadMediaTypeParameter(string text, ref int textIndex, ref KeyValuePair<string, string>[] parameters)
        {
            int startIndex = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeMissingValue);
            }
            string parameterName = text.Substring(startIndex, textIndex - startIndex);
            if (text[textIndex] != '=')
            {
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_MediaTypeMissingValue);
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

        private static void ReadQualityValue(string text, ref int textIndex, out int qualityValue)
        {
            switch (text[textIndex++])
            {
                case '0':
                    qualityValue = 0;
                    break;

                case '1':
                    qualityValue = 1;
                    break;

                default:
                    throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
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
                    throw CreateParsingException(System.Data.Services.Strings.HttpContextServiceHost_MalformedHeaderValue);
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
                        throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_EscapeCharWithoutQuotes(parameterName));
                    }
                    textIndex++;
                    if (c == '"')
                    {
                        flag = false;
                        break;
                    }
                    if (textIndex >= headerText.Length)
                    {
                        throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_EscapeCharAtEnd(parameterName));
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
                throw System.Data.Services.Error.HttpHeaderFailure(400, System.Data.Services.Strings.HttpProcessUtility_ClosingQuoteNotFound(parameterName));
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

        internal static string SelectMimeType(string acceptTypesText, string[] availableTypes)
        {
            string str = null;
            int num = -1;
            int num2 = 0;
            int num3 = 0x7fffffff;
            bool flag = false;
            bool flag2 = true;
            if (!string.IsNullOrEmpty(acceptTypesText))
            {
                foreach (MediaType type in MimeTypesFromAcceptHeader(acceptTypesText))
                {
                    flag2 = false;
                    for (int i = 0; i < availableTypes.Length; i++)
                    {
                        string candidate = availableTypes[i];
                        int matchingParts = type.GetMatchingParts(candidate);
                        if (matchingParts >= 0)
                        {
                            if (matchingParts > num)
                            {
                                str = candidate;
                                num = matchingParts;
                                num2 = type.SelectQualityValue();
                                num3 = i;
                                flag = num2 != 0;
                            }
                            else if (matchingParts == num)
                            {
                                int num6 = type.SelectQualityValue();
                                if (num6 > num2)
                                {
                                    str = candidate;
                                    num2 = num6;
                                    num3 = i;
                                    flag = num2 != 0;
                                }
                                else if ((num6 == num2) && (i < num3))
                                {
                                    str = candidate;
                                    num3 = i;
                                }
                            }
                        }
                    }
                }
            }
            if (flag2)
            {
                return availableTypes[0];
            }
            if (!flag)
            {
                str = null;
            }
            return str;
        }

        internal static string SelectRequiredMimeType(string acceptTypesText, string[] exactContentType, string inexactContentType)
        {
            string str = null;
            int num = -1;
            int num2 = 0;
            bool flag = false;
            bool flag2 = true;
            bool flag3 = false;
            if (!string.IsNullOrEmpty(acceptTypesText))
            {
                foreach (MediaType type in MimeTypesFromAcceptHeader(acceptTypesText))
                {
                    flag2 = false;
                    for (int i = 0; i < exactContentType.Length; i++)
                    {
                        if (WebUtil.CompareMimeType(type.MimeType, exactContentType[i]))
                        {
                            str = exactContentType[i];
                            num2 = type.SelectQualityValue();
                            flag = num2 != 0;
                            flag3 = true;
                            break;
                        }
                    }
                    if (flag3)
                    {
                        break;
                    }
                    int matchingParts = type.GetMatchingParts(inexactContentType);
                    if (matchingParts >= 0)
                    {
                        if (matchingParts > num)
                        {
                            str = inexactContentType;
                            num = matchingParts;
                            num2 = type.SelectQualityValue();
                            flag = num2 != 0;
                        }
                        else if (matchingParts == num)
                        {
                            int num5 = type.SelectQualityValue();
                            if (num5 > num2)
                            {
                                str = inexactContentType;
                                num2 = num5;
                                flag = num2 != 0;
                            }
                        }
                    }
                }
            }
            if (!flag && !flag2)
            {
                throw System.Data.Services.Error.HttpHeaderFailure(0x19f, System.Data.Services.Strings.DataServiceException_UnsupportedMediaType);
            }
            if (flag2)
            {
                str = inexactContentType;
            }
            return str;
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

            internal int GetMatchingParts(HttpProcessUtility.MediaType candidate)
            {
                int num = -1;
                if (candidate != null)
                {
                    if (this.type == "*")
                    {
                        return 0;
                    }
                    if (candidate.subType == null)
                    {
                        return num;
                    }
                    string type = candidate.type;
                    if (!WebUtil.CompareMimeType(this.type, type))
                    {
                        return num;
                    }
                    if (this.subType == "*")
                    {
                        return 1;
                    }
                    string subType = candidate.subType;
                    if (WebUtil.CompareMimeType(this.subType, subType) && string.Equals(this.GetParameterValue("odata"), candidate.GetParameterValue("odata"), StringComparison.OrdinalIgnoreCase))
                    {
                        num = 2;
                    }
                }
                return num;
            }

            internal int GetMatchingParts(string candidate)
            {
                return this.GetMatchingParts(HttpProcessUtility.MimeTypesFromAcceptHeader(candidate).Single<HttpProcessUtility.MediaType>());
            }

            internal string GetParameterValue(string parameterName)
            {
                if (this.parameters != null)
                {
                    foreach (KeyValuePair<string, string> pair in this.parameters)
                    {
                        if (string.Equals(pair.Key, parameterName, StringComparison.OrdinalIgnoreCase) && (pair.Value.Trim().Length > 0))
                        {
                            return pair.Value;
                        }
                    }
                }
                return null;
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

            internal int SelectQualityValue()
            {
                int num;
                string parameterValue = this.GetParameterValue("q");
                if (parameterValue == null)
                {
                    return 0x3e8;
                }
                int textIndex = 0;
                HttpProcessUtility.ReadQualityValue(parameterValue, ref textIndex, out num);
                return num;
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

