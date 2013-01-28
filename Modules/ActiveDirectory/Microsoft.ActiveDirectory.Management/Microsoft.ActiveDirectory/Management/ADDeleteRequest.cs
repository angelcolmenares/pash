using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADDeleteRequest : DeleteRequest
	{
		public ADDeleteRequest()
		{
		}

		public ADDeleteRequest(string distinguishedName) : base(distinguishedName)
		{
		}

		public ADDeleteRequest(string distinguishedName, bool deleteDeleted) : base(distinguishedName)
		{
			if (deleteDeleted)
			{
				base.Controls.Add(new ShowDeletedControl());
			}
		}
	}
}