using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal abstract class ShiftReduceParser<YYSTYPE, YYLTYPE>
	where YYSTYPE : struct
	where YYLTYPE : IMerge<YYLTYPE>
	{
		public bool Trace;

		private AScanner<YYSTYPE, YYLTYPE> _scanner;

		private ParserStack<State> _stateStack;

		private int _next;

		private State _currentState;

		private bool _recovering;

		private int _tokensSinceLastError;

		protected YYSTYPE yyval;

		protected YYLTYPE yyloc;

		protected YYLTYPE lastL;

		protected ParserStack<YYSTYPE> valueStack;

		protected ParserStack<YYLTYPE> locationStack;

		protected string[] nonTerminals;

		protected State[] states;

		protected Rule[] rules;

		protected int errToken;

		protected int eofToken;

		public AScanner<YYSTYPE, YYLTYPE> Scanner
		{
			get
			{
				return this._scanner;
			}
			set
			{
				this._scanner = value;
			}
		}

		protected ShiftReduceParser()
		{
			this._stateStack = new ParserStack<State>();
			this.valueStack = new ParserStack<YYSTYPE>();
			this.locationStack = new ParserStack<YYLTYPE>();
		}

		protected void AddState(int statenr, State state)
		{
			this.states[statenr] = state;
			state.Number = statenr;
		}

		protected string CharToString(char ch)
		{
			object[] objArray;
			char chr = ch;
			switch (chr)
			{
				case '\0':
				{
					return "'\\0'";
				}
				case '\u0001':
				case '\u0002':
				case '\u0003':
				case '\u0004':
				case '\u0005':
				case '\u0006':
				{
					objArray = new object[1];
					objArray[0] = ch;
					return string.Format(CultureInfo.InvariantCulture, "'{0}'", objArray);
				}
				case '\a':
				{
					return "'\\a'";
				}
				case '\b':
				{
					return "'\\b'";
				}
				case '\t':
				{
					return "'\\t'";
				}
				case '\n':
				{
					return "'\\n'";
				}
				case '\v':
				{
					return "'\\v'";
				}
				case '\f':
				{
					return "'\\f'";
				}
				case '\r':
				{
					return "'\\r'";
				}
				default:
				{
					objArray = new object[1];
					objArray[0] = (object)ch;
					return string.Format(CultureInfo.InvariantCulture, "'{0}'", objArray);
				}
			}
		}

		public bool DiscardInvalidTokens()
		{
			int defaultAction = this._currentState.DefaultAction;
			if (this._currentState.ParserTable == null)
			{
				if (!this._recovering || this._tokensSinceLastError != 0)
				{
					return true;
				}
				else
				{
					if (this.Trace)
					{
						object[] str = new object[1];
						str[0] = this.TerminalToString(this._next);
						ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Error: panic discard of {0}", str);
					}
					this._next = 0;
					return true;
				}
			}
			else
			{
				while (true)
				{
					if (this._next == 0)
					{
						if (this.Trace)
						{
							ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Reading a token: ", new object[0]);
						}
						this._next = this._scanner.yylex();
					}
					if (this.Trace)
					{
						object[] objArray = new object[1];
						objArray[0] = this.TerminalToString(this._next);
						ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Next token is {0}", objArray);
					}
					if (this._next == this.eofToken)
					{
						return false;
					}
					if (this._currentState.ParserTable.ContainsKey(this._next))
					{
						defaultAction = this._currentState.ParserTable[this._next];
					}
					if (defaultAction != 0)
					{
						break;
					}
					if (this.Trace)
					{
						object[] str1 = new object[1];
						str1[0] = this.TerminalToString(this._next);
						ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Error: Discarding {0}", str1);
					}
					this._next = 0;
				}
				return true;
			}
		}

		private void DisplayProduction(Rule rule)
		{
			if (rule.RightSymbols.Count != 0)
			{
				foreach (int rightSymbol in rule.RightSymbols)
				{
					object[] str = new object[1];
					str[0] = this.SymbolToString(rightSymbol);
					ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("{0} ", str);
				}
			}
			else
			{
				ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("/* empty */ ", new object[0]);
			}
			object[] objArray = new object[1];
			objArray[0] = this.SymbolToString(rule.LeftSymbol);
			ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("-> {0}", objArray);
		}

		private void DisplayRule(int rule_nr)
		{
			object[] ruleNr = new object[1];
			ruleNr[0] = rule_nr;
			ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Reducing stack by rule {0}, ", ruleNr);
			this.DisplayProduction(this.rules[rule_nr]);
		}

		private void DisplayStack()
		{
			ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("State now", new object[0]);
			for (int i = 0; i < this._stateStack.Top; i++)
			{
				object[] number = new object[1];
				number[0] = this._stateStack.Elements[i].Number;
				ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg(" {0}", number);
			}
		}

		protected abstract void DoAction(int action);

		public bool ErrorRecovery()
		{
			if (!this._recovering)
			{
				this.ReportError();
			}
			if (this.FindErrorRecoveryState())
			{
				this.ShiftErrorToken();
				bool flag = this.DiscardInvalidTokens();
				this._recovering = true;
				this._tokensSinceLastError = 0;
				return flag;
			}
			else
			{
				return false;
			}
		}

		public bool FindErrorRecoveryState()
		{
			while (this._currentState.ParserTable == null || !this._currentState.ParserTable.ContainsKey(this.errToken) || this._currentState.ParserTable[this.errToken] <= 0)
			{
				if (this.Trace)
				{
					object[] number = new object[1];
					number[0] = this._stateStack.TopElement().Number;
					ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Error: popping state {0}", number);
				}
				this._stateStack.Pop();
				this.valueStack.Pop();
				this.locationStack.Pop();
				if (this.Trace)
				{
					this.DisplayStack();
				}
				if (!this._stateStack.IsEmpty())
				{
					this._currentState = this._stateStack.TopElement();
				}
				else
				{
					if (this.Trace)
					{
						ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Aborting: didn't find a state that accepts error token", new object[0]);
					}
					return false;
				}
			}
			return true;
		}

		protected abstract void Initialize();

		public bool Parse()
		{
			int defaultAction;
			this.Initialize();
			this._next = 0;
			this._currentState = this.states[0];
			this._stateStack.Push(this._currentState);
			this.valueStack.Push(this.yyval);
			this.locationStack.Push(this.yyloc);
			do
			{
			Label0:
				if (this.Trace)
				{
					object[] number = new object[1];
					number[0] = this._currentState.Number;
					ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Entering state {0} ", number);
				}
				defaultAction = this._currentState.DefaultAction;
				if (this._currentState.ParserTable != null)
				{
					if (this._next == 0)
					{
						if (this.Trace)
						{
							ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Reading a token: ", new object[0]);
						}
						this.lastL = this._scanner.yylloc;
						this._next = this._scanner.yylex();
					}
					if (this.Trace)
					{
						object[] str = new object[1];
						str[0] = this.TerminalToString(this._next);
						ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Next token is {0}", str);
					}
					if (this._currentState.ParserTable.ContainsKey(this._next))
					{
						defaultAction = this._currentState.ParserTable[this._next];
					}
				}
				if (defaultAction <= 0)
				{
					if (defaultAction >= 0)
					{
						continue;
					}
					this.Reduce(-defaultAction);
					if (defaultAction == -1)
					{
						return true;
					}
					else
					{
						goto Label0;
					}
				}
				else
				{
					this.Shift(defaultAction);
					goto Label0;
				}
			}
			while (defaultAction != 0 || this.ErrorRecovery());
			return false;
		}

		protected void Reduce(int reduceRule)
		{
			YYLTYPE yYLTYPE;
			if (this.Trace)
			{
				this.DisplayRule(reduceRule);
			}
			Rule rule = this.rules[reduceRule];
			if (rule.RightSymbols.Count != 1)
			{
				this.yyval = default(YYSTYPE);
			}
			else
			{
				this.yyval = this.valueStack.TopElement();
			}
			if (rule.RightSymbols.Count != 1)
			{
				if (rule.RightSymbols.Count != 0)
				{
					YYLTYPE item = this.locationStack.Elements[this.locationStack.Top - rule.RightSymbols.Count];
					YYLTYPE yYLTYPE1 = this.locationStack.TopElement();
					if (item != null && yYLTYPE1 != null)
					{
						this.yyloc = item.Merge(yYLTYPE1);
					}
				}
				else
				{
					ShiftReduceParser<YYSTYPE, YYLTYPE> shiftReduceParser = this;
					if (this._scanner.yylloc != null)
					{
						yYLTYPE = this._scanner.yylloc.Merge(this.lastL);
					}
					else
					{
						YYLTYPE yYLTYPE2 = default(YYLTYPE);
						yYLTYPE = yYLTYPE2;
					}
					shiftReduceParser.yyloc = yYLTYPE;
				}
			}
			else
			{
				this.yyloc = this.locationStack.TopElement();
			}
			this.DoAction(reduceRule);
			for (int i = 0; i < rule.RightSymbols.Count; i++)
			{
				this._stateStack.Pop();
				this.valueStack.Pop();
				this.locationStack.Pop();
			}
			if (this.Trace)
			{
				this.DisplayStack();
			}
			this._currentState = this._stateStack.TopElement();
			if (this._currentState.GoTo.ContainsKey(rule.LeftSymbol))
			{
				this._currentState = this.states[this._currentState.GoTo[rule.LeftSymbol]];
			}
			this._stateStack.Push(this._currentState);
			this.valueStack.Push(this.yyval);
			this.locationStack.Push(this.yyloc);
		}

		public void ReportError()
		{
			List<string> strs = new List<string>();
			string str = this.TerminalToString(this._next);
			if (this._currentState.ParserTable.Count < 7)
			{
				foreach (int key in this._currentState.ParserTable.Keys)
				{
					strs.Add(this.TerminalToString(key));
				}
			}
			this._scanner.yyerror(strs.ToArray(), str);
		}

		protected void Shift(int shiftState)
		{
			if (this.Trace)
			{
				object[] str = new object[1];
				str[0] = this.TerminalToString(this._next);
				ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Shifting token {0}, ", str);
			}
			this._currentState = this.states[shiftState];
			this.valueStack.Push(this._scanner.yylval);
			this._stateStack.Push(this._currentState);
			this.locationStack.Push(this._scanner.yylloc);
			if (this._recovering)
			{
				if (this._next != this.errToken)
				{
					ShiftReduceParser<YYSTYPE, YYLTYPE> shiftReduceParser = this;
					shiftReduceParser._tokensSinceLastError = shiftReduceParser._tokensSinceLastError + 1;
				}
				if (this._tokensSinceLastError > 5)
				{
					this._recovering = false;
				}
			}
			if (this._next != this.eofToken)
			{
				this._next = 0;
			}
		}

		public void ShiftErrorToken()
		{
			int num = this._next;
			this._next = this.errToken;
			this.Shift(this._currentState.ParserTable[this._next]);
			if (this.Trace)
			{
				object[] number = new object[1];
				number[0] = this._currentState.Number;
				ShiftReduceParser<YYSTYPE, YYLTYPE>.TraceMsg("Entering state {0} ", number);
			}
			this._next = num;
		}

		private string SymbolToString(int symbol)
		{
			if (symbol >= 0)
			{
				return this.TerminalToString(symbol);
			}
			else
			{
				return this.nonTerminals[-symbol];
			}
		}

		protected abstract string TerminalToString(int terminal);

		private static void TraceMsg(string msgFormat, object[] args)
		{
		}

		protected void yyclearin()
		{
			this._next = 0;
		}

		protected void yyerrok()
		{
			this._recovering = false;
		}
	}
}