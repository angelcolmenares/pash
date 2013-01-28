using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Storage
{
	[Cmdlet("New", "AzureStorageAccount", DefaultParameterSetName="ParameterSetAffinityGroup")]
	public class NewAzureStorageAccountCommand : ServiceManagementCmdletBase
	{
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetAffinityGroup", HelpMessage="Required if Location is not specified. The name of an existing affinity group in the specified subscription.")]
		[ValidateNotNullOrEmpty]
		public string AffinityGroup
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, HelpMessage="A description for the storage account.")]
		[ValidateNotNullOrEmpty]
		public string Description
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, HelpMessage="Label for the storage account.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetLocation", HelpMessage="Required if AffinityGroup is not specified. The location where the storage account is created.")]
		[ValidateNotNullOrEmpty]
		public string Location
		{
			get;
			set;
		}

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="A name for the storage account that is unique to the subscription. Storage account names must be between 3 and 24 characters in length and use numbers and lower-case letters only.")]
		[ValidateNotNullOrEmpty]
		public string StorageAccountName
		{
			get;
			set;
		}

		public NewAzureStorageAccountCommand()
		{
		}

		public NewAzureStorageAccountCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void NewStorageAccountProcess()
		{
			Action<string> action = null;
			CreateStorageServiceInput createStorageServiceInput = new CreateStorageServiceInput();
			createStorageServiceInput.ServiceName = this.StorageAccountName;
			createStorageServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
			createStorageServiceInput.Description = this.Description;
			createStorageServiceInput.AffinityGroup = this.AffinityGroup;
			createStorageServiceInput.Location = this.Location;
			CreateStorageServiceInput createStorageServiceInput1 = createStorageServiceInput;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					CmdletExtensions.WriteVerboseOutputForObject(this, createStorageServiceInput1);
					NewAzureStorageAccountCommand newAzureStorageAccountCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.CreateStorageService(s, createStorageServiceInput1);
					}
					((CmdletBase<IServiceManagement>)newAzureStorageAccountCommand).RetryCall(action);
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
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.NewStorageAccountProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		private void ValidateParameters()
		{
			if (string.IsNullOrEmpty(this.Label))
			{
				this.Label = this.StorageAccountName;
			}
		}
	}
}