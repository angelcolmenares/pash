using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Remove", "AzureDeployment")]
	public class RemoveAzureDeploymentCommand : ServiceManagementCmdletBase
	{
		[Parameter(HelpMessage="Do not confirm deletion of deployment")]
		public SwitchParameter Force
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

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot. Staging | Production")]
		[ValidateSet(new string[] { "Staging", "Production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		public RemoveAzureDeploymentCommand()
		{
		}

		public RemoveAzureDeploymentCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				SwitchParameter force = this.Force;
				if (force.IsPresent || base.ShouldContinue("This cmdlet will remove deployed applications including VMs from the specified deployment slot. Do you want to continue?", "Deployment Deletion"))
				{
					this.RemoveDeploymentProcess();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveDeploymentProcess()
		{
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureDeploymentCommand removeAzureDeploymentCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteDeploymentBySlot(s, this.ServiceName, this.Slot);
					}
					((CmdletBase<IServiceManagement>)removeAzureDeploymentCommand).RetryCall(action);
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