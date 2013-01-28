using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal abstract class Rule
	{
		public IssuanceStatement IssuanceStatement
		{
			get;
			set;
		}

		public RuleOutput Output
		{
			get;
			set;
		}

		protected Rule()
		{
		}

		public virtual bool Compare(Rule other)
		{
			if (other != null)
			{
				if (this.Output == other.Output)
				{
					if (this.IssuanceStatement.Compare(other.IssuanceStatement))
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

		public virtual RuleStruct GetStruct()
		{
			RuleStruct @struct = new RuleStruct();
			@struct.selectionConditionList.selectionConditionCount = 0;
			@struct.selectionConditionList.selectionArray = null;
			@struct.action = this.IssuanceStatement.GetStruct();
			if (this.Output != RuleOutput.EvaluationContext)
			{
				return @struct;
			}
			else
			{
				throw new Exception("Unexpected Condition: Output == RuleOutput.EvaluationContext ");
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(" => ");
			if (this.Output != RuleOutput.EvaluationContext)
			{
				stringBuilder.Append("issue");
			}
			else
			{
				stringBuilder.Append("add");
			}
			stringBuilder.Append(this.IssuanceStatement.ToString());
			stringBuilder.AppendLine(";");
			return stringBuilder.ToString();
		}

		public abstract void Validate();
	}
}