using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class POLICY_ACCOUNT_DOMAIN_INFO
	{
		public LSA_UNICODE_STRING domainName;

		public IntPtr domainSid;

		public POLICY_ACCOUNT_DOMAIN_INFO()
		{
			this.domainName = new LSA_UNICODE_STRING();
			this.domainSid = IntPtr.Zero;
		}
	}
}