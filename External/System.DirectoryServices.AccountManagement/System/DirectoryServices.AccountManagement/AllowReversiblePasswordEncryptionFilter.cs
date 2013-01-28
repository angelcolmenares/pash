using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class AllowReversiblePasswordEncryptionFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			}
		}

		public AllowReversiblePasswordEncryptionFilter()
		{
		}
	}
}