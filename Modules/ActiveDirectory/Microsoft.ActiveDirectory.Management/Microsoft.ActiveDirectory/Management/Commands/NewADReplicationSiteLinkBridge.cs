using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADReplicationSiteLinkBridge", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216379", SupportsShouldProcess=true)]
	public class NewADReplicationSiteLinkBridge : ADNewCmdletBase<NewADReplicationSiteLinkBridgeParameterSet, ADReplicationSiteLinkBridgeFactory<ADReplicationSiteLinkBridge>, ADReplicationSiteLinkBridge>
	{
		public NewADReplicationSiteLinkBridge()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			ADInterSiteTransportProtocolType value = ADInterSiteTransportProtocolType.IP;
			if (this._cmdletParameters["InterSiteTransportProtocol"] != null)
			{
				ADInterSiteTransportProtocolType? interSiteTransportProtocol = this._cmdletParameters.InterSiteTransportProtocol;
				value = interSiteTransportProtocol.Value;
			}
			return ADTopologyUtil.CreateISTPPath(value, this.GetRootDSE().ConfigurationNamingContext);
		}
	}
}