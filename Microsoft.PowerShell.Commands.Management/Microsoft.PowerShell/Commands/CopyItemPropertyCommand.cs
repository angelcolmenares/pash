using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Copy", "ItemProperty", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113293")]
	public class CopyItemPropertyCommand : PassThroughItemPropertyCommandBase
	{
		private string property;

		private string destination;

		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
		public string Destination
		{
			get
			{
				return this.destination;
			}
			set
			{
				this.destination = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string[] LiteralPath
		{
			get
			{
				return this.paths;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.paths = value;
			}
		}

		[Alias(new string[] { "PSProperty" })]
		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string[] Path
		{
			get
			{
				return this.paths;
			}
			set
			{
				this.paths = value;
			}
		}

		public CopyItemPropertyCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (!(this.Path != null & (int)this.Path.Length > 0))
			{
				return base.InvokeProvider.Property.CopyPropertyDynamicParameters(".", this.property, this.Destination, this.property, context);
			}
			else
			{
				return base.InvokeProvider.Property.CopyPropertyDynamicParameters(this.Path[0], this.property, this.Destination, this.property, context);
			}
		}

		protected override void ProcessRecord()
		{
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				try
				{
					base.InvokeProvider.Property.Copy(str, this.property, this.Destination, this.property, base.GetCurrentContext());
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