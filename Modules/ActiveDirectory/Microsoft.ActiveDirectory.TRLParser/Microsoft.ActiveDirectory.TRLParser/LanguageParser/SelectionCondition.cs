using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class SelectionCondition
	{
		private NonNullableCollection<ClaimCondition> _conditions;

		public NonNullableCollection<ClaimCondition> Conditions
		{
			get
			{
				return this._conditions;
			}
		}

		public string ConditionTag
		{
			get;
			set;
		}

		public SelectionCondition()
		{
			this._conditions = new NonNullableCollection<ClaimCondition>();
		}

		public virtual bool Compare(SelectionCondition other)
		{
			if (other != null)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(this.ConditionTag, other.ConditionTag))
				{
					if (this.Conditions.Count == other.Conditions.Count)
					{
						int num = 0;
						while (num < this.Conditions.Count)
						{
							if (this.Conditions[num].Compare(other.Conditions[num]))
							{
								num++;
							}
							else
							{
								return false;
							}
						}
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

		public virtual SelectionConditionStruct GetStruct()
		{
			SelectionConditionStruct conditionTag = new SelectionConditionStruct();
			conditionTag.tag = this.ConditionTag;
			conditionTag.claimConditionArray = new ClaimConditionStruct[this.Conditions.Count];
			conditionTag.claimConditionCount = this.Conditions.Count;
			for (int i = 0; i < this.Conditions.Count; i++)
			{
				conditionTag.claimConditionArray[i] = this.Conditions[i].GetStruct();
			}
			return conditionTag;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(this.ConditionTag))
			{
				stringBuilder.Append(this.ConditionTag);
				stringBuilder.Append(":");
			}
			stringBuilder.Append("[");
			for (int i = 0; i < this.Conditions.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(this.Conditions[i].ToString());
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public virtual void Validate()
		{
			foreach (ClaimCondition condition in this.Conditions)
			{
				condition.Validate();
			}
		}
	}
}