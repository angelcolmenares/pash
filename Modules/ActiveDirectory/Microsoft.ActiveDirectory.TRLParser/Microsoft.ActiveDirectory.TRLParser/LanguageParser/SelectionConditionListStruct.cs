using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct SelectionConditionListStruct
	{
		public int selectionConditionCount;

		public SelectionConditionStruct[] selectionArray;

	}
}