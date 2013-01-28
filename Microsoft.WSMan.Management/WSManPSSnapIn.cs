using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	[RunInstaller(true)]
	public class WSManPSSnapIn : PSSnapIn
	{
		public override string Description
		{
			get
			{
				return "This is a PowerShell snap-in that includes the WsMan cmdlets.";
			}
		}

		public override string DescriptionResource
		{
			get
			{
				return "WsManPSSnapIn,This is a PowerShell snap-in that includes the WsMan cmdlets.";
			}
		}

		public override string Name
		{
			get
			{
				return "WsManPSSnapIn";
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
				return "WsManPSSnapIn,Microsoft";
			}
		}

		public WSManPSSnapIn()
		{
		}
	}
}