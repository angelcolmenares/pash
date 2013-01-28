using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class LockoutTimeFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";
			}
		}

		public LockoutTimeFilter()
		{
		}
	}
}