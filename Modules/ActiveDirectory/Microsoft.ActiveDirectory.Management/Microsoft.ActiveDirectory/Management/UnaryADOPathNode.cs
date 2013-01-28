using System;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class UnaryADOPathNode : IADOPathNode
	{
		private ADOperator _operator;

		private IADOPathNode _childExpr;

		internal IADOPathNode ChildNode
		{
			get
			{
				return this._childExpr;
			}
		}

		internal ADOperator Operator
		{
			get
			{
				return this._operator;
			}
		}

		internal UnaryADOPathNode(ADOperator op, IADOPathNode expr)
		{
			if (expr != null)
			{
				this._operator = op;
				this._childExpr = expr;
				return;
			}
			else
			{
				throw new ArgumentNullException("expr");
			}
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			stringBuilder.Append(ADOPathUtil.GetLdapFilterString(this._operator));
			string ldapFilterString = this._childExpr.GetLdapFilterString();
			if (this._childExpr as UnaryADOPathNode != null)
			{
				UnaryADOPathNode unaryADOPathNode = (UnaryADOPathNode)this._childExpr;
				if (unaryADOPathNode._operator == ADOperator.Not && this._operator == ADOperator.Not)
				{
					return unaryADOPathNode._childExpr.GetLdapFilterString();
				}
			}
			stringBuilder.Append(ldapFilterString);
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			stringBuilder.Append("-").Append(this._operator.ToString().ToLowerInvariant());
			stringBuilder.Append(" ");
			stringBuilder.Append(this._childExpr.GetLdapFilterString());
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}