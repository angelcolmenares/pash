using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Copy", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113292")]
	public class CopyItemCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		private string destination;

		private bool container;

		private bool containerSpecified;

		private bool recurse;

		private bool passThrough;

		[Parameter]
		public SwitchParameter Container
		{
			get
			{
				return this.container;
			}
			set
			{
				this.containerSpecified = true;
				this.container = value;
			}
		}

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThrough;
			}
			set
			{
				this.passThrough = value;
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

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return base.DoesProviderSupportShouldProcess(this.paths);
			}
		}

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
				if (!this.containerSpecified)
				{
					this.container = this.recurse;
				}
			}
		}

		public CopyItemCommand()
		{
			this.container = true;
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Item.CopyItemDynamicParameters(".", this.Destination, this.Recurse, context);
			}
			else
			{
				return base.InvokeProvider.Item.CopyItemDynamicParameters(this.Path[0], this.Destination, this.Recurse, context);
			}
		}

		protected override void ProcessRecord()
		{
			CopyContainers copyContainer;
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			cmdletProviderContext.PassThru = this.PassThru;
			string[] strArrays = this.paths;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				object[] destination = new object[2];
				destination[0] = str;
				destination[1] = this.Destination;
				CoreCommandBase.tracer.WriteLine("Copy {0} to {1}", destination);
				try
				{
					if (this.Container)
					{
						copyContainer = CopyContainers.CopyTargetContainer;
					}
					else
					{
						copyContainer = CopyContainers.CopyChildrenOfTargetContainer;
					}
					CopyContainers copyContainer1 = copyContainer;
					base.InvokeProvider.Item.Copy(str, this.Destination, this.Recurse, copyContainer1, cmdletProviderContext);
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