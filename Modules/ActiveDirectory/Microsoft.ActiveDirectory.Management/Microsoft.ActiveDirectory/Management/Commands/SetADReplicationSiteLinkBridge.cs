using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADReplicationSiteLinkBridge", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216416", SupportsShouldProcess=true)]
	public class SetADReplicationSiteLinkBridge : ADSetCmdletBase<SetADReplicationSiteLinkBridgeParameterSet, ADReplicationSiteLinkBridgeFactory<ADReplicationSiteLinkBridge>, ADReplicationSiteLinkBridge>
	{
		public SetADReplicationSiteLinkBridge()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}