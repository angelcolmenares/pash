using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class PropertyADOPathNode : IADOPathNode, IDataNode
	{
		private string _propertyName;

		object Microsoft.ActiveDirectory.Management.IDataNode.DataObject
		{
			get
			{
				return this._propertyName;
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

		internal string PropertyName
		{
			get
			{
				return this._propertyName;
			}
			set
			{
				this._propertyName = value;
			}
		}

		internal PropertyADOPathNode(string propertyName)
		{
			this._propertyName = propertyName;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			return this._propertyName;
		}

		public override string ToString()
		{
			return this._propertyName;
		}
	}
}