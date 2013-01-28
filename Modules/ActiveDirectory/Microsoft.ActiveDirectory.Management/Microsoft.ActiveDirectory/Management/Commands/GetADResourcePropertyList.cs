using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADResourcePropertyList", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216363", DefaultParameterSetName="Filter")]
	public class GetADResourcePropertyList : ADGetCmdletBase<GetADResourcePropertyListParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList>
	{
		public GetADResourcePropertyList()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetResourcePropertyListContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetResourcePropertyListContainerPath();
		}

		private string GetResourcePropertyListContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}