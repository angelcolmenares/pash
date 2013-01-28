using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Complete", "Transaction", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135200")]
	public class CompleteTransactionCommand : PSCmdlet
	{
		public CompleteTransactionCommand()
		{
		}

		protected override void EndProcessing()
		{
			if (base.ShouldProcess(NavigationResources.TransactionResource, NavigationResources.CommitAction))
			{
				base.Context.TransactionManager.Commit();
			}
		}
	}
}