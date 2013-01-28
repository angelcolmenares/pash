using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	public class IaaSDeploymentManagementCmdletBase : ServiceManagementCmdletBase
	{
		protected bool CreatingNewDeployment
		{
			get;
			set;
		}

		protected Deployment CurrentDeployment
		{
			get;
			set;
		}

		protected Operation GetDeploymentOperation
		{
			get;
			set;
		}

		protected string GetDeploymentServiceName
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public virtual string ServiceName
		{
			get;
			set;
		}

		public IaaSDeploymentManagementCmdletBase()
		{
			this.CurrentDeployment = null;
			this.GetDeploymentOperation = null;
			this.CreatingNewDeployment = false;
		}

		protected override void ProcessRecord()
		{
			Func<string, Deployment> func = null;
			base.ProcessRecord();
			if (!string.IsNullOrEmpty(this.ServiceName))
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					new List<PersistentVMRoleContext>();
					try
					{
						IaaSDeploymentManagementCmdletBase iaaSDeploymentManagementCmdletBase = this;
						IaaSDeploymentManagementCmdletBase iaaSDeploymentManagementCmdletBase1 = this;
						if (func == null)
						{
							func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, "Production");
						}
						iaaSDeploymentManagementCmdletBase.CurrentDeployment = ((CmdletBase<IServiceManagement>)iaaSDeploymentManagementCmdletBase1).RetryCall<Deployment>(func);
						this.GetDeploymentOperation = base.WaitForOperation("Get Deployment");
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						if (communicationException as EndpointNotFoundException == null)
						{
							throw;
						}
						else
						{
							return;
						}
					}
				}
			}
		}

		protected virtual void ValidateParameters()
		{
		}
	}
}