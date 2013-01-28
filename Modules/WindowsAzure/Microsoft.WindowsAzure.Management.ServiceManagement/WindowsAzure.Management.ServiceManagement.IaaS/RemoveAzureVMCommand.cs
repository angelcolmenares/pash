using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Remove", "AzureVM")]
	public class RemoveAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the role to remove.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public RemoveAzureVMCommand()
		{
		}

		public RemoveAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.RemoveVirtualMachineProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveVirtualMachineProcess()
		{
			int count;
			Func<string, Deployment> func = null;
			Action<string> action = null;
			Action<string> action1 = null;
			if (base.CurrentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					RemoveAzureVMCommand removeAzureVMCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, "Production");
					}
					Deployment deployment = ((CmdletBase<IServiceManagement>)removeAzureVMCommand).RetryCall<Deployment>(func);
					count = deployment.RoleInstanceList.Count;
				}
				using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						if (count <= 1)
						{
							RemoveAzureVMCommand removeAzureVMCommand1 = this;
							if (action1 == null)
							{
								action1 = (string s) => base.Channel.DeleteDeploymentBySlot(s, this.ServiceName, "Production");
							}
							((CmdletBase<IServiceManagement>)removeAzureVMCommand1).RetryCall(action1);
						}
						else
						{
							RemoveAzureVMCommand removeAzureVMCommand2 = this;
							if (action == null)
							{
								action = (string s) => base.Channel.DeleteRole(s, this.ServiceName, base.CurrentDeployment.Name, this.Name);
							}
							((CmdletBase<IServiceManagement>)removeAzureVMCommand2).RetryCall(action);
						}
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
				return;
			}
			else
			{
				return;
			}
		}
	}
}