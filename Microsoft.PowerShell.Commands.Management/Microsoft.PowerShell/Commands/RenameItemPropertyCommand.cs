using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Rename", "ItemProperty", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113383")]
	public class RenameItemPropertyCommand : PassThroughItemPropertyCommandBase
	{
		private string path;

		private string property;

		private string newName;

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.path = value;
			}
		}

		[Alias(new string[] { "PSProperty" })]
		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
		public string Name
		{
			get
			{
				return this.property;
			}
			set
			{
				this.property = value;
			}
		}

		[Parameter(Mandatory=true, Position=2, ValueFromPipelineByPropertyName=true)]
		public string NewName
		{
			get
			{
				return this.newName;
			}
			set
			{
				this.newName = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		public RenameItemPropertyCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null)
			{
				return base.InvokeProvider.Property.RenamePropertyDynamicParameters(".", this.Name, this.NewName, context);
			}
			else
			{
				return base.InvokeProvider.Property.RenamePropertyDynamicParameters(this.Path, this.Name, this.NewName, context);
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
				cmdletProviderContext.PassThru = base.PassThru;
				base.InvokeProvider.Property.Rename(this.path, this.Name, this.NewName, cmdletProviderContext);
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