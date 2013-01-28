using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	public class VirtualMachineConfigurationCmdletBase : PSCmdlet
	{
		[Alias(new string[] { "InputObject" })]
		[Parameter(Mandatory=true, ValueFromPipeline=true, HelpMessage="Virtual Machine to update.")]
		[ValidateNotNullOrEmpty]
		public IPersistentVM VM
		{
			get;
			set;
		}

		public VirtualMachineConfigurationCmdletBase()
		{
		}
	}
}