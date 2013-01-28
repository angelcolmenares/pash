using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class CannotChangePasswordFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			}
		}

		public CannotChangePasswordFilter()
		{
		}
	}
}