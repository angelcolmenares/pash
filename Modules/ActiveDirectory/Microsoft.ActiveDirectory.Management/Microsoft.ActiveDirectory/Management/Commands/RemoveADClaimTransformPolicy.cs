using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADClaimTransformPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216388", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADClaimTransformPolicy : ADRemoveCmdletBase<RemoveADClaimTransformPolicyParameterSet, ADClaimTransformPolicyFactory<ADClaimTransformPolicy>, ADClaimTransformPolicy>
	{
		public RemoveADClaimTransformPolicy()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}