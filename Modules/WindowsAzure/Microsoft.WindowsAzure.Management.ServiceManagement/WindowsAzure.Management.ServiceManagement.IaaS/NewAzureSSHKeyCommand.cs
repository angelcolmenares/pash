using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("New", "AzureSSHKey")]
	public class NewAzureSSHKeyCommand : Cmdlet
	{
		[Parameter(Position=1, Mandatory=true, HelpMessage="Fingerprint of the SSH Key Pair")]
		[ValidateNotNullOrEmpty]
		public string Fingerprint
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="keypair", HelpMessage="Add a key pair")]
		[ValidateNotNullOrEmpty]
		public SwitchParameter KeyPair
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, HelpMessage="Path of the SSH Key Pair")]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get;
			set;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="publickey", HelpMessage="Add a public")]
		[ValidateNotNullOrEmpty]
		public SwitchParameter PublicKey
		{
			get;
			set;
		}

		public NewAzureSSHKeyCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				SwitchParameter keyPair = this.KeyPair;
				if (!keyPair.IsPresent)
				{
					LinuxProvisioningConfigurationSet.SSHPublicKey sSHPublicKey = new LinuxProvisioningConfigurationSet.SSHPublicKey();
					sSHPublicKey.Fingerprint = this.Fingerprint;
					sSHPublicKey.Path = this.Path;
					base.WriteObject(sSHPublicKey, true);
				}
				else
				{
					LinuxProvisioningConfigurationSet.SSHKeyPair sSHKeyPair = new LinuxProvisioningConfigurationSet.SSHKeyPair();
					sSHKeyPair.Fingerprint = this.Fingerprint;
					sSHKeyPair.Path = this.Path;
					base.WriteObject(sSHKeyPair, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}