using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class StorageServiceOperationContext : ManagementOperationContext
	{
		public string StorageAccountName
		{
			get;
			set;
		}

		public StorageServiceOperationContext()
		{
		}
	}
}