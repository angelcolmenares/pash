using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class PersistentVM : IPersistentVM
	{
		public string AvailabilitySetName
		{
			get;
			set;
		}

		public Collection<ConfigurationSet> ConfigurationSets
		{
			get;
			set;
		}

		public Collection<DataVirtualHardDisk> DataVirtualHardDisks
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public OSVirtualHardDisk OSVirtualHardDisk
		{
			get;
			set;
		}

		public string RoleName
		{
			get;
			set;
		}

		public string RoleSize
		{
			get;
			set;
		}

		public string RoleType
		{
			get;
			set;
		}

		public PersistentVM()
		{
		}

		public PersistentVM GetInstance()
		{
			return this;
		}
	}
}