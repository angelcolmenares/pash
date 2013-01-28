using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class HomeDirectoryFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.HomeDirectory";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			}
		}

		public HomeDirectoryFilter()
		{
		}
	}
}