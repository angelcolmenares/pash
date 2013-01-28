using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class StorageServiceKeyOperationContext : StorageServiceOperationContext
	{
		public string Primary
		{
			get;
			set;
		}

		public string Secondary
		{
			get;
			set;
		}

		public StorageServiceKeyOperationContext()
		{
		}
	}
}