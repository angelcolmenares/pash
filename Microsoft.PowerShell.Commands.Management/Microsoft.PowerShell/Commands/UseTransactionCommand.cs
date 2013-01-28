using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Use", "Transaction", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135271")]
	public class UseTransactionCommand : PSCmdlet
	{
		private ScriptBlock transactedScript;

		[Parameter(Position=0, Mandatory=true)]
		public ScriptBlock TransactedScript
		{
			get
			{
				return this.transactedScript;
			}
			set
			{
				this.transactedScript = value;
			}
		}

		public UseTransactionCommand()
		{
		}

		protected override void EndProcessing()
		{
			using (base.CurrentPSTransaction)
			{
				try
				{
					this.transactedScript.InvokeUsingCmdlet(this, false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, null, new object[0], AutomationNull.Value, new object[0]);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					ErrorRecord errorRecord = new ErrorRecord(exception, "TRANSACTED_SCRIPT_EXCEPTION", ErrorCategory.NotSpecified, null);
					bool flag = false;
					Exception innerException = exception;
					while (innerException != null)
					{
						if (innerException as TimeoutException == null)
						{
							innerException = innerException.InnerException;
						}
						else
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						errorRecord = new ErrorRecord(new InvalidOperationException(TransactionResources.TransactionTimedOut), "TRANSACTION_TIMEOUT", ErrorCategory.InvalidOperation, exception);
					}
					base.WriteError(errorRecord);
				}
			}
		}
	}
}