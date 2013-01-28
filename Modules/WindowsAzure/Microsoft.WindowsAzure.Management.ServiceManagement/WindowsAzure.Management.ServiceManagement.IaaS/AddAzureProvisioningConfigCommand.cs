using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.ServiceManagement.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Add", "AzureProvisioningConfig", DefaultParameterSetName="Windows")]
	public class AddAzureProvisioningConfigCommand : ProvisioningConfigurationCmdletBase
	{
		[Alias(new string[] { "InputObject" })]
		[Parameter(Mandatory=true, ValueFromPipeline=true, HelpMessage="Virtual Machine to update.")]
		[ValidateNotNullOrEmpty]
		public IPersistentVM VM
		{
			get;
			set;
		}

		public AddAzureProvisioningConfigCommand()
		{
		}

		protected override void ProcessRecord()
		{
			bool flag;
			bool flag1;
			try
			{
				base.ProcessRecord();
				PersistentVM instance = this.VM.GetInstance();
				SwitchParameter linux = base.Linux;
				if (!linux.IsPresent)
				{
					WindowsProvisioningConfigurationSet windowsProvisioningConfigurationSet = instance.ConfigurationSets.OfType<WindowsProvisioningConfigurationSet>().SingleOrDefault<WindowsProvisioningConfigurationSet>();
					if (windowsProvisioningConfigurationSet == null)
					{
						windowsProvisioningConfigurationSet = new WindowsProvisioningConfigurationSet();
						instance.ConfigurationSets.Add(windowsProvisioningConfigurationSet);
					}
					base.SetProvisioningConfiguration(windowsProvisioningConfigurationSet);
					windowsProvisioningConfigurationSet.ComputerName = instance.RoleName;
					SwitchParameter noRDPEndpoint = base.NoRDPEndpoint;
					if (!noRDPEndpoint.IsPresent)
					{
						NetworkConfigurationSet networkConfigurationSet = instance.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
						if (networkConfigurationSet == null)
						{
							networkConfigurationSet = new NetworkConfigurationSet();
							instance.ConfigurationSets.Add(networkConfigurationSet);
						}
						if (networkConfigurationSet.InputEndpoints == null)
						{
							networkConfigurationSet.InputEndpoints = new Collection<InputEndpoint>();
						}
						foreach (InputEndpoint inputEndpoint in networkConfigurationSet.InputEndpoints)
						{
							if (string.Compare(inputEndpoint.Name, "RDP", StringComparison.OrdinalIgnoreCase) != 0 && inputEndpoint.LocalPort != 0xd3d)
							{
								continue;
							}
							flag1 = false;
							int? nullable = null;
							inputEndpoint.Port = nullable;
							break;
						}
						if (flag1)
						{
							InputEndpoint inputEndpoint1 = new InputEndpoint();
							inputEndpoint1.LocalPort = 0xd3d;
							inputEndpoint1.Protocol = "tcp";
							inputEndpoint1.Name = "RDP";
							networkConfigurationSet.InputEndpoints.Add(inputEndpoint1);
						}
					}
				}
				else
				{
					LinuxProvisioningConfigurationSet linuxProvisioningConfigurationSet = instance.ConfigurationSets.OfType<LinuxProvisioningConfigurationSet>().SingleOrDefault<LinuxProvisioningConfigurationSet>();
					if (linuxProvisioningConfigurationSet == null)
					{
						linuxProvisioningConfigurationSet = new LinuxProvisioningConfigurationSet();
						instance.ConfigurationSets.Add(linuxProvisioningConfigurationSet);
					}
					base.SetProvisioningConfiguration(linuxProvisioningConfigurationSet);
					linuxProvisioningConfigurationSet.HostName = instance.RoleName;
					SwitchParameter disableSSH = base.DisableSSH;
					if (disableSSH.IsPresent)
					{
						SwitchParameter noSSHEndpoint = base.NoSSHEndpoint;
						if (!noSSHEndpoint.IsPresent)
						{
							goto Label0;
						}
					}
					NetworkConfigurationSet inputEndpoints = instance.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
					if (inputEndpoints == null)
					{
						inputEndpoints = new NetworkConfigurationSet();
						inputEndpoints.InputEndpoints = new Collection<InputEndpoint>();
						instance.ConfigurationSets.Add(inputEndpoints);
					}
					foreach (InputEndpoint inputEndpoint2 in inputEndpoints.InputEndpoints)
					{
						if (string.Compare(inputEndpoint2.Name, "SSH", StringComparison.OrdinalIgnoreCase) != 0 && inputEndpoint2.LocalPort != 22)
						{
							continue;
						}
						flag = false;
						int? nullable1 = null;
						inputEndpoint2.Port = nullable1;
						break;
					}
					if (flag)
					{
						InputEndpoint inputEndpoint3 = new InputEndpoint();
						inputEndpoint3.LocalPort = 22;
						inputEndpoint3.Protocol = "tcp";
						inputEndpoint3.Name = "SSH";
						inputEndpoints.InputEndpoints.Add(inputEndpoint3);
					}
				}
			Label0:
				CmdletExtensions.WriteVerboseOutputForObject(this, this.VM);
				base.WriteObject(this.VM, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		protected void ValidateParameters()
		{
			PersistentVM vM = (PersistentVM)this.VM;
			if (string.Compare(base.ParameterSetName, "Linux", StringComparison.OrdinalIgnoreCase) != 0 || ValidationHelpers.IsLinuxPasswordValid(base.Password))
			{
				if ((string.Compare(base.ParameterSetName, "Windows", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(base.ParameterSetName, "WindowsDomain", StringComparison.OrdinalIgnoreCase) == 0) && !ValidationHelpers.IsWindowsPasswordValid(base.Password))
				{
					throw new ArgumentException("Password does not meet complexity requirements.");
				}
				else
				{
					if (string.Compare(base.ParameterSetName, "Linux", StringComparison.OrdinalIgnoreCase) != 0 || ValidationHelpers.IsLinuxHostNameValid(vM.RoleName))
					{
						if ((string.Compare(base.ParameterSetName, "Windows", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(base.ParameterSetName, "WindowsDomain", StringComparison.OrdinalIgnoreCase) == 0) && !ValidationHelpers.IsWindowsComputerNameValid(vM.RoleName))
						{
							throw new ArgumentException("Computer Name is invalid.");
						}
						else
						{
							return;
						}
					}
					else
					{
						throw new ArgumentException("Hostname is invalid.");
					}
				}
			}
			else
			{
				throw new ArgumentException("Password does not meet complexity requirements.");
			}
		}
	}
}