using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.DnsClient.Commands
{
	[RunInstaller(true)]
	public class DnsClientCommands : PSSnapIn
	{
		public override string Description
		{
			get
			{
				return "PowerShell cmdlets that provide DNS functionality.";
			}
		}

		public override string DescriptionResource
		{
			get
			{
				return "DnsClientCommands,PowerShell cmdlets that provide DNS functionality.";
			}
		}

		public override string Name
		{
			get
			{
				return "DnsClientCommands";
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
				return "DnsClientCommands,Microsoft";
			}
		}

		public DnsClientCommands()
		{
		}
	}
}