using Microsoft.ActiveDirectory.TRLParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class NewClaimIssuanceStatement : IssuanceStatement
	{
		private NonNullableCollection<ClaimPropertyAssignment> _claimPropertyAssignments;

		public NonNullableCollection<ClaimPropertyAssignment> ClaimPropertyAssignments
		{
			get
			{
				return this._claimPropertyAssignments;
			}
		}

		public NewClaimIssuanceStatement()
		{
			this._claimPropertyAssignments = new NonNullableCollection<ClaimPropertyAssignment>();
		}

		public override bool Compare(IssuanceStatement other)
		{
			NewClaimIssuanceStatement newClaimIssuanceStatement = other as NewClaimIssuanceStatement;
			if (newClaimIssuanceStatement != null)
			{
				if (this.ClaimPropertyAssignments.Count == newClaimIssuanceStatement.ClaimPropertyAssignments.Count)
				{
					int num = 0;
					while (num < this.ClaimPropertyAssignments.Count)
					{
						if (this.ClaimPropertyAssignments[num].Compare(newClaimIssuanceStatement.ClaimPropertyAssignments[num]))
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

		public override ActionStruct GetStruct()
		{
			ActionStruct count = new ActionStruct();
			count.actionType = 2;
			count.propertyIssuanceCount = this.ClaimPropertyAssignments.Count;
			count.propertyAssignmentArray = new ClaimPropertyAssignmentStruct[this.ClaimPropertyAssignments.Count];
			for (int i = 0; i < this.ClaimPropertyAssignments.Count; i++)
			{
				count.propertyAssignmentArray[i] = this.ClaimPropertyAssignments[i].GetStruct();
			}
			count.tag = null;
			return count;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for (int i = 0; i < this.ClaimPropertyAssignments.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(this.ClaimPropertyAssignments[i].ToString());
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public override void Validate(Rule context)
		{
			Utility.VerifyNonNullArgument("context", context);
			new Dictionary<string, object>(StringComparer.Ordinal);
			Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.Ordinal);
			foreach (ClaimPropertyAssignment claimPropertyAssignment in this.ClaimPropertyAssignments)
			{
				claimPropertyAssignment.Validate(context);
				//claimPropertyAssignment.ClaimProperty.PropertyType;
				string str = claimPropertyAssignment.ClaimProperty.PropertyType.ToString();
				if (!strs.ContainsKey(str))
				{
					strs.Add(str, null);
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = str;
					throw new PolicyValidationException(SR.GetString("POLICY0009", objArray));
				}
			}
			if (strs.ContainsKey(ClaimPropertyType.Type.ToString()))
			{
				if (strs.ContainsKey(ClaimPropertyType.Value.ToString()))
				{
					return;
				}
				else
				{
					throw new PolicyValidationException(SR.GetString("POLICY0008", new object[0]));
				}
			}
			else
			{
				throw new PolicyValidationException(SR.GetString("POLICY0007", new object[0]));
			}
		}
	}
}