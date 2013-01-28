using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PasswordNotRequiredFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			}
		}

		public PasswordNotRequiredFilter()
		{
		}
	}
}