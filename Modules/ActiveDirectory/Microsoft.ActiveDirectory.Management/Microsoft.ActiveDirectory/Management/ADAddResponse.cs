using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADAddResponse : ADResponse
	{
		public ADAddResponse(string dn, DirectoryControl[] controls, ResultCode result, string message) : base(dn, controls, result, message, null)
		{
		}

		public ADAddResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}