using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class UserPrincipalNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "Principal.UserPrincipalName";

		public override string PropertyName
		{
			get
			{
				return "Principal.UserPrincipalName";
			}
		}

		public UserPrincipalNameFilter()
		{
		}
	}
}