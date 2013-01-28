using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADModifyResponse : ADResponse
	{
		public ADModifyResponse(string dn, DirectoryControl[] controls, ResultCode result, string message) : base(dn, controls, result, message, null)
		{
		}

		public ADModifyResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}