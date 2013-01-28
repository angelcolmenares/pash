using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADReplicationSite", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216393", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADReplicationSite : ADRemoveCmdletBase<RemoveADReplicationSiteParameterSet, ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>
	{
		public RemoveADReplicationSite()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ADRemoveSiteCmdletAddRecursiveCSRoutine));
		}

		private bool ADRemoveSiteCmdletAddRecursiveCSRoutine()
		{
			this._cmdletParameters["Recursive"] = true;
			return true;
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}
	}
}