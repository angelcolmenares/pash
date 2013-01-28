using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Clear", "ItemProperty", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113284")]
	public class ClearItemPropertyCommand : PassThroughItemPropertyCommandBase
	{
		private string property;

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

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		public ClearItemPropertyCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			Collection<string> strs = new Collection<string>();
			strs.Add(this.property);
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Property.ClearPropertyDynamicParameters(".", strs, context);
			}
			else
			{
				return base.InvokeProvider.Property.ClearPropertyDynamicParameters(this.Path[0], strs, context);
			}
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = base.PassThru;
			Collection<string> strs = new Collection<string>();
			strs.Add(this.property);
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				try
				{
					base.InvokeProvider.Property.Clear(str, strs, cmdletProviderContext);
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