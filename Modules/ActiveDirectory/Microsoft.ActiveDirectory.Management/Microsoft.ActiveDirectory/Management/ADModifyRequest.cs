using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADModifyRequest : ModifyRequest
	{
		public ADModifyRequest()
		{
		}

		public ADModifyRequest(string distinguishedName, DirectoryAttributeModification[] modifications) : base(distinguishedName, modifications)
		{
		}

		public ADModifyRequest(string distinguishedName, DirectoryAttributeOperation operation, string attributeName, object[] values) : base(distinguishedName, operation, attributeName, values)
		{
		}
	}
}