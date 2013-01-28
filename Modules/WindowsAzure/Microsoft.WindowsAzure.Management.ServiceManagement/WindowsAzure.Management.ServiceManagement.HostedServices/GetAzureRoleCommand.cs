using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
	[Cmdlet("Get", "AzureRole")]
	public class GetAzureRoleCommand : ServiceManagementCmdletBase
	{
		private Deployment currentDeployment;

		private Operation getDeploymentOperation;

		[Parameter(Position=3, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Get Instance Details")]
		public SwitchParameter InstanceDetails
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the role.")]
		public string RoleName
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the hosted service.")]
		public string ServiceName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Deployment slot")]
		[ValidateSet(new string[] { "Staging", "Production" }, IgnoreCase=true)]
		public string Slot
		{
			get;
			set;
		}

		public GetAzureRoleCommand()
		{
		}

		public GetAzureRoleCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		private void GetCurrentDeployment()
		{
			Func<string, Deployment> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					new List<PersistentVMRoleContext>();
					GetAzureRoleCommand getAzureRoleCommand = this;
					GetAzureRoleCommand getAzureRoleCommand1 = this;
					if (func == null)
					{
						func = (string s) => base.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot);
					}
					getAzureRoleCommand.currentDeployment = ((CmdletBase<IServiceManagement>)getAzureRoleCommand1).RetryCall<Deployment>(func);
					this.getDeploymentOperation = base.WaitForOperation("Get Deployment");
				}
				catch (CommunicationException communicationException)
				{
					throw;
				}
			}
		}

		public void GetRoleProcess()
		{
			RoleList roleList;
			RoleInstanceList roleInstanceList;
			Func<Role, bool> func = null;
			Func<RoleInstance, bool> func1 = null;
			this.GetCurrentDeployment();
			if (this.currentDeployment != null)
			{
				SwitchParameter instanceDetails = this.InstanceDetails;
				if (instanceDetails.IsPresent)
				{
					Collection<RoleInstanceContext> roleInstanceContexts = new Collection<RoleInstanceContext>();
					if (!string.IsNullOrEmpty(this.RoleName))
					{
						RoleInstanceList roleInstanceList1 = this.currentDeployment.RoleInstanceList;
						if (func1 == null)
						{
							func1 = (RoleInstance r) => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase);
						}
						roleInstanceList = new RoleInstanceList(roleInstanceList1.Where<RoleInstance>(func1));
					}
					else
					{
						roleInstanceList = this.currentDeployment.RoleInstanceList;
					}
					foreach (RoleInstance roleInstance in roleInstanceList)
					{
						RoleInstanceContext roleInstanceContext = new RoleInstanceContext();
						roleInstanceContext.ServiceName = this.ServiceName;
						roleInstanceContext.set_OperationId(this.getDeploymentOperation.OperationTrackingId);
						roleInstanceContext.set_OperationDescription(base.CommandRuntime.ToString());
						roleInstanceContext.set_OperationStatus(this.getDeploymentOperation.Status);
						roleInstanceContext.InstanceErrorCode = roleInstance.InstanceErrorCode;
						roleInstanceContext.InstanceFaultDomain = roleInstance.InstanceFaultDomain;
						roleInstanceContext.InstanceName = roleInstance.InstanceName;
						roleInstanceContext.InstanceSize = roleInstance.InstanceSize;
						roleInstanceContext.InstanceStateDetails = roleInstance.InstanceStateDetails;
						roleInstanceContext.InstanceStatus = roleInstance.InstanceStatus;
						roleInstanceContext.InstanceUpgradeDomain = roleInstance.InstanceUpgradeDomain;
						roleInstanceContext.RoleName = roleInstance.RoleName;
						roleInstanceContext.DeploymentID = this.currentDeployment.PrivateID;
						roleInstanceContext.InstanceEndpoints = roleInstance.InstanceEndpoints;
						RoleInstanceContext roleInstanceContext1 = roleInstanceContext;
						roleInstanceContexts.Add(roleInstanceContext1);
					}
					base.WriteObject(roleInstanceContexts, true);
				}
				else
				{
					Collection<RoleContext> roleContexts = new Collection<RoleContext>();
					if (!string.IsNullOrEmpty(this.RoleName))
					{
						RoleList roleList1 = this.currentDeployment.RoleList;
						if (func == null)
						{
							func = (Role r) => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase);
						}
						roleList = new RoleList(roleList1.Where<Role>(func));
					}
					else
					{
						roleList = this.currentDeployment.RoleList;
					}
					List<Role>.Enumerator enumerator = roleList.GetEnumerator();
					try
					{
						Func<RoleInstance, bool> func2 = null;
						while (enumerator.MoveNext())
						{
							Role current = enumerator.Current;
							RoleContext roleContext = new RoleContext();
							RoleContext roleContext1 = roleContext;
							RoleInstanceList roleInstanceList2 = this.currentDeployment.RoleInstanceList;
							if (func2 == null)
							{
								func2 = (RoleInstance ri) => ri.RoleName.Equals(current.RoleName, StringComparison.OrdinalIgnoreCase);
							}
							roleContext1.InstanceCount = roleInstanceList2.Where<RoleInstance>(func2).Count<RoleInstance>();
							roleContext.RoleName = current.RoleName;
							roleContext.set_OperationDescription(base.CommandRuntime.ToString());
							roleContext.set_OperationStatus(this.getDeploymentOperation.Status);
							roleContext.set_OperationId(this.getDeploymentOperation.OperationTrackingId);
							roleContext.ServiceName = this.ServiceName;
							roleContext.DeploymentID = this.currentDeployment.PrivateID;
							RoleContext roleContext2 = roleContext;
							roleContexts.Add(roleContext2);
						}
					}
					finally
					{
						enumerator.Dispose();
					}
					base.WriteObject(roleContexts, true);
					return;
				}
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.GetRoleProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}