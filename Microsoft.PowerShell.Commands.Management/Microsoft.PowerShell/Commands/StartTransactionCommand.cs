using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Start", "Transaction", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135262")]
	public class StartTransactionCommand : PSCmdlet
	{
		private bool timeoutSpecified;

		private TimeSpan timeout;

		private SwitchParameter independent;

		private RollbackSeverity rollbackPreference;

		[Parameter]
		public SwitchParameter Independent
		{
			get
			{
				return this.independent;
			}
			set
			{
				this.independent = value;
			}
		}

		[Parameter]
		public RollbackSeverity RollbackPreference
		{
			get
			{
				return this.rollbackPreference;
			}
			set
			{
				this.rollbackPreference = value;
			}
		}

		[Alias(new string[] { "TimeoutMins" })]
		[Parameter]
		public int Timeout
		{
			get
			{
				return (int)this.timeout.TotalMinutes;
			}
			set
			{
				if (value != 0)
				{
					this.timeout = TimeSpan.FromMinutes((double)value);
				}
				else
				{
					this.timeout = TimeSpan.FromTicks((long)1);
				}
				this.timeoutSpecified = true;
			}
		}

		public StartTransactionCommand()
		{
			this.timeout = TimeSpan.MinValue;
		}

		protected override void EndProcessing()
		{
			if (base.ShouldProcess(NavigationResources.TransactionResource, NavigationResources.CreateAction))
			{
				if (!this.timeoutSpecified)
				{
					if (base.MyInvocation.CommandOrigin != CommandOrigin.Runspace)
					{
						this.timeout = TimeSpan.FromMinutes(30);
					}
					else
					{
						this.timeout = TimeSpan.MaxValue;
					}
				}
				if (!this.independent)
				{
					base.Context.TransactionManager.CreateOrJoin(this.rollbackPreference, this.timeout);
				}
				else
				{
					base.Context.TransactionManager.CreateNew(this.rollbackPreference, this.timeout);
					return;
				}
			}
		}
	}
}