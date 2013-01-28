using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class SpecialExtendedConstants
	{
		internal const long AccountHasNoExpirationDate = 0x89f7ff5f7b58000L;

		internal const long PasswordPolicyHasNoExpiration = 0L;

		internal const int SupportDeviceAuthorizationBit = 0x20000;

	}
}