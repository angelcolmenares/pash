using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Clear", "ADClaimTransformLink", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216342", SupportsShouldProcess=true)]
	public class ClearADClaimTransformLink : ADSetCmdletBase<ClearADClaimTransformLinkParameterSet, ADTrustFactory<ADTrust>, ADTrust>
	{
		public ClearADClaimTransformLink()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.ADSClearADClaimTransformLinkCSRoutine));
		}

		private bool ADSClearADClaimTransformLinkCSRoutine()
		{
			string value;
			ADObject extendedObjectFromIdentity;
			ADClaimTransformPolicy item = this._cmdletParameters["Policy"] as ADClaimTransformPolicy;
			ADTrustRole? nullable = (ADTrustRole?)(this._cmdletParameters["TrustRole"] as ADTrustRole?);
			this._cmdletParameters.RemoveParameter("TrustedPolicy");
			this._cmdletParameters.RemoveParameter("TrustingPolicy");
			if (item != null || nullable.HasValue)
			{
				if (item != null)
				{
					CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
					if (!item.IsSearchResult)
					{
						ADClaimTransformPolicyFactory<ADClaimTransformPolicy> aDClaimTransformPolicyFactory = new ADClaimTransformPolicyFactory<ADClaimTransformPolicy>();
						aDClaimTransformPolicyFactory.SetCmdletSessionInfo(cmdletSessionInfo);
						string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
						ADObject directoryObjectFromIdentity = aDClaimTransformPolicyFactory.GetDirectoryObjectFromIdentity(item, str);
						value = directoryObjectFromIdentity["distinguishedName"].Value as string;
					}
					else
					{
						value = item["distinguishedName"].Value as string;
					}
					ADTrust aDTrust = this._cmdletParameters["Identity"] as ADTrust;
					if (!aDTrust.IsSearchResult)
					{
						ADTrustFactory<ADTrust> aDTrustFactory = new ADTrustFactory<ADTrust>();
						aDTrustFactory.SetCmdletSessionInfo(cmdletSessionInfo);
						string wellKnownGuidDN = Utils.GetWellKnownGuidDN(this.GetSessionInfo(), base.GetDefaultPartitionPath(), WellKnownGuids.SystemsContainerGuid);
						extendedObjectFromIdentity = aDTrustFactory.GetExtendedObjectFromIdentity(aDTrust, wellKnownGuidDN);
					}
					else
					{
						extendedObjectFromIdentity = aDTrust;
					}
					if (nullable.HasValue)
					{
						if (nullable.Value != ADTrustRole.Trusted)
						{
							if (ADTrustRole.Trusting == nullable.Value && string.Compare(extendedObjectFromIdentity["TrustingPolicy"].Value as string, value, StringComparison.OrdinalIgnoreCase) == 0)
							{
								this._cmdletParameters["TrustingPolicy"] = null;
							}
						}
						else
						{
							if (string.Compare(extendedObjectFromIdentity["TrustedPolicy"].Value as string, value, StringComparison.OrdinalIgnoreCase) == 0)
							{
								this._cmdletParameters["TrustedPolicy"] = null;
							}
						}
					}
					else
					{
						if (string.Compare(extendedObjectFromIdentity["TrustingPolicy"].Value as string, value, StringComparison.OrdinalIgnoreCase) == 0)
						{
							this._cmdletParameters["TrustingPolicy"] = null;
						}
						if (string.Compare(extendedObjectFromIdentity["TrustedPolicy"].Value as string, value, StringComparison.OrdinalIgnoreCase) == 0)
						{
							this._cmdletParameters["TrustedPolicy"] = null;
						}
					}
				}
				else
				{
					ADTrustRole aDTrustRole = nullable.Value;
					switch (aDTrustRole)
					{
						case ADTrustRole.Trusted:
						{
							this._cmdletParameters["TrustedPolicy"] = null;
							break;
						}
						case ADTrustRole.Trusting:
						{
							this._cmdletParameters["TrustingPolicy"] = null;
							break;
						}
					}
				}
				return true;
			}
			else
			{
				throw new ArgumentException(StringResources.ADTrustNoDirectionAndPolicyError);
			}
		}

		protected internal override string GetDefaultCreationPath()
		{
			return Utils.GetWellKnownGuidDN(this.GetSessionInfo(), this.GetDefaultPartitionPath(), WellKnownGuids.SystemsContainerGuid);
		}
	}
}