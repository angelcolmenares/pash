using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADResourcePropertyList", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216398", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADResourcePropertyList : ADRemoveCmdletBase<RemoveADResourcePropertyListParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList>
	{
		public RemoveADResourcePropertyList()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}