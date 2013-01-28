using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Remove", "AzureDisk")]
	public class RemoveAzureDiskCommand : ServiceManagementCmdletBase
	{
		[Parameter(Mandatory=false, HelpMessage="Specify to remove the underlying VHD from the blob storage.")]
		public SwitchParameter DeleteVHD
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the data disk in the disk library to remove.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		public RemoveAzureDiskCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.RemoveDiskProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveDiskProcess()
		{
			Func<string, Disk> func = null;
			Action<string> action = null;
			try
			{
				Uri mediaLink = null;
				SwitchParameter deleteVHD = this.DeleteVHD;
				if (deleteVHD.IsPresent)
				{
					using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
					{
						RemoveAzureDiskCommand removeAzureDiskCommand = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetDisk(s, this.DiskName);
						}
						Disk disk = ((CmdletBase<IServiceManagement>)removeAzureDiskCommand).RetryCall<Disk>(func);
						mediaLink = disk.MediaLink;
					}
				}
				using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
				{
					RemoveAzureDiskCommand removeAzureDiskCommand1 = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteDisk(s, this.DiskName);
					}
					((CmdletBase<IServiceManagement>)removeAzureDiskCommand1).RetryCall(action);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					managementOperationContext.set_OperationId(operation.OperationTrackingId);
					managementOperationContext.set_OperationStatus(operation.Status);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
				SwitchParameter switchParameter = this.DeleteVHD;
				if (switchParameter.IsPresent)
				{
					Disks.RemoveVHD(base.Channel, base.get_CurrentSubscription().get_SubscriptionId(), mediaLink);
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