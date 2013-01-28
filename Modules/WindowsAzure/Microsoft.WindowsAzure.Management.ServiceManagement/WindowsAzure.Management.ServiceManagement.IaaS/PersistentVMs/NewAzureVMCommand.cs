using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Threading;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("New", "AzureVM", DefaultParameterSetName="ExistingService")]
	public class NewAzureVMCommand : IaaSDeploymentManagementCmdletBase
	{
		private bool createdDeployment;

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="CreateService", HelpMessage="Required if Location is not specified. The name of an existing affinity group associated with this subscription.")]
		[ValidateNotNullOrEmpty]
		public string AffinityGroup
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="CreateService", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment Label. Will default to service name if not specified.")]
		[Parameter(Mandatory=false, ParameterSetName="ExistingService", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment Label. Will default to service name if not specified.")]
		[ValidateNotNullOrEmpty]
		public string DeploymentLabel
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="CreateService", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment Name. Will default to service name if not specified.")]
		[Parameter(Mandatory=false, ParameterSetName="ExistingService", ValueFromPipelineByPropertyName=true, HelpMessage="Deployment Name. Will default to service name if not specified.")]
		[ValidateNotNullOrEmpty]
		public string DeploymentName
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="ExistingService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="DNS Settings for Deployment.")]
		[Parameter(Mandatory=false, ParameterSetName="CreateService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="DNS Settings for Deployment.")]
		[ValidateNotNullOrEmpty]
		public DnsServer[] DnsSettings
		{
			get;
			set;
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CreateService", HelpMessage="Required if AffinityGroup is not specified. The data center region where the cloud service will be created.")]
		[ValidateNotNullOrEmpty]
		public string Location
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="CreateService", HelpMessage="A description for the cloud service. The description may be up to 1024 characters in length.")]
		[ValidateNotNullOrEmpty]
		public string ServiceDescription
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="CreateService", HelpMessage="The label may be up to 100 characters in length. Defaults to Service Name.")]
		[ValidateNotNullOrEmpty]
		public string ServiceLabel
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="ExistingService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service Name")]
		[Parameter(Mandatory=true, ParameterSetName="CreateService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="Service Name")]
		[ValidateNotNullOrEmpty]
		public override string ServiceName
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="ExistingService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="List of VMs to Deploy.")]
		[Parameter(Mandatory=true, ParameterSetName="CreateService", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, HelpMessage="List of VMs to Deploy.")]
		[ValidateNotNullOrEmpty]
		public PersistentVM[] VMs
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="ExistingService", HelpMessage="Virtual network name.")]
		[Parameter(Mandatory=false, ParameterSetName="CreateService", HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		public NewAzureVMCommand()
		{
		}

		public NewAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected bool DoesCloudServiceExist(string serviceName)
		{
			Func<string, AvailabilityResponse> func = null;
			bool result = false;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					NewAzureVMCommand newAzureVMCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.IsDNSAvailable(s, serviceName);
					}
					AvailabilityResponse availabilityResponse = ((CmdletBase<IServiceManagement>)newAzureVMCommand).RetryCall<AvailabilityResponse>(func);
					base.WaitForOperation(base.CommandRuntime.ToString(), true);
					result = !availabilityResponse.Result;
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null)
					{
						this.WriteErrorDetails(communicationException);
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		public void NewAzureVMProcess()
		{
			NewAzureVMCommand.NewAzureVMCommand variable = null;
			int num;
			List<PersistentVMRole> persistentVMRoles = new List<PersistentVMRole>();
			NewAzureVMCommand persistentVMRoles1 = this;
			var persistentVMs = new List<PersistentVMRole>();
			SubscriptionData currentSubscription = CmdletSubscriptionExtensions.GetCurrentSubscription(this);
			PersistentVM[] vMs = this.VMs;
			for (int i = 0; i < (int)vMs.Length; i++)
			{
				PersistentVM uri = vMs[i];
				if (uri.OSVirtualHardDisk.MediaLink == null && string.IsNullOrEmpty(uri.OSVirtualHardDisk.DiskName))
				{
					CloudStorageAccount currentStorageAccount = null;
					try
					{
						currentStorageAccount = currentSubscription.GetCurrentStorageAccount(base.Channel);
					}
					catch (EndpointNotFoundException endpointNotFoundException)
					{
						throw new ArgumentException("CurrentStorageAccount is not accessible. Ensure the current storage account is accessible and in the same location or affinity group as your cloud service.");
					}
					if (currentStorageAccount != null)
					{
						DateTime now = DateTime.Now;
						string roleName = uri.RoleName;
						if (uri.OSVirtualHardDisk.DiskLabel != null)
						{
							roleName = string.Concat(roleName, "-", uri.OSVirtualHardDisk.DiskLabel);
						}
						object[] serviceName = new object[6];
						serviceName[0] = this.ServiceName;
						serviceName[1] = roleName;
						serviceName[2] = now.Year;
						serviceName[3] = now.Month;
						serviceName[4] = now.Day;
						serviceName[5] = now.Millisecond;
						string str = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", serviceName);
						string absoluteUri = currentStorageAccount.BlobEndpoint.AbsoluteUri;
						if (!absoluteUri.EndsWith("/"))
						{
							absoluteUri = string.Concat(absoluteUri, "/");
						}
						uri.OSVirtualHardDisk.MediaLink = new Uri(string.Concat(absoluteUri, "vhds/", str));
					}
					else
					{
						throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storage account to set it.");
					}
				}
				foreach (DataVirtualHardDisk dataVirtualHardDisk in uri.DataVirtualHardDisks)
				{
					if (dataVirtualHardDisk.MediaLink == null && string.IsNullOrEmpty(dataVirtualHardDisk.DiskName))
					{
						CloudStorageAccount cloudStorageAccount = currentSubscription.GetCurrentStorageAccount(base.Channel);
						if (cloudStorageAccount != null)
						{
							DateTime dateTime = DateTime.Now;
							string roleName1 = uri.RoleName;
							if (dataVirtualHardDisk.DiskLabel != null)
							{
								roleName1 = string.Concat(roleName1, "-", dataVirtualHardDisk.DiskLabel);
							}
							object[] year = new object[6];
							year[0] = this.ServiceName;
							year[1] = roleName1;
							year[2] = dateTime.Year;
							year[3] = dateTime.Month;
							year[4] = dateTime.Day;
							year[5] = dateTime.Millisecond;
							string str1 = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", year);
							string absoluteUri1 = cloudStorageAccount.BlobEndpoint.AbsoluteUri;
							if (!absoluteUri1.EndsWith("/"))
							{
								absoluteUri1 = string.Concat(absoluteUri1, "/");
							}
							dataVirtualHardDisk.MediaLink = new Uri(string.Concat(absoluteUri1, "vhds/", str1));
						}
						else
						{
							throw new ArgumentException("CurrentStorageAccount is not set or not accessible. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
						}
					}
					if (uri.DataVirtualHardDisks.Count<DataVirtualHardDisk>() <= 1)
					{
						continue;
					}
					Thread.Sleep(1);
				}
				PersistentVMRole persistentVMRole = new PersistentVMRole();
				persistentVMRole.AvailabilitySetName = uri.AvailabilitySetName;
				persistentVMRole.ConfigurationSets = uri.ConfigurationSets;
				persistentVMRole.DataVirtualHardDisks = uri.DataVirtualHardDisks;
				persistentVMRole.OSVirtualHardDisk = uri.OSVirtualHardDisk;
				persistentVMRole.RoleName = uri.RoleName;
				persistentVMRole.RoleSize = uri.RoleSize;
				persistentVMRole.RoleType = uri.RoleType;
				persistentVMRole.Label = uri.Label;
				PersistentVMRole persistentVMRole1 = persistentVMRole;
				persistentVMRoles1.persistentVMs.Add(persistentVMRole1);
			}
			new List<string>();
			Operation operation = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					if (base.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase))
					{
						CreateHostedServiceInput createHostedServiceInput = new CreateHostedServiceInput();
						createHostedServiceInput.AffinityGroup = this.AffinityGroup;
						createHostedServiceInput.Location = this.Location;
						createHostedServiceInput.ServiceName = this.ServiceName;
						if (this.ServiceDescription != null)
						{
							createHostedServiceInput.Description = this.ServiceDescription;
						}
						else
						{
							DateTime now1 = DateTime.Now;
							DateTime universalTime = now1.ToUniversalTime();
							string str2 = string.Format("Implicitly created hosted service{0}", universalTime.ToString("yyyy-MM-dd HH:mm"));
							createHostedServiceInput.Description = str2;
						}
						if (this.ServiceLabel != null)
						{
							createHostedServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.ServiceLabel);
						}
						else
						{
							createHostedServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.ServiceName);
						}
						CmdletExtensions.WriteVerboseOutputForObject(this, createHostedServiceInput);
						base.RetryCall((string s) => base.Channel.CreateHostedService(s, createHostedServiceInput));
						Operation operation1 = base.WaitForOperation(string.Concat(base.CommandRuntime.ToString(), " - Create Cloud Service"));
						ManagementOperationContext managementOperationContext = new ManagementOperationContext();
						managementOperationContext.set_OperationDescription(string.Concat(base.CommandRuntime.ToString(), " - Create Cloud Service"));
						managementOperationContext.set_OperationId(operation1.OperationTrackingId);
						managementOperationContext.set_OperationStatus(operation1.Status);
						ManagementOperationContext managementOperationContext1 = managementOperationContext;
						base.WriteObject(managementOperationContext1, true);
					}
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
					return;
				}
			}
			if (operation == null || string.Compare(operation.Status, "Failed", StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (base.CurrentDeployment != null)
				{
					if (this.VNetName != null || this.DnsSettings != null || !string.IsNullOrEmpty(this.DeploymentLabel) || !string.IsNullOrEmpty(this.DeploymentName))
					{
						base.WriteWarning("VNetName, DnsSettings, DeploymentLabel or DeploymentName Name can only be specified on new deployments.");
					}
				}
				else
				{
					using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
					{
						try
						{
							if (string.IsNullOrEmpty(this.DeploymentName))
							{
								this.DeploymentName = this.ServiceName;
							}
							if (string.IsNullOrEmpty(this.DeploymentLabel))
							{
								this.DeploymentLabel = this.ServiceName;
							}
							Deployment deployment = new Deployment();
							deployment.DeploymentSlot = "Production";
							deployment.Name = this.DeploymentName;
							deployment.Label = this.DeploymentLabel;
							List<Role> roles = new List<Role>();
							roles.Add(persistentVMRoles1.persistentVMs[0]);
							deployment.RoleList = new RoleList(roles);
							deployment.VirtualNetworkName = this.VNetName;
							Deployment dnsSetting = deployment;
							if (this.DnsSettings != null)
							{
								dnsSetting.Dns = new DnsSettings();
								dnsSetting.Dns.DnsServers = new DnsServerList();
								DnsServer[] dnsSettings = this.DnsSettings;
								for (int j = 0; j < (int)dnsSettings.Length; j++)
								{
									DnsServer dnsServer = dnsSettings[j];
									dnsSetting.Dns.DnsServers.Add(dnsServer);
								}
							}
							CmdletExtensions.WriteVerboseOutputForObject(this, dnsSetting);
							base.RetryCall((string s) => base.Channel.CreateDeployment(s, this.ServiceName, dnsSetting));
							Operation operation2 = base.WaitForOperation(string.Concat(base.CommandRuntime.ToString(), " - Create Deployment with VM ", persistentVMRoles1.persistentVMs[0].RoleName));
							ManagementOperationContext managementOperationContext2 = new ManagementOperationContext();
							managementOperationContext2.set_OperationDescription(string.Concat(base.CommandRuntime.ToString(), " - Create Deployment with VM ", persistentVMRoles1.persistentVMs[0].RoleName));
							managementOperationContext2.set_OperationId(operation2.OperationTrackingId);
							managementOperationContext2.set_OperationStatus(operation2.Status);
							ManagementOperationContext managementOperationContext3 = managementOperationContext2;
							base.WriteObject(managementOperationContext3, true);
						}
						catch (CommunicationException communicationException3)
						{
							CommunicationException communicationException2 = communicationException3;
							if (communicationException2 as EndpointNotFoundException == null)
							{
								this.WriteErrorDetails(communicationException2);
								return;
							}
							else
							{
								throw new Exception("Cloud Service does not exist. Specify -Location or -AffinityGroup to create one.");
							}
						}
						this.createdDeployment = true;
					}
				}
				if (!this.createdDeployment && base.CurrentDeployment != null)
				{
					this.DeploymentName = base.CurrentDeployment.Name;
				}
				if (this.createdDeployment)
				{
					num = 1;
				}
				else
				{
					num = 0;
				}
				int num1 = num;
				Action<string> action = null;
				while (num1 < persistentVMRoles1.persistentVMs.Count)
				{
					if (operation == null || string.Compare(operation.Status, "Failed", StringComparison.OrdinalIgnoreCase) != 0)
					{
						using (OperationContextScope operationContextScope2 = new OperationContextScope((IContextChannel)base.Channel))
						{
							try
							{
								CmdletExtensions.WriteVerboseOutputForObject(this, persistentVMRoles1.persistentVMs[num1]);
								NewAzureVMCommand newAzureVMCommand = this;
								if (action == null)
								{
									action = (string s) => base.Channel.AddRole(s, this.ServiceName, this.DeploymentName, persistentVMRoles[num1]);
								}
								((CmdletBase<IServiceManagement>)newAzureVMCommand).RetryCall(action);
								Operation operation3 = base.WaitForOperation(string.Concat(base.CommandRuntime.ToString(), " - Create VM ", persistentVMRoles1.persistentVMs[num1].RoleName));
								ManagementOperationContext managementOperationContext4 = new ManagementOperationContext();
								managementOperationContext4.set_OperationDescription(string.Concat(base.CommandRuntime.ToString(), " - Create VM ", persistentVMRoles1.persistentVMs[num1].RoleName));
								managementOperationContext4.set_OperationId(operation3.OperationTrackingId);
								managementOperationContext4.set_OperationStatus(operation3.Status);
								ManagementOperationContext managementOperationContext5 = managementOperationContext4;
								base.WriteObject(managementOperationContext5, true);
							}
							catch (CommunicationException communicationException5)
							{
								CommunicationException communicationException4 = communicationException5;
								this.WriteErrorDetails(communicationException4);
								return;
							}
						}
						NewAzureVMCommand.NewAzureVMCommand variable1 = variable;
						variable1.i = variable1.i + 1;
					}
					else
					{
						return;
					}
				}
				return;
			}
		}

		protected override void ProcessRecord()
		{
			try
			{
				this.ValidateParameters();
				base.ProcessRecord();
				this.NewAzureVMProcess();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		protected override void ValidateParameters()
		{
			base.ValidateParameters();
			if (!base.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(this.Location) && !string.IsNullOrEmpty(this.AffinityGroup))
				{
					throw new ArgumentException("Location or AffinityGroup can only be specified when creating a new cloud service.");
				}
			}
			else
			{
				if (string.IsNullOrEmpty(this.Location) && string.IsNullOrEmpty(this.AffinityGroup))
				{
					throw new ArgumentException("Location or AffinityGroup is required when creating a new Cloud Service.");
				}
			}
			if (!base.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(this.VNetName) || !string.IsNullOrEmpty(this.AffinityGroup))
			{
				if ((base.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) || base.ParameterSetName.Equals("CreateDeployment", StringComparison.OrdinalIgnoreCase)) && this.DnsSettings != null && string.IsNullOrEmpty(this.VNetName))
				{
					throw new ArgumentException("VNetName is required when specifying DNS Settings.");
				}
				else
				{
					PersistentVM[] vMs = this.VMs;
					int num = 0;
					while (num < (int)vMs.Length)
					{
						PersistentVM persistentVM = vMs[num];
						ProvisioningConfigurationSet provisioningConfigurationSet = persistentVM.ConfigurationSets.OfType<ProvisioningConfigurationSet>().SingleOrDefault<ProvisioningConfigurationSet>();
						if (provisioningConfigurationSet != null || persistentVM.OSVirtualHardDisk.SourceImageName == null)
						{
							num++;
						}
						else
						{
							throw new ArgumentException(string.Format("Virtual Machine {0} is missing provisioning configuration", persistentVM.RoleName));
						}
					}
					return;
				}
			}
			else
			{
				throw new ArgumentException("Must specify the same affinity group as the virtual network is deployed to.");
			}
		}
	}
}