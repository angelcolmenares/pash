using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADCentralAccessRule", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216407", SupportsShouldProcess=true)]
	public class SetADCentralAccessRule : ADSetCmdletBase<SetADCentralAccessRuleParameterSet, ADCentralAccessRuleFactory<ADCentralAccessRule>, ADCentralAccessRule>
	{
		private const string _debugCategory = "SetADCentralAccessRule";

		public SetADCentralAccessRule()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Central Access Rules,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}