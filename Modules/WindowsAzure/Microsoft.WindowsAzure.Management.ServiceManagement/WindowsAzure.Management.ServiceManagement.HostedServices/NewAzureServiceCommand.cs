using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("New", "AzureService", DefaultParameterSetName="ParameterSetAffinityGroup")]
	public class NewAzureServiceCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetAffinityGroup", HelpMessage="Required if Location is not specified. The name of an existing affinity group associated with this subscription.")]
		[ValidateNotNullOrEmpty]
		public string AffinityGroup
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetAffinityGroup", HelpMessage="A description for the cloud service. The description may be up to 1024 characters in length.")]
		[Parameter(Position=3, Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetLocation", HelpMessage="A description for the cloud service. The description may be up to 1024 characters in length.")]
		[ValidateNotNullOrEmpty]
		public string Description
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=false, ParameterSetName="ParameterSetLocation", HelpMessage="A label for the cloud service that is Base64-encoded. The label may be up to 100 characters in length. Default: ServiceName.")]
		[Parameter(Position=2, Mandatory=false, ParameterSetName="ParameterSetAffinityGroup", HelpMessage="A label for the cloud service that is Base64-encoded. The label may be up to 100 characters in length. Default: ServiceName.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetLocation", HelpMessage="Required if AffinityGroup is not specified. The data center region where the clou service will be created.")]
		[ValidateNotNullOrEmpty]
		public string Location
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetAffinityGroup", HelpMessage="A name for the hosted service that is unique to the subscription.")]
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ParameterSetLocation", HelpMessage="A name for the hosted service that is unique to the subscription.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		public NewAzureServiceCommand()
		{
		}

		public NewAzureServiceCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void NewHostedServiceProcess()
		{
			string serviceName;
			Action<string> action = null;
			CreateHostedServiceInput createHostedServiceInput = new CreateHostedServiceInput();
			createHostedServiceInput.ServiceName = this.ServiceName;
			if (string.IsNullOrEmpty(this.Label))
			{
				serviceName = this.ServiceName;
			}
			else
			{
				serviceName = this.Label;
			}
			string str = serviceName;
			createHostedServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(str);
			createHostedServiceInput.Description = this.Description;
			createHostedServiceInput.AffinityGroup = this.AffinityGroup;
			createHostedServiceInput.Location = this.Location;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					CmdletExtensions.WriteVerboseOutputForObject(this, createHostedServiceInput);
					NewAzureServiceCommand newAzureServiceCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.CreateHostedService(s, createHostedServiceInput);
					}
					((CmdletBase<IServiceManagement>)newAzureServiceCommand).RetryCall(action);
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
				this.NewHostedServiceProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}