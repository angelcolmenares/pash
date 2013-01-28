using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct ClaimConditionStruct
	{
		public uint property;

		public uint comparisonOperator;

		public string comparisonValue;

	}
}