using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell
{
	[RunInstaller(true)]
	public sealed class PSHostPSSnapIn : PSSnapIn
	{
		public override string Description
		{
			get
			{
				return "This PSSnapIn contains cmdlets used by the MSH host.";
			}
		}

		public override string DescriptionResource
		{
			get
			{
				return "HostMshSnapInResources,Description";
			}
		}

		public override string Name
		{
			get
			{
				return "Microsoft.PowerShell.Host";
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
				return "HostMshSnapInResources,Vendor";
			}
		}

		public PSHostPSSnapIn()
		{
		}
	}
}