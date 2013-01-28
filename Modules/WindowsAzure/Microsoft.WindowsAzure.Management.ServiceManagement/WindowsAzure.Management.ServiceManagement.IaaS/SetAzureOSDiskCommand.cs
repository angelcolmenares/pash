using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureOSDisk")]
	public class SetAzureOSDiskCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Controls the platform caching behavior of data disk blob for read / write efficiency.")]
		[ValidateSet(new string[] { "ReadOnly", "ReadWrite" }, IgnoreCase=true)]
		public string HostCaching
		{
			get;
			set;
		}

		public SetAzureOSDiskCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				PersistentVM instance = base.VM.GetInstance();
				if (instance.OSVirtualHardDisk == null)
				{
					base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("An OSDisk has not been defined for this VM. Use New-OSDisk to assign a new OS disk."), string.Empty, ErrorCategory.InvalidData, null));
				}
				OSVirtualHardDisk oSVirtualHardDisk = instance.OSVirtualHardDisk;
				oSVirtualHardDisk.HostCaching = this.HostCaching;
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