using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationSite", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216357", DefaultParameterSetName="Identity")]
	public class GetADReplicationSite : ADGetCmdletBase<GetADReplicationSiteParameterSet, ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>
	{
		public GetADReplicationSite()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ADGetSiteCmdletCalculateIdentityCSRoutine));
		}

		private bool ADGetSiteCmdletCalculateIdentityCSRoutine()
		{
			if (this._cmdletParameters["Identity"] == null)
			{
				if (this._cmdletParameters["Filter"] == null)
				{
					CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
					ADDomainController aDDomainController = new ADDomainController(cmdletSessionInfo.ADRootDSE.DNSHostName);
					ADDomainControllerFactory<ADDomainController> aDDomainControllerFactory = new ADDomainControllerFactory<ADDomainController>();
					aDDomainControllerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADDomainController extendedObjectFromIdentity = aDDomainControllerFactory.GetExtendedObjectFromIdentity(aDDomainController, null, null, false);
					this._cmdletParameters["Identity"] = new ADReplicationSite(extendedObjectFromIdentity.Site);
					return true;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}

		protected internal override string GetDefaultQueryPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}
	}
}