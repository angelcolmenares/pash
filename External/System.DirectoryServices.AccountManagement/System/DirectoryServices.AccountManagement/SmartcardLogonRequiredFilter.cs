using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class SmartcardLogonRequiredFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			}
		}

		public SmartcardLogonRequiredFilter()
		{
		}
	}
}