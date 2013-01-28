using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Rename", "Item", SupportsShouldProcess=true, SupportsTransactions=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113382")]
	public class RenameItemCommand : CoreCommandWithCredentialsBase
	{
		private string path;

		private string newName;

		private bool passThrough;

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
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath")]
		public string LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
				base.SuppressWildcardExpansion = true;
			}
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string NewName
		{
			get
			{
				return this.newName;
			}
			set
			{
				this.newName = value;
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

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				string[] strArrays = new string[1];
				strArrays[0] = this.path;
				return base.DoesProviderSupportShouldProcess(strArrays);
			}
		}

		public RenameItemCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			return base.InvokeProvider.Item.RenameItemDynamicParameters(this.Path, this.NewName, context);
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			try
			{
				if (!base.InvokeProvider.Item.Exists(this.Path, cmdletProviderContext))
				{
					object[] path = new object[1];
					path[0] = this.Path;
					PSInvalidOperationException pSInvalidOperationException = PSTraceSource.NewInvalidOperationException("NavigationResources", "RenameItemDoesntExist", path);
					base.WriteError(new ErrorRecord(pSInvalidOperationException.ErrorRecord, pSInvalidOperationException));
					return;
				}
			}
			catch (PSNotSupportedException pSNotSupportedException1)
			{
				PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
				base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
				return;
			}
			catch (DriveNotFoundException driveNotFoundException1)
			{
				DriveNotFoundException driveNotFoundException = driveNotFoundException1;
				base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
				return;
			}
			catch (ProviderNotFoundException providerNotFoundException1)
			{
				ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
				base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
				return;
			}
			catch (ItemNotFoundException itemNotFoundException1)
			{
				ItemNotFoundException itemNotFoundException = itemNotFoundException1;
				base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
				return;
			}
			bool flag = false;
			try
			{
				flag = base.SessionState.Path.IsCurrentLocationOrAncestor(this.path, cmdletProviderContext);
			}
			catch (PSNotSupportedException pSNotSupportedException3)
			{
				PSNotSupportedException pSNotSupportedException2 = pSNotSupportedException3;
				base.WriteError(new ErrorRecord(pSNotSupportedException2.ErrorRecord, pSNotSupportedException2));
				return;
			}
			catch (DriveNotFoundException driveNotFoundException3)
			{
				DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
				base.WriteError(new ErrorRecord(driveNotFoundException2.ErrorRecord, driveNotFoundException2));
				return;
			}
			catch (ProviderNotFoundException providerNotFoundException3)
			{
				ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
				base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
				return;
			}
			catch (ItemNotFoundException itemNotFoundException3)
			{
				ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
				base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
				return;
			}
			if (!flag)
			{
				cmdletProviderContext.PassThru = this.PassThru;
				object[] newName = new object[2];
				newName[0] = this.Path;
				newName[1] = this.NewName;
				CoreCommandBase.tracer.WriteLine("Rename {0} to {1}", newName);
				try
				{
					base.InvokeProvider.Item.Rename(this.Path, this.NewName, cmdletProviderContext);
				}
				catch (PSNotSupportedException pSNotSupportedException5)
				{
					PSNotSupportedException pSNotSupportedException4 = pSNotSupportedException5;
					base.WriteError(new ErrorRecord(pSNotSupportedException4.ErrorRecord, pSNotSupportedException4));
					return;
				}
				catch (DriveNotFoundException driveNotFoundException5)
				{
					DriveNotFoundException driveNotFoundException4 = driveNotFoundException5;
					base.WriteError(new ErrorRecord(driveNotFoundException4.ErrorRecord, driveNotFoundException4));
					return;
				}
				catch (ProviderNotFoundException providerNotFoundException5)
				{
					ProviderNotFoundException providerNotFoundException4 = providerNotFoundException5;
					base.WriteError(new ErrorRecord(providerNotFoundException4.ErrorRecord, providerNotFoundException4));
					return;
				}
				catch (ItemNotFoundException itemNotFoundException5)
				{
					ItemNotFoundException itemNotFoundException4 = itemNotFoundException5;
					base.WriteError(new ErrorRecord(itemNotFoundException4.ErrorRecord, itemNotFoundException4));
					return;
				}
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = this.Path;
				PSInvalidOperationException pSInvalidOperationException1 = PSTraceSource.NewInvalidOperationException("NavigationResources", "RenamedItemInUse", objArray);
				base.WriteError(new ErrorRecord(pSInvalidOperationException1.ErrorRecord, pSInvalidOperationException1));
			}
		}
	}
}