using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExpirationDateFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			}
		}

		public ExpirationDateFilter()
		{
		}
	}
}