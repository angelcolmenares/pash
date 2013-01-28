using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PermittedWorkstationFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			}
		}

		public PermittedWorkstationFilter()
		{
		}
	}
}