using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADResourceProperty", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216362", DefaultParameterSetName="Filter")]
	public class GetADResourceProperty : ADGetCmdletBase<GetADResourcePropertyParameterSet, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public GetADResourceProperty()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetResourcePropertyContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetResourcePropertyContainerPath();
		}

		private string GetResourcePropertyContainerPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
		}
	}
}