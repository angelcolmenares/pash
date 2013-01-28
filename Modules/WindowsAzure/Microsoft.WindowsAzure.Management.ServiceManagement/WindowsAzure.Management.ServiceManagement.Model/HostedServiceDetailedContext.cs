using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class HostedServiceDetailedContext : HostedServiceContext
	{
		public string AffinityGroup
		{
			get;
			set;
		}

		public string DateCreated
		{
			get;
			set;
		}

		public string DateModified
		{
			get;
			set;
		}

		public string Description
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

		public string Status
		{
			get;
			set;
		}

		public HostedServiceDetailedContext()
		{
		}
	}
}