using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADCentralAccessRule", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216387", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADCentralAccessRule : ADRemoveCmdletBase<RemoveADCentralAccessRuleParameterSet, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		public RemoveADCentralAccessRule()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}