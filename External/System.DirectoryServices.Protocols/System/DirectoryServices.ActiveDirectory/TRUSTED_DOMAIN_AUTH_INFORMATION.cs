using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class TRUSTED_DOMAIN_AUTH_INFORMATION
	{
		public int IncomingAuthInfos;

		public IntPtr IncomingAuthenticationInformation;

		public IntPtr IncomingPreviousAuthenticationInformation;

		public int OutgoingAuthInfos;

		public IntPtr OutgoingAuthenticationInformation;

		public IntPtr OutgoingPreviousAuthenticationInformation;

		public TRUSTED_DOMAIN_AUTH_INFORMATION()
		{
		}
	}
}