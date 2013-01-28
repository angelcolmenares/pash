using System;

namespace Microsoft.PowerShell.Commands
{
	internal static class DownLevelHelper
	{
		internal static bool IsWin8AndAbove()
		{
			bool flag = false;
			OperatingSystem oSVersion = Environment.OSVersion;
			PlatformID platform = oSVersion.Platform;
			Version version = oSVersion.Version;
			if (platform.Equals(PlatformID.Win32NT) && (version.Major > 6 || version.Major == 6 && version.Minor >= 2))
			{
				flag = true;
			}
			return flag;
		}
	}
}