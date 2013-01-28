using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExpiredAccountFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfoExpired";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfoExpired";
			}
		}

		public ExpiredAccountFilter()
		{
		}
	}
}