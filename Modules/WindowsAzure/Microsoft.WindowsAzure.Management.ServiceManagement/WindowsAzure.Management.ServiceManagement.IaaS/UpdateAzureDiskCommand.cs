using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Update", "AzureDisk")]
	public class UpdateAzureDiskCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the disk in the disk library.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Label of the disk.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		public UpdateAzureDiskCommand()
		{
		}

		private static DiskContext.RoleReference CreateRoleReference(RoleReference roleReference)
		{
			if (roleReference != null)
			{
				DiskContext.RoleReference deploymentName = new DiskContext.RoleReference();
				deploymentName.DeploymentName = roleReference.DeploymentName;
				deploymentName.HostedServiceName = roleReference.HostedServiceName;
				deploymentName.RoleName = roleReference.RoleName;
				return deploymentName;
			}
			else
			{
				return null;
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.UpdateDiskProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void UpdateDiskProcess()
		{
			string empty = string.Empty;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					Disk disk = new Disk();
					disk.Name = this.DiskName;
					disk.Label = this.Label;
					Disk disk1 = disk;
					CmdletExtensions.WriteVerboseOutputForObject(this, disk1);
					Disk disk2 = base.RetryCall<Disk>((string s) => base.Channel.UpdateDisk(s, this.DiskName, disk1));
					if (disk2 != null)
					{
						DiskContext diskContext = new DiskContext();
						diskContext.set_OperationId(empty);
						diskContext.DiskName = disk1.Name;
						diskContext.Label = disk1.Label;
						diskContext.IsCorrupted = disk1.IsCorrupted;
						diskContext.AffinityGroup = disk1.AffinityGroup;
						diskContext.OS = disk1.OS;
						diskContext.Location = disk1.Location;
						diskContext.MediaLink = disk1.MediaLink;
						diskContext.DiskSizeInGB = disk1.LogicalDiskSizeInGB;
						diskContext.SourceImageName = disk1.SourceImageName;
						diskContext.AttachedTo = UpdateAzureDiskCommand.CreateRoleReference(disk1.AttachedTo);
						DiskContext diskContext1 = diskContext;
						Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
						diskContext1.set_OperationDescription(base.CommandRuntime.ToString());
						diskContext1.set_OperationId(operation.OperationTrackingId);
						diskContext1.set_OperationStatus(operation.Status);
						base.WriteObject(diskContext1, true);
					}
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
		}
	}
}