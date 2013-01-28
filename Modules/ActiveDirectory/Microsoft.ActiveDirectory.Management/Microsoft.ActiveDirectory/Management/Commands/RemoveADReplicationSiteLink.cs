using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADReplicationSiteLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216394", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADReplicationSiteLink : ADRemoveCmdletBase<RemoveADReplicationSiteLinkParameterSet, ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>
	{
		public RemoveADReplicationSiteLink()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}