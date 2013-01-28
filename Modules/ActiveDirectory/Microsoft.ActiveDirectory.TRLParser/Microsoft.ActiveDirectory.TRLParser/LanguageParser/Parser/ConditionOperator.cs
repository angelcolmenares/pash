using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class ConditionOperator
	{
		public readonly static string EQ;

		public readonly static string NEQ;

		public readonly static string REGEXP_MATCH;

		public readonly static string REGEXP_NOT_MATCH;

		static ConditionOperator()
		{
			ConditionOperator.EQ = "==";
			ConditionOperator.NEQ = "!=";
			ConditionOperator.REGEXP_MATCH = "=~";
			ConditionOperator.REGEXP_NOT_MATCH = "!~";
		}

		public ConditionOperator()
		{
		}
	}
}