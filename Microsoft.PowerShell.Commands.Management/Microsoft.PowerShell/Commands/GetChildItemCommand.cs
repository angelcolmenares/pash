using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "ChildItem", DefaultParameterSetName="Items", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113308")]
	public class GetChildItemCommand : CoreCommandBase
	{
		private const string childrenSet = "Items";

		private const string literalChildrenSet = "LiteralItems";

		private string[] paths;

		private bool recurse;

		private bool childNames;

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

		[Parameter(Position=1)]
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
		[Parameter(ParameterSetName="LiteralItems", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		public SwitchParameter Name
		{
			get
			{
				return this.childNames;
			}
			set
			{
				this.childNames = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Items", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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

		[Alias(new string[] { "s" })]
		[Parameter]
		public SwitchParameter Recurse
		{
			get
			{
				return this.recurse;
			}
			set
			{
				this.recurse = value;
			}
		}

		public GetChildItemCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			object childItemsDynamicParameters;
			string str;
			if (this.paths == null || (int)this.paths.Length <= 0)
			{
				str = ".";
			}
			else
			{
				str = this.paths[0];
			}
			string parameterSetName = base.ParameterSetName;
			string str1 = parameterSetName;
			if (parameterSetName == null || !(str1 == "Items") && !(str1 == "LiteralItems"))
			{
				childItemsDynamicParameters = base.InvokeProvider.ChildItem.GetChildItemsDynamicParameters(str, this.Recurse, context);
			}
			else
			{
				if (!this.Name)
				{
					childItemsDynamicParameters = base.InvokeProvider.ChildItem.GetChildItemsDynamicParameters(str, this.Recurse, context);
				}
				else
				{
					childItemsDynamicParameters = base.InvokeProvider.ChildItem.GetChildNamesDynamicParameters(str, context);
				}
			}
			return childItemsDynamicParameters;
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
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
				string parameterSetName = base.ParameterSetName;
				string str1 = parameterSetName;
				if (parameterSetName != null && (str1 == "Items" || str1 == "LiteralItems"))
				{
					try
					{
						if (!this.Name)
						{
							base.InvokeProvider.ChildItem.Get(str, this.Recurse, cmdletProviderContext);
						}
						else
						{
							base.InvokeProvider.ChildItem.GetNames(str, ReturnContainers.ReturnMatchingContainers, this.Recurse, cmdletProviderContext);
						}
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