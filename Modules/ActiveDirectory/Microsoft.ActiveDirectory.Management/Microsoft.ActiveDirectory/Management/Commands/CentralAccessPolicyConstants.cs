using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class CentralAccessPolicyConstants
	{
		internal static int MaximumCARsPerCap;

		static CentralAccessPolicyConstants()
		{
			CentralAccessPolicyConstants.MaximumCARsPerCap = 127;
		}
	}
}