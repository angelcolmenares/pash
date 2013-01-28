using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("New", "AzureQuickVM", DefaultParameterSetName="Windows")]
	public class NewQuickVM : IaaSDeploymentManagementCmdletBase
	{
		private bool CreatedDeployment;

		[Parameter(Mandatory=false, HelpMessage="Use when creating the first virtual machine in a cloud service (or specify location). The name of an existing affinity group associated with this subscription.")]
		[ValidateNotNullOrEmpty]
		public string AffinityGroup
		{
			get;
			set;
		}

		[Parameter(HelpMessage="The name of the availability set.")]
		[ValidateNotNullOrEmpty]
		public string AvailabilitySetName
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Set of certificates to install in the VM.")]
		[ValidateNotNullOrEmpty]
		public CertificateSettingList Certificates
		{
			get;
			set;
		}

		[Parameter(HelpMessage="DNS Settings for Deployment.")]
		[ValidateNotNullOrEmpty]
		public DnsServer[] DnsSettings
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Controls the platform caching behavior of the OS disk.")]
		[ValidateSet(new string[] { "ReadWrite", "ReadOnly" }, IgnoreCase=true)]
		public string HostCaching
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, HelpMessage="Reference to a platform stock image or a user image from the image repository.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Represents the size of the machine.")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge" }, IgnoreCase=true)]
		public string InstanceSize
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Linux", HelpMessage="Create a Linux VM")]
		public SwitchParameter Linux
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Linux", HelpMessage="User to Create")]
		[ValidateNotNullOrEmpty]
		public string LinuxUser
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, HelpMessage="Use when creating the first virtual machine in a cloud service (or specify affinity group).  The data center region where the cloud service will be created.")]
		[ValidateNotNullOrEmpty]
		public string Location
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Location of the where the VHD should be created. This link refers to a blob in a storage account. If not specified the VHD will be created in the current storage account in the vhds container.")]
		[ValidateNotNullOrEmpty]
		public string MediaLocation
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, HelpMessage="Virtual Machine Name")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, HelpMessage="Administrator password to use for the role.")]
		[ValidateNotNullOrEmpty]
		public string Password
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, HelpMessage="Service Name")]
		[ValidateNotNullOrEmpty]
		public override string ServiceName
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="SSH Key Pairs")]
		public LinuxProvisioningConfigurationSet.SSHKeyPairList SSHKeyPairs
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="SSH Public Key List")]
		public LinuxProvisioningConfigurationSet.SSHPublicKeyList SSHPublicKeys
		{
			get;
			set;
		}

		[AllowEmptyCollection]
		[AllowNull]
		[Parameter(Mandatory=false, HelpMessage="The list of subnet names.")]
		public string[] SubnetNames
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Virtual network name.")]
		public string VNetName
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Windows", HelpMessage="Create a Windows VM")]
		public SwitchParameter Windows
		{
			get;
			set;
		}

		public NewQuickVM()
		{
		}

		public NewQuickVM(IServiceManagement channel)
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
					NewQuickVM newQuickVM = this;
					if (func == null)
					{
						func = (string s) => base.Channel.IsDNSAvailable(s, serviceName);
					}
					AvailabilityResponse availabilityResponse = ((CmdletBase<IServiceManagement>)newQuickVM).RetryCall<AvailabilityResponse>(func);
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
			NewQuickVM.NewQuickVM variable = null;
			string serviceName;
			string instanceSize;
			Uri uri;
			string name;
			string str;
			Action<string> action = null;
			SubscriptionData currentSubscription = CmdletSubscriptionExtensions.GetCurrentSubscription(this);
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
				NewQuickVM.NewQuickVM variable1 = variable;
				PersistentVMRole persistentVMRole = new PersistentVMRole();
				persistentVMRole.AvailabilitySetName = this.AvailabilitySetName;
				persistentVMRole.ConfigurationSets = new Collection<ConfigurationSet>();
				persistentVMRole.DataVirtualHardDisks = new Collection<DataVirtualHardDisk>();
				PersistentVMRole persistentVMRole1 = persistentVMRole;
				if (string.IsNullOrEmpty(this.Name))
				{
					serviceName = this.ServiceName;
				}
				else
				{
					serviceName = this.Name;
				}
				persistentVMRole1.RoleName = serviceName;
				PersistentVMRole persistentVMRole2 = persistentVMRole;
				if (string.IsNullOrEmpty(this.InstanceSize))
				{
					instanceSize = null;
				}
				else
				{
					instanceSize = this.InstanceSize;
				}
				persistentVMRole2.RoleSize = instanceSize;
				persistentVMRole.RoleType = "PersistentVMRole";
				persistentVMRole.Label = ServiceManagementHelper.EncodeToBase64String(this.ServiceName);
				variable1.vm = persistentVMRole;
				PersistentVMRole persistentVMRole3 = uri1;
				OSVirtualHardDisk oSVirtualHardDisk = new OSVirtualHardDisk();
				oSVirtualHardDisk.DiskName = null;
				oSVirtualHardDisk.SourceImageName = this.ImageName;
				OSVirtualHardDisk oSVirtualHardDisk1 = oSVirtualHardDisk;
				if (string.IsNullOrEmpty(this.MediaLocation))
				{
					uri = null;
				}
				else
				{
					uri = new Uri(this.MediaLocation);
				}
				oSVirtualHardDisk1.MediaLink = uri;
				oSVirtualHardDisk.HostCaching = this.HostCaching;
				persistentVMRole3.OSVirtualHardDisk = oSVirtualHardDisk;
				if (oSVirtualHardDisk1.MediaLink == null && string.IsNullOrEmpty(oSVirtualHardDisk1.DiskName))
				{
					DateTime now = DateTime.Now;
					object[] roleName = new object[6];
					roleName[0] = this.ServiceName;
					roleName[1] = uri1.RoleName;
					roleName[2] = now.Year;
					roleName[3] = now.Month;
					roleName[4] = now.Day;
					roleName[5] = now.Millisecond;
					string str1 = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", roleName);
					string absoluteUri = currentStorageAccount.BlobEndpoint.AbsoluteUri;
					if (!absoluteUri.EndsWith("/"))
					{
						absoluteUri = string.Concat(absoluteUri, "/");
					}
					oSVirtualHardDisk1.MediaLink = new Uri(string.Concat(absoluteUri, "vhds/", str1));
				}
				NetworkConfigurationSet networkConfigurationSet = new NetworkConfigurationSet();
				networkConfigurationSet.InputEndpoints = new Collection<InputEndpoint>();
				if (this.SubnetNames != null)
				{
					networkConfigurationSet.SubnetNames = new SubnetNamesCollection();
					string[] subnetNames = this.SubnetNames;
					for (int i = 0; i < (int)subnetNames.Length; i++)
					{
						string str2 = subnetNames[i];
						networkConfigurationSet.SubnetNames.Add(str2);
					}
				}
				if (!base.ParameterSetName.Equals("Windows", StringComparison.OrdinalIgnoreCase))
				{
					LinuxProvisioningConfigurationSet linuxProvisioningConfigurationSet = new LinuxProvisioningConfigurationSet();
					LinuxProvisioningConfigurationSet linuxProvisioningConfigurationSet1 = linuxProvisioningConfigurationSet;
					if (string.IsNullOrEmpty(this.Name))
					{
						name = this.ServiceName;
					}
					else
					{
						name = this.Name;
					}
					linuxProvisioningConfigurationSet1.HostName = name;
					linuxProvisioningConfigurationSet.UserName = this.LinuxUser;
					linuxProvisioningConfigurationSet.UserPassword = this.Password;
					linuxProvisioningConfigurationSet.DisableSshPasswordAuthentication = new bool?(false);
					if (this.SSHKeyPairs != null && this.SSHKeyPairs.Count > 0 || this.SSHPublicKeys != null && this.SSHPublicKeys.Count > 0)
					{
						linuxProvisioningConfigurationSet.SSH = new LinuxProvisioningConfigurationSet.SSHSettings();
						linuxProvisioningConfigurationSet.SSH.PublicKeys = this.SSHPublicKeys;
						linuxProvisioningConfigurationSet.SSH.KeyPairs = this.SSHKeyPairs;
					}
					InputEndpoint inputEndpoint = new InputEndpoint();
					inputEndpoint.LocalPort = 22;
					inputEndpoint.Protocol = "tcp";
					inputEndpoint.Name = "SSH";
					networkConfigurationSet.InputEndpoints.Add(inputEndpoint);
					uri1.ConfigurationSets.Add(linuxProvisioningConfigurationSet);
					uri1.ConfigurationSets.Add(networkConfigurationSet);
				}
				else
				{
					WindowsProvisioningConfigurationSet windowsProvisioningConfigurationSet = new WindowsProvisioningConfigurationSet();
					windowsProvisioningConfigurationSet.AdminPassword = this.Password;
					WindowsProvisioningConfigurationSet windowsProvisioningConfigurationSet1 = windowsProvisioningConfigurationSet;
					if (string.IsNullOrEmpty(this.Name))
					{
						str = this.ServiceName;
					}
					else
					{
						str = this.Name;
					}
					windowsProvisioningConfigurationSet1.ComputerName = str;
					windowsProvisioningConfigurationSet.EnableAutomaticUpdates = new bool?(true);
					windowsProvisioningConfigurationSet.ResetPasswordOnFirstLogon = false;
					windowsProvisioningConfigurationSet.StoredCertificateSettings = this.Certificates;
					InputEndpoint inputEndpoint1 = new InputEndpoint();
					inputEndpoint1.LocalPort = 0xd3d;
					inputEndpoint1.Protocol = "tcp";
					inputEndpoint1.Name = "RemoteDesktop";
					networkConfigurationSet.InputEndpoints.Add(inputEndpoint1);
					uri1.ConfigurationSets.Add(windowsProvisioningConfigurationSet);
					uri1.ConfigurationSets.Add(networkConfigurationSet);
				}
				new List<string>();
				Operation operation = null;
				bool flag = this.DoesCloudServiceExist(this.ServiceName);
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					try
					{
						DateTime dateTime = DateTime.Now;
						DateTime universalTime = dateTime.ToUniversalTime();
						string str3 = string.Format("Implicitly created hosted service{0}", universalTime.ToString("yyyy-MM-dd HH:mm"));
						if (!string.IsNullOrEmpty(this.Location) || !string.IsNullOrEmpty(this.AffinityGroup) || !string.IsNullOrEmpty(this.VNetName) && !flag)
						{
							CreateHostedServiceInput createHostedServiceInput = new CreateHostedServiceInput();
							createHostedServiceInput.AffinityGroup = this.AffinityGroup;
							createHostedServiceInput.Location = this.Location;
							createHostedServiceInput.ServiceName = this.ServiceName;
							createHostedServiceInput.Description = str3;
							createHostedServiceInput.Label = ServiceManagementHelper.EncodeToBase64String(this.ServiceName);
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
						if (this.VNetName != null || this.DnsSettings != null)
						{
							base.WriteWarning("VNetName or DnsSettings can only be specified on new deployments.");
						}
					}
					else
					{
						using (OperationContextScope operationContextScope1 = new OperationContextScope((IContextChannel)base.Channel))
						{
							try
							{
								Deployment deployment = new Deployment();
								deployment.DeploymentSlot = "Production";
								deployment.Name = this.ServiceName;
								deployment.Label = this.ServiceName;
								List<Role> roles = new List<Role>();
								roles.Add(uri1);
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
								Operation operation2 = base.WaitForOperation(string.Concat(base.CommandRuntime.ToString(), " - Create Deployment with VM ", uri1.RoleName));
								ManagementOperationContext managementOperationContext2 = new ManagementOperationContext();
								managementOperationContext2.set_OperationDescription(string.Concat(base.CommandRuntime.ToString(), " - Create Deployment with VM ", uri1.RoleName));
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
									throw new Exception("Cloud Service does not exist. Specify -Location or -Affinity group to create one.");
								}
							}
							this.CreatedDeployment = true;
						}
					}
					if (operation == null || string.Compare(operation.Status, "Failed", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (!this.CreatedDeployment)
						{
							using (OperationContextScope operationContextScope2 = new OperationContextScope((IContextChannel)base.Channel))
							{
								try
								{
									CmdletExtensions.WriteVerboseOutputForObject(this, uri1);
									NewQuickVM newQuickVM = this;
									if (action == null)
									{
										action = (string s) => base.Channel.AddRole(s, this.ServiceName, this.ServiceName, uri1);
									}
									((CmdletBase<IServiceManagement>)newQuickVM).RetryCall(action);
									Operation operation3 = base.WaitForOperation(string.Concat(base.CommandRuntime.ToString(), " - Create VM ", uri1.RoleName));
									ManagementOperationContext managementOperationContext4 = new ManagementOperationContext();
									managementOperationContext4.set_OperationDescription(string.Concat(base.CommandRuntime.ToString(), " - Create VM ", uri1.RoleName));
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
						}
						return;
					}
					else
					{
						return;
					}
				}
				return;
			}
			else
			{
				throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
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
			bool flag;
			base.ValidateParameters();
			if (this.DnsSettings == null || !string.IsNullOrEmpty(this.VNetName))
			{
				if (!base.ParameterSetName.Contains("Linux") || !string.IsNullOrEmpty(this.LinuxUser))
				{
					if (!base.ParameterSetName.Contains("Linux") || ValidationHelpers.IsLinuxPasswordValid(this.Password))
					{
						if (!base.ParameterSetName.Contains("Windows") || ValidationHelpers.IsWindowsPasswordValid(this.Password))
						{
							if (base.ParameterSetName.Contains("Linux"))
							{
								if (!string.IsNullOrEmpty(this.Name))
								{
									flag = ValidationHelpers.IsLinuxHostNameValid(this.Name);
								}
								else
								{
									flag = ValidationHelpers.IsLinuxHostNameValid(this.ServiceName);
								}
								if (!flag)
								{
									throw new ArgumentException("Hostname is invalid.");
								}
							}
							if (string.IsNullOrEmpty(this.Name))
							{
								if (base.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.ServiceName))
								{
									throw new ArgumentException("Computer Name is invalid.");
								}
							}
							else
							{
								if (base.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.Name))
								{
									throw new ArgumentException("Computer Name is invalid.");
								}
							}
							if (string.IsNullOrEmpty(this.VNetName) || !string.IsNullOrEmpty(this.Location) || !string.IsNullOrEmpty(this.AffinityGroup))
							{
								return;
							}
							else
							{
								throw new ArgumentException("Virtual Network Name may only be specified on the initial deployment. Specify Location or Affinity Group to create a new cloud service and deployment.");
							}
						}
						else
						{
							throw new ArgumentException("Password does not meet complexity requirements.");
						}
					}
					else
					{
						throw new ArgumentException("Password does not meet complexity requirements.");
					}
				}
				else
				{
					throw new ArgumentException("Specify -LinuxUser when creating Linux Virtual Machines");
				}
			}
			else
			{
				throw new ArgumentException("VNetName is required when specifying DNS Settings.");
			}
		}
	}
}