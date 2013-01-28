using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class SpecialDirectoryConstants
	{
		internal const long AccountHasNoExpirationDate = 0x7fffffffffffffffL;

		internal const int InstanceTypeIsNamingContext = 1;

		internal const long PasswordPolicyHasNoExpiration = -9223372036854775808L;

	}
}