using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADResourcePropertyValueType", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216364", DefaultParameterSetName="Filter")]
	public class GetADResourcePropertyValueType : ADGetCmdletBase<GetADResourcePropertyValueTypeParameterSet, ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>, ADResourcePropertyValueType>
	{
		public GetADResourcePropertyValueType()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetResourcePropertyValueTypeContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetResourcePropertyValueTypeContainerPath();
		}

		private string GetResourcePropertyValueTypeContainerPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Value Types,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}
	}
}