using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tokenizer
{
	internal sealed class Tokenizer : IEnumerable<Token>, IEnumerable
	{
		private readonly IEnumerable<CharacterAndCoordinate> m_inputStream;

		private readonly string m_documentPath;

		internal string DocumentPath
		{
			get
			{
				return this.m_documentPath;
			}
		}

		public Tokenizer(IEnumerable<char> inputStream, string documentPath)
		{
			this.m_inputStream = new Coordinatizer(inputStream, documentPath);
			this.m_documentPath = documentPath;
		}

		public IEnumerator<Token> GetEnumerator()
		{
			return new Tokenizer.TokenEnumerator(this);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private sealed class TokenEnumerator : IEnumerator<Token>, IDisposable, IEnumerator
		{
			private readonly Tokenizer.TokenPartEnumerator m_wrappedEnumerator;

			private readonly List<Token> m_bufferedTokens;

			private readonly Tokenizer m_owner;

			private Token m_current;

			public Token Current
			{
				get
				{
					return this.m_current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			internal TokenEnumerator(Tokenizer owner)
			{
				this.m_bufferedTokens = new List<Token>();
				this.m_wrappedEnumerator = new Tokenizer.TokenPartEnumerator(owner);
				this.m_owner = owner;
			}

			public void Dispose()
			{
				this.m_wrappedEnumerator.Dispose();
			}

			public bool MoveNext()
			{
				if (this.m_bufferedTokens.Count <= 0)
				{
					bool flag = this.m_wrappedEnumerator.MoveNext();
					if (flag)
					{
						Token current = this.m_wrappedEnumerator.Current;
						if (current.Type != TokenType.StringPart)
						{
							this.m_current = current;
							return true;
						}
						else
						{
							int count = 0;
							this.m_bufferedTokens.Add(current);
							while (this.m_wrappedEnumerator.MoveNext())
							{
								current = this.m_wrappedEnumerator.Current;
								TokenType type = current.Type;
								if (type == TokenType.Whitespace)
								{
									this.m_bufferedTokens.Add(current);
								}
								else
								{
									if (type != TokenType.StringPart)
									{
										break;
									}
									count = this.m_bufferedTokens.Count;
									this.m_bufferedTokens.Add(current);
								}
							}
							Token[] array = this.m_bufferedTokens.Take<Token>(count + 1).ToArray<Token>();
							DocumentRange location = array[0].Location;
							DocumentRange documentRange = array[(int)array.Length - 1].Location;
							DocumentRange documentRange1 = new DocumentRange(this.m_owner.DocumentPath, location.Start, documentRange.End);
							this.m_current = new TokenStringValue(array, documentRange1);
							this.m_bufferedTokens.RemoveRange(0, count + 1);
							this.m_bufferedTokens.Add(current);
							return true;
						}
					}
					else
					{
						return flag;
					}
				}
				else
				{
					this.m_current = this.m_bufferedTokens[0];
					this.m_bufferedTokens.RemoveAt(0);
					return true;
				}
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}

		private sealed class TokenPartEnumerator : IEnumerator<Token>, IDisposable, IEnumerator
		{
			private TokenBuffer m_buffer;

			private Token m_current;

			private Tokenizer.TokenPartEnumerator.InputState m_inputState;

			public Token Current
			{
				get
				{
					return this.m_current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public TokenPartEnumerator(Tokenizer owner)
			{
				this.m_buffer = new TokenBuffer(owner.m_inputStream.GetEnumerator(), owner.m_documentPath);
			}

			public void Dispose()
			{
			}

			private static bool IsDelimiter(char ch)
			{
				char chr = ch;
				if (chr > ' ')
				{
					if (chr > ';')
					{
						if (chr == '[' || chr == ']')
						{
							return true;
						}
						else if (chr == '\\')
						{
							return false;
						}
						if (chr == '{' || chr == '}')
						{
							return true;
						}
						else if (chr == '|')
						{
							return false;
						}
					}
					else
					{
						if (chr == '(' || chr == ')' || chr == ',')
						{
							return true;
						}
						else if (chr == '*' || chr == '+')
						{
							return false;
						}
						if (chr == ':' || chr == ';')
						{
							return true;
						}
					}
				}
				else
				{
					if (chr != '\0')
					{
						if (chr == '\t' || chr == '\n' || chr == '\f' || chr == '\r')
						{
							return true;
						}
						else if (chr == '\v')
						{
							return false;
						}
						if (chr == ' ')
						{
							return true;
						}
					}
					else
					{
						return true;
					}
				}
				return false;
			}

			private static bool IsFirstIdentifierCharacter(char ch)
			{
				if (char.IsUpper(ch) || char.IsLower(ch))
				{
					return true;
				}
				else
				{
					return ch == '\u005F';
				}
			}

			private static bool IsWhitespace(char ch)
			{
				char chr = ch;
				switch (chr)
				{
					case '\t':
					case '\n':
					case '\f':
					case '\r':
					{
						return true;
					}
					case '\v':
					{
						return false;
					}
					default:
					{
						if (chr != ' ')
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
			}

			public bool MoveNext()
			{
				TokenAlias tokenAlia = null;
				TokenStringPart tokenStringPart = null;
				TokenIdentifier tokenIdentifier = null;
				Tokenizer.TokenPartEnumerator.InputState mInputState = this.m_inputState;
				switch (mInputState)
				{
					case Tokenizer.TokenPartEnumerator.InputState.NotStarted:
					{
						this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Started;
						this.m_current = new TokenStartOfInput(this.m_buffer.DocumentPath);
						return true;
					}
					case Tokenizer.TokenPartEnumerator.InputState.Started:
					{
						if (this.TryMatchWhitespace(out this.m_current))
						{
							break;
						}
						char item = this.m_buffer[0];
						if (item == '\0')
						{
							this.m_current = new TokenEndOfInput(this.m_buffer.GetRange(0));
							this.m_buffer = null;
							this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
							return true;
						}
						else
						{
							switch (item)
							{
								case '\"':
								{
									if (!this.TryMatchString(this.m_buffer, out tokenStringPart))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									this.m_current = tokenStringPart;
									return true;
								}
								case '#':
								{
									if (!this.TryMatchPragma(out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case '$':
								{
									if (!Tokenizer.TokenPartEnumerator.TryMatchAlias(this.m_buffer, out tokenAlia))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									this.m_current = tokenAlia;
									return true;
								}
								case '%':
								case '&':
								case '\'':
								case '*':
								case '.':
								case '<':
								case '>':
								case '?':
								case '@':
								case 'G':
								case 'H':
								case 'J':
								case 'K':
								case 'L':
								case 'V':
								case 'W':
								case 'X':
								case 'Y':
								case 'Z':
								case '\\':
								case '\u005E':
								case '\u005F':
								case '\u0060':
								case 'g':
								case 'h':
								case 'j':
								case 'k':
								case 'l':
								case 'v':
								case 'w':
								case 'x':
								case 'y':
								case 'z':
								case '|':
								{
									if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
									{
										break;
									}
									this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
									this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
									return true;
								}
								case '(':
								{
									this.m_current = new TokenOpenParens(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case ')':
								{
									this.m_current = new TokenCloseParens(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case '+':
								case '-':
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
								{
									if (!this.TryMatchNumber(out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case ',':
								{
									this.m_current = new TokenComma(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case '/':
								{
									if (this.m_buffer[1] != '/')
									{
										if (this.m_buffer[1] != '*')
										{
											if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
											{
												break;
											}
											this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
											this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
											return true;
										}
										int num = 2;
										while (true)
										{
											char chr = this.m_buffer[num];
											if (chr == 0)
											{
												this.m_current = new TokenComment(this.m_buffer.GetRange(num));
												this.m_buffer.Discard(num);
												return true;
											}
											if (chr == '*' && this.m_buffer[num + 1] == '/')
											{
												break;
											}
											num++;
										}
										this.m_current = new TokenComment(this.m_buffer.GetRange(num + 1));
										this.m_buffer.Discard(num + 2);
										return true;
									}
									else
									{
										int num1 = 2;
										while (true)
										{
											char item1 = this.m_buffer[num1];
											if (item1 == '\r' || item1 == '\n' || item1 == 0)
											{
												break;
											}
											num1++;
										}
										this.m_current = new TokenComment(this.m_buffer.GetRange(num1));
										this.m_buffer.Discard(num1);
										return true;
									}
								}
								case ':':
								{
									this.m_current = new TokenColon(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case ';':
								{
									this.m_current = new TokenSemicolon(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case '=':
								{
									this.m_current = new TokenEquals(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case 'A':
								case 'a':
								{
									if (!this.TryMatchKeyword("any", KeywordType.ANY, out this.m_current) && !this.TryMatchKeyword("as", KeywordType.AS, out this.m_current) && !this.TryMatchKeyword("association", KeywordType.ASSOCIATION, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'B':
								case 'b':
								{
									if (!this.TryMatchKeyword("boolean", KeywordType.DT_BOOL, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'C':
								case 'c':
								{
									if (!this.TryMatchKeyword("char16", KeywordType.DT_CHAR16, out this.m_current) && !this.TryMatchKeyword("class", KeywordType.CLASS, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'D':
								case 'd':
								{
									if (!this.TryMatchKeyword("datetime", KeywordType.DT_DATETIME, out this.m_current) && !this.TryMatchKeyword("disableoverride", KeywordType.DISABLEOVERRIDE, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'E':
								case 'e':
								{
									if (!this.TryMatchKeyword("enableoverride", KeywordType.ENABLEOVERRIDE, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'F':
								case 'f':
								{
									if (!this.TryMatchKeyword("false", KeywordType.FALSE, out this.m_current) && !this.TryMatchKeyword("flavor", KeywordType.FLAVOR, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'I':
								case 'i':
								{
									if (!this.TryMatchKeyword("indication", KeywordType.INDICATION, out this.m_current) && !this.TryMatchKeyword("instance", KeywordType.INSTANCE, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'M':
								case 'm':
								{
									if (!this.TryMatchKeyword("method", KeywordType.METHOD, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'N':
								case 'n':
								{
									if (!this.TryMatchKeyword("null", KeywordType.NULL, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'O':
								case 'o':
								{
									if (!this.TryMatchKeyword("null", KeywordType.OF, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'P':
								case 'p':
								{
									if (!this.TryMatchKeyword("parameter", KeywordType.PARAMETER, out this.m_current) && !this.TryMatchKeyword("property", KeywordType.PROPERTY, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'Q':
								case 'q':
								{
									if (!this.TryMatchKeyword("qualifier", KeywordType.QUALIFIER, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'R':
								case 'r':
								{
									if (!this.TryMatchKeyword("real32", KeywordType.DT_REAL32, out this.m_current) && !this.TryMatchKeyword("real64", KeywordType.DT_REAL64, out this.m_current) && !this.TryMatchKeyword("REF", KeywordType.REF, out this.m_current) && !this.TryMatchKeyword("REFERENCE", KeywordType.REFERENCE, out this.m_current) && !this.TryMatchKeyword("RESTRICTED", KeywordType.RESTRICTED, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'S':
								case 's':
								{
									if (!this.TryMatchKeyword("schema", KeywordType.SCHEMA, out this.m_current) && !this.TryMatchKeyword("string", KeywordType.DT_STR, out this.m_current) && !this.TryMatchKeyword("scope", KeywordType.SCOPE, out this.m_current) && !this.TryMatchKeyword("sint16", KeywordType.DT_SINT16, out this.m_current) && !this.TryMatchKeyword("sint32", KeywordType.DT_SINT32, out this.m_current) && !this.TryMatchKeyword("sint64", KeywordType.DT_SINT64, out this.m_current) && !this.TryMatchKeyword("sint8", KeywordType.DT_SINT8, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'T':
								case 't':
								{
									if (!this.TryMatchKeyword("tosubclass", KeywordType.TOSUBCLASS, out this.m_current) && !this.TryMatchKeyword("translatable", KeywordType.TRANSLATABLE, out this.m_current) && !this.TryMatchKeyword("true", KeywordType.TRUE, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case 'U':
								case 'u':
								{
									if (!this.TryMatchKeyword("uint16", KeywordType.DT_UINT16, out this.m_current) && !this.TryMatchKeyword("uint32", KeywordType.DT_UINT32, out this.m_current) && !this.TryMatchKeyword("uint64", KeywordType.DT_UINT64, out this.m_current) && !this.TryMatchKeyword("uint8", KeywordType.DT_UINT8, out this.m_current))
									{
										if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
										{
											break;
										}
										this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
										this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
										return true;
									}
									return true;
								}
								case '[':
								{
									this.m_current = new TokenOpenBracket(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case ']':
								{
									this.m_current = new TokenCloseBracket(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case '{':
								{
									this.m_current = new TokenOpenBrace(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								case '}':
								{
									this.m_current = new TokenCloseBrace(this.m_buffer.GetRange(1));
									this.m_buffer.Discard(1);
									return true;
								}
								default:
								{
									if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
									{
										break;
									}
									this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
									this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
									return true;
								}
							}
							this.m_current = tokenIdentifier;
							return true;
						}
					}
					case Tokenizer.TokenPartEnumerator.InputState.Finished:
					{
						return false;
					}
					default:
					{
					if (this.TryMatchWhitespace(out this.m_current))
					{
						break;
					}
					char item = this.m_buffer[0];
					if (item == '\0')
					{
						this.m_current = new TokenEndOfInput(this.m_buffer.GetRange(0));
						this.m_buffer = null;
						this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
						return true;
					}
					else
					{
						switch (item)
						{
						case '\"':
						{
							if (!this.TryMatchString(this.m_buffer, out tokenStringPart))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							this.m_current = tokenStringPart;
							return true;
						}
						case '#':
						{
							if (!this.TryMatchPragma(out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case '$':
						{
							if (!Tokenizer.TokenPartEnumerator.TryMatchAlias(this.m_buffer, out tokenAlia))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							this.m_current = tokenAlia;
							return true;
						}
						case '%':
						case '&':
						case '\'':
						case '*':
						case '.':
						case '<':
						case '>':
						case '?':
						case '@':
						case 'G':
						case 'H':
						case 'J':
						case 'K':
						case 'L':
						case 'V':
						case 'W':
						case 'X':
						case 'Y':
						case 'Z':
						case '\\':
						case '\u005E':
						case '\u005F':
						case '\u0060':
						case 'g':
						case 'h':
						case 'j':
						case 'k':
						case 'l':
						case 'v':
						case 'w':
						case 'x':
						case 'y':
						case 'z':
						case '|':
						{
							if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
							{
								break;
							}
							this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
							this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
							return true;
						}
						case '(':
						{
							this.m_current = new TokenOpenParens(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case ')':
						{
							this.m_current = new TokenCloseParens(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case '+':
						case '-':
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
						{
							if (!this.TryMatchNumber(out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case ',':
						{
							this.m_current = new TokenComma(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case '/':
						{
							if (this.m_buffer[1] != '/')
							{
								if (this.m_buffer[1] != '*')
								{
									if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
									{
										break;
									}
									this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
									this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
									return true;
								}
								int num = 2;
								while (true)
								{
									char chr = this.m_buffer[num];
									if (chr == 0)
									{
										this.m_current = new TokenComment(this.m_buffer.GetRange(num));
										this.m_buffer.Discard(num);
										return true;
									}
									if (chr == '*' && this.m_buffer[num + 1] == '/')
									{
										break;
									}
									num++;
								}
								this.m_current = new TokenComment(this.m_buffer.GetRange(num + 1));
								this.m_buffer.Discard(num + 2);
								return true;
							}
							else
							{
								int num1 = 2;
								while (true)
								{
									char item1 = this.m_buffer[num1];
									if (item1 == '\r' || item1 == '\n' || item1 == 0)
									{
										break;
									}
									num1++;
								}
								this.m_current = new TokenComment(this.m_buffer.GetRange(num1));
								this.m_buffer.Discard(num1);
								return true;
							}
						}
						case ':':
						{
							this.m_current = new TokenColon(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case ';':
						{
							this.m_current = new TokenSemicolon(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case '=':
						{
							this.m_current = new TokenEquals(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case 'A':
						case 'a':
						{
							if (!this.TryMatchKeyword("any", KeywordType.ANY, out this.m_current) && !this.TryMatchKeyword("as", KeywordType.AS, out this.m_current) && !this.TryMatchKeyword("association", KeywordType.ASSOCIATION, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'B':
						case 'b':
						{
							if (!this.TryMatchKeyword("boolean", KeywordType.DT_BOOL, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'C':
						case 'c':
						{
							if (!this.TryMatchKeyword("char16", KeywordType.DT_CHAR16, out this.m_current) && !this.TryMatchKeyword("class", KeywordType.CLASS, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'D':
						case 'd':
						{
							if (!this.TryMatchKeyword("datetime", KeywordType.DT_DATETIME, out this.m_current) && !this.TryMatchKeyword("disableoverride", KeywordType.DISABLEOVERRIDE, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'E':
						case 'e':
						{
							if (!this.TryMatchKeyword("enableoverride", KeywordType.ENABLEOVERRIDE, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'F':
						case 'f':
						{
							if (!this.TryMatchKeyword("false", KeywordType.FALSE, out this.m_current) && !this.TryMatchKeyword("flavor", KeywordType.FLAVOR, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'I':
						case 'i':
						{
							if (!this.TryMatchKeyword("indication", KeywordType.INDICATION, out this.m_current) && !this.TryMatchKeyword("instance", KeywordType.INSTANCE, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'M':
						case 'm':
						{
							if (!this.TryMatchKeyword("method", KeywordType.METHOD, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'N':
						case 'n':
						{
							if (!this.TryMatchKeyword("null", KeywordType.NULL, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'O':
						case 'o':
						{
							if (!this.TryMatchKeyword("null", KeywordType.OF, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'P':
						case 'p':
						{
							if (!this.TryMatchKeyword("parameter", KeywordType.PARAMETER, out this.m_current) && !this.TryMatchKeyword("property", KeywordType.PROPERTY, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'Q':
						case 'q':
						{
							if (!this.TryMatchKeyword("qualifier", KeywordType.QUALIFIER, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'R':
						case 'r':
						{
							if (!this.TryMatchKeyword("real32", KeywordType.DT_REAL32, out this.m_current) && !this.TryMatchKeyword("real64", KeywordType.DT_REAL64, out this.m_current) && !this.TryMatchKeyword("REF", KeywordType.REF, out this.m_current) && !this.TryMatchKeyword("REFERENCE", KeywordType.REFERENCE, out this.m_current) && !this.TryMatchKeyword("RESTRICTED", KeywordType.RESTRICTED, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'S':
						case 's':
						{
							if (!this.TryMatchKeyword("schema", KeywordType.SCHEMA, out this.m_current) && !this.TryMatchKeyword("string", KeywordType.DT_STR, out this.m_current) && !this.TryMatchKeyword("scope", KeywordType.SCOPE, out this.m_current) && !this.TryMatchKeyword("sint16", KeywordType.DT_SINT16, out this.m_current) && !this.TryMatchKeyword("sint32", KeywordType.DT_SINT32, out this.m_current) && !this.TryMatchKeyword("sint64", KeywordType.DT_SINT64, out this.m_current) && !this.TryMatchKeyword("sint8", KeywordType.DT_SINT8, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'T':
						case 't':
						{
							if (!this.TryMatchKeyword("tosubclass", KeywordType.TOSUBCLASS, out this.m_current) && !this.TryMatchKeyword("translatable", KeywordType.TRANSLATABLE, out this.m_current) && !this.TryMatchKeyword("true", KeywordType.TRUE, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case 'U':
						case 'u':
						{
							if (!this.TryMatchKeyword("uint16", KeywordType.DT_UINT16, out this.m_current) && !this.TryMatchKeyword("uint32", KeywordType.DT_UINT32, out this.m_current) && !this.TryMatchKeyword("uint64", KeywordType.DT_UINT64, out this.m_current) && !this.TryMatchKeyword("uint8", KeywordType.DT_UINT8, out this.m_current))
							{
								if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
								{
									break;
								}
								this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
								this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
								return true;
							}
							return true;
						}
						case '[':
						{
							this.m_current = new TokenOpenBracket(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case ']':
						{
							this.m_current = new TokenCloseBracket(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case '{':
						{
							this.m_current = new TokenOpenBrace(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						case '}':
						{
							this.m_current = new TokenCloseBrace(this.m_buffer.GetRange(1));
							this.m_buffer.Discard(1);
							return true;
						}
						default:
						{
							if (Tokenizer.TokenPartEnumerator.TryMatchIdentifier(this.m_buffer, out tokenIdentifier))
							{
								break;
							}
							this.m_current = new TokenFailure(this.m_buffer.GetRange(0));
							this.m_inputState = Tokenizer.TokenPartEnumerator.InputState.Finished;
							return true;
						}
						}
						this.m_current = tokenIdentifier;
						return true;
					}
					}
				}
				return true;
			}

			void System.Collections.IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			private static bool TryMatchAlias(TokenBuffer buffer, out TokenAlias alias)
			{
				if (buffer[0] != '$' || !Tokenizer.TokenPartEnumerator.IsFirstIdentifierCharacter(buffer[1]))
				{
					alias = null;
					return false;
				}
				else
				{
					int num = 2;
					while (true)
					{
						char item = buffer[num];
						if (!Tokenizer.TokenPartEnumerator.IsFirstIdentifierCharacter(item) && !char.IsDigit(item))
						{
							break;
						}
						num++;
					}
					alias = new TokenAlias(buffer.GetString(num), buffer.GetRange(num));
					buffer.Discard(num);
					return true;
				}
			}

			private bool TryMatchBinaryNumber(int digitsStartOffset, bool isNegative, out Token token)
			{
				throw new NotSupportedException();
			}

			private bool TryMatchDecimalNumber(int digitsStartOffset, bool isNegative, out Token token)
			{
				long num = (long)0;
				int num1 = digitsStartOffset;
				while (true)
				{
					char item = this.m_buffer[num1];
					char chr = item;
					if (chr == '0' || chr == '1' || chr == '2' || chr == '3' || chr == '4' || chr == '5' || chr == '6' || chr == '7' || chr == '8' || chr == '9')
					{
						num = num * (long)10 + (long)(item - 48);
						num1++;
					}
				}
				if (isNegative)
				{
					num = -num;
				}
				token = new TokenInteger(num, this.m_buffer.GetRange(num1));
				this.m_buffer.Discard(num1);
				return true;
			}

			private bool TryMatchHexNumber(int digitsStartOffset, bool isNegative, out Token token)
			{
				char item;
				char chr;
				long num = (long)0;
				int num1 = digitsStartOffset;
				while (true)
				{
					item = this.m_buffer[num1];
					chr = item;
					if (chr == '0' || chr == '1' || chr == '2' || chr == '3' || chr == '4' || chr == '5' || chr == '6' || chr == '7' || chr == '8' || chr == '9')
					{
						num = num * (long)16 + (long)(item - 48);
					}
					else if (chr == ':' || chr == ';' || chr == '<' || chr == '=' || chr == '>' || chr == '?' || chr == '@')
					{
						break;
					}
					else if (chr == 'A' || chr == 'B' || chr == 'C' || chr == 'D' || chr == 'E' || chr == 'F')
					{
						num = num * (long)16 + (long)(item - 65);
					}
					else
					{
						goto Label0;
					}
				Label1:
					num1++;
				}
				if (isNegative)
				{
					num = -num;
				}
				token = new TokenInteger(num, this.m_buffer.GetRange(num1));
				this.m_buffer.Discard(num1);
				return true;
			Label0:
				if (chr == 'a' || chr == 'b' || chr == 'c' || chr == 'd' || chr == 'e' || chr == 'f')
				{
					num = num * (long)16 + (long)(item - 97);
					num1++;
				}
				if (isNegative)
				{
					num = -num;
				}
				token = new TokenInteger(num, this.m_buffer.GetRange(num1));
				this.m_buffer.Discard(num1);
				return true;
			}

			private static bool TryMatchIdentifier(TokenBuffer buffer, out TokenIdentifier identifier)
			{
				if (!Tokenizer.TokenPartEnumerator.IsFirstIdentifierCharacter(buffer[0]))
				{
					identifier = null;
					return false;
				}
				else
				{
					int num = 1;
					while (true)
					{
						char item = buffer[num];
						if (!Tokenizer.TokenPartEnumerator.IsFirstIdentifierCharacter(item) && !char.IsDigit(item))
						{
							break;
						}
						num++;
					}
					identifier = new TokenIdentifier(buffer.GetString(num), buffer.GetRange(num));
					buffer.Discard(num);
					return true;
				}
			}

			private bool TryMatchKeyword(string keyword, KeywordType type, out Token token)
			{
				int num = 0;
				while (num < keyword.Length)
				{
					if (this.m_buffer.IsMatchAt(keyword[num], num))
					{
						num++;
					}
					else
					{
						token = null;
						return false;
					}
				}
				char item = this.m_buffer[keyword.Length];
				if (!Tokenizer.TokenPartEnumerator.IsDelimiter(item))
				{
					token = null;
					return false;
				}
				else
				{
					token = new TokenKeyword(type, this.m_buffer.GetString(keyword.Length), this.m_buffer.GetRange(keyword.Length));
					this.m_buffer.Discard(keyword.Length);
					return true;
				}
			}

			private bool TryMatchNumber(out Token token)
			{
				bool flag;
				char chr;
				int num = 0;
				char item = this.m_buffer[num];
				switch (item)
				{
					case '+':
					{
						flag = false;
						num++;
						break;
					}
					case ',':
					{
						flag = false;
						break;
					}
					case '-':
					{
						flag = true;
						num++;
						break;
					}
					default:
					{
						flag = false;
						break;
					}
				}
				bool item1 = this.m_buffer[num] == '0';
				if (item1)
				{
					num++;
					char chr1 = this.m_buffer[num];
					if (chr1 == 'X' || chr1 == 'x')
					{
						return this.TryMatchHexNumber(num + 1, flag, out token);
					}
				}
				int num1 = num;
				while (true)
				{
					char item2 = this.m_buffer[num1];
					chr = item2;
					if (chr == '.')
					{
						break;
					}
					else if (chr == '/')
					{
						goto Label1;
					}
					else if (chr == '0' || chr == '1' || chr == '2' || chr == '3' || chr == '4' || chr == '5' || chr == '6' || chr == '7' || chr == '8' || chr == '9')
					{
						num1++;
					}
					else
					{
						goto Label2;
					}
				}
				return this.TryMatchRealNumber(num, flag, out token);
			Label1:
				if (!item1)
				{
					return this.TryMatchDecimalNumber(num, flag, out token);
				}
				else
				{
					return this.TryMatchOctalNumber(num, flag, out token);
				}
			Label2:
				if (chr == 'b')
				{
					return this.TryMatchBinaryNumber(num, flag, out token);
				}
				else
				{
					goto Label1;
				}
			}

			private bool TryMatchOctalNumber(int digitsStartOffset, bool isNegative, out Token token)
			{
				long num = (long)0;
				int num1 = digitsStartOffset;
				while (true)
				{
					char item = this.m_buffer[num1];
					char chr = item;
					if (chr == '0' || chr == '1' || chr == '2' || chr == '3' || chr == '4' || chr == '5' || chr == '6' || chr == '7')
					{
						num = num * (long)8 + (long)(item - 48);
						num1++;
					}
				}
				if (isNegative)
				{
					num = -num;
				}
				token = new TokenInteger(num, this.m_buffer.GetRange(num1));
				this.m_buffer.Discard(num1);
				return true;
			}

			private bool TryMatchPragma(out Token token)
			{
				string str = "#pragma";
				int num = 0;
				while (num < str.Length)
				{
					if (this.m_buffer.IsMatchAt(str[num], num))
					{
						num++;
					}
					else
					{
						token = null;
						return false;
					}
				}
				char item = this.m_buffer[str.Length];
				if (!Tokenizer.TokenPartEnumerator.IsDelimiter(item))
				{
					token = null;
					return false;
				}
				else
				{
					token = new TokenPragma(this.m_buffer.GetString(str.Length), this.m_buffer.GetRange(str.Length));
					this.m_buffer.Discard(str.Length);
					return true;
				}
			}

			private bool TryMatchRealNumber(int digitsStartOffset, bool isNegative, out Token token)
			{
				throw new NotSupportedException();
			}

			private bool TryMatchString(TokenBuffer buffer, out TokenStringPart value)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num = 1;
				while (true)
				{
					char item = buffer[num];
					char chr = item;
					if (chr == '\0')
					{
						value = null;
						return false;
					}
					if (chr == '\"')
					{
						break;
					}
					if (chr == '\\')
					{
						num++;
						item = buffer[num];
						char chr1 = item;
						if (chr1 == 'n')
						{
							item = '\n';
						}
						else
						{
							if (chr1 == 'r')
							{
								item = '\r';
								goto Label1;
							}
							else if (chr1 == 's')
							{
								goto Label1;
							}
							else if (chr1 == 't')
							{
								item = '\t';
								goto Label1;
							}
						}
					}
				Label1:
					stringBuilder.Append(item);
					num++;
				}
				value = new TokenStringPart(buffer.GetRange(num + 2), stringBuilder.ToString());
				buffer.Discard(num + 1);
				return true;
			}

			private bool TryMatchWhitespace(out Token whitespaceToken)
			{
				char item = this.m_buffer[0];
				if (Tokenizer.TokenPartEnumerator.IsWhitespace(item))
				{
					int num = 1;
					while (true)
					{
						item = this.m_buffer[num];
						if (!Tokenizer.TokenPartEnumerator.IsWhitespace(item))
						{
							break;
						}
						num++;
					}
					whitespaceToken = new TokenWhitespace(this.m_buffer.GetRange(num));
					this.m_buffer.Discard(num);
					return true;
				}
				else
				{
					whitespaceToken = null;
					return false;
				}
			}

			private enum InputState
			{
				NotStarted,
				Started,
				Finished
			}
		}
	}
}