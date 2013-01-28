using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class PersistentVMRoleListContext
	{
		public string Name
		{
			get;
			set;
		}

		public string ServiceName
		{
			get;
			set;
		}

		public string Status
		{
			get;
			set;
		}

		public PersistentVMRoleListContext()
		{
		}
	}
}