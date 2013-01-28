using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationFailure", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216353", DefaultParameterSetName="Target")]
	public class GetADReplicationFailure : ADTargetScopeEnumerationServerCmdletBase<GetADReplicationFailureParameterSet, ADReplicationFailureFactory<ADReplicationFailure>, ADReplicationFailure>
	{
		public GetADReplicationFailure()
		{
		}

		internal override void PerServerProcessRecord()
		{
			ADEntity rootDSE;
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			string[] strArrays = new string[2];
			strArrays[0] = "msDS-ReplConnectionFailures";
			strArrays[1] = "msDS-ReplLinkFailures";
			string[] strArrays1 = strArrays;
			using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(cmdletSessionInfo.ADSessionInfo))
			{
				rootDSE = aDObjectSearcher.GetRootDSE(strArrays1);
			}
			foreach (ADReplicationFailure extendedObjectFromDirectoryObject in this._factory.GetExtendedObjectFromDirectoryObject(rootDSE, "msDS-ReplLinkFailures", "DS_REPL_KCC_DSA_FAILURE"))
			{
				base.WriteObject(extendedObjectFromDirectoryObject);
			}
			foreach (ADReplicationFailure aDReplicationFailure in this._factory.GetExtendedObjectFromDirectoryObject(rootDSE, "msDS-ReplConnectionFailures", "DS_REPL_KCC_DSA_FAILURE"))
			{
				base.WriteObject(aDReplicationFailure);
			}
		}
	}
}