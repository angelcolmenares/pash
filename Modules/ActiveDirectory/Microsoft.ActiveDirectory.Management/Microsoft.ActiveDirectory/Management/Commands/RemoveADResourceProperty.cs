using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADResourceProperty", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216397", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADResourceProperty : ADRemoveCmdletBase<RemoveADResourcePropertyParameterSet, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public RemoveADResourceProperty()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
		}
	}
}