using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADReplicationSiteLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216378", SupportsShouldProcess=true)]
	public class NewADReplicationSiteLink : ADNewCmdletBase<NewADReplicationSiteLinkParameterSet, ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>
	{
		public NewADReplicationSiteLink()
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