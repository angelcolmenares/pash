using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class HomeDriveFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.HomeDrive";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.HomeDrive";
			}
		}

		public HomeDriveFilter()
		{
		}
	}
}