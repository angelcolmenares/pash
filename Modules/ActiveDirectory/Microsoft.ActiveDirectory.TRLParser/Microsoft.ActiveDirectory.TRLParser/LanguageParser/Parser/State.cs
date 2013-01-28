using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class State
	{
		private int _number;

		private Dictionary<int, int> _parserTable;

		private Dictionary<int, int> _goToTable;

		private int _defaultAction;

		public int DefaultAction
		{
			get
			{
				return this._defaultAction;
			}
		}

		public Dictionary<int, int> GoTo
		{
			get
			{
				return this._goToTable;
			}
		}

		public int Number
		{
			get
			{
				return this._number;
			}
			set
			{
				this._number = value;
			}
		}

		public Dictionary<int, int> ParserTable
		{
			get
			{
				return this._parserTable;
			}
		}

		public State(int[] actions, int[] goTo) : this(actions)
		{
			this._goToTable = new Dictionary<int, int>();
			for (int i = 0; i < (int)goTo.Length; i = i + 2)
			{
				this.GoTo.Add(goTo[i], goTo[i + 1]);
			}
		}

		public State(int[] actions)
		{
			this._parserTable = new Dictionary<int, int>();
			for (int i = 0; i < (int)actions.Length; i = i + 2)
			{
				this._parserTable.Add(actions[i], actions[i + 1]);
			}
		}

		public State(int defaultAction)
		{
			this._defaultAction = defaultAction;
		}
	}
}