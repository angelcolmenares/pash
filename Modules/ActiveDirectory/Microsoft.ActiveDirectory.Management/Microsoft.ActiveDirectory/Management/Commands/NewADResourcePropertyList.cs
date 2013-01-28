using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADResourcePropertyList", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216382", SupportsShouldProcess=true)]
	public class NewADResourcePropertyList : ADNewCmdletBase<NewADResourcePropertyListParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList>
	{
		public NewADResourcePropertyList()
		{
		}

		protected internal override string GetDefaultCreationPathBase()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}