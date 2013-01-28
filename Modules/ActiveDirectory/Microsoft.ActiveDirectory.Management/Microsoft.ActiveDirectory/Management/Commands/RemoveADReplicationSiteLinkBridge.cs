using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADReplicationSiteLinkBridge", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216395", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADReplicationSiteLinkBridge : ADRemoveCmdletBase<RemoveADReplicationSiteLinkBridgeParameterSet, ADReplicationSiteLinkBridgeFactory<ADReplicationSiteLinkBridge>, ADReplicationSiteLinkBridge>
	{
		public RemoveADReplicationSiteLinkBridge()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}