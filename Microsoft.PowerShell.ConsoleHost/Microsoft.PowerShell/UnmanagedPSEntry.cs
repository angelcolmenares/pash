using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell
{
	public sealed class UnmanagedPSEntry
	{
		public UnmanagedPSEntry()
		{
		}

		public int Start(string consoleFilePath, string[] args)
		{
			int num;
			string message;
			Guid activityId = EtwActivity.GetActivityId();
			if (activityId == Guid.Empty)
			{
				EtwActivity.SetActivityId(EtwActivity.CreateActivityId());
			}
			PSEtwLog.LogOperationalInformation(PSEventId.Perftrack_ConsoleStartupStart, PSOpcode.WinStart, PSTask.PowershellConsoleStartup, PSKeyword.UseAlwaysOperational, new object[0]);
			WindowsErrorReporting.RegisterWindowsErrorReporting(false);
			try
			{
				Thread.CurrentThread.CurrentUICulture = NativeCultureResolver.UICulture;
				Thread.CurrentThread.CurrentCulture = NativeCultureResolver.Culture;
				RunspaceConfigForSingleShell runspaceConfigForSingleShell = null;
				PSConsoleLoadException pSConsoleLoadException = null;
				if (!string.IsNullOrEmpty(consoleFilePath))
				{
					runspaceConfigForSingleShell = RunspaceConfigForSingleShell.Create(consoleFilePath, out pSConsoleLoadException);
				}
				else
				{
					ConsoleHost.DefaultInitialSessionState = InitialSessionState.CreateDefault2();
					runspaceConfigForSingleShell = null;
					if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
					{
						ConsoleHost.DefaultInitialSessionState.WarmUpTabCompletionOnIdle = true;
					}
				}
				int num1 = 0;
				try
				{
					RunspaceConfigForSingleShell runspaceConfigForSingleShell1 = runspaceConfigForSingleShell;
					string shellBanner = ManagedEntranceStrings.ShellBanner;
					string shellHelp = ManagedEntranceStrings.ShellHelp;
					if (pSConsoleLoadException == null)
					{
						message = null;
					}
					else
					{
						message = pSConsoleLoadException.Message;
					}
					num1 = ConsoleShell.Start(runspaceConfigForSingleShell1, shellBanner, shellHelp, message, args);
				}
				catch (HostException hostException1)
				{
					HostException hostException = hostException1;
					if (hostException.InnerException != null && hostException.InnerException.GetType() == typeof(Win32Exception))
					{
						Win32Exception innerException = hostException.InnerException as Win32Exception;
						if (innerException.NativeErrorCode == 6 || innerException.NativeErrorCode == 0x4d4)
						{
							num = num1;
							return num;
						}
					}
					WindowsErrorReporting.FailFast(hostException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					WindowsErrorReporting.FailFast(exception);
				}
				num = num1;
			}
			finally
			{
				WindowsErrorReporting.WaitForPendingReports();
			}
			return num;
		}
	}
}