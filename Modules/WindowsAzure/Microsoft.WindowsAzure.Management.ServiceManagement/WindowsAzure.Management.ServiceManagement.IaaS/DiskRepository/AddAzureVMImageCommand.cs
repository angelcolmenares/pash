using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
	[Cmdlet("Add", "AzureVMImage")]
	public class AddAzureVMImageCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the image in the image library.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, HelpMessage="Label of the image.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Location of the physical blob backing the image. This link refers to a blob in a storage account.")]
		[ValidateNotNullOrEmpty]
		public string MediaLocation
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The OS Type of the Image (Windows or Linux)")]
		[ValidateSet(new string[] { "Windows", "Linux" }, IgnoreCase=true)]
		public string OS
		{
			get;
			set;
		}

		public AddAzureVMImageCommand()
		{
		}

		public void AddVMImageProcess()
		{
			AddAzureVMImageCommand.AddAzureVMImageCommand variable = null;
			string imageName;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					AddAzureVMImageCommand.AddAzureVMImageCommand variable1 = variable;
					OSImage oSImage = new OSImage();
					oSImage.Name = this.ImageName;
					oSImage.MediaLink = new Uri(this.MediaLocation);
					OSImage oSImage1 = oSImage;
					if (string.IsNullOrEmpty(this.Label))
					{
						imageName = this.ImageName;
					}
					else
					{
						imageName = this.Label;
					}
					oSImage1.Label = imageName;
					oSImage.OS = this.OS;
					variable1.image = oSImage;
					CmdletExtensions.WriteVerboseOutputForObject(this, LambdaVar23);
					OSImage oSImage2 = base.RetryCall<OSImage>((string s) => base.Channel.CreateOSImage(s, LambdaVar23));
					OSImageContext oSImageContext = new OSImageContext();
					oSImageContext.AffinityGroup = oSImage2.AffinityGroup;
					oSImageContext.Category = oSImage2.Category;
					oSImageContext.Label = oSImage2.Label;
					oSImageContext.Location = oSImage2.Location;
					oSImageContext.MediaLink = oSImage2.MediaLink;
					oSImageContext.ImageName = oSImage2.Name;
					oSImageContext.OS = oSImage2.OS;
					OSImageContext oSImageContext1 = oSImageContext;
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					oSImageContext1.set_OperationDescription(base.CommandRuntime.ToString());
					oSImageContext1.set_OperationId(operation.OperationTrackingId);
					oSImageContext1.set_OperationStatus(operation.Status);
					base.WriteObject(oSImageContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.AddVMImageProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}