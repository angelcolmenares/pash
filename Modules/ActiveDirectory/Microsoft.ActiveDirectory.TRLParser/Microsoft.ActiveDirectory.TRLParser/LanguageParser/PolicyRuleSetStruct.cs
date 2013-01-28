using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct PolicyRuleSetStruct
	{
		public int ruleCount;

		public RuleStruct[] ruleArray;

	}
}