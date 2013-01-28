using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management
{
	[RunInstaller(true)]
	public class ActiveDirectoryPowerShellSnapIn : PSSnapIn
	{
		public override string Description
		{
			get
			{
				return "This is a PowerShellsnap-in that registers the Active Directory provider and cmdlets.";
			}
		}

		public override string DescriptionResource
		{
			get
			{
				return "ADProviderSnapin,This is a PowerShellsnap-in that registers the Active Directory provider and cmdlets.";
			}
		}

		public override string Name
		{
			get
			{
				return "ADProviderSnapin";
			}
		}

		public override string Vendor
		{
			get
			{
				return "Microsoft";
			}
		}

		public override string VendorResource
		{
			get
			{
				return "ADProviderSnapin,Microsoft";
			}
		}

		public ActiveDirectoryPowerShellSnapIn()
		{
		}
	}
}