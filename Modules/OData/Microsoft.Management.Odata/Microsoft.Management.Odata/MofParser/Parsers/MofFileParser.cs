using Microsoft.Management.Odata;
using Microsoft.Management.Odata.MofParser;
using Microsoft.Management.Odata.MofParser.ParseTree;
using System;
using System.Collections.Generic;
using System.IO;
using Tokenizer;

namespace Microsoft.Management.Odata.MofParser.Parsers
{
	internal sealed class MofFileParser
	{
		private readonly TokenFilter m_filteredTokens;

		private readonly string m_filePath;

		private readonly Queue<Token> m_queue;

		private MofFileParser(string mofFilePath)
		{
			this.m_filePath = mofFilePath;
			string str = File.ReadAllText(mofFilePath);
			this.m_filteredTokens = new TokenFilter(new Tokenizer.Tokenizer(str, mofFilePath), new Predicate<Token>(MofFileParser.IgnoreWhitespaceFilter));
			this.m_queue = new Queue<Token>(this.m_filteredTokens);
		}

		private MofFileParser(IEnumerable<char> inputStream, string documentPath)
		{
			this.m_filePath = documentPath;
			this.m_filteredTokens = new TokenFilter(new Tokenizer.Tokenizer(inputStream, documentPath), new Predicate<Token>(MofFileParser.IgnoreWhitespaceFilter));
			this.m_queue = new Queue<Token>(this.m_filteredTokens);
		}

		private void Consume<T>()
		where T : Token
		{
			Token token = this.m_queue.Dequeue();
			if (token as T != null)
			{
				return;
			}
			else
			{
				throw new ParseFailureException(string.Format("Expected a token of type {0} but got a {1}.", typeof(T).ToString(), token.Type.ToString()), token.Location);
			}
		}

		private Token Consume(TokenType tokenType)
		{
			object str;
			DocumentRange location;
			Token token = this.m_queue.Dequeue();
			if (token == null || token.Type != tokenType)
			{
				string str1 = "Expected a token of type {0} but got a {1}.";
				string str2 = tokenType.ToString();
				if (token != null)
				{
					str = token.Type.ToString();
				}
				else
				{
					str = "null";
				}
				string str3 = string.Format(str1, str2, str);
				if (token != null)
				{
					location = token.Location;
				}
				else
				{
					DocumentRange documentRange = new DocumentRange();
					location = documentRange;
				}
				throw new ParseFailureException(str3, location);
			}
			else
			{
				return token;
			}
		}

		private TokenKeyword Consume(KeywordType keywordType)
		{
			TokenKeyword keyword = this.GetKeyword();
			if (keyword.KeywordType == keywordType)
			{
				return keyword;
			}
			else
			{
				throw new ParseFailureException(string.Format("Expected keyword {0} but got {1}.", keywordType.ToString(), keyword.KeywordType), keyword.Location);
			}
		}

		private TokenIdentifier GetIdentifier()
		{
			return this.GetToken<TokenIdentifier>();
		}

		private TokenKeyword GetKeyword()
		{
			return this.GetToken<TokenKeyword>();
		}

		private T GetToken<T>()
		where T : Token
		{
			Token token = this.m_queue.Dequeue();
			T t = (T)(token as T);
			if (t != null)
			{
				return t;
			}
			else
			{
				throw new ParseFailureException(string.Format("Expected a {0} but got a {1}.", typeof(T).ToString(), token.Type.ToString()), token.Location);
			}
		}

		private static bool IgnoreWhitespaceFilter(Token token)
		{
			return token.Type != TokenType.Whitespace;
		}

		private DataType ParseBuiltInType()
		{
			DataType flag;
			TokenKeyword keyword = this.GetKeyword();
			KeywordType keywordType = keyword.KeywordType;
			if (keywordType == KeywordType.DT_BOOL)
			{
				flag = MofDataType.Bool;
			}
			else if (keywordType == KeywordType.DT_CHAR16)
			{
				flag = MofDataType.Char16;
			}
			else if (keywordType == KeywordType.DT_DATETIME)
			{
				flag = MofDataType.DateTime;
			}
			else if (keywordType == KeywordType.DT_REAL32)
			{
				flag = MofDataType.Real32;
			}
			else if (keywordType == KeywordType.DT_REAL64)
			{
				flag = MofDataType.Real64;
			}
			else if (keywordType == KeywordType.DT_SINT16)
			{
				flag = MofDataType.SInt16;
			}
			else if (keywordType == KeywordType.DT_SINT32)
			{
				flag = MofDataType.SInt32;
			}
			else if (keywordType == KeywordType.DT_SINT64)
			{
				flag = MofDataType.SInt64;
			}
			else if (keywordType == KeywordType.DT_SINT8)
			{
				flag = MofDataType.SInt8;
			}
			else if (keywordType == KeywordType.DT_STR)
			{
				flag = MofDataType.String;
			}
			else if (keywordType == KeywordType.DT_UINT16)
			{
				flag = MofDataType.UInt16;
			}
			else if (keywordType == KeywordType.DT_UINT32)
			{
				flag = MofDataType.UInt32;
			}
			else if (keywordType == KeywordType.DT_UINT64)
			{
				flag = MofDataType.UInt64;
			}
			else if (keywordType == KeywordType.DT_UINT8)
			{
				flag = MofDataType.UInt8;
			}
			else
			{
				throw new ParseFailureException(string.Format("Expected a built-in data type, but got: {0}", (object)keyword.KeywordType.ToString()), keyword.Location);
			}
			return flag;
		}

		private DataType ParseBuiltInTypeOrObjectReference()
		{
			TokenIdentifier tokenIdentifier = this.PeekAs<TokenIdentifier>();
			if (tokenIdentifier == null || tokenIdentifier.IsKeyword)
			{
				return this.ParseBuiltInType();
			}
			else
			{
				ClassName className = this.ParseClassName();
				this.Consume(KeywordType.REF);
				return new ObjectReference(className);
			}
		}

		private ClassDeclaration ParseClass(QualifierList qualifiers)
		{
			DocumentRange location = this.Consume(KeywordType.CLASS).Location;
			ClassName className = this.ParseClassName();
			AliasIdentifier aliasIdentifier = null;
			if (!this.PeekKeyword(KeywordType.AS))
			{
				ClassName className1 = null;
				if (this.TryConsume(TokenType.Colon))
				{
					className1 = this.ParseClassName();
				}
				this.Consume(TokenType.OpenBrace);
				ClassFeatureList classFeatureList = this.ParseClassFeatureList();
				this.Consume(TokenType.Semicolon);
				return new ClassDeclaration(location, className, aliasIdentifier, className1, qualifiers, classFeatureList);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private ClassFeatureList ParseClassFeatureList()
		{
			List<ClassFeature> classFeatures = new List<ClassFeature>();
			while (!this.TryConsume(TokenType.CloseBrace))
			{
				QualifierList qualifierList = this.ParseQualifierList();
				DocumentRange location = this.m_queue.Peek().Location;
				DataType arrayType = this.ParseBuiltInTypeOrObjectReference();
				string value = this.GetIdentifier().Value;
				if (!this.TryConsume(TokenType.OpenParens))
				{
					if (this.TryConsume(TokenType.OpenBracket))
					{
						if (this.PeekToken(TokenType.CloseBracket))
						{
							this.Consume(TokenType.CloseBracket);
							int? nullable = null;
							arrayType = new ArrayType(arrayType, nullable);
						}
						else
						{
							throw new NotSupportedException();
						}
					}
					object obj = null;
					if (!this.TryConsume(TokenType.Equals))
					{
						this.Consume(TokenType.Semicolon);
						classFeatures.Add(new PropertyDeclaration(location, value, arrayType, obj, qualifierList));
					}
					else
					{
						throw new ParseFailureException(Resources.MofInitializerNotSupported, location);
					}
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			return new ClassFeatureList(classFeatures.ToArray());
		}

		private ClassName ParseClassName()
		{
			string value = this.GetIdentifier().Value;
			char[] chrArray = new char[1];
			chrArray[0] = '\u005F';
			string[] strArrays = value.Split(chrArray, 2);
			if ((int)strArrays.Length == 1)
			{
				return new ClassName(null, value);
			}
			else
			{
				return new ClassName(strArrays[0], strArrays[1]);
			}
		}

		private CompilerDirective ParseCompilerDirective()
		{
			DocumentRange location = this.Consume(TokenType.Pragma).Location;
			TokenIdentifier identifier = this.GetIdentifier();
			string str = this.ParseSingleStringParameter();
			string valueUpperCase = identifier.ValueUpperCase;
			string str1 = valueUpperCase;
			if (valueUpperCase != null)
			{
				if (str1 == "INCLUDE")
				{
					return new PragmaInclude(location, str);
				}
				else
				{
					if (str1 == "INSTANCELOCALE")
					{
						return new PragmaInstanceLocale(location, str);
					}
					else
					{
						if (str1 == "LOCALE")
						{
							return new PragmaLocale(location, str);
						}
						else
						{
							if (str1 == "NAMESPACE")
							{
								return new PragmaNamespace(location, str);
							}
						}
					}
				}
			}
			throw new ParseFailureException(string.Format("Unexpected #pragma type: {0}", identifier.Value), identifier.Location);
		}

		private object ParseConstantValue()
		{
			Token token = this.m_queue.Dequeue();
			TokenType type = token.Type;
			switch (type)
			{
				case TokenType.StringValue:
				{
					string stringValue = ((TokenStringValue)token).StringValue;
					return stringValue;
				}
				case TokenType.Character:
				{
					throw new NotSupportedException();
				}
				case TokenType.Integer:
				{
					return (int)((TokenInteger)token).Value;
				}
				case TokenType.Real:
				{
					throw new NotSupportedException();
				}
			}
			throw new ParseFailureException(string.Format("Unexpected token while parsing a constant value: {0}", token.Type), token.Location);
		}

		private Flavor ParseFlavors()
		{
			throw new NotImplementedException();
		}

		public static MofSpecification ParseMofFile(string mofFilePath)
		{
			MofFileParser mofFileParser = new MofFileParser(mofFilePath);
			return mofFileParser.ParseMofSpecification();
		}

		public static MofSpecification ParseMofFile(IEnumerable<char> inputStream, string documentPath)
		{
			MofFileParser mofFileParser = new MofFileParser(inputStream, documentPath);
			return mofFileParser.ParseMofSpecification();
		}

		private MofSpecification ParseMofSpecification()
		{
			Token token;
			DocumentRange documentRange;
			List<MofProduction> mofProductions = new List<MofProduction>();
			if (this.m_queue.Count > 0)
			{
				if (this.m_queue.Dequeue().Type == TokenType.StartOfInput)
				{
					QualifierList qualifierList = null;
					while (true)
					{
						if (this.m_queue.Count <= 0)
						{
							documentRange = new DocumentRange();
							throw new ParseFailureException("No EndOfInput token found at the end of the input", documentRange);
						}
						token = this.m_queue.Peek();
						TokenType type = token.Type;
						if (type == TokenType.EndOfInput)
						{
							if (qualifierList == null)
							{
								return new MofSpecification(mofProductions.ToArray());
							}
							else
							{
								throw new ParseFailureException("Found qualifiers that are not applied to any production.", token.Location);
							}
						}
						else
						{
							if (type == TokenType.Identifier)
							{
								if (qualifierList == null)
								{
									qualifierList = new QualifierList(new Qualifier[0]);
								}
								TokenIdentifier tokenIdentifier = (TokenIdentifier)token;
								if (!tokenIdentifier.IsKeyword)
								{
									throw new ParseFailureException(string.Format("Unexpected identifier: {0}", tokenIdentifier), tokenIdentifier.Location);
								}
								else
								{
									TokenKeyword tokenKeyword = (TokenKeyword)token;
									KeywordType keywordType = tokenKeyword.KeywordType;
									if (keywordType == KeywordType.CLASS)
									{
										mofProductions.Add(this.ParseClass(qualifierList));
										qualifierList = null;
										continue;
									}
									else
									{
										if (keywordType != KeywordType.INSTANCE)
										{
										}
										throw new ParseFailureException(string.Format("Unexpected keyword: {0}", tokenKeyword), tokenKeyword.Location);
									}
								}
							}
							else if (type == TokenType.Alias)
							{
								break;
							}
							else if (type == TokenType.Pragma)
							{
								if (qualifierList == null)
								{
									mofProductions.Add(this.ParseCompilerDirective());
									continue;
								}
								else
								{
									throw new ParseFailureException("Qualifiers are not legal on pragmas.", token.Location);
								}
							}
							if (type != TokenType.OpenBracket)
							{
								break;
							}
							qualifierList = this.ParseQualifierList();
						}
					}
					throw new ParseFailureException("Unexpected token", token.Location);
				}
				else
				{
					DocumentCoordinate documentCoordinate = new DocumentCoordinate();
					DocumentCoordinate documentCoordinate1 = new DocumentCoordinate();
					throw new ParseFailureException("Expected a StartOfInput token.", new DocumentRange(this.m_filePath, documentCoordinate, documentCoordinate1));
				}
			}
			documentRange = new DocumentRange();
			throw new ParseFailureException("No EndOfInput token found at the end of the input", documentRange);
		}

		private Qualifier ParseQualifier()
		{
			TokenIdentifier identifier = this.GetIdentifier();
			Token token = this.m_queue.Peek();
			TokenType type = token.Type;
			if (type != TokenType.Comma)
			{
				if (type == TokenType.OpenParens)
				{
					this.m_queue.Dequeue();
					object obj = this.ParseConstantValue();
					this.Consume(TokenType.CloseParens);
					Flavor flavor = Flavor.None;
					if (this.PeekAs<TokenColon>() != null)
					{
						this.Consume<TokenColon>();
						flavor = this.ParseFlavors();
					}
					return new Qualifier(identifier.Location, identifier.Value, obj, flavor);
				}
				else
				{
					if (type != TokenType.CloseBracket)
					{
						throw new ParseFailureException(string.Format("Unexpected token while parsing a qualifer: {0}", token.Type.ToString()), token.Location);
					}
				}
			}
			return new Qualifier(identifier.Location, identifier.Value, null, Flavor.None);
		}

		private QualifierList ParseQualifierList()
		{
			Token token;
			if (this.PeekToken(TokenType.OpenBracket))
			{
				this.m_queue.Dequeue();
				List<Qualifier> qualifiers = new List<Qualifier>();
				do
				{
					qualifiers.Add(this.ParseQualifier());
					token = this.m_queue.Dequeue();
					if (token.Type != TokenType.CloseBracket)
					{
						continue;
					}
					return new QualifierList(qualifiers.ToArray());
				}
				while (token.Type == TokenType.Comma);
				throw new ParseFailureException(string.Format("Unexpected qualifier separator token in a qualifer list: {0}", token.Type.ToString()), token.Location);
			}
			else
			{
				return new QualifierList(new Qualifier[0]);
			}
		}

		private string ParseSingleStringParameter()
		{
			this.Consume<TokenOpenParens>();
			string stringValue = this.GetToken<TokenStringValue>().StringValue;
			this.Consume<TokenCloseParens>();
			return stringValue;
		}

		private T PeekAs<T>()
		where T : Token
		{
			return (T)(this.m_queue.Peek() as T);
		}

		private bool PeekKeyword(KeywordType keywordType)
		{
			TokenKeyword tokenKeyword = this.PeekAs<TokenKeyword>();
			if (tokenKeyword == null)
			{
				return false;
			}
			else
			{
				return tokenKeyword.KeywordType == keywordType;
			}
		}

		private bool PeekToken(TokenType tokenType)
		{
			return this.m_queue.Peek().Type == tokenType;
		}

		private bool TryConsume(KeywordType keywordType)
		{
			TokenKeyword tokenKeyword = this.PeekAs<TokenKeyword>();
			if (tokenKeyword == null || tokenKeyword.KeywordType != keywordType)
			{
				return false;
			}
			else
			{
				this.m_queue.Dequeue();
				return true;
			}
		}

		private bool TryConsume(TokenType tokenType)
		{
			Token token = this.m_queue.Peek();
			if (token == null || token.Type != tokenType)
			{
				return false;
			}
			else
			{
				this.m_queue.Dequeue();
				return true;
			}
		}
	}
}