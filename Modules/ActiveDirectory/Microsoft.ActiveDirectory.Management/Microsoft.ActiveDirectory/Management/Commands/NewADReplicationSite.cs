using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADReplicationSite", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216377", SupportsShouldProcess=true)]
	public class NewADReplicationSite : ADNewCmdletBase<NewADReplicationSiteParameterSet, ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>
	{
		public NewADReplicationSite()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}
	}
}