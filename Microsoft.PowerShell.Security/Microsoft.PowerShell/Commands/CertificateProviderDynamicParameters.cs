using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class CertificateProviderDynamicParameters
	{
		private SwitchParameter codeSigningCert;

		private SwitchParameter sslServerAuthentication;

		private DnsNameRepresentation dnsName;

		private string[] eku;

		private int expiringInDays;

		[Parameter]
		public SwitchParameter CodeSigningCert
		{
			get
			{
				return this.codeSigningCert;
			}
			set
			{
				this.codeSigningCert = value;
			}
		}

		[Parameter]
		public DnsNameRepresentation DnsName
		{
			get
			{
				return this.dnsName;
			}
			set
			{
				this.dnsName = value;
			}
		}

		[Parameter]
		public string[] Eku
		{
			get
			{
				return this.eku;
			}
			set
			{
				this.eku = value;
			}
		}

		[Parameter]
		public int ExpiringInDays
		{
			get
			{
				return this.expiringInDays;
			}
			set
			{
				this.expiringInDays = value;
			}
		}

		[Parameter]
		public SwitchParameter SSLServerAuthentication
		{
			get
			{
				return this.sslServerAuthentication;
			}
			set
			{
				this.sslServerAuthentication = value;
			}
		}

		public CertificateProviderDynamicParameters()
		{
			this.codeSigningCert = new SwitchParameter();
			this.sslServerAuthentication = new SwitchParameter();
			this.expiringInDays = -1;
		}
	}
}