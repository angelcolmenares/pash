using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("Get", "AzureSubnet")]
	public class GetAzureSubnetCommand : VirtualMachineConfigurationCmdletBase
	{
		public GetAzureSubnetCommand()
		{
		}

		protected override void ProcessRecord()
		{
			PersistentVM instance = base.VM.GetInstance();
			NetworkConfigurationSet networkConfigurationSet = instance.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
			if (networkConfigurationSet != null)
			{
				base.WriteObject(networkConfigurationSet.SubnetNames, true);
				return;
			}
			else
			{
				base.WriteObject(null);
				return;
			}
		}
	}
}