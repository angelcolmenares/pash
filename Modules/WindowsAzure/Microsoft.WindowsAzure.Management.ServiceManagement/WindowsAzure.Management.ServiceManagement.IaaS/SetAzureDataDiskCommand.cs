using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Set", "AzureDataDisk")]
	public class SetAzureDataDiskCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Controls the platform caching behavior of data disk blob for read efficiency.")]
		[ValidateSet(new string[] { "None", "ReadOnly", "ReadWrite" }, IgnoreCase=true)]
		public string HostCaching
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, HelpMessage="Numerical value that defines the slot where the data drive will be mounted in the virtual machine.")]
		[ValidateNotNullOrEmpty]
		public int LUN
		{
			get;
			set;
		}

		public SetAzureDataDiskCommand()
		{
		}

		protected Collection<DataVirtualHardDisk> GetDataDisks()
		{
			PersistentVM instance = base.VM.GetInstance();
			if (instance.DataVirtualHardDisks == null)
			{
				instance.DataVirtualHardDisks = new Collection<DataVirtualHardDisk>();
			}
			return instance.DataVirtualHardDisks;
		}

		protected override void ProcessRecord()
		{
			Func<DataVirtualHardDisk, bool> func = null;
			try
			{
				base.ProcessRecord();
				Collection<DataVirtualHardDisk> dataDisks = this.GetDataDisks();
				Collection<DataVirtualHardDisk> dataVirtualHardDisks = dataDisks;
				if (func == null)
				{
					func = (DataVirtualHardDisk disk) => disk.Lun == this.LUN;
				}
				DataVirtualHardDisk hostCaching = dataVirtualHardDisks.SingleOrDefault<DataVirtualHardDisk>(func);
				if (hostCaching == null)
				{
					object[] lUN = new object[1];
					lUN[0] = this.LUN;
					base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A data disk is not currently assigned to LUN #{0} for this VM. Use New-DataDisk to define it.", lUN)), string.Empty, ErrorCategory.InvalidData, null));
				}
				hostCaching.HostCaching = this.HostCaching;
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