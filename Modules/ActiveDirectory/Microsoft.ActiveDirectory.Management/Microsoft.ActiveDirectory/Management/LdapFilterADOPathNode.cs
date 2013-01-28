using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class LdapFilterADOPathNode : IADOPathNode, IDataNode
	{
		private string _ldapFilterString;

		internal string LdapFilterString
		{
			get
			{
				return this._ldapFilterString;
			}
			set
			{
				this._ldapFilterString = value;
			}
		}

		object Microsoft.ActiveDirectory.Management.IDataNode.DataObject
		{
			get
			{
				return this._ldapFilterString;
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

		internal LdapFilterADOPathNode(string ldapFilterString)
		{
			this._ldapFilterString = ldapFilterString;
		}

		string Microsoft.ActiveDirectory.Management.IADOPathNode.GetLdapFilterString()
		{
			return this._ldapFilterString;
		}

		public override string ToString()
		{
			return this._ldapFilterString;
		}
	}
}