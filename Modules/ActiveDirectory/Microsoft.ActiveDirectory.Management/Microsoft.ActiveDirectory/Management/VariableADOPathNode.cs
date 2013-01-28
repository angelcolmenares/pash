using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class VariableADOPathNode : IADOPathNode, IDataNode
	{
		private string _varStr;

		private object _varValue;

		private bool _encodeAsteriskChar;

		internal bool EncodeAsteriskChar
		{
			get
			{
				return this._encodeAsteriskChar;
			}
			set
			{
				this._encodeAsteriskChar = value;
			}
		}

		object Microsoft.ActiveDirectory.Management.IDataNode.DataObject
		{
			get
			{
				return this._varValue;
			}
		}

		bool? Microsoft.ActiveDirectory.Management.IDataNode.EncodeAsteriskChar
		{
			get
			{
				return new bool?(this._encodeAsteriskChar);
			}
			set
			{
				if (value.HasValue && value.HasValue)
				{
					this._encodeAsteriskChar = value.Value;
				}
			}
		}

		internal string VariableExpression
		{
			get
			{
				return this._varStr;
			}
		}

		internal VariableADOPathNode(string varNameStr, EvaluateVariableDelegate variableConverterDelegate) : this(varNameStr, variableConverterDelegate, false)
		{
		}

		internal VariableADOPathNode(string varNameStr, EvaluateVariableDelegate variableConverterDelegate, bool encodeAsteriskChar)
		{
			this._varStr = varNameStr;
			this._varValue = variableConverterDelegate(this._varStr);
			this._encodeAsteriskChar = encodeAsteriskChar;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			if (this._varValue == null)
			{
				return "";
			}
			else
			{
				return ADOPathUtil.LdapSearchEncodeObject(this._varValue, this._encodeAsteriskChar);
			}
		}

		public override string ToString()
		{
			return this._varStr;
		}
	}
}