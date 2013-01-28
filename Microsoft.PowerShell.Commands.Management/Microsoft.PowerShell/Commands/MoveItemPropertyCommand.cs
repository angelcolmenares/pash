using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Move", "ItemProperty", SupportsShouldProcess=true, DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113351")]
	public class MoveItemPropertyCommand : PassThroughItemPropertyCommandBase
	{
		private string[] property;

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
		public string[] Name
		{
			get
			{
				return this.property;
			}
			set
			{
				if (value == null)
				{
					value = new string[0];
				}
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

		public MoveItemPropertyCommand()
		{
			this.property = new string[0];
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			string empty = string.Empty;
			if (this.Name != null && (int)this.Name.Length > 0)
			{
				empty = this.Name[0];
			}
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Property.MovePropertyDynamicParameters(".", empty, this.Destination, empty, context);
			}
			else
			{
				return base.InvokeProvider.Property.MovePropertyDynamicParameters(this.Path[0], empty, this.Destination, empty, context);
			}
		}

		protected override void ProcessRecord()
		{
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				string[] name = this.Name;
				for (int j = 0; j < (int)name.Length; j++)
				{
					string str1 = name[j];
					try
					{
						base.InvokeProvider.Property.Move(str, str1, this.Destination, str1, base.GetCurrentContext());
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
}