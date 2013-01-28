using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.AffinityGroups
{
	[Cmdlet("New", "AzureAffinityGroup")]
	public class NewAzureAffinityGroupCommand : ServiceManagementCmdletBase
	{
		[Parameter(HelpMessage="Description of the affinity group.")]
		[ValidateLength(0, 0x400)]
		public string Description
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Label of the affinity group.")]
		[ValidateLength(1, 100)]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, HelpMessage="Location of the affinity group.")]
		[ValidateNotNullOrEmpty]
		public string Location
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

		public NewAzureAffinityGroupCommand()
		{
		}

		public NewAzureAffinityGroupCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public void NewAffinityGroupProcess()
		{
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					CreateAffinityGroupInput createAffinityGroupInput = new CreateAffinityGroupInput();
					createAffinityGroupInput.Description = this.Description;
					createAffinityGroupInput.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
					createAffinityGroupInput.Location = this.Location;
					createAffinityGroupInput.Name = this.Name;
					CmdletExtensions.WriteVerboseOutputForObject(this, createAffinityGroupInput);
					base.RetryCall((string s) => base.Channel.CreateAffinityGroup(s, createAffinityGroupInput));
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
				this.NewAffinityGroupProcess();
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
				this.Label = this.Name;
			}
		}
	}
}