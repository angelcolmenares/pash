using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Pop", "Location", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113369")]
	public class PopLocationCommand : CoreCommandBase
	{
		private bool passThrough;

		private string stackName;

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThrough;
			}
			set
			{
				this.passThrough = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string StackName
		{
			get
			{
				return this.stackName;
			}
			set
			{
				this.stackName = value;
			}
		}

		public PopLocationCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				PathInfo pathInfo = base.SessionState.Path.PopLocation(this.stackName);
				if (this.PassThru)
				{
					base.WriteObject(pathInfo);
				}
			}
			catch (DriveNotFoundException driveNotFoundException1)
			{
				DriveNotFoundException driveNotFoundException = driveNotFoundException1;
				base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
			}
			catch (ProviderNotFoundException providerNotFoundException1)
			{
				ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
				base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
			}
			catch (PSArgumentException pSArgumentException1)
			{
				PSArgumentException pSArgumentException = pSArgumentException1;
				base.WriteError(new ErrorRecord(pSArgumentException.ErrorRecord, pSArgumentException));
			}
			catch (ItemNotFoundException itemNotFoundException1)
			{
				ItemNotFoundException itemNotFoundException = itemNotFoundException1;
				base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
			}
		}
	}
}