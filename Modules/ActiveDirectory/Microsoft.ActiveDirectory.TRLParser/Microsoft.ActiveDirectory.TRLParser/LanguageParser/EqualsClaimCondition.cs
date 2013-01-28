using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class EqualsClaimCondition : ClaimCondition
	{
		public EqualsClaimCondition(ClaimProperty claimProperty, Expression expression)
		{
			Utility.VerifyNonNullArgument("claimProperty", claimProperty);
			Utility.VerifyNonNullArgument("expression", expression);
			base.ClaimProperty = claimProperty;
			base.Expression = expression;
		}

		public override bool Compare(ClaimCondition other)
		{
			EqualsClaimCondition equalsClaimCondition = other as EqualsClaimCondition;
			if (equalsClaimCondition != null)
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
			stringBuilder.Append(base.ClaimProperty);
			stringBuilder.Append(" == ");
			stringBuilder.Append(base.Expression);
			return stringBuilder.ToString();
		}
	}
}