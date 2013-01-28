using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class CopyClaimIssuanceStatement : IssuanceStatement
	{
		public string ConditionTag
		{
			get;
			set;
		}

		public CopyClaimIssuanceStatement(string conditionTag)
		{
			Utility.VerifyNonNullArgument("conditionTag", conditionTag);
			this.ConditionTag = conditionTag;
		}

		public override bool Compare(IssuanceStatement other)
		{
			CopyClaimIssuanceStatement copyClaimIssuanceStatement = other as CopyClaimIssuanceStatement;
			if (copyClaimIssuanceStatement != null)
			{
				return StringComparer.OrdinalIgnoreCase.Equals(this.ConditionTag, copyClaimIssuanceStatement.ConditionTag);
			}
			else
			{
				return false;
			}
		}

		public override ActionStruct GetStruct()
		{
			ActionStruct conditionTag = new ActionStruct();
			conditionTag.actionType = 1;
			conditionTag.tag = this.ConditionTag;
			conditionTag.propertyIssuanceCount = 0;
			conditionTag.propertyAssignmentArray = null;
			return conditionTag;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(claim = ");
			stringBuilder.Append(this.ConditionTag);
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public override void Validate(Rule context)
		{
			Utility.VerifyNonNullArgument("context", context);
			Utility.VerifyNonNull("ConditionTag", this.ConditionTag);
			SelectionRule selectionRule = context as SelectionRule;
			if (selectionRule != null)
			{
				IEnumerator<SelectionCondition> enumerator = selectionRule.Conditions.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						SelectionCondition current = enumerator.Current;
						if (StringComparer.OrdinalIgnoreCase.Compare(this.ConditionTag, current.ConditionTag) != 0)
						{
							continue;
						}
						return;
					}
					object[] conditionTag = new object[1];
					conditionTag[0] = this.ConditionTag;
					throw new PolicyValidationException(SR.GetString("POLICY0011", conditionTag));
				}
				return;
			}
			else
			{
				object[] str = new object[1];
				str[0] = context.GetType().ToString();
				throw new PolicyValidationException(SR.GetString("POLICY0001", str));
			}
		}
	}
}