using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADResourcePropertyListMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216399", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADResourcePropertyListMember : ADSetObjectMember<RemoveADResourcePropertyListMemberParameterSet, ADResourcePropertyListFactory<ADResourcePropertyList>, ADResourcePropertyList, ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>
	{
		public RemoveADResourcePropertyListMember() : base((SetADMemberOperationType)1)
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