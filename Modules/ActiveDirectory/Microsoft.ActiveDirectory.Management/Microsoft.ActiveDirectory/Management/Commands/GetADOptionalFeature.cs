using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADOptionalFeature", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219318", DefaultParameterSetName="Filter")]
	public class GetADOptionalFeature : ADGetCmdletBase<GetADOptionalFeatureParameterSet, ADOptionalFeatureFactory<ADOptionalFeature>, ADOptionalFeature>
	{
		public GetADOptionalFeature()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().ConfigurationNamingContext;
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetRootDSE().ConfigurationNamingContext;
		}
	}
}