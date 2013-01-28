using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class EmailAddressFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.EmailAddress";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.EmailAddress";
			}
		}

		public EmailAddressFilter()
		{
		}
	}
}