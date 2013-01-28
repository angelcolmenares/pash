using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class CompositeADOPathNode : IADOPathNode
	{
		private ADOperator _operator;

		private IList<IADOPathNode> _childNodes;

		internal IList<IADOPathNode> ChildNodes
		{
			get
			{
				return this._childNodes;
			}
		}

		internal ADOperator Operator
		{
			get
			{
				return this._operator;
			}
		}

		internal CompositeADOPathNode(ADOperator op, IADOPathNode[] exprList)
		{
			if (exprList != null)
			{
				if ((int)exprList.Length >= 2)
				{
					this._operator = op;
					this._childNodes = new List<IADOPathNode>();
					IADOPathNode[] aDOPathNodeArray = exprList;
					for (int i = 0; i < (int)aDOPathNodeArray.Length; i++)
					{
						IADOPathNode aDOPathNode = aDOPathNodeArray[i];
						this.AddExpressionToChildNodes(aDOPathNode);
					}
					return;
				}
				else
				{
					throw new ArgumentException(StringResources.ADFilterExprListLessThanTwo);
				}
			}
			else
			{
				throw new ArgumentNullException("exprList");
			}
		}

		private void AddExpressionToChildNodes(IADOPathNode expr)
		{
			CompositeADOPathNode compositeADOPathNode = expr as CompositeADOPathNode;
			if (compositeADOPathNode == null || compositeADOPathNode._operator != this._operator)
			{
				this._childNodes.Add(expr);
			}
			else
			{
				foreach (IADOPathNode _childNode in compositeADOPathNode._childNodes)
				{
					this._childNodes.Add(_childNode);
				}
			}
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			stringBuilder.Append(ADOPathUtil.GetLdapFilterString(this._operator));
			foreach (IADOPathNode _childNode in this._childNodes)
			{
				stringBuilder.Append(_childNode.GetLdapFilterString());
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			stringBuilder.Append(this._childNodes[0].GetLdapFilterString());
			stringBuilder.Append(" ");
			for (int i = 1; i < this._childNodes.Count; i++)
			{
				stringBuilder.Append("-").Append(this._operator.ToString().ToLowerInvariant());
				stringBuilder.Append(" ");
				stringBuilder.Append(this._childNodes[i].GetLdapFilterString());
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}