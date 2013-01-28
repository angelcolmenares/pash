using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADDeleteResponse : ADResponse
	{
		public ADDeleteResponse(string dn, DirectoryControl[] controls, ResultCode result, string message) : base(dn, controls, result, message, null)
		{
		}

		public ADDeleteResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}