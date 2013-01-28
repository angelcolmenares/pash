using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
	[Cmdlet("Remove", "AzureVMImage")]
	public class RemoveAzureVMImageCommand : ServiceManagementCmdletBase
	{
		[Parameter(Mandatory=false, HelpMessage="Specify to remove the underlying VHD from the blob storage.")]
		public SwitchParameter DeleteVHD
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the image in the image library to remove.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		public RemoveAzureVMImageCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.RemoveVMImageProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveVMImageProcess()
		{
			Func<string, OSImage> func = null;
			Action<string> action = null;
			try
			{
				Uri mediaLink = null;
				SwitchParameter deleteVHD = this.DeleteVHD;
				if (deleteVHD.IsPresent)
				{
					using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
					{
						RemoveAzureVMImageCommand removeAzureVMImageCommand = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetOSImage(s, this.ImageName);
						}
						OSImage oSImage = ((CmdletBase<IServiceManagement>)removeAzureVMImageCommand).RetryCall<OSImage>(func);
						mediaLink = oSImage.MediaLink;
					}
				}
				using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
				{
					RemoveAzureVMImageCommand removeAzureVMImageCommand1 = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteOSImage(s, this.ImageName);
					}
					((CmdletBase<IServiceManagement>)removeAzureVMImageCommand1).RetryCall(action);
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