using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Add", "AzureDisk")]
	public class AddAzureDiskCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the disk in the disk library.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, HelpMessage="Label of the disk.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Location of the physical blob backing the disk. This link refers to a blob in a storage account.")]
		[ValidateNotNullOrEmpty]
		public string MediaLocation
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, HelpMessage="OS on Disk.")]
		[ValidateNotNullOrEmpty]
		public string OS
		{
			get;
			set;
		}

		public AddAzureDiskCommand()
		{
		}

		public void AddDiskProcess()
		{
			AddAzureDiskCommand.AddAzureDiskCommand variable = null;
			string diskName;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					AddAzureDiskCommand.AddAzureDiskCommand variable1 = variable;
					Disk disk = new Disk();
					disk.Name = this.DiskName;
					disk.MediaLink = new Uri(this.MediaLocation);
					disk.OS = this.OS;
					Disk disk1 = disk;
					if (string.IsNullOrEmpty(this.Label))
					{
						diskName = this.DiskName;
					}
					else
					{
						diskName = this.Label;
					}
					disk1.Label = diskName;
					variable1.disk = disk;
					CmdletExtensions.WriteVerboseOutputForObject(this, LambdaVar22);
					Disk disk2 = base.RetryCall<Disk>((string s) => base.Channel.CreateDisk(s, LambdaVar22));
					DiskContext diskContext = new DiskContext();
					diskContext.DiskName = disk2.Name;
					diskContext.Label = disk2.Label;
					diskContext.IsCorrupted = disk2.IsCorrupted;
					diskContext.AffinityGroup = disk2.AffinityGroup;
					diskContext.OS = disk2.OS;
					diskContext.Location = disk2.Location;
					diskContext.MediaLink = disk2.MediaLink;
					diskContext.DiskSizeInGB = disk2.LogicalDiskSizeInGB;
					diskContext.SourceImageName = disk2.SourceImageName;
					diskContext.AttachedTo = AddAzureDiskCommand.CreateRoleReference(disk2.AttachedTo);
					DiskContext diskContext1 = diskContext;
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					diskContext1.set_OperationDescription(base.CommandRuntime.ToString());
					diskContext1.set_OperationId(operation.OperationTrackingId);
					diskContext1.set_OperationStatus(operation.Status);
					base.WriteObject(diskContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
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
				this.AddDiskProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}