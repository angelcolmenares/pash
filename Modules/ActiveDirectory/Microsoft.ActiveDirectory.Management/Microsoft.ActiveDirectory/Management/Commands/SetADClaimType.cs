using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADClaimType", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216411", SupportsShouldProcess=true, DefaultParameterSetName="Identity")]
	public class SetADClaimType : ADSetCmdletBase<SetADClaimTypeParameterSet, ADClaimTypeFactory<ADClaimType>, ADClaimType>
	{
		public SetADClaimType()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
		}
	}
}