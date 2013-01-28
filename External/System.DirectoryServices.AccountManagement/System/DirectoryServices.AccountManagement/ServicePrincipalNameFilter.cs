using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ServicePrincipalNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "ComputerPrincipal.ServicePrincipalNames";

		public override string PropertyName
		{
			get
			{
				return "ComputerPrincipal.ServicePrincipalNames";
			}
		}

		public ServicePrincipalNameFilter()
		{
		}
	}
}