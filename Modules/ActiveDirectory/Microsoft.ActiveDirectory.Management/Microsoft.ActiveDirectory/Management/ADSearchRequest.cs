using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSearchRequest : SearchRequest
	{
		private bool _objectScopedControls;

		public bool ObjectScopedControls
		{
			get
			{
				return this._objectScopedControls;
			}
			set
			{
				this._objectScopedControls = value;
			}
		}

		public ADSearchRequest()
		{
		}

		public ADSearchRequest(string distinguishedName, string ldapFilter, SearchScope searchScope, string[] attributeList) : base(distinguishedName, ldapFilter, searchScope, attributeList)
		{
		}
	}
}