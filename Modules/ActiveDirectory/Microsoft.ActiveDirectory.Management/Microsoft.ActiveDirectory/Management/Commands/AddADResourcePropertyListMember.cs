using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADResourcePropertyListMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216341", SupportsShouldProcess=true)]
	public class AddADResourcePropertyListMember : ADSetObjectMember<AddADResourcePropertyListMemberParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public AddADResourcePropertyListMember() : base(0)
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
		}

		internal override string GetMemberDefaultPartitionPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
		}
	}
}