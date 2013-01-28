using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADResourceProperty", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216418", SupportsShouldProcess=true)]
	public class SetADResourceProperty : ADSetCmdletBase<SetADResourcePropertyParameterSet, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public SetADResourceProperty()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
		}
	}
}