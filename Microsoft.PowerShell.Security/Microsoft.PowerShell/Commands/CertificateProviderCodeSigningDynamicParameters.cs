using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class CertificateProviderCodeSigningDynamicParameters
	{
		private SwitchParameter codeSigningCert;

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

		public CertificateProviderCodeSigningDynamicParameters()
		{
			this.codeSigningCert = new SwitchParameter();
		}
	}
}