using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADCentralAccessPolicyMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216340", SupportsShouldProcess=true)]
	public class AddADCentralAccessPolicyMember : ADSetObjectMember<AddADCentralAccessPolicyMemberParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		public AddADCentralAccessPolicyMember() : base(0)
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