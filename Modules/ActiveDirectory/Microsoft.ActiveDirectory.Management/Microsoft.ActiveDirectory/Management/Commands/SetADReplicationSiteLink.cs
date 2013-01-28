using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADReplicationSiteLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216415", SupportsShouldProcess=true)]
	public class SetADReplicationSiteLink : ADSetCmdletBase<SetADReplicationSiteLinkParameterSet, ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>
	{
		public SetADReplicationSiteLink()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}