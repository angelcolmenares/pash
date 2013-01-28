using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public sealed class DiskContext : ManagementOperationContext
	{
		public string AffinityGroup
		{
			get;
			set;
		}

		public DiskContext.RoleReference AttachedTo
		{
			get;
			set;
		}

		public string DiskName
		{
			get;
			set;
		}

		public int DiskSizeInGB
		{
			get;
			set;
		}

		public bool IsCorrupted
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public Uri MediaLink
		{
			get;
			set;
		}

		public string OS
		{
			get;
			set;
		}

		public string SourceImageName
		{
			get;
			set;
		}

		public DiskContext()
		{
		}

		public class RoleReference
		{
			public string DeploymentName
			{
				get;
				set;
			}

			public string HostedServiceName
			{
				get;
				set;
			}

			public string RoleName
			{
				get;
				set;
			}

			public RoleReference()
			{
			}

			public override string ToString()
			{
				object[] roleName = new object[3];
				roleName[0] = this.RoleName;
				roleName[1] = this.DeploymentName;
				roleName[2] = this.HostedServiceName;
				return string.Format(CultureInfo.InvariantCulture, "RoleName: {0} \n\rDeploymentName: {1} \n\rHostedServiceName: {2}", roleName);
			}
		}
	}
}