using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Network
{
	[Cmdlet("New", "AzureDns")]
	public class NewAzureDnsCommand : Cmdlet
	{
		[Parameter(Position=1, Mandatory=true, HelpMessage="IP Address of the DNS Server")]
		[ValidateNotNullOrEmpty]
		public string IPAddress
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, HelpMessage="Name of the DNS Server")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public NewAzureDnsCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				DnsServer dnsServer = new DnsServer();
				dnsServer.Address = this.IPAddress;
				dnsServer.Name = this.Name;
				base.WriteObject(dnsServer, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}