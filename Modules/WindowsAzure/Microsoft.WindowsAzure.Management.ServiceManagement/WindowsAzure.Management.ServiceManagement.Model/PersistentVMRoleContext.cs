using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class PersistentVMRoleContext : ServiceOperationContext, IPersistentVM
	{
		public string AvailabilitySetName
		{
			get;
			set;
		}

		public string DeploymentName
		{
			get;
			set;
		}

		public string DNSName
		{
			get;
			set;
		}

		public string InstanceErrorCode
		{
			get;
			set;
		}

		public string InstanceFaultDomain
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

		public string InstanceUpgradeDomain
		{
			get;
			set;
		}

		public string IpAddress
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string PowerState
		{
			get;
			set;
		}

		public PersistentVM VM
		{
			get;
			set;
		}

		public PersistentVMRoleContext()
		{
		}

		public PersistentVM GetInstance()
		{
			return this.VM;
		}
	}
}