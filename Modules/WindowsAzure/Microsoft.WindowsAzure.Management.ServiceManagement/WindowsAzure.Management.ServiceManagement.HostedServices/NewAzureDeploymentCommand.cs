using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("New", "AzureDeployment", DefaultParameterSetName="PaaS")]
	public class NewAzureDeploymentCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=2, Mandatory=true, HelpMessage="Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
		[ValidateNotNullOrEmpty]
		public string Configuration
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, HelpMessage="Do not start deployment upon creation.")]
		public SwitchParameter DoNotStart
		{
			get;
			set;
		}

		[Parameter(Position=4, Mandatory=false, HelpMessage="Label for the new deployment.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Alias(new string[] { "DeploymentName" })]
		[Parameter(Position=5, HelpMessage="Deployment name.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, HelpMessage="Package location. This parameter specifies the path or URI to a .cspkg in blob storage. The storage account must belong to the same subscription as the deployment.")]
		[ValidateNotNullOrEmpty]
		public string Package
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Cloud service name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot [Staging | Production].")]
		[ValidateSet(new string[] { "Staging", "Production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, HelpMessage="Indicates whether to treat package validation warnings as errors.")]
		public SwitchParameter TreatWarningsAsError
		{
			get;
			set;
		}

		public NewAzureDeploymentCommand()
		{
		}

		public NewAzureDeploymentCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void NewPaaSDeploymentProcess()
		{
			NewAzureDeploymentCommand.NewAzureDeploymentCommand variable = null;
			Func<string, Deployment> func = null;
			Func<string, Uri> func1 = null;
			Action<string> action = null;
			Action<string> action1 = null;
			bool flag = false;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					new List<PersistentVMRoleContext>();
					NewAzureDeploymentCommand newAzureDeploymentCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, "Production");
					}
					Deployment deployment = ((CmdletBase<IServiceManagement>)newAzureDeploymentCommand).RetryCall<Deployment>(func);
					if (deployment.RoleList != null && string.Compare(deployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
					{
						throw new ArgumentException("Cannot Create New Deployment with Virtual Machines Present");
					}
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null && !base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
				}
			}
			string currentStorageAccount = base.get_CurrentSubscription().get_CurrentStorageAccount();
			if (this.Package.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || this.Package.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
			{
				Uri uri = new Uri(this.Package);
			}
			else
			{
				ProgressRecord progressRecord = new ProgressRecord(0, "Please wait...", "Uploading package to blob storage");
				base.WriteProgress(progressRecord);
				flag = true;
				NewAzureDeploymentCommand.NewAzureDeploymentCommand variable1 = variable;
				NewAzureDeploymentCommand newAzureDeploymentCommand1 = this;
				if (func1 == null)
				{
					func1 = (string s) => AzureBlob.UploadPackageToBlob(base.Channel, currentStorageAccount, s, this.Package);
				}
				variable1.packageUrl = ((CmdletBase<IServiceManagement>)newAzureDeploymentCommand1).RetryCall<Uri>(func1);
			}
			CreateDeploymentInput createDeploymentInput = new CreateDeploymentInput();
			createDeploymentInput.PackageUrl = uri;
			createDeploymentInput.Configuration = Utility.GetConfiguration(this.Configuration);
			createDeploymentInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
			createDeploymentInput.Name = this.Name;
			SwitchParameter doNotStart = this.DoNotStart;
			createDeploymentInput.StartDeployment = new bool?(!doNotStart.IsPresent);
			SwitchParameter treatWarningsAsError = this.TreatWarningsAsError;
			createDeploymentInput.TreatWarningsAsError = new bool?(treatWarningsAsError.IsPresent);
			CreateDeploymentInput createDeploymentInput1 = createDeploymentInput;
			using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					ProgressRecord progressRecord1 = new ProgressRecord(0, "Please wait...", "Creating the new deployment");
					base.WriteProgress(progressRecord1);
					CmdletExtensions.WriteVerboseOutputForObject(this, createDeploymentInput1);
					NewAzureDeploymentCommand newAzureDeploymentCommand2 = this;
					if (action == null)
					{
						action = (string s) => this.Channel.CreateOrUpdateDeployment(s, this.ServiceName, this.Slot, this.deploymentInput);
					}

					((CmdletBase<IServiceManagement>)newAzureDeploymentCommand2).RetryCall(action);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.OperationDescription = base.CommandRuntime.ToString();
					managementOperationContext.OperationId = operation.OperationTrackingId;
					managementOperationContext.OperationStatus = operation.Status;
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
					if (flag)
					{
						NewAzureDeploymentCommand newAzureDeploymentCommand3 = this;
						if (action1 == null)
						{
							action1 = (string s) => AzureBlob.DeletePackageFromBlob(base.Channel, currentStorageAccount, s, uri);
						}
						((CmdletBase<IServiceManagement>)newAzureDeploymentCommand3).RetryCall(action1);
					}
				}
				catch (CommunicationException communicationException3)
				{
					CommunicationException communicationException2 = communicationException3;
					this.WriteErrorDetails(communicationException2);
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.NewPaaSDeploymentProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		private void ValidateParameters()
		{
			if (string.IsNullOrEmpty(this.Slot))
			{
				this.Slot = "Production";
			}
			if (string.IsNullOrEmpty(this.Name))
			{
				Guid guid = Guid.NewGuid();
				this.Name = guid.ToString();
			}
			if (string.IsNullOrEmpty(this.Label))
			{
				this.Label = this.Name;
			}
			if (!string.IsNullOrEmpty(base.get_CurrentSubscription().get_CurrentStorageAccount()))
			{
				return;
			}
			else
			{
				throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
			}
		}
	}
}