using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Reset", "AzureRoleInstance", DefaultParameterSetName="ParameterSetGetDeployment")]
	public class ResetAzureRoleInstanceCommand : ServiceManagementCmdletBase
	{
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the role instance.")]
		[ValidateNotNullOrEmpty]
		public string InstanceName
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Reboot the role instance.")]
		public SwitchParameter Reboot
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Reimage the role instance.")]
		public SwitchParameter Reimage
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the hosted service.")]
		[ValidateNotNullOrEmpty]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Slot of the deployment.")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "staging", "production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		public ResetAzureRoleInstanceCommand()
		{
		}

		public ResetAzureRoleInstanceCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				this.ValidateParameters();
				base.ProcessRecord();
				this.ResetRoleInstanceProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		private void RebootSingleInstance(string instanceName)
		{
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				ResetAzureRoleInstanceCommand resetAzureRoleInstanceCommand = this;
				if (action == null)
				{
					action = (string s) => base.Channel.RebootDeploymentRoleInstanceBySlot(s, this.ServiceName, this.Slot, instanceName);
				}
				((CmdletBase<IServiceManagement>)resetAzureRoleInstanceCommand).RetryCall(action);
				Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
				ManagementOperationContext managementOperationContext = new ManagementOperationContext();
				managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
				managementOperationContext.set_OperationId(operation.OperationTrackingId);
				managementOperationContext.set_OperationStatus(operation.Status);
				ManagementOperationContext managementOperationContext1 = managementOperationContext;
				base.WriteObject(managementOperationContext1, true);
			}
		}

		private void ReimageSingleInstance(string instanceName)
		{
			Action<string> action = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				ResetAzureRoleInstanceCommand resetAzureRoleInstanceCommand = this;
				if (action == null)
				{
					action = (string s) => base.Channel.ReimageDeploymentRoleInstanceBySlot(s, this.ServiceName, this.Slot, instanceName);
				}
				((CmdletBase<IServiceManagement>)resetAzureRoleInstanceCommand).RetryCall(action);
				Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
				ManagementOperationContext managementOperationContext = new ManagementOperationContext();
				managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
				managementOperationContext.set_OperationId(operation.OperationTrackingId);
				managementOperationContext.set_OperationStatus(operation.Status);
				ManagementOperationContext managementOperationContext1 = managementOperationContext;
				base.WriteObject(managementOperationContext1, true);
			}
		}

		public void ResetRoleInstanceProcess()
		{
			if (this.InstanceName != null)
			{
				if (!this.Reboot)
				{
					if (this.Reimage)
					{
						this.ReimageSingleInstance(this.InstanceName);
					}
				}
				else
				{
					this.RebootSingleInstance(this.InstanceName);
					return;
				}
			}
		}

		private void ValidateParameters()
		{
			if (!this.Reboot || !this.Reimage)
			{
				if (!this.Reboot && !this.Reimage)
				{
					base.ThrowTerminatingError(new ErrorRecord(new ArgumentException("Reboot or Reimage parameters should be specified."), string.Empty, ErrorCategory.InvalidData, null));
				}
				return;
			}
			else
			{
				base.ThrowTerminatingError(new ErrorRecord(new ArgumentException("Reboot and Reimage parameters are mutually exclusive."), string.Empty, ErrorCategory.InvalidData, null));
				return;
			}
		}
	}
}