using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADClaimTransformPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216347", DefaultParameterSetName="Filter")]
	public class GetADClaimTransformPolicy : ADGetCmdletBase<GetADClaimTransformPolicyParameterSet, ADClaimTransformPolicyFactory<ADClaimTransformPolicy>, ADClaimTransformPolicy>
	{
		public GetADClaimTransformPolicy()
		{
		}

		private string GetClaimTransformPolicyContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetClaimTransformPolicyContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetClaimTransformPolicyContainerPath();
		}
	}
}