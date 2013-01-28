using System;
using System.Management.Automation.Internal.Host;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Quit", "Shell")]
	public class QuitShellCommand : PSCmdlet
	{

		protected override void BeginProcessing ()
		{
			var shouldExit = true;
			InternalHost host = base.Host as InternalHost;
			if (host != null) {
				if (host.Runspace.InNestedPrompt)
				{
					host.ExitNestedPrompt ();
					shouldExit = host.ShouldExit;
				}
				if (shouldExit)
				{
					base.WriteVerbose ("\r\n");
					System.Management.Automation.Sqm.PSSQMAPI.LogAllDataSuppressExceptions();
					Environment.Exit (0);
				}
			}
			else if (base.Context.IsSingleShell) {
				base.WriteVerbose ("\r\n\r\nExiting PowerShell...\r\n");
				Environment.Exit (0);
			}

			//base.BeginProcessing ();
		}
	}
}

