using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Transaction", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135220")]
	[OutputType(new Type[] { typeof(PSTransaction) })]
	public class GetTransactionCommand : PSCmdlet
	{
		public GetTransactionCommand()
		{
		}

		protected override void EndProcessing()
		{
			base.WriteObject(base.Context.TransactionManager.GetCurrent());
		}
	}
}