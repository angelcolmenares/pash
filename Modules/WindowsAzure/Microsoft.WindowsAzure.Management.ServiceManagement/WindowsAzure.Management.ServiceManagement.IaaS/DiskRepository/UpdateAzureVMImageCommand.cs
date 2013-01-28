using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
	[Cmdlet("Update", "AzureVMImage")]
	public class UpdateAzureVMImageCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the image in the image library.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Label of the image.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		public UpdateAzureVMImageCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				string empty = string.Empty;
				OSImage oSImage = this.UpdateVMImageProcess(out empty);
				if (oSImage != null)
				{
					OSImageContext oSImageContext = new OSImageContext();
					oSImageContext.set_OperationId(empty);
					oSImageContext.AffinityGroup = oSImage.AffinityGroup;
					oSImageContext.Category = oSImage.Category;
					oSImageContext.Label = oSImage.Label;
					oSImageContext.Location = oSImage.Location;
					oSImageContext.MediaLink = oSImage.MediaLink;
					oSImageContext.ImageName = oSImage.Name;
					oSImageContext.OS = oSImage.OS;
					OSImageContext oSImageContext1 = oSImageContext;
					base.WriteObject(oSImageContext1, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public OSImage UpdateVMImageProcess(out string operationId)
		{
			operationId = string.Empty;
			OSImage oSImage = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					OSImage imageName = new OSImage();
					imageName.Name = this.ImageName;
					imageName.Label = this.Label;
					OSImage oSImage1 = imageName;
					CmdletExtensions.WriteVerboseOutputForObject(this, oSImage1);
					oSImage = base.RetryCall<OSImage>((string s) => base.Channel.UpdateOSImage(s, this.ImageName, oSImage1));
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					managementOperationContext.set_OperationId(operation.OperationTrackingId);
					managementOperationContext.set_OperationStatus(operation.Status);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return oSImage;
		}
	}
}