using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class ClaimPropertyAccessExpression : Expression
	{
		public ClaimProperty ClaimProperty
		{
			get;
			set;
		}

		public string ConditionTag
		{
			get;
			set;
		}

		public ClaimPropertyAccessExpression(ClaimProperty claimProperty, string conditionTag)
		{
			Utility.VerifyNonNullArgument("claimProperty", claimProperty);
			Utility.VerifyNonNullArgument("conditionTag", conditionTag);
			this.ClaimProperty = claimProperty;
			this.ConditionTag = conditionTag;
		}

		public override bool Compare(Expression other)
		{
			ClaimPropertyAccessExpression claimPropertyAccessExpression = other as ClaimPropertyAccessExpression;
			if (claimPropertyAccessExpression != null)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(this.ConditionTag, claimPropertyAccessExpression.ConditionTag))
				{
					if (this.ClaimProperty.Compare(claimPropertyAccessExpression.ClaimProperty))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public override ExpressionStruct GetStruct()
		{
			ExpressionStruct @struct = base.GetStruct();
			@struct.type = ExpressionType.Reference;
			@struct.ReferenceExpression.issuanceTag = this.ConditionTag;
			@struct.ReferenceExpression.property = (uint)this.ClaimProperty.PropertyType;
			return @struct;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.ConditionTag);
			stringBuilder.Append(".");
			stringBuilder.Append(this.ClaimProperty.ToString());
			return stringBuilder.ToString();
		}

		public override void Validate(object context)
		{
			Utility.VerifyNonNull("ClaimProperty", this.ClaimProperty);
			Utility.VerifyNonNull("ConditionTag", this.ConditionTag);
			this.ClaimProperty.Validate();
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
					throw new PolicyValidationException(SR.GetString("POLICY0025", conditionTag));
				}
				return;
			}
			else
			{
				object[] str = new object[1];
				str[0] = context.GetType().ToString();
				throw new PolicyValidationException(SR.GetString("POLICY0015", str));
			}
		}
	}
}