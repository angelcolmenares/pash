using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("New", "AzureCertificateSetting")]
	public class NewAzureCertificateSettingCommand : Cmdlet
	{
		public string StoreLocation
		{
			get
			{
				return "LocalMachine";
			}
		}

		[Parameter(Position=0, Mandatory=false, HelpMessage="Store Name of the Certificate. Default is My.")]
		[ValidateSet(new string[] { "AddressBook", "AuthRoot", "CertificateAuthority", "Disallowed", "My", "Root", "TrustedPeople", "TrustedPublisher" })]
		public string StoreName
		{
			get;
			set;
		}

		[Parameter(Position=1, Mandatory=true, HelpMessage="Certificate Thumbprint.")]
		[ValidateNotNullOrEmpty]
		public string Thumbprint
		{
			get;
			set;
		}

		public NewAzureCertificateSettingCommand()
		{
		}

		protected override void ProcessRecord()
		{
			string storeName;
			try
			{
				base.ProcessRecord();
				CertificateSetting certificateSetting = new CertificateSetting();
				certificateSetting.StoreLocation = this.StoreLocation;
				CertificateSetting certificateSetting1 = certificateSetting;
				if (string.IsNullOrEmpty(this.StoreName))
				{
					storeName = "My";
				}
				else
				{
					storeName = this.StoreName;
				}
				certificateSetting1.StoreName = storeName;
				certificateSetting.Thumbprint = this.Thumbprint;
				base.WriteObject(certificateSetting, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}