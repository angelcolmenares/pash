using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureDisk")]
	public class GetAzureDiskCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=false, HelpMessage="Name of the disk in the disk library.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		public GetAzureDiskCommand()
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

		public IEnumerable<Disk> GetDiskProcess(out Operation operation)
		{
			IEnumerable<Disk> disks;
			Func<string, Disk> func = null;
			Func<string, DiskList> func1 = null;
			IEnumerable<Disk> disks1 = null;
			operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (string.IsNullOrEmpty(this.DiskName))
					{
						GetAzureDiskCommand getAzureDiskCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.ListDisks(s);
						}
						disks1 = ((CmdletBase<IServiceManagement>)getAzureDiskCommand).RetryCall<DiskList>(func1);
					}
					else
					{
						Disk[] diskArray = new Disk[1];
						Disk[] diskArray1 = diskArray;
						int num = 0;
						GetAzureDiskCommand getAzureDiskCommand1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetDisk(s, this.DiskName);
						}
						diskArray1[num] = ((CmdletBase<IServiceManagement>)getAzureDiskCommand1).RetryCall<Disk>(func);
						disks1 = diskArray;
					}
					operation = base.WaitForOperation(base.CommandRuntime.ToString());
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
					else
					{
						disks = null;
						return disks;
					}
				}
				return disks1;
			}
			return disks;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				Operation operation = null;
				IEnumerable<Disk> diskProcess = this.GetDiskProcess(out operation);
				if (diskProcess != null)
				{
					List<DiskContext> diskContexts = new List<DiskContext>();
					foreach (Disk disk in diskProcess)
					{
						DiskContext diskContext = new DiskContext();
						diskContext.set_OperationId(operation.OperationTrackingId);
						diskContext.set_OperationDescription(base.CommandRuntime.ToString());
						diskContext.set_OperationStatus(operation.Status);
						diskContext.DiskName = disk.Name;
						diskContext.Label = disk.Label;
						diskContext.IsCorrupted = disk.IsCorrupted;
						diskContext.AffinityGroup = disk.AffinityGroup;
						diskContext.OS = disk.OS;
						diskContext.Location = disk.Location;
						diskContext.MediaLink = disk.MediaLink;
						diskContext.DiskSizeInGB = disk.LogicalDiskSizeInGB;
						diskContext.SourceImageName = disk.SourceImageName;
						diskContext.AttachedTo = GetAzureDiskCommand.CreateRoleReference(disk.AttachedTo);
						diskContexts.Add(diskContext);
					}
					base.WriteObject(diskContexts, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}