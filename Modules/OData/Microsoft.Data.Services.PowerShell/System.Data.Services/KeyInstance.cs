namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Runtime.InteropServices;

    internal class KeyInstance
    {
        private static readonly KeyInstance Empty = new KeyInstance();
        private readonly Dictionary<string, object> namedValues;
        private readonly List<object> positionalValues;

        private KeyInstance()
        {
        }

        private KeyInstance(Dictionary<string, object> namedValues, List<object> positionalValues)
        {
            this.namedValues = namedValues;
            this.positionalValues = positionalValues;
        }

        internal bool TryConvertValues(ResourceType type)
        {
            if (this.namedValues != null)
            {
                for (int i = 0; i < type.KeyProperties.Count; i++)
                {
                    object obj2;
                    object obj3;
                    ResourceProperty property = type.KeyProperties[i];
                    if (!this.namedValues.TryGetValue(property.Name, out obj2))
                    {
                        return false;
                    }
                    string text = (string) obj2;
                    if (!WebConvert.TryKeyStringToPrimitive(text, property.Type, out obj3))
                    {
                        return false;
                    }
                    this.namedValues[property.Name] = obj3;
                }
            }
            else
            {
                for (int j = 0; j < type.KeyProperties.Count; j++)
                {
                    object obj4;
                    string str2 = (string) this.positionalValues[j];
                    if (!WebConvert.TryKeyStringToPrimitive(str2, type.KeyProperties[j].Type, out obj4))
                    {
                        return false;
                    }
                    this.positionalValues[j] = obj4;
                }
            }
            return true;
        }

        private static bool TryParseFromUri(string text, bool allowNamedValues, bool allowNull, out KeyInstance instance)
        {
            Dictionary<string, object> dictionary = null;
            List<object> list = null;
            ExpressionLexer lexer = new ExpressionLexer(text);
            Token currentToken = lexer.CurrentToken;
            if (currentToken.Id == TokenId.End)
            {
                instance = Empty;
                return true;
            }
            instance = null;
            do
            {
                if ((currentToken.Id == TokenId.Identifier) && allowNamedValues)
                {
                    if (list != null)
                    {
                        return false;
                    }
                    string identifier = lexer.CurrentToken.GetIdentifier();
                    lexer.NextToken();
                    if (lexer.CurrentToken.Id != TokenId.Equal)
                    {
                        return false;
                    }
                    lexer.NextToken();
                    if (!lexer.CurrentToken.IsKeyValueToken)
                    {
                        return false;
                    }
                    string str2 = lexer.CurrentToken.Text;
                    WebUtil.CreateIfNull<Dictionary<string, object>>(ref dictionary);
                    if (dictionary.ContainsKey(identifier))
                    {
                        return false;
                    }
                    dictionary.Add(identifier, str2);
                }
                else
                {
                    if (!currentToken.IsKeyValueToken && (!allowNull || (currentToken.Id != TokenId.NullLiteral)))
                    {
                        return false;
                    }
                    if (dictionary != null)
                    {
                        return false;
                    }
                    WebUtil.CreateIfNull<List<object>>(ref list);
                    list.Add(lexer.CurrentToken.Text);
                }
                lexer.NextToken();
                currentToken = lexer.CurrentToken;
                if (currentToken.Id == TokenId.Comma)
                {
                    lexer.NextToken();
                    currentToken = lexer.CurrentToken;
                    if (currentToken.Id == TokenId.End)
                    {
                        return false;
                    }
                }
            }
            while (currentToken.Id != TokenId.End);
            instance = new KeyInstance(dictionary, list);
            return true;
        }

        internal static bool TryParseKeysFromUri(string text, out KeyInstance instance)
        {
            return TryParseFromUri(text, true, false, out instance);
        }

        internal static bool TryParseNullableTokens(string text, out KeyInstance instance)
        {
            return TryParseFromUri(text, false, true, out instance);
        }

        internal bool AreValuesNamed
        {
            get
            {
                return (this.namedValues != null);
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (this == Empty);
            }
        }

        internal IDictionary<string, object> NamedValues
        {
            get
            {
                return this.namedValues;
            }
        }

        internal IList<object> PositionalValues
        {
            get
            {
                return this.positionalValues;
            }
        }

        internal int ValueCount
        {
            get
            {
                if (this == Empty)
                {
                    return 0;
                }
                if (this.namedValues != null)
                {
                    return this.namedValues.Count;
                }
                return this.positionalValues.Count;
            }
        }
    }
}

