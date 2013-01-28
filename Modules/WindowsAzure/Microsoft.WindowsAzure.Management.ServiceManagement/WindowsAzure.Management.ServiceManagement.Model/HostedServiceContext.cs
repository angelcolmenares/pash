using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class HostedServiceContext : ServiceOperationContext
	{
		public Uri Url
		{
			get;
			set;
		}

		public HostedServiceContext()
		{
		}
	}
}