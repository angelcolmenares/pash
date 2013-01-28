using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADComputerParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		public DateTime? AccountExpirationDate
		{
			get
			{
				return (DateTime?)(base["AccountExpirationDate"] as DateTime?);
			}
			set
			{
				base["AccountExpirationDate"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? AccountNotDelegated
		{
			get
			{
				return (bool?)(base["AccountNotDelegated"] as bool?);
			}
			set
			{
				base["AccountNotDelegated"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Add
		{
			get
			{
				return base["Add"] as Hashtable;
			}
			set
			{
				base["Add"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? AllowReversiblePasswordEncryption
		{
			get
			{
				return (bool?)(base["AllowReversiblePasswordEncryption"] as bool?);
			}
			set
			{
				base["AllowReversiblePasswordEncryption"] = value;
			}
		}

		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return (ADAuthType)base["AuthType"];
			}
			set
			{
				base["AuthType"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? CannotChangePassword
		{
			get
			{
				return (bool?)(base["CannotChangePassword"] as bool?);
			}
			set
			{
				base["CannotChangePassword"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateSetOperationsHashtable(typeof(X509Certificate))]
		public Hashtable Certificates
		{
			get
			{
				return base["Certificates"] as Hashtable;
			}
			set
			{
				base["Certificates"] = new ADMultivalueHashtableParameter<X509Certificate>(value);
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? ChangePasswordAtLogon
		{
			get
			{
				return (bool?)(base["ChangePasswordAtLogon"] as bool?);
			}
			set
			{
				base["ChangePasswordAtLogon"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNullOrEmpty]
		public string[] Clear
		{
			get
			{
				return base["Clear"] as string[];
			}
			set
			{
				base["Clear"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public bool? CompoundIdentitySupported
		{
			get
			{
				return (bool?)(base["CompoundIdentitySupported"] as bool?);
			}
			set
			{
				base["CompoundIdentitySupported"] = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return base["Credential"] as PSCredential;
			}
			set
			{
				base["Credential"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Description
		{
			get
			{
				return base["Description"] as string;
			}
			set
			{
				base["Description"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string DisplayName
		{
			get
			{
				return base["DisplayName"] as string;
			}
			set
			{
				base["DisplayName"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string DNSHostName
		{
			get
			{
				return base["DNSHostName"] as string;
			}
			set
			{
				base["DNSHostName"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? Enabled
		{
			get
			{
				return (bool?)(base["Enabled"] as bool?);
			}
			set
			{
				base["Enabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string HomePage
		{
			get
			{
				return base["HomePage"] as string;
			}
			set
			{
				base["HomePage"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ADComputer Identity
		{
			get
			{
				return base["Identity"] as ADComputer;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADComputer Instance
		{
			get
			{
				return base["Instance"] as ADComputer;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADKerberosEncryptionType KerberosEncryptionType
		{
			get
			{
				return (ADKerberosEncryptionType)base["KerberosEncryptionType"];
			}
			set
			{
				base["KerberosEncryptionType"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Location
		{
			get
			{
				return base["Location"] as string;
			}
			set
			{
				base["Location"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADPrincipal ManagedBy
		{
			get
			{
				return base["ManagedBy"] as ADPrincipal;
			}
			set
			{
				base["ManagedBy"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OperatingSystem
		{
			get
			{
				return base["OperatingSystem"] as string;
			}
			set
			{
				base["OperatingSystem"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OperatingSystemHotfix
		{
			get
			{
				return base["OperatingSystemHotfix"] as string;
			}
			set
			{
				base["OperatingSystemHotfix"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OperatingSystemServicePack
		{
			get
			{
				return base["OperatingSystemServicePack"] as string;
			}
			set
			{
				base["OperatingSystemServicePack"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OperatingSystemVersion
		{
			get
			{
				return base["OperatingSystemVersion"] as string;
			}
			set
			{
				base["OperatingSystemVersion"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNullOrEmpty]
		public string Partition
		{
			get
			{
				return base["Partition"] as string;
			}
			set
			{
				base["Partition"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter PassThru
		{
			get
			{
				return base.GetSwitchParameter("PassThru");
			}
			set
			{
				base["PassThru"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? PasswordNeverExpires
		{
			get
			{
				return (bool?)(base["PasswordNeverExpires"] as bool?);
			}
			set
			{
				base["PasswordNeverExpires"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? PasswordNotRequired
		{
			get
			{
				return (bool?)(base["PasswordNotRequired"] as bool?);
			}
			set
			{
				base["PasswordNotRequired"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADPrincipal[] PrincipalsAllowedToDelegateToAccount
		{
			get
			{
				return base["PrincipalsAllowedToDelegateToAccount"] as ADPrincipal[];
			}
			set
			{
				base["PrincipalsAllowedToDelegateToAccount"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Remove
		{
			get
			{
				return base["Remove"] as Hashtable;
			}
			set
			{
				base["Remove"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Replace
		{
			get
			{
				return base["Replace"] as Hashtable;
			}
			set
			{
				base["Replace"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string SAMAccountName
		{
			get
			{
				return base["SamAccountName"] as string;
			}
			set
			{
				base["SamAccountName"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return base["Server"] as string;
			}
			set
			{
				base["Server"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable ServicePrincipalNames
		{
			get
			{
				return base["ServicePrincipalNames"] as Hashtable;
			}
			set
			{
				base["ServicePrincipalNames"] = new ADMultivalueHashtableParameter<string>(value);
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? TrustedForDelegation
		{
			get
			{
				return (bool?)(base["TrustedForDelegation"] as bool?);
			}
			set
			{
				base["TrustedForDelegation"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string UserPrincipalName
		{
			get
			{
				return base["UserPrincipalName"] as string;
			}
			set
			{
				base["UserPrincipalName"] = value;
			}
		}

		public SetADComputerParameterSet()
		{
		}
	}
}