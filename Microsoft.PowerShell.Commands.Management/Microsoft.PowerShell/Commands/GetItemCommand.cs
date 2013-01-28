using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Item", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113319")]
	public class GetItemCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		[Parameter]
		public override string[] Exclude
		{
			get
			{
				return base.Exclude;
			}
			set
			{
				base.Exclude = value;
			}
		}

		[Parameter]
		public override string Filter
		{
			get
			{
				return base.Filter;
			}
			set
			{
				base.Filter = value;
			}
		}

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

		[Parameter]
		public override string[] Include
		{
			get
			{
				return base.Include;
			}
			set
			{
				base.Include = value;
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

		public GetItemCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Item.GetItemDynamicParameters(".", context);
			}
			else
			{
				return base.InvokeProvider.Item.GetItemDynamicParameters(this.Path[0], context);
			}
		}

		protected override void ProcessRecord()
		{
			string[] strArrays = this.paths;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				try
				{
					base.InvokeProvider.Item.Get(str, this.CmdletProviderContext);
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