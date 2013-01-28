using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("New", "AzureVMConfig", DefaultParameterSetName="ImageName")]
	public class NewAzureVMConfigCommand : PSCmdlet
	{
		private const string RoleType = "PersistentVMRole";

		[Parameter(HelpMessage="The name of the availability set.")]
		[ValidateNotNullOrEmpty]
		public string AvailabilitySetName
		{
			get;
			set;
		}

		[Parameter(Position=3, Mandatory=false, ParameterSetName="ImageName", HelpMessage="Label of the new disk to be created.")]
		[ValidateNotNullOrEmpty]
		public string DiskLabel
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ParameterSetName="DiskName", HelpMessage="Friendly name of the OS disk in the disk repository.")]
		[ValidateNotNullOrEmpty]
		public string DiskName
		{
			get;
			set;
		}

		[Parameter(HelpMessage="Controls the platform caching behavior of the OS disk.")]
		[ValidateSet(new string[] { "ReadWrite", "ReadOnly" }, IgnoreCase=true)]
		public string HostCaching
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ParameterSetName="ImageName", HelpMessage="Reference to a platform stock image or a user image from the image repository.")]
		[ValidateNotNullOrEmpty]
		public string ImageName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, HelpMessage="Represents the size of the machine.")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge" }, IgnoreCase=true)]
		public string InstanceSize
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="The VM label.")]
		[ValidateNotNullOrEmpty]
		public string Label
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=false, ParameterSetName="ImageName", HelpMessage="Location of the where the VHD should be created. This link refers to a blob in a storage account. If not specified the VHD will be created in the default storage account with the following format :vhds/servicename-vmname-year-month-day-ms")]
		[ValidateNotNullOrEmpty]
		public string MediaLocation
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="The virtual machine name.")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public NewAzureVMConfigCommand()
		{
		}

		protected override void ProcessRecord()
		{
			Uri uri;
			string diskLabel;
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				if (string.IsNullOrEmpty(this.Label))
				{
					this.Label = this.Name;
				}
				this.Label = ServiceManagementHelper.EncodeToBase64String(this.Label);
				PersistentVM persistentVM = new PersistentVM();
				persistentVM.AvailabilitySetName = this.AvailabilitySetName;
				persistentVM.ConfigurationSets = new Collection<ConfigurationSet>();
				persistentVM.DataVirtualHardDisks = new Collection<DataVirtualHardDisk>();
				persistentVM.RoleName = this.Name;
				persistentVM.RoleSize = this.InstanceSize;
				persistentVM.RoleType = "PersistentVMRole";
				persistentVM.Label = this.Label;
				PersistentVM persistentVM1 = persistentVM;
				PersistentVM persistentVM2 = persistentVM1;
				OSVirtualHardDisk oSVirtualHardDisk = new OSVirtualHardDisk();
				oSVirtualHardDisk.DiskName = this.DiskName;
				oSVirtualHardDisk.SourceImageName = this.ImageName;
				OSVirtualHardDisk oSVirtualHardDisk1 = oSVirtualHardDisk;
				if (string.IsNullOrEmpty(this.MediaLocation))
				{
					uri = null;
				}
				else
				{
					uri = new Uri(this.MediaLocation);
				}
				oSVirtualHardDisk1.MediaLink = uri;
				oSVirtualHardDisk.HostCaching = this.HostCaching;
				OSVirtualHardDisk oSVirtualHardDisk2 = oSVirtualHardDisk;
				if (string.IsNullOrEmpty(this.DiskLabel))
				{
					diskLabel = null;
				}
				else
				{
					diskLabel = this.DiskLabel;
				}
				oSVirtualHardDisk2.DiskLabel = diskLabel;
				persistentVM2.OSVirtualHardDisk = oSVirtualHardDisk;
				CmdletExtensions.WriteVerboseOutputForObject(this, persistentVM1);
				base.WriteObject(persistentVM1, true);
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
			if ((currentSubscription == null || currentSubscription.get_CurrentStorageAccount() == null) && this.MediaLocation == null)
			{
				throw new ArgumentException("Must specify MediaLocation or set a current storage account using Set-AzureSubscription.");
			}
			else
			{
				return;
			}
		}
	}
}