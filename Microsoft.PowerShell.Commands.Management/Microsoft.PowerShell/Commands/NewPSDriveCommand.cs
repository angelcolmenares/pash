using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "PSDrive", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113357")]
	public class NewPSDriveCommand : CoreCommandWithCredentialsBase
	{
		private bool persist;

		private string name;

		private string provider;

		private string root;

		private string description;

		private string scope;

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				if (value != null)
				{
					this.description = value;
					return;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (value != null)
				{
					this.name = value;
					return;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public SwitchParameter Persist
		{
			get
			{
				return this.persist;
			}
			set
			{
				this.persist = value;
			}
		}

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return true;
			}
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string PSProvider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value != null)
				{
					this.provider = value;
					return;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
			}
		}

		[AllowEmptyString]
		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string Root
		{
			get
			{
				return this.root;
			}
			set
			{
				if (value != null)
				{
					this.root = value;
					return;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Scope
		{
			get
			{
				return this.scope;
			}
			set
			{
				this.scope = value;
			}
		}

		public NewPSDriveCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			return base.SessionState.Drive.NewDriveDynamicParameters(this.PSProvider, context);
		}

		protected override void ProcessRecord()
		{
			ProviderInfo singleProvider = null;
			try
			{
				singleProvider = base.SessionState.Internal.GetSingleProvider(this.PSProvider);
			}
			catch (ProviderNotFoundException providerNotFoundException1)
			{
				ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
				base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
				return;
			}
			if (singleProvider != null)
			{
				string newDriveConfirmAction = NavigationResources.NewDriveConfirmAction;
				string newDriveConfirmResourceTemplate = NavigationResources.NewDriveConfirmResourceTemplate;
				object[] name = new object[3];
				name[0] = this.Name;
				name[1] = singleProvider.FullName;
				name[2] = this.Root;
				string str = string.Format(Thread.CurrentThread.CurrentCulture, newDriveConfirmResourceTemplate, name);
				if (base.ShouldProcess(str, newDriveConfirmAction))
				{
					if (this.Persist && !singleProvider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
					{
						ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(FileSystemProviderStrings.PersistNotSupported), "DriveRootNotNetworkPath", ErrorCategory.InvalidArgument, this);
						base.ThrowTerminatingError(errorRecord);
					}
					PSDriveInfo pSDriveInfo = new PSDriveInfo(this.Name, singleProvider, this.Root, this.Description, base.Credential, this.Persist);
					try
					{
						base.SessionState.Drive.New(pSDriveInfo, this.Scope, this.CmdletProviderContext);
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
					catch (ProviderNotFoundException providerNotFoundException3)
					{
						ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
						base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
					}
					catch (PSArgumentException pSArgumentException1)
					{
						PSArgumentException pSArgumentException = pSArgumentException1;
						base.WriteError(new ErrorRecord(pSArgumentException.ErrorRecord, pSArgumentException));
					}
					catch (ItemNotFoundException itemNotFoundException1)
					{
						ItemNotFoundException itemNotFoundException = itemNotFoundException1;
						base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
					}
					catch (SessionStateOverflowException sessionStateOverflowException)
					{
						throw;
					}
					catch (SessionStateException sessionStateException1)
					{
						SessionStateException sessionStateException = sessionStateException1;
						base.WriteError(new ErrorRecord(sessionStateException.ErrorRecord, sessionStateException));
					}
				}
			}
		}
	}
}