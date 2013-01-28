using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureSubnet")]
	public class SetAzureSubnetCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="The list of subnet names.")]
		public string[] SubnetNames
		{
			get;
			set;
		}

		public SetAzureSubnetCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				PersistentVM instance = base.VM.GetInstance();
				NetworkConfigurationSet networkConfigurationSet = instance.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
				if (networkConfigurationSet == null)
				{
					networkConfigurationSet = new NetworkConfigurationSet();
					instance.ConfigurationSets.Add(networkConfigurationSet);
				}
				networkConfigurationSet.SubnetNames = new SubnetNamesCollection();
				string[] subnetNames = this.SubnetNames;
				for (int i = 0; i < (int)subnetNames.Length; i++)
				{
					string str = subnetNames[i];
					networkConfigurationSet.SubnetNames.Add(str);
				}
				base.WriteObject(base.VM, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}