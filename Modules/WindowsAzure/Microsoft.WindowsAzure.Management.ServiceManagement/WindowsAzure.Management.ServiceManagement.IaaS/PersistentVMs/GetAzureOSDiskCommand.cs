using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("Get", "AzureOSDisk")]
	public class GetAzureOSDiskCommand : VirtualMachineConfigurationCmdletBase
	{
		public GetAzureOSDiskCommand()
		{
		}

		protected override void ProcessRecord()
		{
			PersistentVM instance = base.VM.GetInstance();
			base.WriteObject(instance.OSVirtualHardDisk, true);
		}
	}
}