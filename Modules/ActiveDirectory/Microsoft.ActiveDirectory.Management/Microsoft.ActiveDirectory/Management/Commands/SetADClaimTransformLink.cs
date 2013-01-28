using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADClaimTransformLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216409", SupportsShouldProcess=true)]
	public class SetADClaimTransformLink : ADSetCmdletBase<SetADClaimTransformLinkParameterSet, ADTrustFactory<ADTrust>, ADTrust>
	{
		public SetADClaimTransformLink()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADSetADClaimTransformLinkBeginCSRoutine));
		}

		private bool ADSetADClaimTransformLinkBeginCSRoutine()
		{
			ADClaimTransformPolicy item = this._cmdletParameters["Policy"] as ADClaimTransformPolicy;
			ADTrustRole? nullable = (ADTrustRole?)(this._cmdletParameters["TrustRole"] as ADTrustRole?);
			string value = null;
			if (item != null)
			{
				if (!item.IsSearchResult)
				{
					ADClaimTransformPolicyFactory<ADClaimTransformPolicy> aDClaimTransformPolicyFactory = new ADClaimTransformPolicyFactory<ADClaimTransformPolicy>();
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					aDClaimTransformPolicyFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
					ADObject directoryObjectFromIdentity = aDClaimTransformPolicyFactory.GetDirectoryObjectFromIdentity(item, str);
					value = directoryObjectFromIdentity["distinguishedName"].Value as string;
				}
				else
				{
					value = item["distinguishedName"].Value as string;
				}
			}
			ADTrustRole aDTrustRole = nullable.Value;
			switch (aDTrustRole)
			{
				case ADTrustRole.Trusted:
				{
					this._cmdletParameters["TrustedPolicy"] = value;
					break;
				}
				case ADTrustRole.Trusting:
				{
					this._cmdletParameters["TrustingPolicy"] = value;
					break;
				}
			}
			this._cmdletParameters.RemoveParameter("Policy");
			return true;
		}

		protected internal override string GetDefaultCreationPath()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.SystemsContainerGuid);
		}
	}
}