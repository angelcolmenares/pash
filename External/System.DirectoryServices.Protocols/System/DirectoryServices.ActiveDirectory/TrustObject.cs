using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class TrustObject
	{
		public string NetbiosDomainName;

		public string DnsDomainName;

		public int Flags;

		public int ParentIndex;

		public TrustType TrustType;

		public int TrustAttributes;

		public int OriginalIndex;

		public TrustObject()
		{
		}
	}
}