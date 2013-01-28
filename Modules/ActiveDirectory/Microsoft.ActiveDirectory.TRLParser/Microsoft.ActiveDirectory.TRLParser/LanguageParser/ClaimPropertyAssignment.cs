using System;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class ClaimPropertyAssignment
	{
		public ClaimProperty ClaimProperty
		{
			get;
			set;
		}

		public Expression Expression
		{
			get;
			set;
		}

		public ClaimPropertyAssignment(ClaimProperty claimProperty, Expression expression)
		{
			Utility.VerifyNonNullArgument("claimProperty", claimProperty);
			Utility.VerifyNonNullArgument("expression", expression);
			this.ClaimProperty = claimProperty;
			this.Expression = expression;
		}

		public virtual bool Compare(ClaimPropertyAssignment other)
		{
			if (other != null)
			{
				if (this.ClaimProperty.Compare(other.ClaimProperty))
				{
					if (this.Expression.Compare(other.Expression))
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

		public virtual ClaimPropertyAssignmentStruct GetStruct()
		{
			ClaimPropertyAssignmentStruct propertyType = new ClaimPropertyAssignmentStruct();
			propertyType.property = (uint)this.ClaimProperty.PropertyType;
			propertyType.issueValue = this.Expression.GetStruct();
			return propertyType;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.ClaimProperty.ToString());
			stringBuilder.Append(" = ");
			stringBuilder.Append(this.Expression.ToString());
			return stringBuilder.ToString();
		}

		public virtual void Validate(Rule context)
		{
			Utility.VerifyNonNull("ClaimProperty", this.ClaimProperty);
			Utility.VerifyNonNull("Expression", this.Expression);
			this.ClaimProperty.Validate();
			this.Expression.Validate(context);
		}
	}
}