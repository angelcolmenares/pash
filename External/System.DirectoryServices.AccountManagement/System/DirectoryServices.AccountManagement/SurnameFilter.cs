using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class SurnameFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.Surname";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.Surname";
			}
		}

		public SurnameFilter()
		{
		}
	}
}