using System;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class BinaryADOPathNode : IADOPathNode
	{
		private ADOperator _operator;

		private IADOPathNode _leftExpr;

		private IADOPathNode _rightExpr;

		internal IADOPathNode LeftNode
		{
			get
			{
				return this._leftExpr;
			}
		}

		internal ADOperator Operator
		{
			get
			{
				return this._operator;
			}
		}

		internal IADOPathNode RightNode
		{
			get
			{
				return this._rightExpr;
			}
		}

		internal BinaryADOPathNode(ADOperator op, IADOPathNode leftExpr, IADOPathNode rightExpr)
		{
			this._operator = op;
			this._leftExpr = leftExpr;
			this._rightExpr = rightExpr;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			ADOperator aDOperator;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			bool flag1 = false;
			if (this._operator == ADOperator.Gt || this._operator == ADOperator.Lt || this._operator == ADOperator.Ne || this._operator == ADOperator.NotLike)
			{
				flag = true;
				flag1 = true;
				ADOperator aDOperator1 = this._operator;
				if (aDOperator1 == ADOperator.Lt)
				{
					aDOperator = ADOperator.Ge;
					stringBuilder.Append("(");
					if (flag)
					{
						stringBuilder.Append("!");
					}
					stringBuilder.Append(this._leftExpr.GetLdapFilterString());
					stringBuilder.Append(ADOPathUtil.GetLdapFilterString(aDOperator));
					stringBuilder.Append(this._rightExpr.GetLdapFilterString());
					stringBuilder.Append(")");
					if (flag1)
					{
						stringBuilder.Insert(0, "(&");
						stringBuilder.Append("(");
						stringBuilder.Append(this._leftExpr.GetLdapFilterString());
						stringBuilder.Append("=*))");
					}
					return stringBuilder.ToString();
				}
				else if (aDOperator1 == ADOperator.Gt)
				{
					aDOperator = ADOperator.Le;
					stringBuilder.Append("(");
					if (flag)
					{
						stringBuilder.Append("!");
					}
					stringBuilder.Append(this._leftExpr.GetLdapFilterString());
					stringBuilder.Append(ADOPathUtil.GetLdapFilterString(aDOperator));
					stringBuilder.Append(this._rightExpr.GetLdapFilterString());
					stringBuilder.Append(")");
					if (flag1)
					{
						stringBuilder.Insert(0, "(&");
						stringBuilder.Append("(");
						stringBuilder.Append(this._leftExpr.GetLdapFilterString());
						stringBuilder.Append("=*))");
					}
					return stringBuilder.ToString();
				}
				else if (aDOperator1 == ADOperator.Approx || aDOperator1 == ADOperator.RecursiveMatch)
				{
					throw new InvalidOperationException("Code flow should never come here");
				}
				else if (aDOperator1 == ADOperator.Ne)
				{
					aDOperator = ADOperator.Eq;
					stringBuilder.Append("(");
					if (flag)
					{
						stringBuilder.Append("!");
					}
					stringBuilder.Append(this._leftExpr.GetLdapFilterString());
					stringBuilder.Append(ADOPathUtil.GetLdapFilterString(aDOperator));
					stringBuilder.Append(this._rightExpr.GetLdapFilterString());
					stringBuilder.Append(")");
					if (flag1)
					{
						stringBuilder.Insert(0, "(&");
						stringBuilder.Append("(");
						stringBuilder.Append(this._leftExpr.GetLdapFilterString());
						stringBuilder.Append("=*))");
					}
					return stringBuilder.ToString();
				}
				if (aDOperator1 != ADOperator.NotLike)
				{
					throw new InvalidOperationException("Code flow should never come here");
				}
				aDOperator = ADOperator.Eq;
				string ldapFilterString = this._rightExpr.GetLdapFilterString();
				if (ADOPathUtil.IsValueAllAsterisk(ldapFilterString))
				{
					flag1 = false;
				}
			}
			else
			{
				aDOperator = this._operator;
			}
			stringBuilder.Append("(");
			if (flag)
			{
				stringBuilder.Append("!");
			}
			stringBuilder.Append(this._leftExpr.GetLdapFilterString());
			stringBuilder.Append(ADOPathUtil.GetLdapFilterString(aDOperator));
			stringBuilder.Append(this._rightExpr.GetLdapFilterString());
			stringBuilder.Append(")");
			if (flag1)
			{
				stringBuilder.Insert(0, "(&");
				stringBuilder.Append("(");
				stringBuilder.Append(this._leftExpr.GetLdapFilterString());
				stringBuilder.Append("=*))");
			}
			return stringBuilder.ToString();
			throw new InvalidOperationException("Code flow should never come here");
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			stringBuilder.Append(this._leftExpr.GetLdapFilterString());
			stringBuilder.Append(" ");
			stringBuilder.Append("-").Append(this._operator.ToString().ToLowerInvariant());
			stringBuilder.Append(" ");
			stringBuilder.Append(this._rightExpr.GetLdapFilterString());
			stringBuilder.Append(" ");
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}