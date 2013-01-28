using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class BadPasswordAttemptFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";
			}
		}

		public BadPasswordAttemptFilter()
		{
		}
	}
}