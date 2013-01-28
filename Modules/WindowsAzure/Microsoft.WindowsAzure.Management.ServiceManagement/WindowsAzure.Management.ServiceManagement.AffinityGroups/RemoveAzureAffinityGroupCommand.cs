using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.AffinityGroups
{
	[Cmdlet("Remove", "AzureAffinityGroup")]
	public class RemoveAzureAffinityGroupCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=true, HelpMessage="Affinity Group name.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public RemoveAzureAffinityGroupCommand()
		{
		}

		public RemoveAzureAffinityGroupCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.RemoveAffinityGroupProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public void RemoveAffinityGroupProcess()
		{
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureAffinityGroupCommand removeAzureAffinityGroupCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.DeleteAffinityGroup(s, this.Name);
					}
					((CmdletBase<IServiceManagement>)removeAzureAffinityGroupCommand).RetryCall(action);
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