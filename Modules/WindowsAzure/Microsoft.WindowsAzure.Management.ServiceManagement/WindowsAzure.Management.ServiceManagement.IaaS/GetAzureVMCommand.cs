using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVM")]
	public class GetAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, HelpMessage="The name of the virtual machine to get.")]
		public virtual string Name
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Service name.")]
		[ValidateNotNullOrEmpty]
		public override string ServiceName
		{
			get;
			set;
		}

		public GetAzureVMCommand()
		{
		}

		public GetAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<PersistentVMRoleContext> GetVirtualMachineProcess()
		{
			RoleList roleList;
			GetAzureVMCommand.GetAzureVMCommand variable = null;
			IEnumerable<PersistentVMRoleContext> persistentVMRoleContexts;
			Func<Role, bool> func = null;
			if (string.IsNullOrEmpty(this.ServiceName) || base.CurrentDeployment != null)
			{
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						List<PersistentVMRoleContext> persistentVMRoleContexts1 = new List<PersistentVMRoleContext>();
						if (!string.IsNullOrEmpty(this.ServiceName))
						{
							if (!string.IsNullOrEmpty(this.Name))
							{
								RoleList roleList1 = base.CurrentDeployment.RoleList;
								if (func == null)
								{
									func = (Role r) => r.RoleName.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase);
								}
								roleList = new RoleList(roleList1.Where<Role>(func));
							}
							else
							{
								roleList = base.CurrentDeployment.RoleList;
							}
							for (int i = 0; i < roleList.Count; i++)
							{
								string empty = string.Empty;
								try
								{
									empty = roleList[i].RoleName;
									PersistentVMRole item = (PersistentVMRole)roleList[i];
									PersistentVMRoleContext persistentVMRoleContext = new PersistentVMRoleContext();
									if (base.CurrentDeployment != null)
									{
										persistentVMRoleContext.DNSName = base.CurrentDeployment.Url.AbsoluteUri;
									}
									persistentVMRoleContext.ServiceName = this.ServiceName;
									persistentVMRoleContext.Name = item.RoleName;
									persistentVMRoleContext.DeploymentName = base.CurrentDeployment.Name;
									persistentVMRoleContext.VM = new PersistentVM();
									persistentVMRoleContext.VM.AvailabilitySetName = item.AvailabilitySetName;
									persistentVMRoleContext.AvailabilitySetName = item.AvailabilitySetName;
									persistentVMRoleContext.Label = item.Label;
									persistentVMRoleContext.VM.ConfigurationSets = item.ConfigurationSets;
									persistentVMRoleContext.VM.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
									persistentVMRoleContext.VM.DataVirtualHardDisks = item.DataVirtualHardDisks;
									persistentVMRoleContext.VM.Label = item.Label;
									persistentVMRoleContext.VM.OSVirtualHardDisk = item.OSVirtualHardDisk;
									persistentVMRoleContext.VM.RoleName = item.RoleName;
									persistentVMRoleContext.Name = item.RoleName;
									persistentVMRoleContext.VM.RoleSize = item.RoleSize;
									persistentVMRoleContext.InstanceSize = item.RoleSize;
									persistentVMRoleContext.VM.RoleType = item.RoleType;
									persistentVMRoleContext.InstanceStatus = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceStatus;
									persistentVMRoleContext.IpAddress = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == this.vm.RoleName).First<RoleInstance>().IpAddress;
									persistentVMRoleContext.InstanceStateDetails = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceStateDetails;
									persistentVMRoleContext.PowerState = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().PowerState;
									persistentVMRoleContext.InstanceErrorCode = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceErrorCode;
									persistentVMRoleContext.InstanceName = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceName;
									int? instanceFaultDomain = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceFaultDomain;
									int value = instanceFaultDomain.Value;
									persistentVMRoleContext.InstanceFaultDomain = value.ToString();
									int? instanceUpgradeDomain = base.CurrentDeployment.RoleInstanceList.Where<RoleInstance>((RoleInstance r) => r.RoleName == item.RoleName).First<RoleInstance>().InstanceUpgradeDomain;
									int num = instanceUpgradeDomain.Value;
									persistentVMRoleContext.InstanceUpgradeDomain = num.ToString();
									persistentVMRoleContext.set_OperationDescription(base.CommandRuntime.ToString());
									persistentVMRoleContext.set_OperationId(base.GetDeploymentOperation.OperationTrackingId);
									persistentVMRoleContext.set_OperationStatus(base.GetDeploymentOperation.Status);
									persistentVMRoleContexts1.Add(persistentVMRoleContext);
								}
								catch (Exception exception)
								{
									base.WriteObject(string.Format("Could not read properties for virtual machine: {0}. It may still be provisioning.", empty));
								}
							}
							if (!string.IsNullOrEmpty(this.Name) && persistentVMRoleContexts1 != null && persistentVMRoleContexts1.Count > 0)
							{
								this.SaveRoleState(persistentVMRoleContexts1[0].VM);
							}
							persistentVMRoleContexts = persistentVMRoleContexts1;
							return persistentVMRoleContexts;
						}
						else
						{
							this.ListAllVMs();
							persistentVMRoleContexts = null;
							return persistentVMRoleContexts;
						}
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
						{
							this.WriteErrorDetails(communicationException);
						}
						else
						{
							persistentVMRoleContexts = null;
							return persistentVMRoleContexts;
						}
					}
					persistentVMRoleContexts = null;
				}
				return persistentVMRoleContexts;
			}
			else
			{
				return null;
			}
		}

		private void ListAllVMs()
		{
			Func<string, HostedServiceList> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				GetAzureVMCommand getAzureVMCommand = this;
				if (func == null)
				{
					func = (string s) => base.Channel.ListHostedServices(s);
				}
				HostedServiceList hostedServiceList = ((CmdletBase<IServiceManagement>)getAzureVMCommand).RetryCall<HostedServiceList>(func);
				if (hostedServiceList != null)
				{
					List<HostedService>.Enumerator enumerator = hostedServiceList.GetEnumerator();
					try
					{
						Func<string, Deployment> func1 = null;
						while (enumerator.MoveNext())
						{
							HostedService current = enumerator.Current;
							using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
							{
								try
								{
									GetAzureVMCommand getAzureVMCommand1 = this;
									if (func1 == null)
									{
										func1 = (string s) => base.Channel.GetDeploymentBySlot(s, current.ServiceName, "Production");
									}
									Deployment deployment = ((CmdletBase<IServiceManagement>)getAzureVMCommand1).RetryCall<Deployment>(func1);
									List<Role>.Enumerator enumerator1 = deployment.RoleList.GetEnumerator();
									try
									{
										Func<RoleInstance, bool> func2 = null;
										while (enumerator1.MoveNext())
										{
											Role role = enumerator1.Current;
											if (role.RoleType != "PersistentVMRole")
											{
												continue;
											}
											RoleInstanceList roleInstanceList = deployment.RoleInstanceList;
											if (func2 == null)
											{
												func2 = (RoleInstance r) => r.RoleName == role.RoleName;
											}
											RoleInstance roleInstance = roleInstanceList.Where<RoleInstance>(func2).First<RoleInstance>();
											PersistentVMRoleListContext persistentVMRoleListContext = new PersistentVMRoleListContext();
											persistentVMRoleListContext.ServiceName = current.ServiceName;
											persistentVMRoleListContext.Status = roleInstance.InstanceStatus;
											persistentVMRoleListContext.Name = roleInstance.RoleName;
											PersistentVMRoleListContext persistentVMRoleListContext1 = persistentVMRoleListContext;
											base.WriteObject(persistentVMRoleListContext1, true);
										}
									}
									finally
									{
										enumerator1.Dispose();
									}
								}
								catch (CommunicationException communicationException1)
								{
									CommunicationException communicationException = communicationException1;
									if (communicationException as EndpointNotFoundException == null)
									{
										throw;
									}
								}
							}
						}
					}
					finally
					{
						enumerator.Dispose();
					}
				}
			}
		}

		private IEnumerable<string> ListVMRoles()
		{
			IEnumerable<string> strs;
			Func<string, Deployment> func = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				GetAzureVMCommand getAzureVMCommand = this;
				if (func == null)
				{
					func = (string s) => base.Channel.GetDeployment(s, this.ServiceName, base.CurrentDeployment.Name);
				}
				Deployment deployment = ((CmdletBase<IServiceManagement>)getAzureVMCommand).RetryCall<Deployment>(func);
				RoleList roleList = deployment.RoleList;
				IEnumerable<Role> roles = roleList.Where<Role>((Role r) => r.RoleType == "PersistentVMRole");
				strs = roles.Select<Role, string>((Role r) => r.RoleName);
			}
			return strs;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				IEnumerable<PersistentVMRoleContext> virtualMachineProcess = this.GetVirtualMachineProcess();
				if (virtualMachineProcess != null)
				{
					base.WriteObject(virtualMachineProcess, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (exception as EndpointNotFoundException == null || base.IsVerbose())
				{
					base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
				}
				else
				{
					base.WriteObject(null);
				}
			}
		}

		protected virtual void SaveRoleState(PersistentVM role)
		{
		}
	}
}