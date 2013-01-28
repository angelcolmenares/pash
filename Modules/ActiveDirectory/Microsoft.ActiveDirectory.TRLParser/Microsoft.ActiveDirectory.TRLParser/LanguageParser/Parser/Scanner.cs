using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal sealed class Scanner : ScanBase, IColorScan
	{
		private const int maxAccept = 22;

		private const int initial = 23;

		private const int eofNum = 0;

		private const int goStart = -1;

		private const int INITIAL = 0;

		public ScanBuffer buffer;

		private IErrorHandler handler;

		private int scState;

		private static int parserMax;

		private int _state;

		private int _currentStart;

		private int _chr;

		private int _cNum;

		private int _lNum;

		private int _lineStartNum;

		private int _tokPos;

		private int _tokNum;

		private int _tokLen;

		private int _tokCol;

		private int _tokLin;

		private int _tokEPos;

		private int _tokECol;

		private int _tokELin;

		private string _tokTxt;

		private static int[] _startState;

		private static sbyte[] map0;

		private static Scanner.Table[] NxS;

		protected override int CurrentSc
		{
			get
			{
				return this.scState;
			}
			set
			{
				this.scState = value;
				this._currentStart = Scanner._startState[value];
			}
		}

		private int yycol
		{
			get
			{
				return this._tokCol;
			}
		}

		private int yyline
		{
			get
			{
				return this._tokLin;
			}
		}

		public string Yytext
		{
			get
			{
				if (this._tokTxt == null)
				{
					this._tokTxt = this.buffer.GetString(this._tokPos, this._tokEPos);
				}
				return this._tokTxt;
			}
		}

		static Scanner()
		{
			Scanner.parserMax = Scanner.GetMaxParseToken();
			int[] numArray = new int[2];
			numArray[0] = 23;
			Scanner._startState = numArray;
			sbyte[] numArray1 = new sbyte[] { 17, 17, 17, 17, 17, 17, 17, 17, 17, 18, 0, 17, 17, 18, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 18, 11, 16, 17, 17, 17, 13, 17, 9, 10, 17, 17, 5, 17, 6, 17, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 4, 3, 17, 1, 2, 17, 17, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 7, 17, 8, 17, 14, 17, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 17, 17, 17, 12 };
			Scanner.map0 = numArray1;
			Scanner.NxS = new Scanner.Table[25];
			Scanner.NxS[0] = new Scanner.Table(0, 0, 0, null);
			sbyte[] numArray2 = new sbyte[2];
			numArray2[0] = 1;
			numArray2[1] = 1;
			Scanner.NxS[1] = new Scanner.Table(18, 2, -1, numArray2);
			sbyte[] numArray3 = new sbyte[] { 22, -1, -1, -1, -1, -1, -1, -1, 20, 21 };
			Scanner.NxS[2] = new Scanner.Table(12, 10, -1, numArray3);
			Scanner.NxS[3] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[4] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[5] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[6] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[7] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[8] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[9] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[10] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[11] = new Scanner.Table(0, 0, -1, null);
			sbyte[] numArray4 = new sbyte[] { 19, -1, -1, -1, -1, -1, -1, -1, 18 };
			Scanner.NxS[12] = new Scanner.Table(12, 9, -1, numArray4);
			sbyte[] numArray5 = new sbyte[1];
			numArray5[0] = 17;
			Scanner.NxS[13] = new Scanner.Table(13, 1, -1, numArray5);
			sbyte[] numArray6 = new sbyte[2];
			numArray6[0] = 14;
			numArray6[1] = 14;
			Scanner.NxS[14] = new Scanner.Table(14, 2, -1, numArray6);
			sbyte[] numArray7 = new sbyte[] { }; //<PrivateImplementationDetails>{9CE3AC6B-BBB9-4096-83BA-86F2584C17AC}.$$method0x600011f-4 };
			Scanner.NxS[15] = new Scanner.Table(16, 4, 24, numArray7);
			Scanner.NxS[16] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[17] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[18] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[19] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[20] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[21] = new Scanner.Table(0, 0, -1, null);
			Scanner.NxS[22] = new Scanner.Table(0, 0, -1, null);
			sbyte[] numArray8 = new sbyte[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 3, 13, 14, 3, 15, 3 };
			Scanner.NxS[23] = new Scanner.Table(1, 17, 1, numArray8);
			sbyte[] numArray9 = new sbyte[] { }; //<PrivateImplementationDetails>{9CE3AC6B-BBB9-4096-83BA-86F2584C17AC}.$$method0x600011f-6 };
			Scanner.NxS[24] = new Scanner.Table(16, 4, 24, numArray9);
		}

		public Scanner(Stream file)
		{
			this._currentStart = 23;
			this.handler = null;
			this.buffer = TextBuffer.NewTextBuff(file);
			this._cNum = -1;
			this._chr = 10;
			this.GetChr();
		}

		public Scanner()
		{
			this._currentStart = 23;
		}

		private void GetChr()
		{
			if (this._chr == 10)
			{
				this._lineStartNum = this._cNum + 1;
				Scanner scanner = this;
				scanner._lNum = scanner._lNum + 1;
			}
			this._chr = this.buffer.Read();
			Scanner scanner1 = this;
			scanner1._cNum = scanner1._cNum + 1;
		}

		private static int GetMaxParseToken()
		{
			FieldInfo field = typeof(Tokens).GetField("maxParseToken");
			if (field == null)
			{
				return 0x7fffffff;
			}
			else
			{
				return (int)field.GetValue(null);
			}
		}

		public int GetNext(ref int state, out int startPosition, out int endPosition)
		{
			this.EolState = state;
			Tokens token = (Tokens)this.Scan();
			state = this.EolState;
			startPosition = this._tokPos;
			endPosition = this._tokEPos - 1;
			return (int)token;
		}

		private sbyte Map(int chr)
		{
			if (chr >= 127)
			{
				return 17;
			}
			else
			{
				return Scanner.map0[chr];
			}
		}

		private void MarkEnd()
		{
			this._tokTxt = null;
			this._tokLen = this._cNum - this._tokNum;
			this._tokEPos = this.buffer.ReadPosition;
			this._tokELin = this._lNum;
			this._tokECol = this._cNum - this._lineStartNum;
		}

		private void MarkToken()
		{
			this._tokPos = this.buffer.ReadPosition;
			this._tokNum = this._cNum;
			this._tokLin = this._lNum;
			this._tokCol = this._cNum - this._lineStartNum;
		}

		private int NextState()
		{
			int nxS;
			if (this._chr != -1)
			{
				int num = this.Map(this._chr) - Scanner.NxS[this._state].min;
				if (num < 0)
				{
					num = num + 19;
				}
				if (num < Scanner.NxS[this._state].rng)
				{
					nxS = Scanner.NxS[this._state].nxt[num];
				}
				else
				{
					nxS = Scanner.NxS[this._state].dflt;
				}
				if (nxS == -1)
				{
					return this._currentStart;
				}
				else
				{
					return nxS;
				}
			}
			else
			{
				if (this._state > 22 || this._state == this._currentStart)
				{
					return 0;
				}
				else
				{
					return this._currentStart;
				}
			}
		}

		private Scanner.Result Recurse2(Scanner.Context ctx, int next)
		{
			this.SaveStateAndPos(ctx);
			this._state = next;
			if (this._state != 0)
			{
				this.GetChr();
				bool flag = false;
				while (true)
				{
					int num = this.NextState();
					next = num;
					if (num == this._currentStart)
					{
						break;
					}
					if (flag && next > 22)
					{
						this.SaveStateAndPos(ctx);
					}
					this._state = next;
					if (this._state == 0)
					{
						return Scanner.Result.accept;
					}
					this.GetChr();
					flag = this._state <= 22;
				}
				if (!flag)
				{
					return Scanner.Result.noMatch;
				}
				else
				{
					return Scanner.Result.accept;
				}
			}
			else
			{
				return Scanner.Result.accept;
			}
		}

		private void RestoreStateAndPos(Scanner.Context ctx)
		{
			this.buffer.Position = ctx.bPos;
			this._cNum = ctx.cNum;
			this._state = ctx.state;
			this._chr = ctx.cChr;
		}

		private void SaveStateAndPos(Scanner.Context ctx)
		{
			ctx.bPos = this.buffer.Position;
			ctx.cNum = this._cNum;
			ctx.state = this._state;
			ctx.cChr = this._chr;
		}

		private int Scan()
		{
			while (true)
			{
				bool flag = false;
				this._state = this._currentStart;
				while (this.NextState() == this._state)
				{
					this.GetChr();
				}
				this.MarkToken();
				while (true)
				{
					int num = this.NextState();
					int num1 = num;
					if (num == this._currentStart)
					{
						break;
					}
					if (!flag || num1 <= 22)
					{
						this._state = num1;
						this.GetChr();
						if (this._state <= 22)
						{
							flag = true;
						}
					}
					else
					{
						Scanner.Context context = new Scanner.Context();
						Scanner.Result result = this.Recurse2(context, num1);
						if (result != Scanner.Result.noMatch)
						{
							break;
						}
						this.RestoreStateAndPos(context);
						break;
					}
				}
				if (this._state <= 22)
				{
					this.MarkEnd();
					int num2 = this._state;
					if (num2 == 0)
					{
						return 2;
					}
					else if (num2 == 1)
					{
						continue;
					}
					else if (num2 == 2)
					{
						return 29;
					}
					else if (num2 == 3 || num2 == 12 || num2 == 13 || num2 == 15)
					{
						this.yyerror("Illegal input", new object[0]);
						continue;
					}
					else if (num2 == 4)
					{
						return 13;
					}
					else if (num2 == 5)
					{
						return 14;
					}
					else if (num2 == 6)
					{
						return 15;
					}
					else if (num2 == 7)
					{
						return 16;
					}
					else if (num2 == 8)
					{
						return 17;
					}
					else if (num2 == 9)
					{
						return 18;
					}
					else if (num2 == 10)
					{
						return 19;
					}
					else if (num2 == 11)
					{
						return 20;
					}
					else if (num2 == 14)
					{
						this.yylval.StringValue = this.Yytext;
						string lower = this.Yytext.ToLower(CultureInfo.InvariantCulture);
						string str = lower;
						if (lower != null)
						{
							if (str == "issue")
							{
								return 5;
							}
							else
							{
								if (str == "type")
								{
									return 8;
								}
								else
								{
									if (str == "value")
									{
										return 9;
									}
									else
									{
										if (str == "valuetype")
										{
											return 10;
										}
										else
										{
											if (str == "claim")
											{
												return 12;
											}
										}
									}
								}
							}
						}
						return 3;
					}
					else if (num2 == 16)
					{
						this.yylval.StringValue = this.Yytext;
						string lower1 = this.Yytext.ToLower(CultureInfo.InvariantCulture);
						string str1 = lower1;
						if (lower1 != null)
						{
							if (str1 == "\"int64\"")
							{
								return 21;
							}
							else
							{
								if (str1 == "\"uint64\"")
								{
									return 22;
								}
								else
								{
									if (str1 == "\"string\"")
									{
										return 23;
									}
									else
									{
										if (str1 == "\"boolean\"")
										{
											return 24;
										}
									}
								}
							}
						}
						return 4;
					}
					else if (num2 == 17)
					{
						return 30;
					}
					else if (num2 == 18)
					{
						return 26;
					}
					else if (num2 == 19)
					{
						return 28;
					}
					else if (num2 == 20)
					{
						return 25;
					}
					else if (num2 == 21)
					{
						return 7;
					}
					else if (num2 == 22)
					{
						break;
					}
				}
				else
				{
					this._state = this._currentStart;
				}
			}
			return 27;
		}

		public void SetSource(string source, int offset)
		{
			this.buffer = new StringBuffer(source);
			this.buffer.Position = offset;
			if (offset == 0)
			{
				this._cNum = -1;
			}
			else
			{
				this._cNum = offset - 1;
			}
			this._chr = 10;
			this.GetChr();
		}

		public override void yyerror(string[] expectedTokens, string actualToken)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] strArrays = expectedTokens;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				stringBuilder.AppendFormat("{0} ", str);
			}
			object[] objArray = new object[2];
			objArray[0] = actualToken;
			objArray[1] = stringBuilder.ToString();
			string str1 = SR.GetString("POLICY0030", objArray);
			if (this.handler != null)
			{
				this.handler.AddError(str1, this.yyline, this.yycol, this.Yytext.Length, 1);
			}
			throw new PolicyLanguageParserException(this.yyline, this.yycol, this.Yytext, str1);
		}

		public override void yyerror(string format, object[] args)
		{
			string str;
			if (format != "Illegal input")
			{
				str = string.Format(CultureInfo.InvariantCulture, format, args);
			}
			else
			{
				str = SR.GetString("POLICY0029", new object[0]);
			}
			if (this.handler != null)
			{
				this.handler.AddError(str, this.yyline, this.yycol, this.Yytext.Length, 1);
			}
			throw new PolicyLanguageParserException(this.yyline, this.yycol, this.Yytext, str);
		}

		public override int yylex()
		{
			int num;
			do
			{
				num = this.Scan();
			}
			while (num >= Scanner.parserMax);
			return num;
		}

		internal class Context
		{
			public int bPos;

			public int cNum;

			public int state;

			public int cChr;

			public Context()
			{
			}
		}

		private enum Result
		{
			accept,
			noMatch,
			contextFound
		}

		private struct Table
		{
			public int min;

			public int rng;

			public int dflt;

			public sbyte[] nxt;

			public Table(int m, int x, int d, sbyte[] n)
			{
				this.min = m;
				this.rng = x;
				this.dflt = d;
				this.nxt = n;
			}
		}
	}
}