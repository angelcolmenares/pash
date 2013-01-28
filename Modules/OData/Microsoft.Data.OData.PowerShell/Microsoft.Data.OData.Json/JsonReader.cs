namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [DebuggerDisplay("{NodeType}: {Value}")]
    internal class JsonReader
    {
        private char[] characterBuffer;
        private const string DateTimeFormatPrefix = "/Date(";
        private const string DateTimeFormatSuffix = ")/";
        private bool endOfInputReached;
        private const int InitialCharacterBufferSize = 0x7f8;
        private const int MaxCharacterCountToMove = 0x40;
        private JsonNodeType nodeType = JsonNodeType.None;
        private object nodeValue = null;
        private readonly TextReader reader;
        private readonly Stack<Scope> scopes;
        private int storedCharacterCount;
        private StringBuilder stringValueBuilder;
        private int tokenStartIndex;

        public JsonReader(TextReader reader)
        {
            this.reader = reader;
            this.characterBuffer = new char[0x7f8];
            this.storedCharacterCount = 0;
            this.tokenStartIndex = 0;
            this.endOfInputReached = false;
            this.scopes = new Stack<Scope>();
            this.scopes.Push(new Scope(ScopeType.Root));
        }

        private string ConsumeTokenToString(int characterCount)
        {
            string str = new string(this.characterBuffer, this.tokenStartIndex, characterCount);
            this.tokenStartIndex += characterCount;
            return str;
        }

        private bool EndOfInput()
        {
            if (this.scopes.Count > 1)
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_EndOfInputWithOpenScope);
            }
            this.nodeType = JsonNodeType.EndOfInput;
            return false;
        }

        private bool EnsureAvailableCharacters(int characterCountAfterTokenStart)
        {
            while ((this.tokenStartIndex + characterCountAfterTokenStart) > this.storedCharacterCount)
            {
                if (!this.ReadInput())
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsWhitespaceCharacter(char character)
        {
            return ((character <= ' ') && (((character == ' ') || (character == '\t')) || ((character == '\n') || (character == '\r'))));
        }

        private object ParseBooleanPrimitiveValue()
        {
            string a = this.ParseName();
            if (string.Equals(a, "false"))
            {
                return false;
            }
            if (!string.Equals(a, "true"))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedToken(a));
            }
            return true;
        }

        private string ParseName()
        {
            int num;
            switch (this.characterBuffer[this.tokenStartIndex])
            {
                case '"':
                case '\'':
                    return this.ParseStringPrimitiveValue();

                default:
                    num = 0;
                    do
                    {
                        char c = this.characterBuffer[this.tokenStartIndex + num];
                        if (((c != '_') && !char.IsLetterOrDigit(c)) && (c != '$'))
                        {
                            break;
                        }
                        num++;
                    }
                    while (((this.tokenStartIndex + num) < this.storedCharacterCount) || this.ReadInput());
                    break;
            }
            return this.ConsumeTokenToString(num);
        }

        private object ParseNullPrimitiveValue()
        {
            string a = this.ParseName();
            if (!string.Equals(a, "null"))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedToken(a));
            }
            return null;
        }

        private object ParseNumberPrimitiveValue()
        {
            double num2;
            int num3;
            int characterCount = 1;
            while (((this.tokenStartIndex + characterCount) < this.storedCharacterCount) || this.ReadInput())
            {
                char c = this.characterBuffer[this.tokenStartIndex + characterCount];
                if (((!char.IsDigit(c) && (c != '.')) && ((c != 'E') && (c != 'e'))) && ((c != '-') && (c != '+')))
                {
                    break;
                }
                characterCount++;
            }
            string s = this.ConsumeTokenToString(characterCount);
            if (int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num3))
            {
                return num3;
            }
            if (!double.TryParse(s, NumberStyles.Float, (IFormatProvider) NumberFormatInfo.InvariantInfo, out num2))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_InvalidNumberFormat(s));
            }
            return num2;
        }

        private JsonNodeType ParseProperty()
        {
            Scope local1 = this.scopes.Peek();
            local1.ValueCount++;
            this.PushScope(ScopeType.Property);
            this.nodeValue = this.ParseName();
            if (string.IsNullOrEmpty((string) this.nodeValue))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_InvalidPropertyNameOrUnexpectedComma((string) this.nodeValue));
            }
            if (!this.SkipWhitespaces() || (this.characterBuffer[this.tokenStartIndex] != ':'))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingColon((string) this.nodeValue));
            }
            this.tokenStartIndex++;
            return JsonNodeType.Property;
        }

        private string ParseStringPrimitiveValue()
        {
            bool flag;
            return this.ParseStringPrimitiveValue(out flag);
        }

        private string ParseStringPrimitiveValue(out bool hasLeadingBackslash)
        {
            hasLeadingBackslash = false;
            char ch = this.characterBuffer[this.tokenStartIndex];
            this.tokenStartIndex++;
            StringBuilder stringValueBuilder = null;
            int characterCount = 0;
            while (((this.tokenStartIndex + characterCount) < this.storedCharacterCount) || this.ReadInput())
            {
                char ch2 = this.characterBuffer[this.tokenStartIndex + characterCount];
                if (ch2 == '\\')
                {
                    int num2;
                    if ((characterCount == 0) && (stringValueBuilder == null))
                    {
                        hasLeadingBackslash = true;
                    }
                    if (stringValueBuilder == null)
                    {
                        if (this.stringValueBuilder == null)
                        {
                            this.stringValueBuilder = new StringBuilder();
                        }
                        else
                        {
                            this.stringValueBuilder.Length = 0;
                        }
                        stringValueBuilder = this.stringValueBuilder;
                    }
                    stringValueBuilder.Append(this.ConsumeTokenToString(characterCount));
                    characterCount = 0;
                    if (!this.EnsureAvailableCharacters(2))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence(@"\"));
                    }
                    ch2 = this.characterBuffer[this.tokenStartIndex + 1];
                    this.tokenStartIndex += 2;
                    switch (ch2)
                    {
                        case '/':
                        case '\\':
                        case '"':
                        case '\'':
                        {
                            stringValueBuilder.Append(ch2);
                            continue;
                        }
                        case 'b':
                        {
                            stringValueBuilder.Append('\b');
                            continue;
                        }
                        case 'f':
                        {
                            stringValueBuilder.Append('\f');
                            continue;
                        }
                        case 'r':
                        {
                            stringValueBuilder.Append('\r');
                            continue;
                        }
                        case 't':
                        {
                            stringValueBuilder.Append('\t');
                            continue;
                        }
                        case 'u':
                            if (!this.EnsureAvailableCharacters(4))
                            {
                                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence(@"\uXXXX"));
                            }
                            break;

                        case 'n':
                        {
                            stringValueBuilder.Append('\n');
                            continue;
                        }
                        default:
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence(@"\" + ch2));
                    }
                    string s = this.ConsumeTokenToString(4);
                    if (!int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence(@"\u" + s));
                    }
                    stringValueBuilder.Append((char) num2);
                    continue;
                }
                if (ch2 == ch)
                {
                    string str2 = this.ConsumeTokenToString(characterCount);
                    this.tokenStartIndex++;
                    if (stringValueBuilder != null)
                    {
                        stringValueBuilder.Append(str2);
                        str2 = stringValueBuilder.ToString();
                    }
                    return str2;
                }
                characterCount++;
            }
            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedEndOfString);
        }

        private JsonNodeType ParseValue()
        {
            Scope local1 = this.scopes.Peek();
            local1.ValueCount++;
            char c = this.characterBuffer[this.tokenStartIndex];
            switch (c)
            {
                case '"':
                case '\'':
                    bool flag;
                    this.nodeValue = this.ParseStringPrimitiveValue(out flag);
                    if (flag)
                    {
                        object obj2 = TryParseDateTimePrimitiveValue((string) this.nodeValue);
                        if (obj2 != null)
                        {
                            this.nodeValue = obj2;
                        }
                    }
                    break;

                case '[':
                    this.PushScope(ScopeType.Array);
                    this.tokenStartIndex++;
                    return JsonNodeType.StartArray;

                case 't':
                case 'f':
                    this.nodeValue = this.ParseBooleanPrimitiveValue();
                    break;

                case '{':
                    this.PushScope(ScopeType.Object);
                    this.tokenStartIndex++;
                    return JsonNodeType.StartObject;

                case 'n':
                    this.nodeValue = this.ParseNullPrimitiveValue();
                    break;

                default:
                    if ((!char.IsDigit(c) && (c != '-')) && (c != '.'))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedToken);
                    }
                    this.nodeValue = this.ParseNumberPrimitiveValue();
                    break;
            }
            this.TryPopPropertyScope();
            return JsonNodeType.PrimitiveValue;
        }

        private void PopScope()
        {
            this.scopes.Pop();
            this.TryPopPropertyScope();
        }

        private void PushScope(ScopeType newScopeType)
        {
            this.scopes.Push(new Scope(newScopeType));
        }

        public virtual bool Read()
        {
            this.nodeValue = null;
            if (!this.SkipWhitespaces())
            {
                return this.EndOfInput();
            }
            Scope scope = this.scopes.Peek();
            bool flag = false;
            if (this.characterBuffer[this.tokenStartIndex] == ',')
            {
                flag = true;
                this.tokenStartIndex++;
                if (!this.SkipWhitespaces())
                {
                    return this.EndOfInput();
                }
            }
            switch (scope.Type)
            {
                case ScopeType.Root:
                    if (flag)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Root));
                    }
                    if (scope.ValueCount > 0)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_MultipleTopLevelValues);
                    }
                    this.nodeType = this.ParseValue();
                    break;

                case ScopeType.Array:
                    if (flag && (scope.ValueCount == 0))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Array));
                    }
                    if (this.characterBuffer[this.tokenStartIndex] == ']')
                    {
                        this.tokenStartIndex++;
                        if (flag)
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Array));
                        }
                        this.PopScope();
                        this.nodeType = JsonNodeType.EndArray;
                    }
                    else
                    {
                        if (!flag && (scope.ValueCount > 0))
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingComma(ScopeType.Array));
                        }
                        this.nodeType = this.ParseValue();
                    }
                    break;

                case ScopeType.Object:
                    if (flag && (scope.ValueCount == 0))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Object));
                    }
                    if (this.characterBuffer[this.tokenStartIndex] == '}')
                    {
                        this.tokenStartIndex++;
                        if (flag)
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Object));
                        }
                        this.PopScope();
                        this.nodeType = JsonNodeType.EndObject;
                    }
                    else
                    {
                        if (!flag && (scope.ValueCount > 0))
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingComma(ScopeType.Object));
                        }
                        this.nodeType = this.ParseProperty();
                    }
                    break;

                case ScopeType.Property:
                    if (flag)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Property));
                    }
                    this.nodeType = this.ParseValue();
                    break;

                default:
                    throw JsonReaderExtensions.CreateException(Strings.General_InternalError(InternalErrorCodes.JsonReader_Read));
            }
            return true;
        }

        private bool ReadInput()
        {
            if (this.endOfInputReached)
            {
                return false;
            }
            if (this.storedCharacterCount == this.characterBuffer.Length)
            {
                if (this.tokenStartIndex == this.storedCharacterCount)
                {
                    this.tokenStartIndex = 0;
                    this.storedCharacterCount = 0;
                }
                else if (this.tokenStartIndex > (this.characterBuffer.Length - 0x40))
                {
                    Array.Copy(this.characterBuffer, this.tokenStartIndex, this.characterBuffer, 0, this.storedCharacterCount - this.tokenStartIndex);
                    this.storedCharacterCount -= this.tokenStartIndex;
                    this.tokenStartIndex = 0;
                }
                else
                {
                    int num = this.characterBuffer.Length * 2;
                    char[] destinationArray = new char[num];
                    Array.Copy(this.characterBuffer, 0, destinationArray, 0, this.characterBuffer.Length);
                    this.characterBuffer = destinationArray;
                }
            }
            int num2 = this.reader.Read(this.characterBuffer, this.storedCharacterCount, this.characterBuffer.Length - this.storedCharacterCount);
            if (num2 == 0)
            {
                this.endOfInputReached = true;
                return false;
            }
            this.storedCharacterCount += num2;
            return true;
        }

        private bool SkipWhitespaces()
        {
            do
            {
                while (this.tokenStartIndex < this.storedCharacterCount)
                {
                    if (!IsWhitespaceCharacter(this.characterBuffer[this.tokenStartIndex]))
                    {
                        return true;
                    }
                    this.tokenStartIndex++;
                }
            }
            while (this.ReadInput());
            return false;
        }

        private static object TryParseDateTimePrimitiveValue(string stringValue)
        {
            if (stringValue.StartsWith("/Date(", StringComparison.Ordinal) && stringValue.EndsWith(")/", StringComparison.Ordinal))
            {
                string s = stringValue.Substring("/Date(".Length, stringValue.Length - ("/Date(".Length + ")/".Length));
                int startIndex = s.IndexOfAny(new char[] { '+', '-' }, 1);
                if (startIndex == -1)
                {
                    long num2;
                    if (long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num2))
                    {
                        return new DateTime(JsonValueUtils.JsonTicksToDateTimeTicks(num2), DateTimeKind.Utc);
                    }
                }
                else
                {
                    long num3;
                    int num4;
                    string str2 = s.Substring(startIndex);
                    if (long.TryParse(s.Substring(0, startIndex), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num3) && int.TryParse(str2, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num4))
                    {
                        return new DateTimeOffset(JsonValueUtils.JsonTicksToDateTimeTicks(num3), new TimeSpan(0, num4, 0));
                    }
                }
            }
            return null;
        }

        private void TryPopPropertyScope()
        {
            if (this.scopes.Peek().Type == ScopeType.Property)
            {
                this.scopes.Pop();
            }
        }

        public virtual JsonNodeType NodeType
        {
            get
            {
                return this.nodeType;
            }
        }

        public virtual object Value
        {
            get
            {
                return this.nodeValue;
            }
        }

        private sealed class Scope
        {
            private readonly JsonReader.ScopeType type;

            public Scope(JsonReader.ScopeType type)
            {
                this.type = type;
            }

            public JsonReader.ScopeType Type
            {
                get
                {
                    return this.type;
                }
            }

            public int ValueCount { get; set; }
        }

        private enum ScopeType
        {
            Root,
            Array,
            Object,
            Property
        }
    }
}

