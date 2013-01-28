using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DS_DOMAIN_TRUSTS
	{
		public IntPtr NetbiosDomainName;

		public IntPtr DnsDomainName;

		public int Flags;

		public int ParentIndex;

		public int TrustType;

		public int TrustAttributes;

		public IntPtr DomainSid;

		public Guid DomainGuid;

		public DS_DOMAIN_TRUSTS()
		{
		}
	}
}