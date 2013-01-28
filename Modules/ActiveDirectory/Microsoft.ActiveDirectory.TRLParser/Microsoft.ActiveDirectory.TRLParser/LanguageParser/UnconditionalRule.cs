using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class UnconditionalRule : Rule
	{
		public UnconditionalRule(IssuanceStatement issuanceStatement) : this(issuanceStatement, (RuleOutput)1)
		{
		}

		public UnconditionalRule(IssuanceStatement issuanceStatement, RuleOutput output)
		{
			Utility.VerifyNonNullArgument("issuanceStatement", issuanceStatement);
			base.IssuanceStatement = issuanceStatement;
			base.Output = output;
		}

		public override bool Compare(Rule other)
		{
			UnconditionalRule unconditionalRule = other as UnconditionalRule;
			if (unconditionalRule != null)
			{
				return base.Compare(other);
			}
			else
			{
				return false;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.ToString());
			return stringBuilder.ToString();
		}

		public override void Validate()
		{
			Utility.VerifyNonNull("IssuanceStatement", base.IssuanceStatement);
			base.IssuanceStatement.Validate(this);
		}
	}
}