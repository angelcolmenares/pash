using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADCentralAccessPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=21638", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADCentralAccessPolicy : ADRemoveCmdletBase<RemoveADCentralAccessPolicyParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy>
	{
		public RemoveADCentralAccessPolicy()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}