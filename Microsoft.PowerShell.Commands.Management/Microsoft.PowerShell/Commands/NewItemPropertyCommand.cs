using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "ItemProperty", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113354")]
	public class NewItemPropertyCommand : ItemPropertyCommandBase
	{
		private string property;

		private string type;

		private object propertyValue;

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

		[Parameter(Position=0, ParameterSetName="Path", Mandatory=true)]
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

		[Alias(new string[] { "Type" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string PropertyType
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public object Value
		{
			get
			{
				return this.propertyValue;
			}
			set
			{
				this.propertyValue = value;
			}
		}

		public NewItemPropertyCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Property.NewPropertyDynamicParameters(".", this.Name, this.PropertyType, this.Value, context);
			}
			else
			{
				return base.InvokeProvider.Property.NewPropertyDynamicParameters(this.Path[0], this.Name, this.PropertyType, this.Value, context);
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
					base.InvokeProvider.Property.New(str, this.Name, this.PropertyType, this.Value, this.CmdletProviderContext);
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