using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleInstanceCountContext : ServiceOperationContext
	{
		public int InstanceCount
		{
			get;
			set;
		}

		public RoleInstanceCountContext()
		{
		}
	}
}