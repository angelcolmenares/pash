using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class EmployeeIDFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.EmployeeId";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.EmployeeId";
			}
		}

		public EmployeeIDFilter()
		{
		}
	}
}