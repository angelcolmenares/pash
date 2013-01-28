using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class MiddleNameFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.MiddleName";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.MiddleName";
			}
		}

		public MiddleNameFilter()
		{
		}
	}
}