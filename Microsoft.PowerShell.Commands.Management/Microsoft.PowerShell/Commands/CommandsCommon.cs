using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	internal static class CommandsCommon
	{
		private static bool alreadyFailing;

		private static bool designForTestability_SkipFailFast;

		static CommandsCommon()
		{
		}

		internal static void CheckForSevereException(Cmdlet cmdlet, Exception e)
		{
			if (e as AccessViolationException != null || e as StackOverflowException != null)
			{
				try
				{
					if (!CommandsCommon.alreadyFailing)
					{
						CommandsCommon.alreadyFailing = true;
						MshLog.LogCommandHealthEvent(cmdlet.Context, e, Severity.Critical);
					}
				}
				finally
				{
					if (!CommandsCommon.designForTestability_SkipFailFast)
					{
						WindowsErrorReporting.FailFast(e);
					}
				}
			}
		}
	}
}