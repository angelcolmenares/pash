using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationSiteLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216358", DefaultParameterSetName="Filter")]
	public class GetADReplicationSiteLink : ADGetCmdletBase<GetADReplicationSiteLinkParameterSet, ADReplicationSiteLinkFactory<ADReplicationSiteLink>, ADReplicationSiteLink>
	{
		public GetADReplicationSiteLink()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetSiteLinkContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetSiteLinkContainerPath();
		}

		private string GetSiteLinkContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
		}
	}
}