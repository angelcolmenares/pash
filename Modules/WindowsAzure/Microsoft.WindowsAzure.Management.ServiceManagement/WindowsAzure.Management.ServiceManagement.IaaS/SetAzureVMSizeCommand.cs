using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureVMSize")]
	public class SetAzureVMSizeCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Represents the size of the machine.")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge" }, IgnoreCase=true)]
		public string InstanceSize
		{
			get;
			set;
		}

		public SetAzureVMSizeCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				PersistentVM instance = base.VM.GetInstance();
				instance.RoleSize = this.InstanceSize;
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