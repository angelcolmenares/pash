using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Save", "AzureVMImage")]
	public class SaveAzureVMImageCommand : IaaSDeploymentManagementCmdletBase
	{
		private const string ProvisioningConfigurationSetParameter = "ProvisioningConfigurationSet";

		private const string ReprovisionPostCaptureAction = "Reprovision";

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the virtual machine to export.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="The label that will have the new image.")]
		[ValidateNotNullOrEmpty]
		public string NewImageLabel
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name that will have the new image.")]
		[ValidateNotNullOrEmpty]
		public string NewImageName
		{
			get;
			set;
		}

		public SaveAzureVMImageCommand()
		{
		}

		public SaveAzureVMImageCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.SaveVirtualMachineImageProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SaveVirtualMachineImageProcess()
		{
			if (base.CurrentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						string str2 = this.Name;
						base.RetryCall((string s) => {
							string newImageName;
							IServiceManagement channel = base.Channel;
							string str = s;
							string serviceName = this.ServiceName;
							string name = this.CurrentDeployment.Name;
							string str1 = str2;
							string newImageName1 = this.NewImageName;
							if (string.IsNullOrEmpty(this.NewImageLabel))
							{
								newImageName = this.NewImageName;
							}
							else
							{
								newImageName = this.NewImageLabel;
							}
							channel.CaptureRole(str, serviceName, name, str1, newImageName1, newImageName, (PostCaptureAction)Enum.Parse(typeof(PostCaptureAction), "Delete"), null);
						}
						);
						Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
						ManagementOperationContext managementOperationContext = new ManagementOperationContext();
						managementOperationContext.OperationDescription = base.CommandRuntime.ToString();
						managementOperationContext.OperationId = operation.OperationTrackingId;
						managementOperationContext.OperationStatus = operation.Status;
						ManagementOperationContext managementOperationContext1 = managementOperationContext;
						base.WriteObject(managementOperationContext1, true);
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this.WriteErrorDetails(communicationException);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		protected override void ValidateParameters()
		{
		}
	}
}