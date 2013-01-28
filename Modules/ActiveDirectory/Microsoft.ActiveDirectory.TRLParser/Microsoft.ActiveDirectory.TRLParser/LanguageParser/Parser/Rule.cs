using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class Rule
	{
		private int _lhs;

		private int[] _rhs;

		public int LeftSymbol
		{
			get
			{
				return this._lhs;
			}
		}

		public IList<int> RightSymbols
		{
			get
			{
				return this._rhs;
			}
		}

		public Rule(int leftSymbol, int[] rightSymbols)
		{
			this._lhs = leftSymbol;
			this._rhs = rightSymbols;
		}
	}
}