using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADModifyDNRequest : ModifyDNRequest
	{
		public ADModifyDNRequest()
		{
		}

		public ADModifyDNRequest(string distinguishedName, string newParentDistinguishedName, string newName) : base(distinguishedName, newParentDistinguishedName, newName)
		{
		}
	}
}