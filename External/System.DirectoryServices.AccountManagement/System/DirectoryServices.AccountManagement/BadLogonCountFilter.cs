using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class BadLogonCountFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.BadLogonCount";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.BadLogonCount";
			}
		}

		public BadLogonCountFilter()
		{
		}
	}
}