using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class AvailabilityResponseContext : ManagementOperationContext
	{
		public bool Result
		{
			get;
			set;
		}

		public AvailabilityResponseContext()
		{
		}
	}
}