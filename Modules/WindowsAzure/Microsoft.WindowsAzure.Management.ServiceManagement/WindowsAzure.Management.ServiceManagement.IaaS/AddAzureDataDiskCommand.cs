using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Add", "AzureDataDisk", DefaultParameterSetName="CreateNew")]
	public class AddAzureDataDiskCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, ParameterSetName="CreateNew", HelpMessage="Specify to create a new data disk.")]
		public SwitchParameter CreateNew
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ParameterSetName="ImportFrom", HelpMessage="Label of the disk.")]
		[Parameter(Position=2, Mandatory=true, ParameterSetName="CreateNew", HelpMessage="Label of the disk.")]
		[ValidateNotNullOrEmpty]
		public string DiskLabel
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ParameterSetName="Import", ValueFromPipelineByPropertyName=true, HelpMessage="Name of the data disk in the disk library.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, ParameterSetName="CreateNew", HelpMessage="Logical disk size in gigabytes.")]
		[ValidateNotNullOrEmpty]
		public int DiskSizeInGB
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Controls the platform caching behavior of data disk blob for read efficiency.")]
		[ValidateSet(new string[] { "ReadOnly", "ReadWrite", "None" }, IgnoreCase=true)]
		public string HostCaching
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="Import", HelpMessage="Specify to import an existing data disk from the disk library.")]
		public SwitchParameter Import
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="ImportFrom", HelpMessage="Specify to import an existing data disk from a storage location.")]
		public SwitchParameter ImportFrom
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=true, HelpMessage="Numerical value that defines the slot where the data drive will be mounted in the virtual machine.")]
		[ValidateNotNullOrEmpty]
		public int LUN
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="CreateNew", ValueFromPipelineByPropertyName=true, HelpMessage="Location of the physical blob backing the data disk to be created.")]
		[Parameter(Mandatory=true, ParameterSetName="ImportFrom", ValueFromPipelineByPropertyName=true, HelpMessage="Location of the physical blob backing the data disk. This link refers to a blob in a storage account.")]
		[ValidateNotNullOrEmpty]
		public string MediaLocation
		{
			get;
			set;
		}

		public AddAzureDataDiskCommand()
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
			Uri uri;
			Uri uri1;
			Func<DataVirtualHardDisk, bool> func = null;
			try
			{
				this.ValidateParameters();
				base.ProcessRecord();
				Collection<DataVirtualHardDisk> dataDisks = this.GetDataDisks();
				Collection<DataVirtualHardDisk> dataVirtualHardDisks = dataDisks;
				if (func == null)
				{
					func = (DataVirtualHardDisk disk) => disk.Lun == this.LUN;
				}
				DataVirtualHardDisk diskLabel = dataVirtualHardDisks.Where<DataVirtualHardDisk>(func).SingleOrDefault<DataVirtualHardDisk>();
				if (diskLabel != null)
				{
					object[] lUN = new object[1];
					lUN[0] = this.LUN;
					base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "A data disk has already been assigned to LUN #{0} for this VM. Specify a different LUN or use Set-DataDisk to change the configuration settings of the existing disk.", lUN)), string.Empty, ErrorCategory.InvalidData, null));
				}
				DataVirtualHardDisk dataVirtualHardDisk = new DataVirtualHardDisk();
				dataVirtualHardDisk.HostCaching = this.HostCaching;
				dataVirtualHardDisk.Lun = this.LUN;
				diskLabel = dataVirtualHardDisk;
				string parameterSetName = base.ParameterSetName;
				string str = parameterSetName;
				if (parameterSetName != null)
				{
					if (str == "CreateNew")
					{
						diskLabel.DiskLabel = this.DiskLabel;
						diskLabel.LogicalDiskSizeInGB = this.DiskSizeInGB;
						DataVirtualHardDisk dataVirtualHardDisk1 = diskLabel;
						if (string.IsNullOrEmpty(this.MediaLocation))
						{
							uri = null;
						}
						else
						{
							uri = new Uri(this.MediaLocation);
						}
						dataVirtualHardDisk1.MediaLink = uri;
					}
					else
					{
						if (str == "Import")
						{
							diskLabel.DiskName = this.DiskName;
						}
						else
						{
							if (str == "ImportFrom")
							{
								diskLabel.DiskName = this.DiskName;
								DataVirtualHardDisk dataVirtualHardDisk2 = diskLabel;
								if (string.IsNullOrEmpty(this.MediaLocation))
								{
									uri1 = null;
								}
								else
								{
									uri1 = new Uri(this.MediaLocation);
								}
								dataVirtualHardDisk2.SourceMediaLink = uri1;
							}
						}
					}
				}
				dataDisks.Add(diskLabel);
				base.WriteObject(base.VM, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		protected void ValidateParameters()
		{
			SubscriptionData currentSubscription = CmdletSubscriptionExtensions.GetCurrentSubscription(this);
			if ((currentSubscription == null || currentSubscription.get_CurrentStorageAccount() == null) && this.MediaLocation == null && string.Compare(base.ParameterSetName, "CreateNew", StringComparison.OrdinalIgnoreCase) == 0)
			{
				throw new ArgumentException("Must specify MediaLocation or set a default storage account using Set-AzureSubscription.");
			}
			else
			{
				return;
			}
		}
	}
}