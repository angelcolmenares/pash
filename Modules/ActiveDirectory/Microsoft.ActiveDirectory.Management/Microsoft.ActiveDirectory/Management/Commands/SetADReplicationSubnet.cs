using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADReplicationSubnet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216417", SupportsShouldProcess=true)]
	public class SetADReplicationSubnet : ADSetCmdletBase<SetADReplicationSubnetParameterSet, ADReplicationSubnetFactory<ADReplicationSubnet>, ADReplicationSubnet>
	{
		private const string _debugCategory = "SetADReplicationSubnet";

		public SetADReplicationSubnet()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Subnets,CN=Sites,", ADPathFormat.X500);
		}
	}
}