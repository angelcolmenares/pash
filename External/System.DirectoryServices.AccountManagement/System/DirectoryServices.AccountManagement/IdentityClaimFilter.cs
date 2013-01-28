using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class IdentityClaimFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.IdentityClaims";

		public override string PropertyName
		{
			get
			{
				return "Principal.IdentityClaims";
			}
		}

		public IdentityClaimFilter()
		{
		}
	}
}