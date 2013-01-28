using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class GivenNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.GivenName";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.GivenName";
			}
		}

		public GivenNameFilter()
		{
		}
	}
}