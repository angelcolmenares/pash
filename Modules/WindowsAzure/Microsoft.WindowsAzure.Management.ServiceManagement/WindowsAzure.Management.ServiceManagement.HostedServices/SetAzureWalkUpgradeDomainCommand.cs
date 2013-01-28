using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Set", "AzureWalkUpgradeDomain")]
	public class SetAzureWalkUpgradeDomainCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=2, Mandatory=true, HelpMessage="Domain number.")]
		[ValidateNotNullOrEmpty]
		public int DomainNumber
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name")]
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

		public SetAzureWalkUpgradeDomainCommand()
		{
		}

		public SetAzureWalkUpgradeDomainCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetWalkUpgradeDomainProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetWalkUpgradeDomainProcess()
		{
			Action<string> action = null;
			WalkUpgradeDomainInput walkUpgradeDomainInput = new WalkUpgradeDomainInput();
			walkUpgradeDomainInput.UpgradeDomain = this.DomainNumber;
			WalkUpgradeDomainInput walkUpgradeDomainInput1 = walkUpgradeDomainInput;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					SetAzureWalkUpgradeDomainCommand setAzureWalkUpgradeDomainCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.WalkUpgradeDomainBySlot(s, this.ServiceName, this.Slot, walkUpgradeDomainInput1);
					}
					((CmdletBase<IServiceManagement>)setAzureWalkUpgradeDomainCommand).RetryCall(action);
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