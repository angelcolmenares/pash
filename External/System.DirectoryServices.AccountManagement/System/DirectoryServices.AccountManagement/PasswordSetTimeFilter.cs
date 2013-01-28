using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PasswordSetTimeFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";
			}
		}

		public PasswordSetTimeFilter()
		{
		}
	}
}