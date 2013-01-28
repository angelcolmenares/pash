using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct SelectionConditionStruct
	{
		public string tag;

		public int claimConditionCount;

		public ClaimConditionStruct[] claimConditionArray;

	}
}