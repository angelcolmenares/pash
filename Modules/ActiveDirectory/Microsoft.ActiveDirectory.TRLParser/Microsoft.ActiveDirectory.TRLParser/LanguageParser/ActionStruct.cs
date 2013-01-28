using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct ActionStruct
	{
		public uint actionType;

		public string tag;

		public int propertyIssuanceCount;

		public ClaimPropertyAssignmentStruct[] propertyAssignmentArray;

	}
}