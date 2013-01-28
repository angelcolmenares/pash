using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADCentralAccessPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216345", DefaultParameterSetName="Filter")]
	public class GetADCentralAccessPolicy : ADGetCmdletBase<GetADCentralAccessPolicyParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy>
	{
		public GetADCentralAccessPolicy()
		{
		}

		private string GetCentralAccessPolicyContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetCentralAccessPolicyContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetCentralAccessPolicyContainerPath();
		}
	}
}