using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class LastLogonTimeFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.LastLogon";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.LastLogon";
			}
		}

		public LastLogonTimeFilter()
		{
		}
	}
}