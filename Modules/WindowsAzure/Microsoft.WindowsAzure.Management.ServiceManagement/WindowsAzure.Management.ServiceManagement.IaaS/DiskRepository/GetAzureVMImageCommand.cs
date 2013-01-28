using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
	[Cmdlet("Get", "AzureVMImage")]
	public class GetAzureVMImageCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=false, HelpMessage="Name of the image in the image library.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		public GetAzureVMImageCommand()
		{
		}

		public IEnumerable<OSImage> GetVMImageProcess(out Operation operation)
		{
			IEnumerable<OSImage> oSImages;
			Func<string, OSImage> func = null;
			Func<string, OSImageList> func1 = null;
			operation = null;
			IEnumerable<OSImage> oSImages1 = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (string.IsNullOrEmpty(this.ImageName))
					{
						GetAzureVMImageCommand getAzureVMImageCommand = this;
						if (func1 == null)
						{
							func1 = (string s) => base.Channel.ListOSImages(s);
						}
						oSImages1 = ((CmdletBase<IServiceManagement>)getAzureVMImageCommand).RetryCall<OSImageList>(func1);
					}
					else
					{
						OSImage[] oSImageArray = new OSImage[1];
						OSImage[] oSImageArray1 = oSImageArray;
						int num = 0;
						GetAzureVMImageCommand getAzureVMImageCommand1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetOSImage(s, this.ImageName);
						}
						oSImageArray1[num] = ((CmdletBase<IServiceManagement>)getAzureVMImageCommand1).RetryCall<OSImage>(func);
						oSImages1 = oSImageArray;
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
						oSImages = null;
						return oSImages;
					}
				}
				return oSImages1;
			}
			return oSImages;
		}

		protected override void ProcessRecord()
		{
			try
			{
				Func<OSImage, OSImageContext> func = null;
				base.ProcessRecord();
				Operation operation = null;
				IEnumerable<OSImage> vMImageProcess = this.GetVMImageProcess(out operation);
				if (vMImageProcess != null)
				{
					IEnumerable<OSImage> oSImages = vMImageProcess;
					if (func == null)
					{
						func = (OSImage d) => {
							OSImageContext oSImageContext = new OSImageContext();
							oSImageContext.set_OperationId(operation.OperationTrackingId);
							oSImageContext.set_OperationDescription(this.CommandRuntime.ToString());
							oSImageContext.set_OperationStatus(operation.Status);
							oSImageContext.AffinityGroup = d.AffinityGroup;
							oSImageContext.Category = d.Category;
							oSImageContext.Label = d.Label;
							oSImageContext.Location = d.Location;
							oSImageContext.MediaLink = d.MediaLink;
							oSImageContext.ImageName = d.Name;
							oSImageContext.OS = d.OS;
							oSImageContext.LogicalSizeInGB = d.LogicalSizeInGB;
							return oSImageContext;
						}
						;
					}
					IEnumerable<OSImageContext> oSImageContexts = oSImages.Select<OSImage, OSImageContext>(func);
					base.WriteObject(oSImageContexts, true);
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