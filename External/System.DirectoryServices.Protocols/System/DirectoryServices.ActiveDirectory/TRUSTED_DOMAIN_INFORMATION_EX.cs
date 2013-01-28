using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class TRUSTED_DOMAIN_INFORMATION_EX
	{
		public LSA_UNICODE_STRING Name;

		public LSA_UNICODE_STRING FlatName;

		public IntPtr Sid;

		public int TrustDirection;

		public int TrustType;

		public TRUST_ATTRIBUTE TrustAttributes;

		public TRUSTED_DOMAIN_INFORMATION_EX()
		{
		}
	}
}