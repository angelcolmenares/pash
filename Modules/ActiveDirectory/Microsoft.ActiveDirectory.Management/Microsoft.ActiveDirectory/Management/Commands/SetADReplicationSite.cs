using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADReplicationSite", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216414", SupportsShouldProcess=true)]
	public class SetADReplicationSite : ADSetCmdletBase<SetADReplicationSiteParameterSet, ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>
	{
		private const string _debugCategory = "SetADReplicationSite";

		public SetADReplicationSite()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}
	}
}