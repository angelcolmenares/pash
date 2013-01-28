using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Clear", "Content", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113282")]
	public class ClearContentCommand : ContentCommandBase
	{
		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return base.DoesProviderSupportShouldProcess(base.Path);
			}
		}

		public ClearContentCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (base.Path == null || (int)base.Path.Length <= 0)
			{
				return base.InvokeProvider.Content.ClearContentDynamicParameters(".", context);
			}
			else
			{
				return base.InvokeProvider.Content.ClearContentDynamicParameters(base.Path[0], context);
			}
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = false;
			string[] path = base.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				try
				{
					base.InvokeProvider.Content.Clear(str, cmdletProviderContext);
				}
				catch (PSNotSupportedException pSNotSupportedException1)
				{
					PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
					base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
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
				catch (ItemNotFoundException itemNotFoundException1)
				{
					ItemNotFoundException itemNotFoundException = itemNotFoundException1;
					base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
				}
			}
		}
	}
}