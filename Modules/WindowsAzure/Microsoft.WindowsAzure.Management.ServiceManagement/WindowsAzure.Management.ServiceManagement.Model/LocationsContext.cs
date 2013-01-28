using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class LocationsContext : ManagementOperationContext
	{
		public AvailableServicesList AvailableServices
		{
			get;
			set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public LocationsContext()
		{
		}
	}
}