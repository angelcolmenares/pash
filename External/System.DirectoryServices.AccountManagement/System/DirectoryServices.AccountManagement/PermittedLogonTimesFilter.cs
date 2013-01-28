using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PermittedLogonTimesFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			}
		}

		public PermittedLogonTimesFilter()
		{
		}
	}
}