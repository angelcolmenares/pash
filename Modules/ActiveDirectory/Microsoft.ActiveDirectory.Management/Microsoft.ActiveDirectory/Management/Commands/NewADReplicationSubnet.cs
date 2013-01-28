using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADReplicationSubnet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216380", SupportsShouldProcess=true)]
	public class NewADReplicationSubnet : ADNewCmdletBase<NewADReplicationSubnetParameterSet, ADReplicationSubnetFactory<ADReplicationSubnet>, ADReplicationSubnet>
	{
		public NewADReplicationSubnet()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Subnets,CN=Sites,", ADPathFormat.X500);
		}
	}
}