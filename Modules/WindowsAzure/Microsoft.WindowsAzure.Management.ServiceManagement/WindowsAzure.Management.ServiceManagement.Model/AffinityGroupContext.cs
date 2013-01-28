using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class AffinityGroupContext : ManagementOperationContext
	{
		public string Description
		{
			get;
			set;
		}

		public IEnumerable<AffinityGroupContext.Service> HostedServices
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public IEnumerable<AffinityGroupContext.Service> StorageServices
		{
			get;
			set;
		}

		public AffinityGroupContext()
		{
		}

		public class Service
		{
			public string ServiceName
			{
				get;
				set;
			}

			public Uri Url
			{
				get;
				set;
			}

			public Service()
			{
			}
		}
	}
}