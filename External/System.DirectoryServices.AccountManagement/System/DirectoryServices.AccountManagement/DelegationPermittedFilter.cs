using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class DelegationPermittedFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			}
		}

		public DelegationPermittedFilter()
		{
		}
	}
}