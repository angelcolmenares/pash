using System;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ObjectADOPathNode : IADOPathNode, IDataNode
	{
		private object _data;

		private bool _encodeAsteriskChar;

		internal object DataObject
		{
			set
			{
				this._data = value;
			}
		}

		object Microsoft.ActiveDirectory.Management.IDataNode.DataObject
		{
			get
			{
				return this._data;
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

		public bool? EncodeAsteriskChar
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

		internal ObjectADOPathNode(object data) : this(data, false)
		{
		}

		internal ObjectADOPathNode(object data, bool encodeAsteriskChar)
		{
			this._data = data;
			this._encodeAsteriskChar = encodeAsteriskChar;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			if (this._data == null)
			{
				return "";
			}
			else
			{
				return ADOPathUtil.LdapSearchEncodeObject(this._data, this._encodeAsteriskChar);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("'");
			stringBuilder.Append(this._data).Append("'");
			return stringBuilder.ToString();
		}
	}
}