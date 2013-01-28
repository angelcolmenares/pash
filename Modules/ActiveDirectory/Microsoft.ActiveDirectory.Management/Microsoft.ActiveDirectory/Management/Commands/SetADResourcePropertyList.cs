using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADResourcePropertyList", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216419", SupportsShouldProcess=true)]
	public class SetADResourcePropertyList : ADSetCmdletBase<SetADResourcePropertyListParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList>
	{
		public SetADResourcePropertyList()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}