using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADAddRequest : AddRequest
	{
		public ADAddRequest()
		{
		}

		public ADAddRequest(string distinguishedName, DirectoryAttribute[] attributes) : base(distinguishedName, attributes)
		{
		}

		public ADAddRequest(string distinguishedName, string objectClass) : base(distinguishedName, objectClass)
		{
		}
	}
}