using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationSiteLinkBridge", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216359", DefaultParameterSetName="Filter")]
	public class GetADReplicationSiteLinkBridge : ADGetCmdletBase<GetADReplicationSiteLinkBridgeParameterSet, ADReplicationSiteLinkBridgeFactory<ADReplicationSiteLinkBridge>, ADReplicationSiteLinkBridge>
	{
		public GetADReplicationSiteLinkBridge()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetSiteLinkBridgeContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetSiteLinkBridgeContainerPath();
		}

		private string GetSiteLinkBridgeContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}