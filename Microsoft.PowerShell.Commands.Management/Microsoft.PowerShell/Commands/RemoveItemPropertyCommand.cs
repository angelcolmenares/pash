using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Remove", "ItemProperty", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113374")]
	public class RemoveItemPropertyCommand : ItemPropertyCommandBase
	{
		private string[] property;

		[Parameter]
		public override SwitchParameter Force
		{
			get
			{
				return base.Force;
			}
			set
			{
				base.Force = value;
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
		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.property;
			}
			set
			{
				if (value != null)
				{
					this.property = value;
					return;
				}
				else
				{
					this.property = new string[0];
					return;
				}
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

		public RemoveItemPropertyCommand()
		{
			this.property = new string[0];
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			string name = null;
			if (this.Name != null && (int)this.Name.Length > 0)
			{
				name = this.Name[0];
			}
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Property.RemovePropertyDynamicParameters(".", name, context);
			}
			else
			{
				return base.InvokeProvider.Property.RemovePropertyDynamicParameters(this.Path[0], name, context);
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
						base.InvokeProvider.Property.Remove(str, str1, this.CmdletProviderContext);
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