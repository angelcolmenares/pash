using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell
{
	[RunInstaller(true)]
	public sealed class PSSecurityPSSnapIn : PSSnapIn
	{
		public override string Description
		{
			get
			{
				return "This PSSnapIn contains cmdlets to manage MSH security.";
			}
		}

		public override string DescriptionResource
		{
			get
			{
				return "SecurityMshSnapInResources,Description";
			}
		}

		public override string Name
		{
			get
			{
				return "Microsoft.PowerShell.Security";
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
				return "SecurityMshSnapInResources,Vendor";
			}
		}

		public PSSecurityPSSnapIn()
		{
		}
	}
}