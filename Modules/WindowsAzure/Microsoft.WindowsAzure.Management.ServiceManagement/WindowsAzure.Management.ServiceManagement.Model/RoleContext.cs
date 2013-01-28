using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleContext : ServiceOperationContext
	{
		public string DeploymentID
		{
			get;
			set;
		}

		public int InstanceCount
		{
			get;
			set;
		}

		public string RoleName
		{
			get;
			set;
		}

		public RoleContext()
		{
		}
	}
}