using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal abstract class IssuanceStatement
	{
		protected IssuanceStatement()
		{
		}

		public abstract bool Compare(IssuanceStatement other);

		public abstract ActionStruct GetStruct();

		public abstract void Validate(Rule context);
	}
}