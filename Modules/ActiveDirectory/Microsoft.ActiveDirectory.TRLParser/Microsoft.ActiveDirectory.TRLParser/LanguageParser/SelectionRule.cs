using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class SelectionRule : Rule
	{
		private NonNullableCollection<SelectionCondition> _conditions;

		public NonNullableCollection<SelectionCondition> Conditions
		{
			get
			{
				return this._conditions;
			}
		}

		public SelectionRule(IssuanceStatement issuanceStatement) : this(issuanceStatement, (RuleOutput)1)
		{
		}

		public SelectionRule(IssuanceStatement issuanceStatement, RuleOutput output)
		{
			this._conditions = new NonNullableCollection<SelectionCondition>();
			Utility.VerifyNonNullArgument("issuanceStatement", issuanceStatement);
			base.IssuanceStatement = issuanceStatement;
			base.Output = output;
		}

		public override bool Compare(Rule other)
		{
			SelectionRule selectionRule = other as SelectionRule;
			if (selectionRule != null)
			{
				if (this.Conditions.Count == selectionRule.Conditions.Count)
				{
					int num = 0;
					while (num < this.Conditions.Count)
					{
						if (this.Conditions[num].Compare(selectionRule.Conditions[num]))
						{
							num++;
						}
						else
						{
							return false;
						}
					}
					return base.Compare(other);
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

		public override RuleStruct GetStruct()
		{
			RuleStruct @struct = base.GetStruct();
			@struct.selectionConditionList.selectionConditionCount = this.Conditions.Count;
			@struct.selectionConditionList.selectionArray = new SelectionConditionStruct[this.Conditions.Count];
			for (int i = 0; i < this.Conditions.Count; i++)
			{
				@struct.selectionConditionList.selectionArray[i] = this.Conditions[i].GetStruct();
			}
			return @struct;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.Conditions.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" && ");
				}
				stringBuilder.AppendLine(this.Conditions[i].ToString());
			}
			stringBuilder.Append(base.ToString());
			return stringBuilder.ToString();
		}

		public override void Validate()
		{
			Utility.VerifyNonNull("IssuanceStatement", base.IssuanceStatement);
			base.IssuanceStatement.Validate(this);
			if (this.Conditions.Count >= 1)
			{
				Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach (SelectionCondition condition in this.Conditions)
				{
					condition.Validate();
					if (string.IsNullOrEmpty(condition.ConditionTag))
					{
						continue;
					}
					if (!strs.ContainsKey(condition.ConditionTag))
					{
						strs.Add(condition.ConditionTag, null);
					}
					else
					{
						object[] conditionTag = new object[1];
						conditionTag[0] = condition.ConditionTag;
						throw new PolicyValidationException(SR.GetString("POLICY0024", conditionTag));
					}
				}
				return;
			}
			else
			{
				throw new PolicyValidationException(SR.GetString("POLICY0026", new object[0]));
			}
		}
	}
}