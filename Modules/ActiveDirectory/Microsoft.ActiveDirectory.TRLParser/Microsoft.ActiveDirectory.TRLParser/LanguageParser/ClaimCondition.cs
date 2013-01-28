using Microsoft.ActiveDirectory.TRLParser;
using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal abstract class ClaimCondition
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

		protected ClaimCondition()
		{
		}

		public virtual bool Compare(ClaimCondition other)
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

		public static EqualsClaimCondition Equal(ClaimProperty claimProperty, Expression expression)
		{
			return new EqualsClaimCondition(claimProperty, expression);
		}

		public virtual ClaimConditionStruct GetStruct()
		{
			ClaimConditionStruct propertyType = new ClaimConditionStruct();
			propertyType.property = (uint)this.ClaimProperty.PropertyType;
			if (this as EqualsClaimCondition == null)
			{
				if (this as NotEqualsClaimCondition == null)
				{
					if (this as RegexMatchClaimCondition == null)
					{
						if (this as RegexNotMatchClaimCondition == null)
						{
							object[] str = new object[1];
							str[0] = this.GetType().ToString();
							throw new InvalidOperationException(SR.GetString("POLICY0039", str));
						}
						else
						{
							propertyType.comparisonOperator = 4;
						}
					}
					else
					{
						propertyType.comparisonOperator = 3;
					}
				}
				else
				{
					propertyType.comparisonOperator = 2;
				}
			}
			else
			{
				propertyType.comparisonOperator = 1;
			}
			if (this.Expression as StringLiteralExpression == null)
			{
				object[] objArray = new object[1];
				objArray[0] = this.Expression.GetType().ToString();
				throw new InvalidOperationException(SR.GetString("POLICY0040", objArray));
			}
			else
			{
				propertyType.comparisonValue = ((StringLiteralExpression)this.Expression).Value;
				return propertyType;
			}
		}

		public static NotEqualsClaimCondition NotEqual(ClaimProperty claimProperty, Expression expression)
		{
			return new NotEqualsClaimCondition(claimProperty, expression);
		}

		public static RegexMatchClaimCondition RegexMatch(ClaimProperty claimProperty, Expression expression)
		{
			return new RegexMatchClaimCondition(claimProperty, expression);
		}

		public static RegexNotMatchClaimCondition RegexNotMatch(ClaimProperty claimProperty, Expression expression)
		{
			return new RegexNotMatchClaimCondition(claimProperty, expression);
		}

		public virtual void Validate()
		{
			Utility.VerifyNonNull("ClaimProperty", this.ClaimProperty);
			Utility.VerifyNonNull("Expression", this.Expression);
			this.ClaimProperty.Validate();
			this.Expression.Validate(this);
		}
	}
}