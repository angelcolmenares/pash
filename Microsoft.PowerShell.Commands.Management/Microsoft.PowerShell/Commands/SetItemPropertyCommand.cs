using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "ItemProperty", DefaultParameterSetName="propertyValuePathSet", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113396")]
	public class SetItemPropertyCommand : PassThroughItemPropertyCommandBase
	{
		private const string propertyValuePathSet = "propertyValuePathSet";

		private const string propertyValueLiteralPathSet = "propertyValueLiteralPathSet";

		private const string propertyPSObjectPathSet = "propertyPSObjectPathSet";

		private const string propertyPSObjectLiteralPathSet = "propertyPSObjectLiteralPathSet";

		private object content;

		private string property;

		private PSObject propertyTable;

		[Parameter(ParameterSetName="propertyPSObjectLiteralPathSet", Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true)]
		[Parameter(ParameterSetName="propertyPSObjectPathSet", Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true)]
		public PSObject InputObject
		{
			get
			{
				return this.propertyTable;
			}
			set
			{
				this.propertyTable = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="propertyValueLiteralPathSet", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		[Parameter(ParameterSetName="propertyPSObjectLiteralPathSet", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
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
		[Parameter(Position=1, ParameterSetName="propertyValuePathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=1, ParameterSetName="propertyValueLiteralPathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(Position=0, ParameterSetName="propertyValuePathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="propertyPSObjectPathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		[AllowNull]
		[Parameter(Position=2, ParameterSetName="propertyValuePathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=2, ParameterSetName="propertyValueLiteralPathSet", Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		public SetItemPropertyCommand()
		{
			this.property = string.Empty;
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			PSObject inputObject = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName == null || !(str == "propertyValuePathSet") && !(str == "propertyValueLiteralPathSet"))
			{
				inputObject = this.InputObject;
			}
			else
			{
				if (!string.IsNullOrEmpty(this.Name))
				{
					inputObject = new PSObject();
					inputObject.Properties.Add(new PSNoteProperty(this.Name, this.Value));
				}
			}
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Property.SetPropertyDynamicParameters(".", inputObject, context);
			}
			else
			{
				return base.InvokeProvider.Property.SetPropertyDynamicParameters(this.Path[0], inputObject, context);
			}
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = base.PassThru;
			PSObject pSObject = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "propertyValuePathSet" || str == "propertyValueLiteralPathSet")
				{
					pSObject = new PSObject();
					pSObject.Properties.Add(new PSNoteProperty(this.Name, this.Value));
				}
				else
				{
					if (str == "propertyPSObjectPathSet")
					{
						pSObject = this.InputObject;
					}
				}
			}
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str1 = path[i];
				try
				{
					base.InvokeProvider.Property.Set(str1, pSObject, cmdletProviderContext);
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