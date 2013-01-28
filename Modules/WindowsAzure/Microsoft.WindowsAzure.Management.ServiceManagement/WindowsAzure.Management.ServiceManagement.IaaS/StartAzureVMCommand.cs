using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Start", "AzureVM", DefaultParameterSetName="ByName")]
	public class StartAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the Virtual Machine to start.", ParameterSetName="ByName")]
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

		public StartAzureVMCommand()
		{
		}

		public StartAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.StartVirtualMachineProcess();
				this.ValidateParameters();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void StartVirtualMachineProcess()
		{
			StartAzureVMCommand.StartAzureVMCommand variable = null;
			string name;
			if (base.CurrentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						StartAzureVMCommand.StartAzureVMCommand variable1 = variable;
						if (base.ParameterSetName == "ByName")
						{
							name = this.Name;
						}
						else
						{
							name = this.VM.RoleName;
						}
						variable1.roleName = name;
						base.RetryCall((string s) => base.Channel.StartRole(s, this.ServiceName, this.CurrentDeployment.Name, LambdaVar43));
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