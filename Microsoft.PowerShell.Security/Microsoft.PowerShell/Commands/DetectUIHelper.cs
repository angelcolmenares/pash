using System;
using System.Diagnostics;
using System.Management.Automation.Host;
using System.Management.Automation.Security;

namespace Microsoft.PowerShell.Commands
{
	internal static class DetectUIHelper
	{
		private static IntPtr hWnd;

		private static bool firstRun;

		static DetectUIHelper()
		{
			DetectUIHelper.hWnd = IntPtr.Zero;
			DetectUIHelper.firstRun = true;
		}

		internal static IntPtr GetOwnerWindow(PSHost host)
		{
			if (DetectUIHelper.firstRun)
			{
				DetectUIHelper.firstRun = false;
				if (DetectUIHelper.IsUIAllowed(host))
				{
					DetectUIHelper.hWnd = Process.GetCurrentProcess().MainWindowHandle;
					if (DetectUIHelper.hWnd == IntPtr.Zero)
					{
						DetectUIHelper.hWnd = NativeMethods.GetConsoleWindow();
					}
					if (DetectUIHelper.hWnd == IntPtr.Zero)
					{
						DetectUIHelper.hWnd = NativeMethods.GetDesktopWindow();
					}
				}
			}
			return DetectUIHelper.hWnd;
		}

		internal static bool IsRemoteCommand(PSHost host)
		{
			return host.Name.Equals("ServerRemoteHost", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsUIAllowed(PSHost host)
		{
			uint num = 0;
			if (!DetectUIHelper.IsRemoteCommand(host))
			{
				int id = Process.GetCurrentProcess().Id;
				if (NativeMethods.ProcessIdToSessionId(id, out num))
				{
					if (num != 0)
					{
						if (Environment.UserInteractive)
						{
							string[] commandLineArgs = Environment.GetCommandLineArgs();
							bool flag = true;
							string[] strArrays = commandLineArgs;
							int num1 = 0;
							while (num1 < (int)strArrays.Length)
							{
								string str = strArrays[num1];
								if (str.Length < 4 || !"-noninteractive".StartsWith(str, StringComparison.OrdinalIgnoreCase))
								{
									num1++;
								}
								else
								{
									flag = false;
									break;
								}
							}
							return flag;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}
}