using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell
{
	public static class ConsoleShell
	{
		public static int Start(RunspaceConfiguration configuration, string bannerText, string helpText, string[] args)
		{
			return ConsoleShell.Start(configuration, bannerText, helpText, null, args);
		}

		internal static int Start(RunspaceConfiguration configuration, string bannerText, string helpText, string preStartWarning, string[] args)
		{
			if (args != null)
			{
				ConsoleControl.UpdateLocaleSpecificFont();
				return ConsoleHost.Start(configuration, bannerText, helpText, preStartWarning, args);
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("args");
			}
		}
	}
}