using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class RoleInstanceContext : ServiceOperationContext
	{
		public string DeploymentID
		{
			get;
			set;
		}

		public InstanceEndpointList InstanceEndpoints
		{
			get;
			set;
		}

		public string InstanceErrorCode
		{
			get;
			set;
		}

		public int? InstanceFaultDomain
		{
			get;
			set;
		}

		public string InstanceName
		{
			get;
			set;
		}

		public string InstanceSize
		{
			get;
			set;
		}

		public string InstanceStateDetails
		{
			get;
			set;
		}

		public string InstanceStatus
		{
			get;
			set;
		}

		public int? InstanceUpgradeDomain
		{
			get;
			set;
		}

		public string RoleName
		{
			get;
			set;
		}

		public RoleInstanceContext()
		{
		}
	}
}