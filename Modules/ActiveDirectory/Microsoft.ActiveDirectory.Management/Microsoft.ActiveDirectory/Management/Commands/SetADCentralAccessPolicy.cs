using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADCentralAccessPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216406", SupportsShouldProcess=true)]
	public class SetADCentralAccessPolicy : ADSetCmdletBase<SetADCentralAccessPolicyParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy>
	{
		public SetADCentralAccessPolicy()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}