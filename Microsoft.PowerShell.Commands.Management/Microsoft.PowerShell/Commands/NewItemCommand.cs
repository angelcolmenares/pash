using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "Item", DefaultParameterSetName="pathSet", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113353")]
	public class NewItemCommand : CoreCommandWithCredentialsBase
	{
		private const string nameSet = "nameSet";

		private const string pathSet = "pathSet";

		private string[] paths;

		private string name;

		private string type;

		private object content;

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

		[Alias(new string[] { "Type" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string ItemType
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

		[AllowEmptyString]
		[AllowNull]
		[Parameter(ParameterSetName="nameSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="pathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="nameSet", Mandatory=false, ValueFromPipelineByPropertyName=true)]
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

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return base.DoesProviderSupportShouldProcess(this.paths);
			}
		}

		[Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public object Value
		{
			get
			{
				return this.content;
			}
			set
			{
				this.content = value;
			}
		}

		public NewItemCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Item.NewItemDynamicParameters(".", this.ItemType, this.Value, context);
			}
			else
			{
				if (!string.IsNullOrEmpty(this.Name))
				{
					return base.InvokeProvider.Item.NewItemDynamicParameters(this.Path[0], this.ItemType, this.Value, context);
				}
				else
				{
					return base.InvokeProvider.Item.NewItemDynamicParameters(WildcardPattern.Escape(this.Path[0]), this.ItemType, this.Value, context);
				}
			}
		}

		protected override void ProcessRecord()
		{
			if (this.paths == null || this.paths != null && (int)this.paths.Length == 0)
			{
				string[] empty = new string[1];
				empty[0] = string.Empty;
				this.paths = empty;
			}
			string[] strArrays = this.paths;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				try
				{
					base.InvokeProvider.Item.New(str, this.Name, this.ItemType, this.Value, this.CmdletProviderContext);
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