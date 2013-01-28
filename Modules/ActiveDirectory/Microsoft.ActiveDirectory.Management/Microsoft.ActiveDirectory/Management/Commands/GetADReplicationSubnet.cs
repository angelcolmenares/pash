using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationSubnet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216360", DefaultParameterSetName="Filter")]
	public class GetADReplicationSubnet : ADGetCmdletBase<GetADReplicationSubnetParameterSet, ADReplicationSubnetFactory<ADReplicationSubnet>, ADReplicationSubnet>
	{
		public GetADReplicationSubnet()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetSubnetContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetSubnetContainerPath();
		}

		private string GetSubnetContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Subnets,CN=Sites,", ADPathFormat.X500);
		}
	}
}