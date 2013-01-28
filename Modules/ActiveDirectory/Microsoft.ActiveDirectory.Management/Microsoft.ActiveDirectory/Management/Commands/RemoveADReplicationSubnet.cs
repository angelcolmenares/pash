using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADReplicationSubnet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216396", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADReplicationSubnet : ADRemoveCmdletBase<RemoveADReplicationSubnetParameterSet, ADReplicationSubnetFactory<ADReplicationSubnet>, ADReplicationSubnet>
	{
		public RemoveADReplicationSubnet()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Subnets,CN=Sites,", ADPathFormat.X500);
		}
	}
}