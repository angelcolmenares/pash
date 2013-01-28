using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("Get", "AzureDns")]
	public class GetAzureDnsCommand : PSCmdlet
	{
		[Alias(new string[] { "InputObject" })]
		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="DnsSettings Returned from Get-AzureDeployment")]
		[ValidateNotNullOrEmpty]
		public DnsSettings DnsSettings
		{
			get;
			set;
		}

		public GetAzureDnsCommand()
		{
		}

		protected override void ProcessRecord()
		{
			if (this.DnsSettings != null && this.DnsSettings.DnsServers != null)
			{
				base.WriteObject(this.DnsSettings.DnsServers, true);
			}
		}
	}
}