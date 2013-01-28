using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADCentralAccessPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216373", SupportsShouldProcess=true)]
	public class NewADCentralAccessPolicy : ADNewCmdletBase<NewADCentralAccessPolicyParameterSet, ADCentralAccessPolicyFactory<ADCentralAccessPolicy>, ADCentralAccessPolicy>
	{
		public NewADCentralAccessPolicy()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}