using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.AffinityGroups
{
	[Cmdlet("Set", "AzureAffinityGroup")]
	public class SetAzureAffinityGroupCommand : ServiceManagementCmdletBase
	{
		[Parameter(HelpMessage="Description of the affinity group.")]
		[ValidateLength(0, 0x400)]
		public string Description
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, HelpMessage="Label of the affinity group.")]
		[ValidateLength(1, 100)]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the affinity group.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public SetAzureAffinityGroupCommand()
		{
		}

		public SetAzureAffinityGroupCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.SetAffinityGroupProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void SetAffinityGroupProcess()
		{
			this.ValidateParameters();
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					UpdateAffinityGroupInput updateAffinityGroupInput = new UpdateAffinityGroupInput();
					updateAffinityGroupInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
					UpdateAffinityGroupInput description = updateAffinityGroupInput;
					if (this.Description != null)
					{
						description.Description = this.Description;
					}
					CmdletExtensions.WriteVerboseOutputForObject(this, description);
					base.RetryCall((string s) => base.Channel.UpdateAffinityGroup(s, this.Name, description));
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

		private void ValidateParameters()
		{
		}
	}
}