using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class ServiceOperationContext : ManagementOperationContext
	{
		public string ServiceName
		{
			get;
			set;
		}

		public ServiceOperationContext()
		{
		}
	}
}