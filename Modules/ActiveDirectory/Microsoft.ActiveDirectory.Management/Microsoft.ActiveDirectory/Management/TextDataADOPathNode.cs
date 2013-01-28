using System;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class TextDataADOPathNode : IADOPathNode, IDataNode
	{
		private bool _encodeAsteriskChar;

		private string _data;

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
				return this.TextValue;
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

		internal string TextValue
		{
			get
			{
				return this._data;
			}
			set
			{
				this._data = value;
			}
		}

		internal TextDataADOPathNode(string data) : this(data, false)
		{
		}

		internal TextDataADOPathNode(string data, bool encodeAsteriskChar)
		{
			this._data = data;
			this._encodeAsteriskChar = encodeAsteriskChar;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			return ADOPathUtil.LdapSearchEncodeString(this._data, this._encodeAsteriskChar);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("'");
			stringBuilder.Append(this._data).Append("'");
			return stringBuilder.ToString();
		}
	}
}