using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Undo", "Transaction", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135268")]
	public class UndoTransactionCommand : PSCmdlet
	{
		public UndoTransactionCommand()
		{
		}

		protected override void EndProcessing()
		{
			if (base.ShouldProcess(NavigationResources.TransactionResource, NavigationResources.RollbackAction))
			{
				base.Context.TransactionManager.Rollback();
			}
		}
	}
}