using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Move", "AzureDeployment")]
	public class MoveAzureDeploymentCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		public MoveAzureDeploymentCommand()
		{
		}

		public MoveAzureDeploymentCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		private string GetDeploymentName(string slot)
		{
			Func<string, Deployment> func = null;
			try
			{
				MoveAzureDeploymentCommand moveAzureDeploymentCommand = this;
				if (func == null)
				{
					func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, slot.ToString());
				}
				Deployment deployment = ((CmdletBase<IServiceManagement>)moveAzureDeploymentCommand).RetryCall<Deployment>(func);
				if (deployment != null)
				{
					string name = deployment.Name;
					return name;
				}
			}
			catch (CommunicationException communicationException)
			{
			}
			return null;
		}

		public void MoveDeploymentProcess()
		{
			Func<string, Deployment> func = null;
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					new List<PersistentVMRoleContext>();
					MoveAzureDeploymentCommand moveAzureDeploymentCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, "Production");
					}
					Deployment deployment = ((CmdletBase<IServiceManagement>)moveAzureDeploymentCommand).RetryCall<Deployment>(func);
					if (deployment.RoleList != null && string.Compare(deployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
					{
						throw new ArgumentException("Cannot Move Deployments with Virtual Machines Present");
					}
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
				}
			}
			string deploymentName = this.GetDeploymentName("Production");
			string str = this.GetDeploymentName("Staging");
			if (str != null)
			{
				SwapDeploymentInput swapDeploymentInput = new SwapDeploymentInput();
				swapDeploymentInput.SourceDeployment = str;
				swapDeploymentInput.Production = deploymentName;
				using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						CmdletExtensions.WriteVerboseOutputForObject(this, swapDeploymentInput);
						MoveAzureDeploymentCommand moveAzureDeploymentCommand1 = this;
						if (action == null)
						{
							action = (string s) => base.Channel.SwapDeployment(s, this.ServiceName, swapDeploymentInput);
						}
						((CmdletBase<IServiceManagement>)moveAzureDeploymentCommand1).RetryCall(action);
						Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
						ManagementOperationContext managementOperationContext = new ManagementOperationContext();
						managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
						managementOperationContext.set_OperationId(operation.OperationTrackingId);
						managementOperationContext.set_OperationStatus(operation.Status);
						ManagementOperationContext managementOperationContext1 = managementOperationContext;
						base.WriteObject(managementOperationContext1, true);
					}
					catch (CommunicationException communicationException3)
					{
						CommunicationException communicationException2 = communicationException3;
						this.WriteErrorDetails(communicationException2);
					}
				}
				return;
			}
			else
			{
				throw new ArgumentException("The Staging deployment slot is empty.");
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.MoveDeploymentProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}