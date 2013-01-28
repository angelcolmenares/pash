using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class VoiceTelephoneNumberFilter : FilterBase
	{
		public const string PropertyNameStatic = "UserPrincipal.VoiceTelephoneNumber";

		public override string PropertyName
		{
			get
			{
				return "UserPrincipal.VoiceTelephoneNumber";
			}
		}

		public VoiceTelephoneNumberFilter()
		{
		}
	}
}