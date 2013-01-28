using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Remove", "PSDrive", DefaultParameterSetName="Name", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113376")]
	public class RemovePSDriveCommand : DriveMatchingCoreCommandBase
	{
		private string[] names;

		private string[] provider;

		private string scope;

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

		[Parameter(Position=0, ParameterSetName="LiteralName", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string[] LiteralName
		{
			get
			{
				return this.names;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.names = value;
			}
		}

		[AllowEmptyCollection]
		[AllowNull]
		[Parameter(Position=0, ParameterSetName="Name", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.names;
			}
			set
			{
				this.names = value;
			}
		}

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return true;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string[] PSProvider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value == null)
				{
					value = new string[0];
				}
				this.provider = value;
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

		public RemovePSDriveCommand()
		{
			this.provider = new string[0];
		}

		protected override void ProcessRecord()
		{
			string removeDriveConfirmAction = NavigationResources.RemoveDriveConfirmAction;
			string removeDriveConfirmResourceTemplate = NavigationResources.RemoveDriveConfirmResourceTemplate;
			bool flag = true;
			if (this.names == null)
			{
				string[] empty = new string[1];
				empty[0] = string.Empty;
				this.names = empty;
				flag = false;
			}
			string[] strArrays = this.names;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				bool flag1 = false;
				try
				{
					foreach (PSDriveInfo matchingDrife in base.GetMatchingDrives(str, this.PSProvider, this.Scope))
					{
						object[] name = new object[3];
						name[0] = matchingDrife.Name;
						name[1] = matchingDrife.Provider;
						name[2] = matchingDrife.Root;
						string str1 = string.Format(Thread.CurrentThread.CurrentCulture, removeDriveConfirmResourceTemplate, name);
						flag1 = true;
						if (!base.ShouldProcess(str1, removeDriveConfirmAction))
						{
							continue;
						}
						if (this.Force || !(matchingDrife == base.SessionState.Drive.Current))
						{
							base.SessionState.Drive.Remove(matchingDrife.Name, this.Force, this.Scope, this.CmdletProviderContext);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = matchingDrife.Name;
							PSInvalidOperationException pSInvalidOperationException = PSTraceSource.NewInvalidOperationException("NavigationResources", "RemoveDriveInUse", objArray);
							base.WriteError(new ErrorRecord(pSInvalidOperationException.ErrorRecord, pSInvalidOperationException));
						}
					}
				}
				catch (DriveNotFoundException driveNotFoundException)
				{
				}
				catch (ProviderNotFoundException providerNotFoundException)
				{
				}
				if (flag && !flag1)
				{
					DriveNotFoundException driveNotFoundException1 = new DriveNotFoundException(str, "DriveNotFound", SessionStateStrings.DriveNotFound);
					base.WriteError(new ErrorRecord(driveNotFoundException1.ErrorRecord, driveNotFoundException1));
				}
			}
		}
	}
}