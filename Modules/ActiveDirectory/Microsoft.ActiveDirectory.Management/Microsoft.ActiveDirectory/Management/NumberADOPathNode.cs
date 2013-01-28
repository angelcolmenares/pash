using System;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	internal class NumberADOPathNode : IADOPathNode, IDataNode
	{
		private string _data;

		private long _numValue;

		internal double DoubleValue
		{
			get
			{
				return (double)this._numValue;
			}
		}

		object Microsoft.ActiveDirectory.Management.IDataNode.DataObject
		{
			get
			{
				return this._numValue;
			}
		}

		bool? Microsoft.ActiveDirectory.Management.IDataNode.EncodeAsteriskChar
		{
			get
			{
				bool? nullable = null;
				return nullable;
			}
			set
			{
			}
		}

		internal NumberADOPathNode(string data)
		{
			this._data = data;
			if (!this._data.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!long.TryParse(data, out this._numValue))
				{
					this._numValue = (long)double.Parse(data);
				}
				return;
			}
			else
			{
				this._numValue = long.Parse(data.Substring(2), NumberStyles.HexNumber);
				return;
			}
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			return this._numValue.ToString("G", CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			return this._data;
		}
	}
}