using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	public class ProvisioningConfigurationCmdletBase : PSCmdlet
	{
		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Set of certificates to install in the VM.")]
		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Set of certificates to install in the VM.")]
		[ValidateNotNullOrEmpty]
		public CertificateSettingList Certificates
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Disable Automatic Updates.")]
		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Disable Automatic Updates.")]
		[ValidateNotNullOrEmpty]
		public SwitchParameter DisableAutomaticUpdates
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="Disable SSH Password Authentication.")]
		public SwitchParameter DisableSSH
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="WindowsDomain", HelpMessage="Domain name.")]
		[ValidateNotNullOrEmpty]
		public string Domain
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="WindowsDomain", HelpMessage="Domain password.")]
		[ValidateNotNullOrEmpty]
		public string DomainPassword
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="WindowsDomain", HelpMessage="Domain user name.")]
		[ValidateNotNullOrEmpty]
		public string DomainUserName
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="WindowsDomain", HelpMessage="Domain to join (FQDN).")]
		[ValidateNotNullOrEmpty]
		public string JoinDomain
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Linux", HelpMessage="Set configuration to Linux.")]
		public SwitchParameter Linux
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Linux", HelpMessage="User to Create")]
		[ValidateNotNullOrEmpty]
		public string LinuxUser
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Machine object organization unit.")]
		[ValidateNotNullOrEmpty]
		public string MachineObjectOU
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Do No Create an RDP Endpoint.")]
		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Do Not Create an RDP Endpoint.")]
		[ValidateNotNullOrEmpty]
		public SwitchParameter NoRDPEndpoint
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="Do not create an SSH Endpoint.")]
		public SwitchParameter NoSSHEndpoint
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Administrator password to use for the role.")]
		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Administrator password to use for the role.")]
		[Parameter(Mandatory=true, ParameterSetName="Linux", HelpMessage="Default password for linux user created.")]
		[ValidateNotNullOrEmpty]
		public string Password
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Specify to force the user to change the password on first logon.")]
		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Specify to force the user to change the password on first logon.")]
		[ValidateNotNullOrEmpty]
		public SwitchParameter ResetPasswordOnFirstLogon
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="SSH Key Pairs")]
		public LinuxProvisioningConfigurationSet.SSHKeyPairList SSHKeyPairs
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="Linux", HelpMessage="SSH Public Key List")]
		public LinuxProvisioningConfigurationSet.SSHPublicKeyList SSHPublicKeys
		{
			get;
			set;
		}

		[Parameter(Mandatory=false, ParameterSetName="WindowsDomain", HelpMessage="Specify the time zone for the virtual machine.")]
		[Parameter(Mandatory=false, ParameterSetName="Windows", HelpMessage="Specify the time zone for the virtual machine.")]
		[ValidateNotNullOrEmpty]
		public string TimeZone
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="Windows", HelpMessage="Set configuration to Windows.")]
		public SwitchParameter Windows
		{
			get;
			set;
		}

		[Parameter(Mandatory=true, ParameterSetName="WindowsDomain", HelpMessage="Set configuration to Windows with Domain Join.")]
		public SwitchParameter WindowsDomain
		{
			get;
			set;
		}

		public ProvisioningConfigurationCmdletBase()
		{
		}

		protected void SetProvisioningConfiguration(LinuxProvisioningConfigurationSet provisioningConfiguration)
		{
			provisioningConfiguration.UserName = this.LinuxUser;
			provisioningConfiguration.UserPassword = this.Password;
			SwitchParameter disableSSH = this.DisableSSH;
			if (!disableSSH.IsPresent)
			{
				provisioningConfiguration.DisableSshPasswordAuthentication = new bool?(false);
			}
			else
			{
				provisioningConfiguration.DisableSshPasswordAuthentication = new bool?(true);
			}
			if (this.SSHKeyPairs != null && this.SSHKeyPairs.Count > 0 || this.SSHPublicKeys != null && this.SSHPublicKeys.Count > 0)
			{
				provisioningConfiguration.SSH = new LinuxProvisioningConfigurationSet.SSHSettings();
				provisioningConfiguration.SSH.PublicKeys = this.SSHPublicKeys;
				provisioningConfiguration.SSH.KeyPairs = this.SSHKeyPairs;
			}
		}

		protected void SetProvisioningConfiguration(WindowsProvisioningConfigurationSet provisioningConfiguration)
		{
			provisioningConfiguration.AdminPassword = this.Password;
			SwitchParameter resetPasswordOnFirstLogon = this.ResetPasswordOnFirstLogon;
			provisioningConfiguration.ResetPasswordOnFirstLogon = resetPasswordOnFirstLogon.IsPresent;
			provisioningConfiguration.StoredCertificateSettings = this.Certificates;
			SwitchParameter disableAutomaticUpdates = this.DisableAutomaticUpdates;
			provisioningConfiguration.EnableAutomaticUpdates = new bool?(!disableAutomaticUpdates.IsPresent);
			if (!string.IsNullOrEmpty(this.TimeZone))
			{
				provisioningConfiguration.TimeZone = this.TimeZone;
			}
			if (base.ParameterSetName == "WindowsDomain")
			{
				WindowsProvisioningConfigurationSet.DomainJoinSettings domainJoinSetting = new WindowsProvisioningConfigurationSet.DomainJoinSettings();
				WindowsProvisioningConfigurationSet.DomainJoinCredentials domainJoinCredential = new WindowsProvisioningConfigurationSet.DomainJoinCredentials();
				domainJoinCredential.Username = this.DomainUserName;
				domainJoinCredential.Password = this.DomainPassword;
				domainJoinCredential.Domain = this.Domain;
				domainJoinSetting.Credentials = domainJoinCredential;
				domainJoinSetting.MachineObjectOU = this.MachineObjectOU;
				domainJoinSetting.JoinDomain = this.JoinDomain;
				provisioningConfiguration.DomainJoin = domainJoinSetting;
			}
		}
	}
}