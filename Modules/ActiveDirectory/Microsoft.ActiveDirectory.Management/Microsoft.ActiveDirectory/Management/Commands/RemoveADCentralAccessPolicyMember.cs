using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADCentralAccessPolicyMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216386", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADCentralAccessPolicyMember : ADSetObjectMember<AddADCentralAccessPolicyMemberParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		public RemoveADCentralAccessPolicyMember() : base((SetADMemberOperationType)1)
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		internal override string GetMemberDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}