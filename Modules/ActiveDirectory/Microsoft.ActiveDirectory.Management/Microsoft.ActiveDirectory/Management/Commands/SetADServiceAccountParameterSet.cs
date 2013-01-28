using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADServiceAccountParameterSet : ADParameterSet
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
		public string[] Certificates
		{
			get
			{
				return base["Certificates"] as string[];
			}
			set
			{
				base["Certificates"] = value;
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
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
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
		public ADServiceAccount Identity
		{
			get
			{
				return base["Identity"] as ADServiceAccount;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADServiceAccount Instance
		{
			get
			{
				return base["Instance"] as ADServiceAccount;
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
		public ADPrincipal[] PrincipalsAllowedToRetrieveManagedPassword
		{
			get
			{
				return base["PrincipalsAllowedToRetrieveManagedPassword"] as ADPrincipal[];
			}
			set
			{
				base["PrincipalsAllowedToRetrieveManagedPassword"] = value;
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
		public string SamAccountName
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

		public SetADServiceAccountParameterSet()
		{
		}
	}
}