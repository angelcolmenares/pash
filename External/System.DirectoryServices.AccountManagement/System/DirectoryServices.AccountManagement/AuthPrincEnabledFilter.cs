using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class AuthPrincEnabledFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.Enabled";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.Enabled";
			}
		}

		public AuthPrincEnabledFilter()
		{
		}
	}
}