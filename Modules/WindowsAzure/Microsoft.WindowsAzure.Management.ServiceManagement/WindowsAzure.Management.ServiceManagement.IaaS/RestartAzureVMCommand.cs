using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Restart", "AzureVM", DefaultParameterSetName="ByName")]
	public class RestartAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the Virtual Machine to restart.", ParameterSetName="ByName")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Alias(new string[] { "InputObject" })]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The Virtual Machine to restart.", ParameterSetName="Input")]
		[ValidateNotNullOrEmpty]
		public PersistentVM VM
		{
			get;
			set;
		}

		public RestartAzureVMCommand()
		{
		}

		public RestartAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				this.RestartVirtualMachineProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RestartVirtualMachineProcess()
		{
			RestartAzureVMCommand.RestartAzureVMCommand variable = null;
			string name;
			if (base.CurrentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						RestartAzureVMCommand.RestartAzureVMCommand variable1 = variable;
						if (base.ParameterSetName == "ByName")
						{
							name = this.Name;
						}
						else
						{
							name = this.VM.RoleName;
						}
						variable1.roleName = name;
						base.RetryCall((string s) => base.Channel.RestartRole(s, this.ServiceName, this.CurrentDeployment.Name, LambdaVar41));
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