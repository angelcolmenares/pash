using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADCentralAccessRule", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216346", DefaultParameterSetName="Filter")]
	public class GetADCentralAccessRule : ADGetCmdletBase<GetADCentralAccessRuleParameterSet, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		public GetADCentralAccessRule()
		{
		}

		private string GetCentralAccessRuleContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetCentralAccessRuleContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetCentralAccessRuleContainerPath();
		}
	}
}