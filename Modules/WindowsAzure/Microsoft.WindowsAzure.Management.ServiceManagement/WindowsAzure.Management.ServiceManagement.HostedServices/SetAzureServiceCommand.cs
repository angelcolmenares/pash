using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Set", "AzureService")]
	public class SetAzureServiceCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=2, ValueFromPipelineByPropertyName=true, HelpMessage="A description for the hosted service. The description may be up to 1024 characters in length.")]
		[ValidateLength(0, 0x400)]
		public string Description
		{
			get;
			set;
		}

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, HelpMessage="A label for the hosted service. The label may be up to 100 characters in length.")]
		[ValidateLength(0, 100)]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		public SetAzureServiceCommand()
		{
		}

		public SetAzureServiceCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetHostedServiceProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetHostedServiceProcess()
		{
			if (this.Label == null && this.Description == null)
			{
				base.ThrowTerminatingError(new ErrorRecord(new Exception("You must specify a value for either Label or Description."), string.Empty, ErrorCategory.InvalidData, null));
			}
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					UpdateHostedServiceInput updateHostedServiceInput = new UpdateHostedServiceInput();
					if (this.Label != null)
					{
						updateHostedServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
					}
					if (this.Description != null)
					{
						updateHostedServiceInput.Description = this.Description;
					}
					CmdletExtensions.WriteVerboseOutputForObject(this, updateHostedServiceInput);
					base.RetryCall((string s) => base.Channel.UpdateHostedService(s, this.ServiceName, updateHostedServiceInput));
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
	}
}