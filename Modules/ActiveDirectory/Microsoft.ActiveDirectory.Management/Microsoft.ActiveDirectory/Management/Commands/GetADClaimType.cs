using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADClaimType", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216348", DefaultParameterSetName="Filter")]
	public class GetADClaimType : ADGetCmdletBase<GetADClaimTypeParameterSet, ADClaimTypeFactory<ADClaimType>, ADClaimType>
	{
		public GetADClaimType()
		{
		}

		private string GetClaimTypeContainerPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetClaimTypeContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetClaimTypeContainerPath();
		}
	}
}