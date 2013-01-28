using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Remove", "AzureDataDisk")]
	public class RemoveAzureDataDiskCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Numerical value that defines the slot where the data drive is mounted in the virtual machine.")]
		[ValidateNotNullOrEmpty]
		public int LUN
		{
			get;
			set;
		}

		public RemoveAzureDataDiskCommand()
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
				DataVirtualHardDisk dataVirtualHardDisk = dataVirtualHardDisks.Where<DataVirtualHardDisk>(func).SingleOrDefault<DataVirtualHardDisk>();
				if (dataVirtualHardDisk == null)
				{
					object[] lUN = new object[1];
					lUN[0] = this.LUN;
					base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A data disk is not currently assigned to LUN #{0} in the configuration of this VM.", lUN)), string.Empty, ErrorCategory.InvalidData, null));
				}
				dataDisks.Remove(dataVirtualHardDisk);
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