using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleInstanceStatusInfoContext : ServiceOperationContext
	{
		public IList<RoleInstanceStatusInfo> RoleInstances
		{
			get;
			private set;
		}

		public RoleInstanceStatusInfoContext()
		{
			this.RoleInstances = new List<RoleInstanceStatusInfo>();
		}
	}
}