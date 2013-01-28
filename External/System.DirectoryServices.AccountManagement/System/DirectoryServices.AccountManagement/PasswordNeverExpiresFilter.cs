using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PasswordNeverExpiresFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			}
		}

		public PasswordNeverExpiresFilter()
		{
		}
	}
}