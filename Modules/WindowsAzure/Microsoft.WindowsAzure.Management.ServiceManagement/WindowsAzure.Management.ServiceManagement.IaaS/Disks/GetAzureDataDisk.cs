using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Disks
{
	[Cmdlet("Get", "AzureDataDisk")]
	public class GetAzureDataDisk : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, HelpMessage="Data Disk LUN")]
		[ValidateNotNullOrEmpty]
		public int? Lun
		{
			get;
			set;
		}

		public GetAzureDataDisk()
		{
		}

		protected override void ProcessRecord()
		{
			Func<DataVirtualHardDisk, bool> func = null;
			PersistentVM instance = base.VM.GetInstance();
			int? nullable1 = this.Lun;
			if (nullable1.HasValue)
			{
				Collection<DataVirtualHardDisk> dataVirtualHardDisks = instance.DataVirtualHardDisks;
				if (func == null)
				{
					func = (DataVirtualHardDisk dd) => {
						int lun = dd.Lun;
						int? nullable = this.Lun;
						if (lun != nullable.GetValueOrDefault())
						{
							return false;
						}
						else
						{
							return nullable.HasValue;
						}
					}
					;
				}
				DataVirtualHardDisk dataVirtualHardDisk = dataVirtualHardDisks.Where<DataVirtualHardDisk>(func).SingleOrDefault<DataVirtualHardDisk>();
				base.WriteObject(dataVirtualHardDisk, true);
				return;
			}
			else
			{
				base.WriteObject(instance.DataVirtualHardDisks, true);
				return;
			}
		}
	}
}