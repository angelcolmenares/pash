using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class WMIConstants
	{
		internal readonly static string Domain;

		internal readonly static string ComputerSystem;

		static WMIConstants()
		{
			WMIConstants.Domain = "domain";
			WMIConstants.ComputerSystem = "Win32_ComputerSystem";
		}
	}
}